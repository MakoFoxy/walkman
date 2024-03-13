using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Player.Helpers.App;
using Serilog;

namespace Player.Reminder
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Serilog.Debugging.SelfLog.Enable(msg =>
            {
                Debug.WriteLine(msg);
            });
            var host = CreateHostBuilder(args).UseConsoleLifetime().Build();
            var logger = host.Services.GetRequiredService<ILogger<Program>>();
            logger.LogTrace("App started");
            try
            {
                AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
                host.Run();
            }
            catch (Exception e)
            {
                Log.Fatal(e, "Host terminated unexpectedly");
            }
            finally
            {
                logger.LogTrace("App closed");
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); })
                .ConfigureServices(services =>
                {
                    services.AddHostedService<PlaylistNotGeneratedWorker>();
                    services.AddHostedService<SelectionOnObjectExpiredWorker>();
                })
                .UseSerilogConfigured();
    }
}