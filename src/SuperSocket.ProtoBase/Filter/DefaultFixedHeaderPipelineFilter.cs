using Dynamic.Core.Extensions;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;

namespace SuperSocket.ProtoBase.Filter
{
    public class DefaultFixedHeaderPipelineFilter : FixedHeaderPipelineFilter<BinaryRequestInfo>
    {
        protected int _headerLength { get; set; }
        protected virtual int _headerKeyLength => 2;
        public  string Key { get;protected set; }

        public DefaultFixedHeaderPipelineFilter():this(5)
        {

        }
        public DefaultFixedHeaderPipelineFilter(int headerSize = 5) : base(headerSize)
        {
            this._headerLength = headerSize;
        }
        protected virtual BinaryRequestInfo ProcessMatchedRequest(ref ReadOnlySequence<byte> buffer)
        {
            BinaryRequestInfo binaryRequestInfo = new BinaryRequestInfo();
            binaryRequestInfo.OriBuffer = buffer.ToArray();
            binaryRequestInfo.Key = this.Key;
            return binaryRequestInfo;
        }
        protected virtual int GetBodyLengthByHeader(ref ReadOnlySequence<byte> buffer)
        {
            byte[] headerKeyArr = buffer.Slice(0, _headerKeyLength).ToArray();
            this.Key = headerKeyArr.ToHex();
            byte[] bodyLengthArr = null; ;
            var offset = _headerLength - _headerKeyLength;
            bodyLengthArr = buffer.Slice(offset, _headerKeyLength).ToArray();
            //判断大端小端
            if (BitConverter.IsLittleEndian)
            {
                bodyLengthArr = bodyLengthArr.Reverse().ToArray();
            }
            var bodyLength = BitConverter.ToInt16(bodyLengthArr, 0);
            return bodyLength;
        }
        protected override int GetBodyLengthFromHeader(ref ReadOnlySequence<byte> buffer)
        {
            return GetBodyLengthByHeader(ref buffer);
        }
        protected override BinaryRequestInfo DecodePackage(ref ReadOnlySequence<byte> buffer)
        {
            return ProcessMatchedRequest(ref buffer);
        }
    }
}
