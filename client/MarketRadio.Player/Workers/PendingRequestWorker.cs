using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MarketRadio.Player.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MarketRadio.Player.Workers
{
    public class PendingRequestWorker : PlayerBackgroundServiceBase
    {
        private readonly IServiceProvider _provider;
        private readonly ILogger<PendingRequestWorker> _logger;
        private readonly PlayerStateManager _stateManager;

        public PendingRequestWorker(IServiceProvider provider,
            PlayerStateManager stateManager,
            ILogger<PendingRequestWorker> logger) : base(stateManager)
        {
            _provider = provider;
            _stateManager = stateManager;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await WaitForObject(stoppingToken);
            
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await DoWork(stoppingToken);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "");
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

        private async Task DoWork(CancellationToken stoppingToken)
        {
            using var scope = _provider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<PlayerContext>();
            var httpClient = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>()
                .CreateClient(nameof(PendingRequestWorker));
            
            var pendingRequests = await context
                .PendingRequest
                .OrderBy(pr => pr.Date)
                .ToListAsync(stoppingToken);

            if (!pendingRequests.Any())
            {
                _logger.LogInformation("No pending requests");
               return;
            }

            _logger.LogInformation("Pending requests count {Count}", pendingRequests.Count);
            
            var timeWithoutInternet = DateTime.Now - pendingRequests.First().Date;

            if (timeWithoutInternet >= TimeSpan.FromHours(1) && !_stateManager.IsOnline)
            {
                _logger.LogWarning("Internet connection lost more then {TimeWithoutInternet} hour", timeWithoutInternet);
            }

            foreach (var request in pendingRequests)
            {
                HttpResponseMessage httpResponseMessage = null!;
                
                if (request.HttpMethod == HttpMethod.Get.Method)
                {
                    httpResponseMessage = await httpClient.GetAsync(request.Url, stoppingToken);
                }

                if (request.HttpMethod == HttpMethod.Post.Method)
                {
                    httpResponseMessage = await httpClient.PostAsync(request.Url, new StringContent(request.Body, Encoding.UTF8, "application/json"), stoppingToken);
                }

                if (request.HttpMethod == HttpMethod.Put.Method)
                {
                    httpResponseMessage = await httpClient.PutAsync(request.Url, new StringContent(request.Body, Encoding.UTF8, "application/json"), stoppingToken);
                }

                if (request.HttpMethod == HttpMethod.Delete.Method)
                {
                    httpResponseMessage = await httpClient.DeleteAsync(request.Url, stoppingToken);
                }

                if (httpResponseMessage.StatusCode == HttpStatusCode.InternalServerError)
                {
                    continue;
                }
                
                httpResponseMessage.EnsureSuccessStatusCode();

                context.PendingRequest.Remove(request);
                await context.SaveChangesAsync(stoppingToken);
            }
        }
    }
}