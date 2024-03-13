using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MarketRadio.Player.DataAccess;
using MarketRadio.Player.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MarketRadio.Player.Workers
{
    public class OpenAppSchedulerWorker : PlayerBackgroundServiceBase
    {
        private readonly WindowsTaskScheduler _windowsTaskScheduler;
        private readonly IServiceProvider _serviceProvider;
        private readonly IApp _app;
        private readonly IWebHostEnvironment _environment;

        public OpenAppSchedulerWorker(
            WindowsTaskScheduler windowsTaskScheduler,
            PlayerStateManager stateManager,
            IServiceProvider serviceProvider,
            IApp app,
            IWebHostEnvironment environment) : base(stateManager)
        {
            _windowsTaskScheduler = windowsTaskScheduler;
            _serviceProvider = serviceProvider;
            _app = app;
            _environment = environment;
        }
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (_environment.IsDevelopment())
            {
                return;
            }
            
            await WaitForObject(stoppingToken);

            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<PlayerContext>();
                var beginTime = await context.ObjectInfos.Select(o => o.BeginTime).SingleAsync(stoppingToken);
                
                var when = beginTime.Subtract(TimeSpan.FromMinutes(30));

                if (when < TimeSpan.Zero)
                {
                    when = when.Add(TimeSpan.FromHours(24));
                }

                var directoryName = AppContext.BaseDirectory;
                var launcherPath = Path.GetFullPath(Path.Combine(directoryName!, "../../", $"{_app.ProductName}.exe"));
                _windowsTaskScheduler.CreateTaskForStartup(when, launcherPath);
            }
            
            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
            }
        }
    }
}