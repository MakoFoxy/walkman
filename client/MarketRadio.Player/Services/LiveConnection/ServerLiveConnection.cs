using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Player.ClientIntegration;
using Player.ClientIntegration.Object;
using Serilog;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace MarketRadio.Player.Services.LiveConnection
{
    public class ServerLiveConnection
    {
        private readonly IConfiguration _configuration;
        private readonly PlayerStateManager _stateManager;
        private readonly ILogger<ServerLiveConnection> _logger;
        private readonly IApp _app;
        private readonly ServerLiveConnectionCommandProcessor _serverLiveConnectionCommandProcessor;
        private HubConnection? _connection;

        public ServerLiveConnection(IConfiguration configuration, 
            PlayerStateManager stateManager,
            ILogger<ServerLiveConnection> logger,
            IApp app,
            ServerLiveConnectionCommandProcessor serverLiveConnectionCommandProcessor)
        {
            _configuration = configuration;
            _stateManager = stateManager;
            _logger = logger;
            _app = app;
            _serverLiveConnectionCommandProcessor = serverLiveConnectionCommandProcessor;
        }
        
        public async Task Connect(Guid objectId)
        {
            if (_connection != null)
            {
                return;
            }

            await ConnectInternal(objectId);
        }
        
        public async Task ConnectToNewObject(Guid objectId)
        {
            if (_connection != null)
            {
                await _connection.StopAsync();
                await _connection.DisposeAsync();
            }

            await ConnectInternal(objectId);
        }

        private async Task ConnectInternal(Guid objectId)
        {
            var publisherUrl = _configuration.GetValue<string>("Endpoints:PublisherUrl");
            _connection = new HubConnectionBuilder()
                .AddNewtonsoftJsonProtocol()
                .WithUrl($"{publisherUrl}/ws/player-client-hub?objectId={objectId}&version={_app.Version}")
                .WithAutomaticReconnect(new PlayerRetryPolicy(_logger))
                .ConfigureLogging(logging =>
                {
                    logging.AddSerilog();
                })
                .Build();

            _connection.Reconnected += async _ =>
            {
                _logger.LogWarning("Reconnected");
                await _stateManager.ChangeOnlineState(true);
            };

            _connection.Closed += async _ =>
            {
                await _stateManager.ChangeOnlineState(false);
            };
            
            _serverLiveConnectionCommandProcessor.Subscribe(_connection);

            if (_connection.State == HubConnectionState.Disconnected)
            {
                try
                {
                    await _connection.StartAsync();
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "");
                }

                if (_connection.State == HubConnectionState.Connected)
                {
                    await _stateManager.ChangeOnlineState(true);
                }
            }
        }

        public Task SendPlaylistStarted()
        {
            if (_connection == null)
            {
                return Task.CompletedTask;
            }
            
            return _connection.SendAsync(OnlineEvents.PlaylistStarted);
        }

        public Task CurrentTrackResponse(OnlineObjectInfo onlineObjectInfo)
        {
            if (_connection == null)
            {
                return Task.CompletedTask;
            }
            
            return _connection.SendAsync(OnlineEvents.CurrentTrackResponse, onlineObjectInfo);
        }
    }
    
    public class PlayerRetryPolicy : IRetryPolicy
    {
        private readonly ILogger _logger;
        private readonly Random _random = new();

        public PlayerRetryPolicy(ILogger logger)
        {
            _logger = logger;
        }

        public TimeSpan? NextRetryDelay(RetryContext retryContext)
        {
            _logger.LogWarning("Reconnecting...");
            return TimeSpan.FromSeconds(_random.NextDouble() * 10);
        }
    }
}