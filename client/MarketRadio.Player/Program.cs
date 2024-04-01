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
    {//    Определяет пространство имен MarketRadio.Player и класс Program, который является точкой входа в приложение.
        public static void Main(string[] args)
        {
#if !DEBUG //    В релизной сборке (#if !DEBUG) проверяет, что текущий процесс запущен в единственном экземпляре. Если нет, приложение завершается.
            if (!IsSingleInstance())
            { 
                return;
            }
#endif //В блоке try создается и запускается хост приложения, а также настраивается реакция на его остановку.
            try
            {
                var host = CreateHostBuilder(args).Build(); //var host = CreateHostBuilder(args).Build();: Создает и собирает хост приложения с помощью метода CreateHostBuilder. 
                var lifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();
                lifetime.ApplicationStopping.Register(() =>
                { //Внутри блока регистрируется обработчик для события ApplicationStopping, который логирует информацию о закрытии приложения.
                    var logger = host.Services.GetRequiredService<ILogger<Program>>();
                    logger.LogInformation("App closed");
                });
                Log.Logger.Information("App started"); //Log.Logger.Information("App started");: Логирует старт приложения.
                host.Run(); //host.Run();: Запускает приложение, блокируя текущий поток до его завершения.
            }
            catch (Exception e)
            {
                Log.Logger.Error(e, "");
            }
            Log.CloseAndFlush();
        }

        private static bool IsSingleInstance()
        { //    Проверяет, запущен ли уже экземпляр приложения. Используется для предотвращения множественных запусков.
            var allProcesses = Process.GetProcesses();
            return allProcesses.Count(p => p.ProcessName == Process.GetCurrentProcess().ProcessName) == 1;
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            //Создает и конфигурирует строителя хоста для .NET приложения.
            Host.CreateDefaultBuilder(args) //.ConfigureServices(...): Настраивает сервисы приложения, в частности, устанавливает поведение при исключениях в фоновых службах на игнорирование.
                .ConfigureServices(services =>
                {
                    services.Configure<HostOptions>(options =>
                    {
                        options.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
                    });
                })
                .ConfigureWebHostDefaults(webBuilder => //.ConfigureWebHostDefaults(...): Конфигурирует веб-хост по умолчанию.
                {
                    webBuilder.ConfigureKestrel(options => options.Listen(IPAddress.Loopback, 48658)); //webBuilder.ConfigureKestrel(...): Настраивает Kestrel в качестве веб-сервера для прослушивания запросов на определенном IP и порту.
                    webBuilder.UseElectron(args); //webBuilder.UseElectron(args): Интегрирует Electron для создания настольного приложения на базе веб-технологий.
                    webBuilder.UseStartup<Startup>(); //webBuilder.UseStartup<Startup>(): Указывает класс Startup как точку начала настройки сервисов и маршрутов.
                })
                .AddSerilog(); //.AddSerilog(): Добавляет Serilog для логирования, используя его вместо стандартного логгера .NET.
    }
    //Этот код иллюстрирует типичный подход к созданию и конфигурированию хоста для .NET Core приложения, с добавлением специфических настроек для работы с Electron и настройки логирования через Serilog.
}