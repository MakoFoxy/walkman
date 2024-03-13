using System;
using System.IO;
using System.Linq;
using ElectronNET.API;
using ElectronNET.API.Entities;
using Mapster;
using MarketRadio.SelectionsLoader.Api;
using MarketRadio.SelectionsLoader.BackgroundServices;
using MarketRadio.SelectionsLoader.DataAccess;
using MarketRadio.SelectionsLoader.Domain;
using MarketRadio.SelectionsLoader.Helpers;
using MarketRadio.SelectionsLoader.Services;
using MarketRadio.SelectionsLoader.Services.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Refit;
using App = MarketRadio.SelectionsLoader.Services.App;

namespace MarketRadio.SelectionsLoader
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            var resultPath = Path.Combine(DefaultLocations.DatabasePath, "database.db");
            var dataSource = $"Data Source={resultPath}";

            if (!Directory.Exists(DefaultLocations.DatabasePath))
            {
                Directory.CreateDirectory(DefaultLocations.DatabasePath);
            }

            services.AddDbContext<DatabaseContext>(builder => { builder.UseSqlite(dataSource); });
            services.AddControllers().AddNewtonsoftJson();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo {Title = "MarketRadio.PlaylistLoader", Version = "v1"});
            });
            var refitSettings = new RefitSettings
            {
                ContentSerializer = new NewtonsoftJsonContentSerializer()
            };
            services.AddRefitClient<ITrackApi>(refitSettings)
                .ConfigureHttpClient(client =>
                {
                    client.Timeout = TimeSpan.FromDays(1);
                    client.BaseAddress =
                        new Uri(Configuration.GetValue<string>("PlaylistLoader:Api:WebAppEndpoint"));
                });
            services.AddRefitClient<IUserApi>(refitSettings)
                .ConfigureHttpClient(client =>
                {
                    client.BaseAddress =
                        new Uri(Configuration.GetValue<string>("PlaylistLoader:Api:WebAppEndpoint"));
                });
            services.AddRefitClient<ISelectionApi>(refitSettings)
                .ConfigureHttpClient(client =>
                {
                    client.BaseAddress =
                        new Uri(Configuration.GetValue<string>("PlaylistLoader:Api:WebAppEndpoint"));
                });
            services.AddHostedService<TaskExecutorBackgroundService>();
            services.AddSingleton<IErrorCollector, ErrorCollector>();
            services.AddSingleton<ICurrentUserKeeper, CurrentUserKeeper>();
            services.AddSingleton<ILoadingState, LoadingState>();
            services.AddSingleton<WindowKeeper>();
            services.AddSingleton<IApp, App>();
            RegisterMappers(services);
        }

        private void RegisterMappers(IServiceCollection services)
        {
            var assemblyTypes = GetType().Assembly.DefinedTypes.ToList();
            var mappers = assemblyTypes
                .Where(t => t.CustomAttributes.Any(ca => ca.AttributeType == typeof(MapperAttribute)))
                .ToList();
            
            foreach (var mapperInterface in mappers)
            {
                var mapperImpl = assemblyTypes.SingleOrDefault(t => t.IsAssignableTo(mapperInterface) && !t.IsInterface);
                services.AddSingleton(mapperInterface, mapperImpl!);
            }
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, WindowKeeper windowKeeper)
        {
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
                var currentUserKeeper = scope.ServiceProvider.GetRequiredService<ICurrentUserKeeper>();
                context.Database.Migrate();
                var settings = context.Settings.SingleOrDefault(s => s.Key == Settings.Token);

                if (settings != null)
                {
                    currentUserKeeper.SetToken(settings.Value);
                }
            }
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "MarketRadio.PlaylistLoader v1"));
            }
            
            app.UseDefaultFiles();
            app.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = context =>
                {
                    context.Context.Response.Headers.Add("Cache-Control", "no-cache, no-store");
                    context.Context.Response.Headers.Add("Expires", "-1");
                }
            });

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
            
            var browserWindowOptions = new BrowserWindowOptions
            {
                WebPreferences = new WebPreferences
                {
                    NodeIntegration = false
                },
                AutoHideMenuBar = true,
            };

            var endpoint = env.IsDevelopment() ? "http://localhost:8080" : "http://localhost:48659";

            System.Threading.Tasks.Task.Run(async () =>
            {
                var window =  await Electron.WindowManager.CreateWindowAsync(browserWindowOptions,
                    endpoint);

                windowKeeper.CurrentWindow = window;
            });
        }
    }
}