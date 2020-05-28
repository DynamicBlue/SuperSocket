using Dynamic.Core.Extensions;
using SuperSocket.ProtoBase;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace SuperSocket.ProtoBase.Filter
{
    public class DefaultBeginEndWraperByteFilter : BeginEndMarkPipelineFilter<BinaryRequestInfo>
    {
        private readonly static  byte[] _BeginMark = new byte[] { 0x7e };
        private readonly static byte[] _EndMark = new byte[] { 0x7e };

        public virtual bool IsContainterOriBuffer => true;

        public byte[] BeginMark { get; set; }
        public byte[] EndMark { get; set; }
        public DefaultBeginEndWraperByteFilter() : this(_BeginMark, _EndMark)
        {

        }
        public DefaultBeginEndWraperByteFilter(ReadOnlyMemory<byte> beginMark, ReadOnlyMemory<byte> endMark):base(beginMark,endMark)
        {
            this.BeginMark = beginMark.ToArray();
            this.EndMark = endMark.ToArray();
        }
        public override BinaryRequestInfo Filter(ref SequenceReader<byte> reader)
        {
            return base.Filter(ref reader);
        }
        protected virtual BinaryRequestInfo ProcessMatchedRequest(ref ReadOnlySequence<byte> buffer)
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
        protected  override BinaryRequestInfo DecodePackage(ref ReadOnlySequence<byte> buffer)
        {
            return ProcessMatchedRequest(ref buffer);
        }
    }
}
