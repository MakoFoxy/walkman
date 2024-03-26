using System;
using System.Diagnostics;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Player.Helpers;
using Player.Helpers.App;
using Serilog;
using Microsoft.Extensions.Configuration;

namespace Player.WebApi
{
    public class Program
    {  //Этот код является частью точки входа в ASP.NET Core Web API приложение. Разберем ключевые части:
        public static void Main(string[] args)
        {
            Serilog.Debugging.SelfLog.Enable(msg =>
            {
                Debug.WriteLine(msg); //Это позволяет Serilog писать свои собственные внутренние сообщения об ошибках в окно вывода отладки в среде разработки. Это может быть полезно при диагностике проблем с конфигурацией логирования.
            });
            var host = CreateHostBuilder(args).Build(); //Это создает экземпляр хоста приложения, который использует ASP.NET Core и его веб-сервер по умолчанию, а также конфигурирует приложение с помощью класса Startup.
            var logger = host.Services.GetRequiredService<ILogger<Program>>();
            var configuration = host.Services.GetRequiredService<IConfiguration>();
            //Эти строки извлекают зарегистрированные службы из контейнера зависимостей: ILogger<Program> для логирования и IConfiguration для доступа к настройкам приложения.
            logger.LogTrace("App started");
            try
            {
                AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
                ConfigurationParams.Log(logger, configuration.AsEnumerable());
                host.Run();
                //В блоке try приложение настраивает специфические параметры (например, для Npgsql), логирует параметры конфигурации, а затем запускает хост, что инициирует слушание входящих запросов к серверу. В случае неожиданных исключений, они логируются, и программа завершает работу с соответствующим сообщением. В блоке finally, гарантируется, что логи корректно записаны и ресурсы освобождены.
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
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureWebHostDefaults(builder =>
                {
                    builder.UseStartup<Startup>();
                })
                .UseSerilogConfigured();
                //Этот метод конфигурирует хост, используя Startup класс для настройки приложения, Autofac как провайдер сервисов для инъекций зависимостей, и Serilog для логирования. UseSerilogConfigured() предположительно является расширением, настраивающим Serilog согласно каким-то кастомным параметрам или предустановкам.
    }
    //Таким образом, эта точка входа настраивает основные компоненты приложения, управляет его жизненным циклом и обрабатывает любые неожиданные ошибки, происходящие на верхнем уровне.
}
