using System;
using System.Diagnostics;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Player.Helpers;
using Player.Helpers.App;
using Serilog;

namespace Player.Publisher
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Serilog.Debugging.SelfLog.Enable(msg =>
            {
                Debug.WriteLine(msg);
                //Включает внутреннее логирование Serilog, которое помогает отлаживать проблемы с конфигурацией логирования, например, если Serilog не может записать в назначенный ему лог. Все сообщения от Serilog будут выводиться в окно отладки.
            });
            var host = CreateHostBuilder(args).UseConsoleLifetime().Build();
            //Создает и настраивает хост для веб-приложения, используя параметры, переданные в командной строке, и конфигурирует его для работы в окружении консоли.
            var logger = host.Services.GetRequiredService<ILogger<Program>>();
            //Получает экземпляр логгера, ассоциированный с классом Program, для логирования сообщений в приложении.
            var configuration = host.Services.GetRequiredService<IConfiguration>();
            //Получает экземпляр интерфейса IConfiguration, который используется для доступа к конфигурации приложения (например, к файлам appsettings.json).
            logger.LogTrace("App started");
            //Записывает в лог сообщение о том, что приложение было запущено.
            try
            {
                AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
                //Устанавливает специфическую конфигурацию для Npgsql, которая относится к обработке меток времени в PostgreSQL.
                ConfigurationParams.Log(logger, configuration.AsEnumerable());
                // Выводит в лог параметры конфигурации, используемые приложением.
                host.Run();
                //Запускает веб-хост, тем самым стартуя приложение и веб-сервер.
            }
            catch (Exception e)
            {
                Log.Fatal(e, "Host terminated unexpectedly");
            }
            finally
            {
                logger.LogTrace("App closed");
                Log.CloseAndFlush();
                //В блоке finally, который выполняется после выхода из блока try или catch, выводится сообщение в лог о том, что приложение было закрыто и выполняется закрытие и сброс логгера Serilog (Log.CloseAndFlush()).
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureWebHostDefaults(builder => { builder.UseStartup<Startup>(); })
                .UseSerilogConfigured();
        //Метод, который настраивает и возвращает конфигурацию хоста. Здесь используется Autofac в качестве поставщика служб DI, настраивается веб-хост с использованием класса Startup, и конфигурируется логирование через Serilog с вызовом UseSerilogConfigured(), который предположительно расширяет настройки Serilog.
    }
}
