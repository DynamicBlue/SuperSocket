using SuperSocket.Channel;

namespace SuperSocket
{
    public interface IChannelCreatorFactory
    {
        IChannelCreator CreateChannelCreator<TPackageInfo>(ListenOptions options, ChannelOptions channelOptions, object pipelineFilterFactory);
    }
}