using Dynamic.Core.Service;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;

namespace SuperSocket.ProtoBase.Filter
{
    public class ReceiveFilter<TPackageInfo> : PipelineFilterBase<TPackageInfo> where TPackageInfo : class
    {
        public override TPackageInfo Filter(ref SequenceReader<byte> reader)
        {
            var filterManager = IocUnity.Get<IReceiveFilterManager<TPackageInfo>>();
            if (filterManager != null)
            {
                var nextFilter = filterManager.Get(ref reader);
                this.NextFilter = nextFilter;
            }
            return null;
        }
    }
}
