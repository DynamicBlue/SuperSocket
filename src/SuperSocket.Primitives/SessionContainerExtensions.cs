using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace SuperSocket
{
    public static class SessionContainerExtensions
    {
        public static ISessionContainer<TSessionData> ToSyncSessionContainer<TSessionData>(this IAsyncSessionContainer<TSessionData> asyncSessionContainer)
        {
            return new AsyncToSyncSessionContainerWraper<TSessionData>(asyncSessionContainer);
        }

        public static IAsyncSessionContainer<TSessionData> ToAsyncSessionContainer<TSessionData>(this ISessionContainer<TSessionData> syncSessionContainer)
        {
            return new SyncToAsyncSessionContainerWraper<TSessionData>(syncSessionContainer);
        }

        public static ISessionContainer<TSessionData> GetSessionContainer<TSessionData>(this IServiceProvider serviceProvider)
        {
            var sessionContainer = serviceProvider.GetServices<IMiddleware>()
                .OfType<ISessionContainer<TSessionData>>()
                .FirstOrDefault();

            if (sessionContainer != null)
                return sessionContainer;

            var asyncSessionContainer = serviceProvider.GetServices<IMiddleware>()
                .OfType<IAsyncSessionContainer<TSessionData>>()
                .FirstOrDefault();

            return asyncSessionContainer?.ToSyncSessionContainer();
        }

        public static IAsyncSessionContainer<TSessionData> GetAsyncSessionContainer<TSessionData>(this IServiceProvider serviceProvider)
        {
            var asyncSessionContainer = serviceProvider.GetServices<IMiddleware>()
                .OfType<IAsyncSessionContainer<TSessionData>>()
                .FirstOrDefault();

            if (asyncSessionContainer != null)
                return asyncSessionContainer;

            var sessionContainer = serviceProvider.GetServices<IMiddleware>()
                .OfType<ISessionContainer<TSessionData>>()
                .FirstOrDefault();

            return sessionContainer?.ToAsyncSessionContainer(); 
        }

        public static ISessionContainer<TSessionData> GetSessionContainer<TSessionData>(this IServer server)
        {
            return server.ServiceProvider.GetSessionContainer<TSessionData>();
        }

        public static IAsyncSessionContainer<TSessionData> GetAsyncSessionContainer<TSessionData>(this IServer server)
        {
            return server.ServiceProvider.GetAsyncSessionContainer<TSessionData>();
        }
    }
}