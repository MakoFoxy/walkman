using System;
using System.Diagnostics;
using System.Text;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Player.DataAccess;
using Player.Helpers;
using Player.Helpers.App;
using Serilog;
using PlaylistGeneratorConfiguration = Player.Services.Configurations.PlaylistGeneratorConfiguration;

namespace Player.PlaylistGenerator
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.InputEncoding = Encoding.Unicode;
            Console.OutputEncoding = Encoding.Unicode;
            
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
                AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
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
                    services.Configure<PlaylistGeneratorConfiguration>(
                        hostContext.Configuration.GetSection("Player:Configurations:PlaylistGenerator"));
                    services
                        .AddDbContext<PlayerContext>(builder =>
                            builder.UseNpgsql(
                                hostContext.Configuration.GetValue<string>("Player:WalkmanConnectionString")))
                        .AddHostedService<PlaylistWithAdvertsGeneratorBackgroundService>()
                        .AddHostedService<EmptyPlaylistGeneratorBackgroundService>();
                })
                .UseServiceProviderFactory(new AutofacServiceProviderFactory(builder =>
                {
                    builder.RegisterAssemblyTypes(typeof(Services.PlaylistServices.PlaylistGenerator).Assembly).AsSelf()
                        .AsImplementedInterfaces();
                }))
                .UseSerilogConfigured();
    }
}
