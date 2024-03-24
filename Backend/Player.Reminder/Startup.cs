using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Player.DataAccess;
using Player.Services;
using Player.Services.Abstractions;

namespace Player.Reminder
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            //    public Startup(IConfiguration configuration) { Configuration = configuration; }: Конструктор класса получает конфигурацию приложения (обычно из файла appsettings.json и его вариаций) и сохраняет её в свойстве Configuration для последующего использования.
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<ServiceSettings>(Configuration.GetSection("Player"));
            services.AddSingleton<ITelegramConfiguration, TelegramConfiguration>();
            services.AddTransient<ITelegramMessageSender, TelegramMessageSender>();
            services.AddDbContext<PlayerContext>(builder =>
                builder.UseNpgsql(Configuration.GetValue<string>("Player:WalkmanConnectionString")));
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Player.Reminder", Version = "v1" });
            });
            //    services.Configure<ServiceSettings>(Configuration.GetSection("Player")): Считывает настройки из конфигурации и применяет их к типу ServiceSettings.

            // services.AddSingleton<ITelegramConfiguration, TelegramConfiguration>(): Регистрирует конфигурацию Telegram как singleton, что означает, что будет создан только один экземпляр этого объекта на все приложение.

            // services.AddTransient<ITelegramMessageSender, TelegramMessageSender>(): Регистрирует сервис отправки сообщений в Telegram таким образом, что каждый раз при запросе ITelegramMessageSender будет создан новый экземпляр TelegramMessageSender.

            // services.AddDbContext<PlayerContext>(...): Регистрирует контекст базы данных PlayerContext для Entity Framework Core, используя строку подключения из конфигурации.

            // services.AddControllers(): Добавляет сервисы, необходимые для работы контроллеров MVC.

            // services.AddSwaggerGen(...): Настраивает Swagger, инструмент для создания документации API, который позволяет визуализировать и тестировать API.
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Player.Reminder v1"));
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
            //    if (env.IsDevelopment()) { ... }: Этот блок кода выполняется только в разработческом окружении (например, на вашем локальном компьютере). В нем настраивается страница исключений для разработчика и Swagger UI.

            // app.UseRouting(): Включает маршрутизацию в приложении.

            // app.UseAuthorization(): Включает систему авторизации. Хотя здесь не настроена аутентификация, авторизация все равно добавляется в конвейер обработки запросов.

            // app.UseEndpoints(endpoints => { endpoints.MapControllers(); }): Определяет, какие маршруты доступны в приложении. В данном случае говорит, что приложение должно использовать маршруты, определенные в контроллерах.
        }
    }
    //Этот класс Startup настраивает все необходимые сервисы и мидлвари для работы вашего ASP.NET Core приложения.
}