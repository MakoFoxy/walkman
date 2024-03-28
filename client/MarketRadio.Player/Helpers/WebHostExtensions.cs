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
    { //Этот код демонстрирует, как можно интегрировать Serilog в .NET Core или ASP.NET Core приложение для расширенного логирования, используя метод расширения для IHostBuilder. Serilog является популярной библиотекой логирования, которая позволяет легко настраивать логирование с помощью различных синков (destinations) и фильтров. Давайте разберем, что делает этот код:
        public static IHostBuilder AddSerilog(this IHostBuilder hostBuilder)
        {
            var template = "[{Timestamp:dd.MM.yyyy HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"; //Определение Шаблона Лога: Шаблон template определяет, как будет выглядеть каждая запись лога. Он включает в себя временную метку, уровень логирования, сообщение и исключение (если есть).
            var logsPath = Path.Combine(DefaultLocations.LogsPath, "app"); //Путь к Логам: logsPath указывает директорию, где будут сохраняться файлы логов. Она формируется комбинацией предопределенного пути к логам и поддиректории "app".

            hostBuilder.UseSerilog((context, serviceProvider, configuration) => //Использует UseSerilog для настройки Serilog как системы логирования.
            {
                var app = serviceProvider.GetRequiredService<IApp>();
                
                configuration
                    .Destructure.JsonNetTypes() //Destructure.JsonNetTypes() позволяет корректно деструктуризировать объекты при логировании.
                    .ReadFrom.Configuration(context.Configuration) //ReadFrom.Configuration(context.Configuration) загружает настройки Serilog из файла конфигурации (например, appsettings.json).
                    .Enrich.FromLogContext() //Enrich методы добавляют дополнительные данные в каждую запись лога, такие как имя пользователя, имя машины и пользовательские свойства (Version, RunId, StartDate).
                    .Enrich.WithEnvironmentUserName()
                    .Enrich.WithMachineName()
                    .Enrich.WithProperty("Version", app.Version)
                    .Enrich.WithProperty("RunId", app.RunId)
                    .Enrich.WithProperty("StartDate", app.StartDate)
                    .WriteTo.Console(outputTemplate: template) //WriteTo.Console и WriteTo.SQLite настраивают вывод логов в консоль и в базу данных SQLite соответственно.
                    .WriteTo.SQLite(Path.Combine(logsPath, "logs_db.db"))
                    .WriteTo.Logger(lc => lc
                        .Filter.ByIncludingOnly(evt => evt.Level == LogEventLevel.Verbose) //Фильтры ByIncludingOnly ограничивают запись логов только сообщениями определенного уровня логирования.
                        .Enrich.FromLogContext()
                        .ReadFrom.Configuration(context.Configuration)
                        .WriteTo.Async(a => a.File(Path.Combine(logsPath, "verbose_.txt"), outputTemplate: template,
                            rollingInterval: RollingInterval.Day))) //WriteTo.Async используется для асинхронной записи логов, что может улучшить производительность за счет буферизации записей лога и их асинхронной записи.
                    .WriteTo.Logger(lc => lc //WriteTo.Logger настраивает дополнительные логгеры для разных уровней логирования (Verbose, Debug, Information, Warning, Error, Fatal), каждый из которых пишет в свой файл с заданным интервалом ротации.
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
    //Этот подход позволяет очень гибко настраивать логирование в приложении, обеспечивая разделение логов по уровням серьезности и типам хранилища (консоль, файлы, базы данных), а также добавление метаданных для обогащения записей лога, что упрощает последующий анализ и отладку приложения.
}