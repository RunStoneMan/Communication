using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;

namespace LockNess.Communication.Core.Filter
{
    public abstract class FixedHeaderPipelineFilter : ISockedFilter
    {
        private bool _foundHeader;
        private readonly int _headerSize;
        private int _totalSize;

        protected FixedHeaderPipelineFilter(int headerSize)
        {

        }

        public void Filter(ref SequenceReader<byte> reader)
        {
            if (!_foundHeader)
            {
                if (reader.Length < _headerSize)
                    //return null;
                    Console.WriteLine("");

                _foundHeader = true;
                var header = reader.Sequence.Slice(0, _headerSize);
                var bodyLength = GetBodyLengthFromHeader(header);

                if (bodyLength < 0)
                {
                    throw new Exception("Failed to get body length from the package header.");
                }
                else if (bodyLength == 0)
                {
                    reader.Advance(_headerSize);
                   // return DecodePackage(header);
                }
                else
                {
                    _totalSize = _headerSize + bodyLength;
                }
            }
            var totalSize = _totalSize;
            if (reader.Length > totalSize)
            {
                reader.Advance(totalSize);
               // return DecodePackage(reader.Sequence.Slice(0, totalSize));
            }
            else if (reader.Length == totalSize)
            {
                reader.Advance(totalSize);
                //return DecodePackage(reader.Sequence);
            }

          //  return null;
        }

        protected abstract int GetBodyLengthFromHeader(ReadOnlySequence<byte> buffer);

        public  void Reset()
        {
            _foundHeader = false;
            _totalSize = 0;
        }
    }
}
