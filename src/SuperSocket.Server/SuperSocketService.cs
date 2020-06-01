using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CacheManager.Core.Logging;
using Dynamic.Core.Log;
using Dynamic.Core.Service;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using SuperSocket;
using SuperSocket.Channel;
using SuperSocket.ProtoBase;
using SuperSocket.Server.Runtime;

namespace SuperSocket.Server
{
    public class SuperSocketService<TSessionData,TReceivePackageInfo> : IHostedService, IServer, IChannelRegister
    {
        private readonly IServiceProvider _serviceProvider;
        public Dynamic.Core.Log.ILogger _logger = LoggerManager.GetLogger("CSuperSocketService");
        public IServiceProvider ServiceProvider
        {
            get { return _serviceProvider; }
        }

        private readonly IOptions<ServerOptions> _serverOptions;
 

       

       

        private IPipelineFilterFactory<TReceivePackageInfo> _pipelineFilterFactory;
        private IChannelCreatorFactory _channelCreatorFactory;
        private List<IChannelCreator> _channelCreators;
        private IPackageHandler<TSessionData,TReceivePackageInfo> _packageHandler;
        private Func<IAppSession<TSessionData>, PackageHandlingException<TReceivePackageInfo>, ValueTask<bool>> _errorHandler;
        
        public string Name { get; }

        private int _sessionCount;

        public int SessionCount => _sessionCount;

        private ISessionFactory<TSessionData> _sessionFactory;

        private IMiddleware[] _middlewares;

        protected IMiddleware[] Middlewares
        {
            get { return _middlewares; }
        }

        private ServerState _state = ServerState.None;

        public ServerState State
        {
            get { return _state; }
        }

        public object DataContext { get; set; }

        private SessionHandlers<TSessionData> _sessionHandlers;

        public SuperSocketService(IServiceProvider serviceProvider, IOptions<ServerOptions> serverOptions, IChannelCreatorFactory channelCreatorFactory)
        {
            var serverOptionsValue =  IocUnity.Get<ServerOptions>();
            _serverOptions = new ServerConfigOptions<ServerOptions>(serverOptionsValue);
            //_serverOptions = serverOptions;
            Name = serverOptions.Value.Name;
            _serviceProvider = serviceProvider;
            _pipelineFilterFactory = GetPipelineFilterFactory();
         
            _channelCreatorFactory = channelCreatorFactory;
            _packageHandler = serviceProvider.GetService<IPackageHandler<TSessionData,TReceivePackageInfo>>();
            _errorHandler = serviceProvider.GetService<Func<IAppSession<TSessionData>, PackageHandlingException<TReceivePackageInfo>, ValueTask<bool>>>();
            _sessionHandlers = serviceProvider.GetService<SessionHandlers<TSessionData>>();

            if (_errorHandler == null)
            {
                _errorHandler = OnSessionErrorAsync;
            }
            
            // initialize session factory
            _sessionFactory = serviceProvider.GetService<ISessionFactory<TSessionData>>();

            if (_sessionFactory == null)
                _sessionFactory = new DefaultSessionFactory<TSessionData>();


            InitializeMiddlewares();
        }

        private void InitializeMiddlewares()
        {
            _middlewares = _serviceProvider.GetServices<IMiddleware>()
                .OrderBy(m => m.Order)
                .ToArray();

            foreach (var m in _middlewares)
            {
                m.Start(this);
            }

            if (_packageHandler == null)
                _packageHandler = _middlewares.OfType<IPackageHandler<TSessionData,TReceivePackageInfo>>().FirstOrDefault();
        }

        private void ShutdownMiddlewares()
        {
            foreach (var m in _middlewares)
            {
                try
                {
                    m.Shutdown(this);
                }
                catch(Exception e)
                {
                    _logger.Error($"The exception was thrown from the middleware {m.GetType().Name} when it is being shutdown.¡¾{e.ToString()}¡¿");
                }                
            }
        }

        protected virtual IPipelineFilterFactory<TReceivePackageInfo> GetPipelineFilterFactory()
        {
            return _serviceProvider.GetRequiredService<IPipelineFilterFactory<TReceivePackageInfo>>();
        }

