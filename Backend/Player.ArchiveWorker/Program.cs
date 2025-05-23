using System;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Player.DataAccess;
using Player.Helpers;
using Player.Helpers.App;
using Serilog;

namespace Player.ArchiveWorker
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
            var configuration = host.Services.GetRequiredService<IConfiguration>();
            logger.LogTrace("App started");
            try
            {
                ConfigurationParams.Log(logger, configuration.AsEnumerable());
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
                .ConfigureServices((hostContext, services) =>
                {
                    services
                        .AddDbContext<PlayerContext>(builder => builder.UseNpgsql(hostContext.Configuration.GetValue<string>("Player:WalkmanConnectionString")))
                        .AddHostedService<Worker>();
                })
                .UseSerilogConfigured();
    }
}