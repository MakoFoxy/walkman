using System;
using System.Threading;
using System.Threading.Tasks;
using MarketRadio.Player.Helpers;
using MarketRadio.Player.Services.Http;
using Microsoft.Extensions.Logging;
using Player.ClientIntegration.Client;

namespace MarketRadio.Player.Workers
{
    public class LocalTimeCheckWorker : PlayerBackgroundServiceBase
    {
        private readonly ISystemService _systemService;
        private readonly ILogger<LocalTimeCheckWorker> _logger;

        public LocalTimeCheckWorker(
            PlayerStateManager stateManager,
            ISystemService systemService,
            ILogger<LocalTimeCheckWorker> logger
            ) : base(stateManager)
        {
            _systemService = systemService;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await DoWork(true);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "");
                }
                
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }

        private async Task DoWork(bool firstRequest)
        {
            var beforeTime = DateTimeOffset.Now;
            var serverTime = await _systemService.GetServerTime();
            var afterTime = DateTimeOffset.Now;

            if (afterTime - beforeTime < TimeSpan.FromSeconds(1))
            {
                CheckLocalTime(beforeTime, serverTime, afterTime);
            }
            else
            {
                await LogOrResendRequest(firstRequest, beforeTime, serverTime, afterTime);
            }
        }

        private async Task LogOrResendRequest(bool firstRequest, DateTimeOffset beforeTime, CurrentTimeDto serverTime,
            DateTimeOffset afterTime)
        {
            if (firstRequest)
            {
                await DoWork(false);
            }
            else
            {
                _logger.LogWarning("Problems with server sync TimeBeforeRequest:{TimeBeforeRequest} ServerTime:{ServerTime} TimeAfterRequest:{TimeAfterRequest}",
                    beforeTime, serverTime.CurrentTime, afterTime);
            }
        }

        private void CheckLocalTime(DateTimeOffset beforeTime, CurrentTimeDto serverTime, DateTimeOffset afterTime)
        {
            if (beforeTime.ResetToSeconds() <= serverTime.CurrentTime.ResetToSeconds() &&
                afterTime.ResetToSeconds() >= serverTime.CurrentTime.ResetToSeconds())
            {
                _logger.LogInformation("Local time is ok TimeAfterRequest:{TimeBeforeRequest} ServerTime:{ServerTime} TimeAfterRequest:{TimeAfterRequest}",
                    beforeTime, serverTime.CurrentTime, afterTime);
            }
            else
            {
                _logger.LogWarning("Problems with local time TimeBeforeRequest:{TimeBeforeRequest} ServerTime:{ServerTime} TimeAfterRequest:{TimeAfterRequest}",
                    beforeTime, serverTime.CurrentTime, afterTime);
            }
        }
    }
}