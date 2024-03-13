using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MarketRadio.Player.Helpers;
using Microsoft.Extensions.Logging;

namespace MarketRadio.Player.Workers
{
    public class DeleteOldLogsWorker : PlayerBackgroundServiceBase
    {
        private readonly ILogger<DeleteOldLogsWorker> _logger;

        public DeleteOldLogsWorker(
            ILogger<DeleteOldLogsWorker> logger,
            PlayerStateManager stateManager
            ) : base(stateManager)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var oldLogFiles = Directory.GetFiles(DefaultLocations.AppLogsPath, "*.txt")
                    .Select(f => new FileInfo(f))
                    .Where(f => f.CreationTime < DateTime.Today.AddMonths(-1))
                    .ToList();
                
                foreach (var oldLogFile in oldLogFiles)
                {
                    _logger.LogInformation("Deleting old log file {FileName}", oldLogFile.Name);
                    oldLogFile.Delete();
                }

                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }
}