using LockNess.Communication.Core.Filter;
using LockNess.Communication.Core.SocketFac;
using LockNess.Communication.Filter;

using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LockNess.Communication.Core
{
    public class TcpConnection: IConnection
    {
        private ISocketBuilder _socketBuilder;
        private Socket _socket;
        private List<ArraySegment<byte>> _segmentsForSend;
        private Pipe Output;
        private ISockedFilter _sockedFilter;
        public TcpConnection(Socket socket)
        { 
            _socket = socket;
            Output = new Pipe();
        }


        public async Task StartAsync()
        {
            Task read= ProcessReadingAsync();
            Task write = ProcessSendsAsync();
            await Task.WhenAll(read);
        }

        private async Task ProcessSendsAsync()
        {
            var output = Output.Reader;

            while (true)
            {
                var result = await output.ReadAsync();

                if (result.IsCanceled)
                    break;

                var completed = result.IsCompleted;

                var buffer = result.Buffer;
                var end = buffer.End;

                if (!buffer.IsEmpty)
                {
                    try
                    {
                        await SendAsync(buffer);
                    }
                    catch (Exception e)
                    {
                     
                        output.Complete(e);
                        return;
                    }
                }

                output.AdvanceTo(end);

                if (completed)
                {
                    break;
                }
            }

            output.Complete();
        }

        private async ValueTask<int> SendAsync(ReadOnlySequence<byte> buffer)
        {
            if (buffer.IsSingleSegment)
            {
                return await _socket.SendAsync(GetArrayByMemory(buffer.First), SocketFlags.None);
            }

            if (_segmentsForSend == null)
            {
                _segmentsForSend = new List<ArraySegment<byte>>();
            }
            else
            {
                _segmentsForSend.Clear();
            }

            var segments = _segmentsForSend;

            foreach (var piece in buffer)
            {
                _segmentsForSend.Add(GetArrayByMemory(piece));
            }

            return await _socket.SendAsync(_segmentsForSend, SocketFlags.None);
        }


        internal ArraySegment<T> GetArrayByMemory<T>(ReadOnlyMemory<T> memory)
        {
            if (!MemoryMarshal.TryGetArray(memory, out var result))
            {
                throw new InvalidOperationException("Buffer backed by array was expected");
            }

            return result;
        }

        public async ValueTask SendAsync(ReadOnlyMemory<byte> buffer)
        {
            var writer = Output.Writer;
            await writer.WriteAsync(buffer);
            await writer.FlushAsync();
        }

        private  async Task ProcessReadingAsync()
        {
            Console.WriteLine($"[{_socket.RemoteEndPoint}]: connected");

            var pipe = new Pipe();
            Task writing = FillPipeAsync(_socket, pipe.Writer);
            Task reading = ReadPipeAsync(_socket, pipe.Reader);
            await Task.WhenAll(reading, writing);

            Console.WriteLine($"[{_socket.RemoteEndPoint}]: disconnected");
        }

        private  async Task FillPipeAsync(Socket socket, PipeWriter writer)
        {
            const int minimumBufferSize = 512;

            while (true)
            {
                try
                {
                    // Request a minimum of 512 bytes from the PipeWriter
                    Memory<byte> memory = writer.GetMemory(minimumBufferSize);

                    int bytesRead = await socket.ReceiveAsync(memory, SocketFlags.None);
                    if (bytesRead == 0)
                    {
                        break;
                    }

                    // Tell the PipeWriter how much was read
                    writer.Advance(bytesRead);
                }
                catch
                {
                    break;
                }

                // Make the data available to the PipeReader
                FlushResult result = await writer.FlushAsync();

                if (result.IsCompleted)
                {
                    break;
                }
            }

            // Signal to the reader that we're done writing
            writer.Complete();
        }

        private async Task ReadPipeAsync(Socket socket, PipeReader reader)
        {
            while (true)
            {
                ReadResult result = await reader.ReadAsync();
                ReadOnlySequence<byte> buffer = result.Buffer;
                SequencePosition consumed = buffer.Start;
                SequencePosition examined = buffer.End;
                if (result.IsCanceled)
                    break;

                var completed = result.IsCompleted;

               var pos= ProcessRead(buffer, out consumed, out examined);
                if (completed)
                {
                    break;
                }

                reader.AdvanceTo(consumed, examined);
            }

            reader.Complete();
        }
        private  SequencePosition ProcessRead(ReadOnlySequence<byte> sequence,out SequencePosition consumed, out SequencePosition examined)
        {
            SequenceReader<byte> reader = new SequenceReader<byte>(sequence);
             consumed = sequence.Start;
             examined = sequence.End;

            while (true)
            {
                if (reader.Length < 4)
                {
                    reader.Advance(reader.Length);
                    break;
                }

                var headsizeS = reader.Sequence.Slice(0, 4);
                var bodysize = System.BitConverter.ToInt32(headsizeS.ToArray());

               
                if (bodysize + 4 <= reader.Length)
                {
                    ProcessLine(_socket, reader.Sequence.Slice(4, bodysize));
                    reader.Advance(bodysize + 4);
                    consumed = reader.Position;
                    continue;
                }
                else
                {
                    break;
                }
            }
            return reader.Position;
        }

        private void Des(ReadOnlySequence<byte> buffer)
        {
            SequenceReader<byte> seqReader = new SequenceReader<byte>(buffer);
            _sockedFilter.Filter(ref seqReader);
        }

        private void ProcessLine(Socket socket, in ReadOnlySequence<byte> buffer)
        {
            Console.WriteLine($"[{socket.RemoteEndPoint}]: ");
            foreach (var segment in buffer)
            {
                Console.Write(Encoding.UTF8.GetString(segment.Span));
            }
            Console.WriteLine();
        }
    }
}
