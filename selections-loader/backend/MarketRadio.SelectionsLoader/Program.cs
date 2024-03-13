using System;
using System.Net;
using ElectronNET.API;
using MarketRadio.SelectionsLoader.Helpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace MarketRadio.SelectionsLoader
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Starting...");
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
                Console.WriteLine(e);
                Log.Logger.Error(e, "");
            }
            Log.CloseAndFlush();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureKestrel(options => options.Listen(IPAddress.Loopback, 48659));
                    webBuilder.UseElectron(args);
                    webBuilder.UseStartup<Startup>();
                })
                .AddSerilog();
    }
}