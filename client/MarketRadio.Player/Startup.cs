using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ElectronNET.API;
using ElectronNET.API.Entities;
using MarketRadio.Player.DataAccess;
using MarketRadio.Player.Helpers;
using MarketRadio.Player.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace MarketRadio.Player
{
    public class Startup
    //Класс Startup в данном контексте является конфигуратором веб-приложения, используемым для настройки сервисов и HTTP запросов. Пошагово разберем ключевые элементы кода.    Определяется пространство имен MarketRadio.Player, а также класс Startup, который содержит конфигурации для запуска приложения.
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            //Конструктор Startup принимает параметр IConfiguration configuration, который инжектируется автоматически и содержит конфигурации приложения (например, из appsettings.json).
        }

        public IConfiguration Configuration { get; } //Свойство Configuration предоставляет доступ к этим конфигурациям внутри класса.

        public void ConfigureServices(IServiceCollection services)
        {//ConfigureServices настраивает сервисы, которые будут использоваться приложением.
            services.AddPlayerDatabase()
                .AddPlayerHttpServices(Configuration)
                .AddPlayerWorkers()
                .AddPlayerServices()
                .AddPackages(); //services.AddPlayerDatabase(), .AddPlayerHttpServices(Configuration), .AddPlayerWorkers(), .AddPlayerServices(), .AddPackages(): это расширения IServiceCollection, предполагающие добавление специфичных для приложения сервисов, таких как база данных, HTTP-сервисы, фоновые работники, другие службы и пакеты.
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, PlayerStateManager playerStateManager)
        {//    Настраивает конвейер обработки HTTP-запросов.
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Market Radio Player Client");
                }); //    В режиме разработки (env.IsDevelopment()) используются страницы для отладки исключений (app.UseDeveloperExceptionPage()) и Swagger для документации API (app.UseSwagger(), app.UseSwaggerUI(...)).
            }

            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetRequiredService<PlayerContext>();
                context.Database.Migrate(); //    Создает область сервисов и проводит миграцию базы данных, что гарантирует актуальность схемы БД при запуске.
            }

            app.UseDefaultFiles();
            app.UseStaticFiles(new StaticFileOptions
            {//    app.UseDefaultFiles() и app.UseStaticFiles(...): настраивают приложение на обслуживание статических файлов из корневой директории, с дополнительными заголовками для отключения кэширования.
                OnPrepareResponse = context =>
                {
                    context.Context.Response.Headers.Append("Cache-Control", "no-cache, no-store");
                    context.Context.Response.Headers.Append("Expires", "-1");
                }
            });

            if (!Directory.Exists(DefaultLocations.TracksPath))
            {
                Directory.CreateDirectory(DefaultLocations.TracksPath); //    Проверяет существование директории для хранения треков и создает ее при необходимости.
            }

            app.UseMiddleware<TrackNotFoundMiddleware>(); //    app.UseMiddleware<TrackNotFoundMiddleware>(): добавляет промежуточное ПО, которое может обрабатывать запросы к несуществующим трекам.

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(DefaultLocations.TracksPath), //    Настраивает обслуживание статических файлов из директории треков, делая их доступными по пути /tracks.
                RequestPath = "/tracks"
            });

            app.UseRouting(); //    Включает маршрутизацию в приложении.

            app.UseAuthorization(); //    Подключает механизм авторизации (здесь без конфигурации, так как конкретные детали не указаны).

            app.UseEndpoints(endpoints =>
            { //    Настраивает конечные точки для маршрутов контроллеров и Hubs WebSocket (/ws/bus).
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapHub<Bus>("/ws/bus");
            });

            var browserWindowOptions = new BrowserWindowOptions
            {//    Создает окно браузера с определенными параметрами (размеры, настройки) при помощи Electron.
                WebPreferences = new WebPreferences
                {
                    NodeIntegration = false
                },
                Resizable = false,
                Fullscreenable = false,
                Width = 480,
                Height = 800,
                AutoHideMenuBar = true,
            };

            if (env.IsDevelopment())
            {//В режиме разработки задает другие параметры для окна браузера и открывает его, указывая URL для разработки.
                browserWindowOptions = new BrowserWindowOptions
                {
                    WebPreferences = new WebPreferences
                    {
                        NodeIntegration = false
                    },
                };
                Task.Run(async () => playerStateManager.CurrentWindow = await Electron.WindowManager.CreateWindowAsync(browserWindowOptions, "http://localhost:8080"));
            }
            else
            {//В продакшн-режиме также создает окно браузера, но с URL, предполагающим работу приложения на продакшн-сервере.
                Task.Run(async () => playerStateManager.CurrentWindow = await Electron.WindowManager.CreateWindowAsync(browserWindowOptions, "http://localhost:48658"));
            }
        }
        //Этот класс Startup демонстрирует комплексную настройку .NET Core приложения, включая конфигурацию сервисов, обработку HTTP-запросов, интеграцию со Swagger для документации API, миграцию базы данных и интеграцию с Electron для создания настольных приложений на базе веб-технологий.

    }
}