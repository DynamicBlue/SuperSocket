using Dynamic.Core.Extensions;
using SuperSocket.ProtoBase;
using SuperSocket.ProtoBase.Filter;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace CSuperSocket.DemoServer.Filter
{
    public class BeginEndWraperByteFilter : DefaultBeginEndWraperByteFilter
    {
        protected readonly static  byte[] BeginMark = new byte[] { 0x7e };
        protected readonly static byte[] EndMark = new byte[] { 0x7e };

        public override bool IsContainterOriBuffer => true;
        public BeginEndWraperByteFilter() : base(BeginMark, EndMark)
        {

        }
        protected override BinaryRequestInfo ProcessMatchedRequest(ref ReadOnlySequence<byte> buffer)
        {
            var requestInfo = new BinaryRequestInfo() { Key = BeginMark.ToHex(), Body = buffer };
            if (IsContainterOriBuffer)
            {
                requestInfo.OriBuffer = new byte[buffer.Length + BeginMark.Length + EndMark.Length];
                long index = 0;
                Array.Copy(BeginMark, 0, requestInfo.OriBuffer, 0, BeginMark.Length);
                index += BeginMark.Length;
                Array.Copy(buffer.ToArray(), 0, requestInfo.OriBuffer, index, buffer.Length);
                index += buffer.Length;
                Array.Copy(EndMark, 0, requestInfo.OriBuffer, index, EndMark.Length);

            }
            return requestInfo;
        }
      
    }
}
