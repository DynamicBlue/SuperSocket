
using System;
using SuperSocket.ProtoBase;
using SuperSocket.Channel;
using System.Threading.Tasks;

namespace SuperSocket
{
    public interface IChannelRegister
    {
        Task RegisterChannel(object connection);
    }
}