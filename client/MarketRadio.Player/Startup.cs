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
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddPlayerDatabase()
                .AddPlayerHttpServices(Configuration)
                .AddPlayerWorkers()
                .AddPlayerServices()
                .AddPackages();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, PlayerStateManager playerStateManager)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Market Radio Player Client");
                });
            }
            
            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetRequiredService<PlayerContext>();
                context.Database.Migrate();
            }

            app.UseDefaultFiles();
            app.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = context =>
                {
                    context.Context.Response.Headers.Append("Cache-Control", "no-cache, no-store");
                    context.Context.Response.Headers.Append("Expires", "-1");
                }
            });

            if (!Directory.Exists(DefaultLocations.TracksPath))
            {
                Directory.CreateDirectory(DefaultLocations.TracksPath);
            }

            app.UseMiddleware<TrackNotFoundMiddleware>();
            
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(DefaultLocations.TracksPath),
                RequestPath = "/tracks"
            });

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapHub<Bus>("/ws/bus");
            });

            var browserWindowOptions = new BrowserWindowOptions
            {
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
            {           
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
            {
                Task.Run(async () => playerStateManager.CurrentWindow = await Electron.WindowManager.CreateWindowAsync(browserWindowOptions, "http://localhost:48658"));
            }
        }
    }
}