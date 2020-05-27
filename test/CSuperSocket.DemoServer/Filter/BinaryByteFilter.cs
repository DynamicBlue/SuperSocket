using Dynamic.Core.Extensions;
using SuperSocket.ProtoBase;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;

namespace CSuperSocket.DemoServer.Filter
{
    public class BinaryByteFilter : BeginEndMarkPipelineFilter<BinaryRequestInfo>
    {
        private readonly static byte[] BeginMark = new byte[] { 0x7e };
        private readonly static byte[] EndMark = new byte[] { 0x7e };

       
        public BinaryByteFilter() : base(BeginMark, EndMark)
        {
        }
        protected override BinaryRequestInfo DecodePackage(ref ReadOnlySequence<byte> buffer)
        {
            return new BinaryRequestInfo() { Key=BeginMark.ToHex(),Body=buffer};
        }
    }
}
