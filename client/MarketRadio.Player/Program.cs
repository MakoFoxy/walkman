using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using ElectronNET.API;
using MarketRadio.Player.Helpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace MarketRadio.Player
{
    public class Program
    {
        public static void Main(string[] args)
        {
#if !DEBUG
            if (!IsSingleInstance())
            {
                return;
            }
#endif
            try
            {
                var host = CreateHostBuilder(args).Build();
                var lifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();
                lifetime.ApplicationStopping.Register(() =>
                {
                    var logger = host.Services.GetRequiredService<ILogger<Program>>();
                    logger.LogInformation("App closed");
                });
                Log.Logger.Information("App started");
                host.Run();
            }
            catch (Exception e)
            {
                Log.Logger.Error(e, "");
            }
            Log.CloseAndFlush();
        }

        private static bool IsSingleInstance()
        {
            var allProcesses = Process.GetProcesses();
            return allProcesses.Count(p => p.ProcessName == Process.GetCurrentProcess().ProcessName) == 1;
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices(services =>
                {
                    services.Configure<HostOptions>(options =>
                    {
                        options.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
                    });
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureKestrel(options => options.Listen(IPAddress.Loopback, 48658));
                    webBuilder.UseElectron(args);
                    webBuilder.UseStartup<Startup>();
                })
                .AddSerilog();
    }
}