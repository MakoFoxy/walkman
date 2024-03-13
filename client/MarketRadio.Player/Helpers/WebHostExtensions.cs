using System.IO;
using Destructurama;
using MarketRadio.Player.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace MarketRadio.Player.Helpers
{
    public static class WebHostExtensions
    {
        public static IHostBuilder AddSerilog(this IHostBuilder hostBuilder)
        {
            var template = "[{Timestamp:dd.MM.yyyy HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}";
            var logsPath = Path.Combine(DefaultLocations.LogsPath, "app");

            hostBuilder.UseSerilog((context, serviceProvider, configuration) =>
            {
                var app = serviceProvider.GetRequiredService<IApp>();
                
                configuration
                    .Destructure.JsonNetTypes()
                    .ReadFrom.Configuration(context.Configuration)
                    .Enrich.FromLogContext()
                    .Enrich.WithEnvironmentUserName()
                    .Enrich.WithMachineName()
                    .Enrich.WithProperty("Version", app.Version)
                    .Enrich.WithProperty("RunId", app.RunId)
                    .Enrich.WithProperty("StartDate", app.StartDate)
                    .WriteTo.Console(outputTemplate: template)
                    .WriteTo.SQLite(Path.Combine(logsPath, "logs_db.db"))
                    .WriteTo.Logger(lc => lc
                        .Filter.ByIncludingOnly(evt => evt.Level == LogEventLevel.Verbose)
                        .Enrich.FromLogContext()
                        .ReadFrom.Configuration(context.Configuration)
                        .WriteTo.Async(a => a.File(Path.Combine(logsPath, "verbose_.txt"), outputTemplate: template,
                            rollingInterval: RollingInterval.Day)))
                    .WriteTo.Logger(lc => lc
                        .Filter.ByIncludingOnly(evt => evt.Level == LogEventLevel.Debug)
                        .Enrich.FromLogContext()
                        .ReadFrom.Configuration(context.Configuration)
                        .WriteTo.Async(a => a.File(Path.Combine(logsPath, "debug_.txt"), outputTemplate: template,
                            rollingInterval: RollingInterval.Day)))
                    .WriteTo.Logger(lc => lc
                        .Filter.ByIncludingOnly(evt => evt.Level == LogEventLevel.Information)
                        .Enrich.FromLogContext()
                        .ReadFrom.Configuration(context.Configuration)
                        .WriteTo.Async(a => a.File(Path.Combine(logsPath, "info_.txt"), outputTemplate: template,
                            rollingInterval: RollingInterval.Day)))
                    .WriteTo.Logger(lc => lc
                        .Filter.ByIncludingOnly(evt => evt.Level == LogEventLevel.Warning)
                        .Enrich.FromLogContext()
                        .ReadFrom.Configuration(context.Configuration)
                        .WriteTo.Async(a => a.File(Path.Combine(logsPath, "warning_.txt"), outputTemplate: template,
                            rollingInterval: RollingInterval.Day)))
                    .WriteTo.Logger(lc => lc
                        .Filter.ByIncludingOnly(evt => evt.Level == LogEventLevel.Error)
                        .Enrich.FromLogContext()
                        .ReadFrom.Configuration(context.Configuration)
                        .WriteTo.Async(a => a.File(Path.Combine(logsPath, "error_.txt"), outputTemplate: template,
                            rollingInterval: RollingInterval.Day)))
                    .WriteTo.Logger(lc => lc
                        .Filter.ByIncludingOnly(evt => evt.Level == LogEventLevel.Fatal)
                        .Enrich.FromLogContext()
                        .ReadFrom.Configuration(context.Configuration)
                        .WriteTo.Async(a => a.File(Path.Combine(logsPath, "fatal_.txt"), outputTemplate: template,
                            rollingInterval: RollingInterval.Day)));
            });
            return hostBuilder;
        }
    }
}