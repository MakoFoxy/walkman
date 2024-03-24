using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Player.Helpers;
using Player.Helpers.App;
using Serilog;

namespace Player.ReportSender
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Serilog.Debugging.SelfLog.Enable(msg =>
            //Serilog.Debugging.SelfLog.Enable: Включает логгирование внутренних сообщений Serilog, что может помочь при отладке проблем с логированием.
            {
                Debug.WriteLine(msg);
            });
            var host = CreateHostBuilder(args).UseConsoleLifetime().Build(); //Создается и строится host с использованием CreateHostBuilder, который включает настройки для веб-хоста и использует Serilog для логирования.
            var logger = host.Services.GetRequiredService<ILogger<Program>>(); //logger.LogTrace: Записывает в лог сообщение о запуске приложения.
            var configuration = host.Services.GetRequiredService<IConfiguration>();
            logger.LogTrace("App started");
            //В блоке try-catch: Запускает хост и отлавливает неожиданные исключения, записывая их в лог.
            try
            {
                //AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true): Эта строка используется для обратной совместимости поведения Npgsql с таймстампами. Npgsql - это .NET драйвер для PostgreSQL.
                AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
                ConfigurationParams.Log(logger, configuration.AsEnumerable()); //ConfigurationParams.Log: Логирует параметры конфигурации, используемые приложением.
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

        public static IHostBuilder CreateHostBuilder(string[] args) => //Host.CreateDefaultBuilder(args): Создает и настраивает строителя хоста по умолчанию.
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(builder => { builder.UseStartup<Startup>(); }) //.ConfigureWebHostDefaults(builder => { builder.UseStartup<Startup>(); }): Настраивает стандартные параметры веб-хоста и указывает класс Startup для дальнейшей конфигурации сервисов и конвейера обработки запросов.
                .UseSerilogConfigured(); //.UseSerilogConfigured(): Интегрирует Serilog в приложение для обработки логирования.
    }
    //Этот код ориентирован на управление жизненным циклом приложения, включая начальную настройку, логирование и обработку исключений. Он также устанавливает базовую конфигурацию для работы с базой данных PostgreSQL и настройку логирования через Serilog.
}