using System;
using System.IO;
using System.Net.Http;
using AudioSwitcher.AudioApi;
using AudioSwitcher.AudioApi.CoreAudio;
using MarketRadio.Player.DataAccess;
using MarketRadio.Player.Services;
using MarketRadio.Player.Services.Http;
using MarketRadio.Player.Services.LiveConnection;
using MarketRadio.Player.Services.System;
using MarketRadio.Player.Workers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Serialization;
using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;
using Refit;
using Serilog;

namespace MarketRadio.Player.Helpers
{
    public static class ServiceCollectionExtensions
    //Этот фрагмент кода демонстрирует, как в .NET Core приложении можно расширить IServiceCollection для добавления сервисов доступа к SQLite базе данных и HTTP-сервисов с использованием HttpClient и Refit. Вот подробности того, что происходит:
    {
        public static IServiceCollection AddPlayerDatabase(this IServiceCollection services)
        {
            if (!Directory.Exists(DefaultLocations.DatabasePath))
            {
                Directory.CreateDirectory(DefaultLocations.DatabasePath); //Проверяет наличие директории для базы данных; если таковой нет, создает ее.
            }

            services.AddDbContext<PlayerContext>(builder =>
            {//Добавляет контекст базы данных (PlayerContext) в коллекцию сервисов, настраивая его для использования SQLite. Путь и имя файла базы данных комбинируются из стандартных расположений.
                builder.UseSqlite($"Data Source={Path.Combine(DefaultLocations.DatabasePath, DefaultLocations.DatabaseFileName)}");
                builder.ConfigureWarnings(b =>
                {
                    b.Log(CoreEventId.NavigationBaseIncludeIgnored);  //Настраивает Entity Framework Core для логирования конкретного предупреждения (NavigationBaseIncludeIgnored).
                });
            });

            return services;
        }

        public static IServiceCollection AddPlayerHttpServices(this IServiceCollection services,
            IConfiguration configuration)
        //Определяет политику повторных попыток для HTTP-запросов, которая обрабатывает временные ошибки и HttpRequestException или TimeoutRejectedException. Интервалы между попытками возрастают экспоненциально, и все исключения логируются.
        {
            var retryPolicy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .Or<HttpRequestException>()//TODO Не работает при включении впн-а не пытается переподключиться
                .Or<TimeoutRejectedException>()
                .WaitAndRetryForeverAsync(
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (exception, _) =>
                    {
                        Log.Error(exception.Exception, "");
                    });

            services.AddHttpClient(nameof(PendingRequestWorker), (provider, client) =>
            { //Регистрирует несколько именованных экземпляров HttpClient (PendingRequestWorker, PlaylistDownloadWorker) с дефолтным заголовком запроса X-Client-Version, который извлекается из сервиса IApp.
                client.DefaultRequestHeaders.Add("X-Client-Version", provider.GetRequiredService<IApp>().Version);
                //Да, этот код является конфигурацией для HttpClient в системе внедрения зависимостей .NET. Он настраивает экземпляр HttpClient, который затем может быть инжектирован и использован в различных частях приложения, например, в сервисах или рабочих процессах, которые требуют выполнения HTTP-запросов к внешним API. Эта конфигурация определяет базовые настройки, такие как заголовки по умолчанию, которые будут применяться ко всем запросам, отправляемым с использованием этого клиента.
            });
            services.AddHttpClient(nameof(PlaylistDownloadWorker), (provider, client) =>
            {
                client.DefaultRequestHeaders.Add("X-Client-Version", provider.GetRequiredService<IApp>().Version);
            });
            services.AddRefitClient<IUserApi>()
                // по сути, использование IUserApi с Refit в вашем .NET приложении действительно служит как ссылка или мост к нужному обработчику на стороне сервера. Каждый метод в интерфейсе IUserApi ассоциируется с определённым HTTP-запросом к вашему веб-сервису. Это позволяет легко взаимодействовать с сервером, используя типизированные методы вместо написания повторяющегося кода для HTTP-запросов.

                .ConfigureHttpClient((provider, client) =>
                {
                    client.BaseAddress = new Uri(configuration.GetValue<string>("Endpoints:ApiUrl")!); //Установка BaseAddress гарантирует, что все HTTP-запросы к API будут направлены на правильный базовый URL.
                    client.DefaultRequestHeaders.Add("X-Client-Version", provider.GetRequiredService<IApp>().Version); //Добавление заголовка X-Client-Version может быть использовано для того, чтобы сервер мог идентифицировать версию клиента и предоставлять совместимый ответ или специальные функции для определенных версий.
                });

            services.AddRefitClient<IObjectApi>()
                .ConfigureHttpClient((provider, client) =>
                {
                    client.BaseAddress = new Uri(configuration.GetValue<string>("Endpoints:ApiUrl")!);
                    client.DefaultRequestHeaders.Add("X-Client-Version", provider.GetRequiredService<IApp>().Version);
                });

            services.AddRefitClient<IPlaylistApi>()
                .ConfigureHttpClient((provider, client) =>
                {
                    client.BaseAddress = new Uri(configuration.GetValue<string>("Endpoints:PublisherUrl")!);
                    client.DefaultRequestHeaders.Add("X-Client-Version", provider.GetRequiredService<IApp>().Version);
                });

            services.AddRefitClient<ISystemService>()
                .ConfigureHttpClient((provider, client) =>
                {
                    client.BaseAddress = new Uri(configuration.GetValue<string>("Endpoints:PublisherUrl")!);
                    client.DefaultRequestHeaders.Add("X-Client-Version", provider.GetRequiredService<IApp>().Version);
                });

            services.AddRefitClient<ITrackApi>()
                .ConfigureHttpClient((provider, client) =>
                {
                    client.BaseAddress = new Uri(configuration.GetValue<string>("Endpoints:PublisherUrl")!);
                    client.DefaultRequestHeaders.Add("X-Client-Version", provider.GetRequiredService<IApp>().Version);
                })
                .AddPolicyHandler(retryPolicy);
            //Использует Refit для создания типизированных HTTP-клиентов для различных API (IUserApi, IObjectApi, IPlaylistApi, ISystemService, ITrackApi). Каждый клиент настраивается с базовым URL и таким же дефолтным заголовком запроса. Клиент ITrackApi дополнительно использует определенную ранее политику повторных попыток.
            return services;
            //             IServiceCollection: Контракт для коллекции описаний сервисов. Используется в .NET Core для внедрения зависимостей (DI).

            // DbContext: Представляет сессию с базой данных и используется для запроса и сохранения экземпляров сущностей.

            // HttpClient: Класс в .NET, используемый для отправки HTTP-запросов и получения HTTP-ответов.

            // Refit: Библиотека, которая автоматически превращает ваш REST API в живой интерфейс.

            // Политика повторных попыток: Стратегия для автоматического повторения неудачных запросов после ожидания определенного времени.
        }

        public static IServiceCollection AddPlayerWorkers(this IServiceCollection services)
        {
            services.AddHostedService<PendingRequestWorker>();
            services.AddHostedService<PlaylistWatcherWorker>();
            services.AddHostedService<PlaylistDownloadWorker>();
            services.AddHostedService<OpenAppSchedulerWorker>();
            services.AddHostedService<UpdateAppWorker>();
            services.AddHostedService<DeleteOldLogsWorker>();
            services.AddHostedService<LocalTimeCheckWorker>();
            services.AddHostedService<TokenUpdateWorker>();
            services.AddHostedService<PingVolumeWorker>();
            return services;
        }

        public static IServiceCollection AddPlayerServices(this IServiceCollection services)
        {//Этот код демонстрирует, как в .NET Core или ASP.NET Core приложении можно зарегистрировать различные сервисы, фоновые задачи (работники) и пакеты для использования через систему внедрения зависимостей (Dependency Injection, DI). Вот подробности каждой части:
            services.AddScoped<PlaylistService>();
            services.AddSingleton<LogsUploader>();
            services.AddScoped<TrackService>();
            services.AddSingleton<WindowsTaskScheduler>();
            services.AddSingleton<ServerLiveConnection>();
            services.AddSingleton<ServerLiveConnectionCommandProcessor>();
            services.AddSingleton<Bus>();
            services.AddSingleton<PlayerStateManager>();
            services.AddSingleton<ObjectSettingsService>();
            services.AddSingleton<IApp, App>();
            services.AddSingleton<IAudioController>(new CoreAudioController());
            return services;
            //    Регистрация сервисов: Здесь регистрируются сервисы, которые обеспечивают бизнес-логику приложения. Это включает в себя PlaylistService и TrackService для управления плейлистами и треками соответственно, LogsUploader для загрузки логов, WindowsTaskScheduler для планирования задач в Windows, ServerLiveConnection для поддержания живого соединения с сервером, ServerLiveConnectionCommandProcessor для обработки команд от сервера, Bus для внутренней коммуникации между компонентами приложения, PlayerStateManager для управления состоянием плеера, ObjectSettingsService для настроек объектов и CoreAudioController для управления аудио.
        }

        public static IServiceCollection AddPackages(this IServiceCollection services)
        {
            services.AddSwaggerGen(options => options.CustomSchemaIds(t => t.FullName)); //Добавляет Swagger, инструмент для документирования API. CustomSchemaIds(t => t.FullName) используется для уникальной идентификации схем в документации.
            services.AddControllersWithViews() 
                .AddNewtonsoftJson(option => //Регистрирует контроллеры и настраивает JSON сериализацию с использованием Newtonsoft.Json для работы с JSON, включая поддержку перечислений как строк.
                {
                    option.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
                });
            services.AddSignalR().AddNewtonsoftJsonProtocol(options => //Добавляет SignalR для поддержки реального времени в веб-приложениях, также настраивая JSON сериализацию через Newtonsoft.Json.
            {
                options.PayloadSerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            });
            return services;
            //    Добавление пакетов и настройка сериализации: В этом методе добавляются дополнительные пакеты и настройки для приложения, включая Swagger для генерации документации API, настройку контроллеров и вьюх с поддержкой сериализации через Newtonsoft.Json, а также настройку SignalR с сериализацией Newtonsoft.Json.
        }
    }
    //Эти методы расширения IServiceCollection позволяют централизованно управлять регистрацией и конфигурацией всех необходимых компонентов приложения, делая его структуру более чистой и удобной для поддержки. Внедрение зависимостей используется для облегчения тестирования и уменьшения связанности компонентов, упрощая разработку и поддержку приложения.
}