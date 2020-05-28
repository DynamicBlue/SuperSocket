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
      
    }
}
