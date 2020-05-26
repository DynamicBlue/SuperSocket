using SuperSocket.ProtoBase;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;

namespace SuperSocket.Server.Runtime
{
    public class BinaryRequestInfo: IKeyedPackageInfo<string>
    {
        public ReadOnlySequence<byte> Body { get; set; }

        public string Key { get; set; }
    }
}
