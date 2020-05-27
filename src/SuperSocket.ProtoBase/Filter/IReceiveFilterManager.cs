using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;

namespace SuperSocket.ProtoBase.Filter
{
    interface IReceiveFilterManager<TPackageInfo> where TPackageInfo : class
    {
        IPipelineFilter<TPackageInfo> Get(ref SequenceReader<byte> reader);
    }
}
