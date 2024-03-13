using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Player.BusinessLogic.Hubs;
using Player.DataAccess;
using Player.Domain;

namespace Player.Publisher.Workers
{
    public class ObjectOnlineStatusSyncWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ObjectOnlineStatusSyncWorker> _logger;
        private readonly IWebHostEnvironment _env;

        public ObjectOnlineStatusSyncWorker(
            IServiceProvider serviceProvider, 
            ILogger<ObjectOnlineStatusSyncWorker> logger,
            IWebHostEnvironment env)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _env = env;
        }
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (_env.IsDevelopment())
            {
                return;
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                await DoWork(stoppingToken);
            }
        }

        private async Task DoWork(CancellationToken stoppingToken)
        {
            using var provider = _serviceProvider.CreateScope();
            
            var context = provider.ServiceProvider.GetRequiredService<PlayerContext>();
            
            try
            {
                await context.BeginTransactionAsync(stoppingToken);

                var onlineObjects = await context.Objects
                    .Where(o => o.IsOnline)
                    .Select(o => o.Id)
                    .ToListAsync(stoppingToken);
                
                var onlineClientIds = PlayerClientHub.ConnectedClients.Select(cc => cc.Id).ToList();
                
                _logger.LogTrace("Online objects in database {@OnlineObjects}", onlineObjects);
                _logger.LogTrace("Online objects in memory {@OnlineObjects}", onlineClientIds);
                
                if (onlineObjects.Intersect(onlineClientIds).Count() == onlineObjects.Count)
                {
                    return;
                }
            
                _logger.LogWarning("Need object online status sync");
                
                var tableName = context.GetTableName<ObjectInfo>();
                var isOnlineColumnName = context.GetColumnName<ObjectInfo>(info => info.IsOnline);
                var idColumnName = context.GetColumnName<ObjectInfo>(info => info.Id);
            
                await context.Database.ExecuteSqlRawAsync($"UPDATE \"{tableName}\" set \"{isOnlineColumnName}\" = false");
                await context.Database.ExecuteSqlRawAsync(
                    $"UPDATE \"{tableName}\" set \"{isOnlineColumnName}\" = true WHERE \"{idColumnName}\" = ANY (@p0)",
                    onlineClientIds);

                await context.SaveChangesAsync(stoppingToken);
                await context.CommitTransactionAsync(stoppingToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "");
                await context.RollbackTransactionAsync(stoppingToken);
            }
        }
    }
}