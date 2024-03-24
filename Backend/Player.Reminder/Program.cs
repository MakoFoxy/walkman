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
            //Serilog.Debugging.SelfLog.Enable(msg => { Debug.WriteLine(msg); });: Включает внутреннее логирование Serilog, позволяя отлаживать проблемы с логированием, направляя сообщения лога в окно вывода отладки.
            var host = CreateHostBuilder(args).UseConsoleLifetime().Build();
            var logger = host.Services.GetRequiredService<ILogger<Program>>();
            logger.LogTrace("App started");
            try
            {
                AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
                //AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);: Устанавливает конфигурацию для Npgsql, чтобы обеспечить обратную совместимость поведения меток времени.
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
                //.ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); }): Настраивает веб-хост приложения, указывая, что начальная конфигурация должна быть взята из класса Startup.
                .ConfigureServices(services =>
                {
                    services.AddHostedService<PlaylistNotGeneratedWorker>();
                    services.AddHostedService<SelectionOnObjectExpiredWorker>();
                })
                .UseSerilogConfigured();
        //.ConfigureServices(services => { ... }): Дополнительно конфигурирует сервисы, которые будут использоваться приложением. Здесь добавляются фоновые службы PlaylistNotGeneratedWorker и SelectionOnObjectExpiredWorker, которые будут запущены и работать в фоне.
        //.UseSerilogConfigured(): Интегрирует Serilog как систему логирования, используя предварительно сконфигурированные настройки (возможно, определенные в методе расширения).
    }
    //В целом, ваш Program.cs файл настраивает и запускает ASP.NET Core приложение, используя Serilog для логирования и добавляя фоновые службы для выполнения задач в фоне.
}