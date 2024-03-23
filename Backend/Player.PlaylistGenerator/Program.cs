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
        //Определяет точку входа в программу. Метод Main является стартовым методом приложения.
        {
            Console.InputEncoding = Encoding.Unicode;
            Console.OutputEncoding = Encoding.Unicode;
            ///Задает кодировку Unicode для ввода и вывода в консоль, что позволяет правильно отображать символы в различных языках.
            Serilog.Debugging.SelfLog.Enable(msg =>
            {
                Debug.WriteLine(msg);
                //Включает внутреннее логирование для Serilog, что позволяет отслеживать потенциальные ошибки конфигурации логирования.
            });
            var host = CreateHostBuilder(args).UseConsoleLifetime().Build();
            //Создает и конфигурирует хост ASP.NET Core, устанавливает время жизни хоста, связанное с консолью, и собирает хост.
            var logger = host.Services.GetRequiredService<ILogger<Program>>();
            var configuration = host.Services.GetRequiredService<IConfiguration>();
            //Извлекает из контейнера зависимостей логгер и конфигурацию, предназначенные для текущего приложения.
            logger.LogTrace("App started");
            //Записывает в журнал сообщение о том, что приложение начало работу.
            try
            {
                AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
                //Устанавливает параметр для контекста приложения, который влияет на поведение Npgsql (библиотека для работы с PostgreSQL), в частности на работу с типами данных времени.
                ConfigurationParams.Log(logger, configuration.AsEnumerable());
                //Логирует параметры конфигурации приложения.
                host.Run();
                // Запускает приложение. Этот метод блокирует текущий поток до завершения работы приложения. 
            }
            catch (Exception e)
            {
                Log.Fatal(e, "Host terminated unexpectedly");
                //Обрабатывает неожиданные исключения, возникшие во время работы приложения, и записывает их в журнал как фатальные ошибки.
            }
            finally
            {
                logger.LogTrace("App closed");
                Log.CloseAndFlush();
                //В блоке finally записывает в журнал сообщение о закрытии приложения и выполняет финализацию логгера (закрывает и сбрасывает буферы).
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
        //Определяет метод для конфигурации и создания экземпляра IHostBuilder, который используется для создания хоста приложения.
            Host.CreateDefaultBuilder(args)
                //Создает строителя хоста с настройками по умолчанию.
                .ConfigureServices((hostContext, services) =>
                {
                    //Настраивает сервисы для приложения, используя контекст хоста и коллекцию сервисов.
                    services.Configure<PlaylistGeneratorConfiguration>(
                        hostContext.Configuration.GetSection("Player:Configurations:PlaylistGenerator"));
                    //Конфигурирует параметры генератора плейлистов на основе секции конфигурации.
                    services
                        .AddDbContext<PlayerContext>(builder =>
                            builder.UseNpgsql(
                                hostContext.Configuration.GetValue<string>("Player:WalkmanConnectionString")))
                        .AddHostedService<PlaylistWithAdvertsGeneratorBackgroundService>()
                        .AddHostedService<EmptyPlaylistGeneratorBackgroundService>();
                    //Регистрирует фоновую службу для генерации пустых плейлистов. Это делает возможным автоматическое добавление службы в жизненный цикл приложения.
                })
                .UseServiceProviderFactory(new AutofacServiceProviderFactory(builder =>
                //Указывает, что для создания сервисов (DI container) будет использоваться Autofac вместо встроенного в ASP.NET Core контейнера. Autofac позволяет более гибко настраивать процесс внедрения зависимостей.

                {
                    builder.RegisterAssemblyTypes(typeof(Services.PlaylistServices.PlaylistGenerator).Assembly).AsSelf()
                        .AsImplementedInterfaces();
                    //Регистрирует все типы из сборки, где находится PlaylistGenerator, как их собственные типы и как реализации интерфейсов, которые они имплементируют. Это позволяет Autofac автоматически решать зависимости при создании объектов этих классов.
                }))
                .UseSerilogConfigured();
        //Настройка Serilog в качестве системы логирования для приложения. Предполагается, что где-то в другом месте программы или в конфигурационных файлах заданы параметры для Serilog, такие как формат сообщений, уровень логирования и назначение вывода (файлы, консоль, удаленные системы логирования и т.д.).
    }
}
//Этот код демонстрирует типичный подход к структурированию и конфигурированию ASP.NET Core приложения, используя практики, обеспечивающие его модульность, настраиваемость и готовность к масштабированию.