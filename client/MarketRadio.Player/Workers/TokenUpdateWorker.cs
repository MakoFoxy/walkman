using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using MarketRadio.Player.DataAccess;
using MarketRadio.Player.DataAccess.Domain;
using MarketRadio.Player.Services.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MarketRadio.Player.Workers
{
    public class TokenUpdateWorker : PlayerBackgroundServiceBase
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TokenUpdateWorker> _logger;

        public TokenUpdateWorker(
            PlayerStateManager stateManager,
            IServiceProvider serviceProvider,
            ILogger<TokenUpdateWorker> logger
        ) : base(stateManager)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await WaitForObject(stoppingToken);
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await DoWork();
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "");
                }
                
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }

        private async Task DoWork()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<PlayerContext>();
            var userSetting = await context.UserSettings.SingleOrDefaultAsync(us => us.Key == UserSetting.Token);

            if (userSetting == null)
            {
                return;
            }

            var userApi = scope.ServiceProvider.GetRequiredService<IUserApi>();
            var response = await userApi.Renew($"Bearer {userSetting.Value}");
            
            switch (response.StatusCode)
            {
                case HttpStatusCode.OK:
                {
                    userSetting.Value = await response.Content.ReadAsStringAsync();
                    await context.SaveChangesAsync();
                    break;
                }
                case HttpStatusCode.Unauthorized:
                    _logger.LogError("Unauthorized");
                    break;
                case HttpStatusCode.InternalServerError:
                    _logger.LogError("InternalServerError in Renew process");
                    break;
                default:
                {
                    var content = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Renew body {Body}", content);
                    break;
                }
            }
        }
    }
}