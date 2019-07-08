using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;

namespace LockNess.Communication.Filter
{
    public interface ISockedFilter
    {
        void Filter(ref SequenceReader<byte> reader);

    }
}
