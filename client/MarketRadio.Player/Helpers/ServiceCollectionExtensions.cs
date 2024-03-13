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
    {
        public static IServiceCollection AddPlayerDatabase(this IServiceCollection services)
        {
            if (!Directory.Exists(DefaultLocations.DatabasePath))
            {
                Directory.CreateDirectory(DefaultLocations.DatabasePath);
            }

            services.AddDbContext<PlayerContext>(builder =>
            {
                builder.UseSqlite($"Data Source={Path.Combine(DefaultLocations.DatabasePath, DefaultLocations.DatabaseFileName)}");
                builder.ConfigureWarnings(b =>
                {
                    b.Log(CoreEventId.NavigationBaseIncludeIgnored);
                });
            });

            return services;
        }

        public static IServiceCollection AddPlayerHttpServices(this IServiceCollection services,
            IConfiguration configuration)
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
            {
                client.DefaultRequestHeaders.Add("X-Client-Version", provider.GetRequiredService<IApp>().Version);
            });
            services.AddHttpClient(nameof(PlaylistDownloadWorker), (provider, client) =>
            {
                client.DefaultRequestHeaders.Add("X-Client-Version", provider.GetRequiredService<IApp>().Version);
            });
            
            services.AddRefitClient<IUserApi>()
                .ConfigureHttpClient((provider, client) =>
                {
                    client.BaseAddress = new Uri(configuration.GetValue<string>("Endpoints:ApiUrl")!);
                    client.DefaultRequestHeaders.Add("X-Client-Version", provider.GetRequiredService<IApp>().Version);
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

            return services;
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
        {
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
        }
        
        public static IServiceCollection AddPackages(this IServiceCollection services)
        {
            services.AddSwaggerGen(options => options.CustomSchemaIds(t => t.FullName));
            services.AddControllersWithViews()
                .AddNewtonsoftJson(option =>
                {
                    option.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
                });
            services.AddSignalR().AddNewtonsoftJsonProtocol(options =>
            {
                options.PayloadSerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            });
            return services;
        }
    }
}