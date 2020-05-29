using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;

namespace SuperSocket.ProtoBase.Filter
{
    public  interface IReceiveFilterManager<TPackageInfo> where TPackageInfo : class
    {
        IPipelineFilter<TPackageInfo> Get(ref SequenceReader<byte> reader);
    }
}