        private bool AddChannelCreator(ListenOptions listenOptions, ServerOptions serverOptions)
        {
            var listener = _channelCreatorFactory.CreateChannelCreator<TReceivePackageInfo>(listenOptions, serverOptions, _pipelineFilterFactory);
            listener.NewClientAccepted += OnNewClientAccept;

            if (!listener.Start())
            {
                _logger.Error($"Failed to listen {listener}.");
                return false;
            }

            _logger.Info($"The listener [{listener}] has been started.");
            _channelCreators.Add(listener);
            return true;
        }

        private Task<bool> StartListenAsync(CancellationToken cancellationToken)
        {
            _channelCreators = new List<IChannelCreator>();

            var serverOptions = _serverOptions.Value;

            if (serverOptions.Listeners != null && serverOptions.Listeners.Any())
            {
                foreach (var l in serverOptions.Listeners)
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    if (!AddChannelCreator(l, serverOptions))
                    {
                        _logger.Error($"Failed to listen {l}.");
                        continue;
                    }
                }
            }
            else
            {
                _logger.Warn("No listner was defined, so this server only can accept connections from the ActiveConnect.");

                if (!AddChannelCreator(null, serverOptions))
                {
                    _logger.Error($"Failed to add the channel creator.");
                    return Task.FromResult(false);
                }
            }

            return Task.FromResult(true);
        }

        protected virtual void OnNewClientAccept(IChannelCreator listener, IChannel channel)
        {
            AcceptNewChannel(channel);
        }

        private void AcceptNewChannel(IChannel channel)
        {
            var session = _sessionFactory.Create() as AppSession<TSessionData>;
            HandleSession(session, channel).DoNotAwait();
        }

        async Task IChannelRegister.RegisterChannel(object connection)
        {
            var channel = await _channelCreators.FirstOrDefault().CreateChannel(connection);
            AcceptNewChannel(channel);
        }

        protected virtual object CreatePipelineContext(IAppSession<TSessionData> session)
        {
            return session;
        }

        private async ValueTask<bool> InitializeSession(IAppSession<TSessionData> session, IChannel channel)
        {
            session.Initialize(this, channel);

            if (channel is IPipeChannel pipeChannel)
            {
                pipeChannel.PipelineFilter.Context = CreatePipelineContext(session);
            }

            var middlewares = _middlewares;

            if (middlewares != null && middlewares.Length > 0)
            {
                for (var i = 0; i < middlewares.Length; i++)
                {
                    var middleware = middlewares[i];

                    if (!await middleware.RegisterSession(session))
                    {
                        _logger.Warn($"A session from {session.RemoteEndPoint} was rejected by the middleware {middleware.GetType().Name}.");
                        return false;
                    }
                }
            }

            return true;
        }


        protected virtual ValueTask OnSessionConnectedAsync(IAppSession<TSessionData> session)
        {
            var connectedHandler = _sessionHandlers?.Connected;

            if (connectedHandler != null)
                return connectedHandler.Invoke(session);

            return new ValueTask();
        }

        protected virtual ValueTask OnSessionClosedAsync(IAppSession<TSessionData> session)
        {
            var closedHandler = _sessionHandlers?.Closed;

            if (closedHandler != null)
                return closedHandler.Invoke(session);

            return new ValueTask();
        }

        protected virtual async ValueTask FireSessionConnectedEvent(AppSession<TSessionData> session)
        {
            if (session is IHandshakeRequiredSession hanshakeSession)
            {
                if (!hanshakeSession.Handshaked)
                    return;
            }

            _logger.Info($"A new session connected: {session.SessionID}");

            try
            {
                Interlocked.Increment(ref _sessionCount);
                await session.FireSessionConnectedAsync();
                await OnSessionConnectedAsync(session);
            }
            catch (Exception e)
            {
                _logger.Error($"There is one exception thrown from the event handler of SessionConnected.¡¾{e.ToString()}¡¿");
            }
        }

        protected virtual async ValueTask FireSessionClosedEvent(AppSession<TSessionData> session)
        {
            if (session is IHandshakeRequiredSession hanshakeSession)
            {
                if (!hanshakeSession.Handshaked)
                    return;
            }

            _logger.Info($"The session disconnected: {session.SessionID}");

            try
            {
                Interlocked.Decrement(ref _sessionCount);
                await session.FireSessionClosedAsync(EventArgs.Empty);
                await OnSessionClosedAsync(session); 
            }
            catch (Exception exc)
            {
                _logger.Error($"There is one exception thrown from the event of OnSessionClosed.¡¾{exc.ToString()}¡¿");
            }
        }

        private async ValueTask HandleSession(AppSession<TSessionData> session, IChannel channel)
        {
            if (!await InitializeSession(session, channel))
                return;

            try
            {
                await FireSessionConnectedEvent(session);

                var packageChannel = channel as IChannel<TReceivePackageInfo>;
                var packageHandler = _packageHandler;

                await foreach (var p in packageChannel.RunAsync())
                {
                    try
                    {
                        if (packageHandler != null)
                            await packageHandler.Handle(session, p);
                    }
                    catch (Exception e)
                    {
                        var toClose = await _errorHandler(session, new PackageHandlingException<TReceivePackageInfo>($"Session {session.SessionID} got an error when handle a package.", p, e));

                        if (toClose)
                        {
                            session.CloseAsync().DoNotAwait();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error($"Failed to handle the session {session.SessionID}.¡¾{e.ToString()}¡¿");
            }
            finally
            {
                await FireSessionClosedEvent(session);
            }
        }

        protected virtual ValueTask<bool> OnSessionErrorAsync(IAppSession<TSessionData> session, PackageHandlingException<TReceivePackageInfo> exception)
        {
            _logger.Error($"Session[{session.SessionID}]: session exception.¡¾{exception.ToString()}¡¿");
            return new ValueTask<bool>(true);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var state = _state;

            if (state != ServerState.None && state != ServerState.Stopped)
            {
                throw new InvalidOperationException($"The server cannot be started right now, because its state is {state}.");
            }

            _state = ServerState.Starting;

            await StartListenAsync(cancellationToken);

            _state = ServerState.Started;

            try
            {
                await OnStartedAsync();
            }
            catch(Exception e)
            {
                _logger.Error($"There is one exception thrown from the method OnStartedAsync().¡¾{e.ToString()}¡¿");
            }
        }

        protected virtual ValueTask OnStartedAsync()
        {
            return new ValueTask();
        }

        protected virtual ValueTask OnStopAsync()
        {
            return new ValueTask();
        }

        private async Task StopListener(IChannelCreator listener)
        {
            await listener.StopAsync();
            _logger.Info($"The listener [{listener}] has been stopped.");
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            var state = _state;

            if (state != ServerState.Started)
            {
                throw new InvalidOperationException($"The server cannot be stopped right now, because its state is {state}.");
            }

            _state = ServerState.Stopping;

            var tasks = _channelCreators.Where(l => l.IsRunning).Select(l => StopListener(l))
                .Union(new Task[] { Task.Run(ShutdownMiddlewares) });

            await Task.WhenAll(tasks);

            try
            {
                await OnStopAsync();
            }
            catch (Exception e)
            {
                _logger.Error($"There is an exception thrown from the method OnStopAsync().¡¾{e.ToString()}¡¿");
            }

            _state = ServerState.Stopped;
        }

        async Task<bool> IServer.StartAsync()
        {
            await StartAsync(CancellationToken.None);
            return true;
        }

        async Task IServer.StopAsync()
        {
            await StopAsync(CancellationToken.None);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        ValueTask IAsyncDisposable.DisposeAsync() => DisposeAsync(true);

        protected virtual async ValueTask DisposeAsync(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    try
                    {
                        if (_state == ServerState.Started)
                        {
                            await StopAsync(CancellationToken.None);
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.Error($"Failed to stop the server¡¾{e.ToString()}¡¿");
                    }
                }

                disposedValue = true;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            DisposeAsync(disposing).GetAwaiter().GetResult();
        }

        void IDisposable.Dispose()
        {
            DisposeAsync(true).GetAwaiter().GetResult();
        }

        #endregion
    }
}
