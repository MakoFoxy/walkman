using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Autofac;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;
using Player.AuthorizationLogic;
using Player.BusinessLogic.Hubs;
using Player.DataAccess;
using Player.Domain;
using Player.Helpers;
using Player.Helpers.ApiInterfaces.AppApiInterface;
using Player.Helpers.Middlewares;
using Player.Publisher.Workers;
using Player.Services;
using Refit;
using Swashbuckle.AspNetCore.SwaggerGen;
using List = Player.BusinessLogic.Features.ActivityTypes.List;
using PlaylistGeneratorConfiguration = Player.Services.Configurations.PlaylistGeneratorConfiguration;

namespace Player.Publisher
{
    public class Startup
    {
        //Инициализирует новый экземпляр класса Startup с конфигурацией и окружением приложения.
        private readonly IWebHostEnvironment _env;

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            _env = env;
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        //    public IConfiguration Configuration { get; }: Предоставляет доступ к настройкам приложения из файлов конфигурации, таких как appsettings.json.

        public void ConfigureServices(IServiceCollection services)

        {
            var appSettings = Configuration["Player:Jwt:Key"];
            //Получает секретный ключ JWT из конфигурационного файла.
            var key = Encoding.ASCII.GetBytes(appSettings);
            //Преобразует строковый ключ в массив байтов.
            services.AddAuthentication(x =>
                {
                    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(x =>
                {
                    x.RequireHttpsMetadata = false;
                    //Отключает требование HTTPS для метаданных токена.
                    x.SaveToken = true;
                    x.TokenValidationParameters = new TokenValidationParameters
                    {
                        // Набор параметров для валидации входящих токенов, включая проверку ключа подписи и отключение проверки издателя и аудитории.
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = false,
                        ValidateAudience = false
                    };
                });
            //Добавляет аутентификацию JWT как метод аутентификации по умолчанию.
            services.AddAuthorization(options =>
            {
                //    Получает список всех разрешений из перечисления Permission.
                //Для каждого разрешения создает политику авторизации, требующую наличие соответствующего разрешения.
                var permissions = typeof(Permission).GetAllPublicConstantValues<string>();

                foreach (var permission in permissions)
                {
                    options.AddPolicy(permission,
                        policy => policy.Requirements.Add(new PermissionRequirement(permission)));
                }
            });

            services.Configure<PlaylistGeneratorConfiguration>(
                // Применяет настройки генератора плейлистов из конфигурации.
                Configuration.GetSection("Player:Configurations:PlaylistGenerator"));
            services.AddDbContext<PlayerContext>(builder =>
            {
                //Регистрирует контекст PlayerContext для доступа к базе данных PostgreSQL. Включает логирование чувствительных данных в режиме разработки.
                builder.UseNpgsql(Configuration.GetSection("Player:WalkmanConnectionString").Value);

                if (_env.IsDevelopment())
                {
                    builder.EnableSensitiveDataLogging();
                }
            });

            services.AddControllers(option =>
                {
                    //Добавляет сервисы для контроллеров и настраивает JSON сериализацию, включая преобразование перечислений в строки.
                    option.Conventions.Add(new RouteTokenTransformerConvention(
                        new SlugifyParameterTransformer()));
                })
                .AddNewtonsoftJson(option =>
                {
                    option.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
                });

            services.AddApiVersioning(
                options => { options.ReportApiVersions = true; });

            services.AddVersionedApiExplorer(
                options =>
                {
                    options.GroupNameFormat = "'v'VVV";
                    options.SubstituteApiVersionInUrl = true;
                });
            services.AddTransient<IConfigureOptions<SwaggerGenOptions>>(provider =>
                ActivatorUtilities.CreateInstance<ConfigureSwaggerOptions>(provider, "Publisher Api"));

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>(); //Регистрирует сервис для доступа к контексту HTTP.
            services.AddMediatR(typeof(List.Handler).Assembly); //Добавляет поддержку MediatR для реализации шаблона медиатора.
            services.AddAutoMapper(typeof(List.Handler).Assembly, GetType().Assembly); //Регистрирует AutoMapper для автоматического маппинга между объектами.
            services.AddSwaggerGen(
                options => { options.OperationFilter<SwaggerDefaultValues>(); options.CustomSchemaIds(x => x.FullName); });

            services.AddSignalR().AddJsonProtocol(options =>
            {
                //Добавляет сервисы SignalR для поддержки веб-сокетов.
                options.PayloadSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            });
            services.AddRefitClient<IObjectApi>()
            //Регистрирует HTTP клиент для взаимодействия с внешним API через интерфейс IObjectApi.
                .ConfigureHttpClient(client => client.BaseAddress = new Uri(Configuration.GetValue<string>("Player:ApiEndpoints:WebAppUrl")));
            services.AddCors(); //Включает поддержку политик CORS.
            services.AddHostedService<ObjectOnlineStatusSyncWorker>(); //Регистрирует фоновую службу для синхронизации онлайн-статуса объектов.
            // ConfigureServices(IServiceCollection services): 
            // Настраивает сервисы, используемые приложением.

            //     Настройка JWT аутентификации.
            //     Регистрация политик авторизации на основе перечисления Permission.
            //     Конфигурация DbContext для подключения к базе данных PostgreSQL.
            //     Настройка контроллеров и JSON сериализации.
            //     Включение версионирования API и Swagger для документирования API.
            //     Регистрация фоновых служб и других сервисов, включая SignalR для реального времени коммуникации и Refit для REST клиента.

        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.RegisterAssemblyTypes(typeof(UserManager).Assembly).AsSelf().AsImplementedInterfaces();
            //ConfigureContainer(ContainerBuilder builder): Дополнительная конфигурация для использования контейнера Autofac в качестве IoC контейнера вместо стандартного.
        }

        public void Configure(IApplicationBuilder app, ILogger<Startup> logger)
        {
            app.UseMiddleware<BadFormatLogMiddleware>();
            //Применяет пользовательское промежуточное ПО (middleware), которое, вероятно, логирует плохо сформированные запросы для отладки и мониторинга.
            if (_env.IsDevelopment())
            {
                //В режиме разработки использует страницу исключений разработчика, которая показывает детальную отладочную информацию при возникновении исключений.
                app.UseDeveloperExceptionPage();
            }

            app.UseCors(builder => builder.AllowAnyMethod()
                .WithOrigins("http://localhost:8082", "https://dev.909.kz", "https://909.kz")
                .AllowCredentials()
                .AllowAnyHeader());
            //Настраивает политику CORS (Cross-Origin Resource Sharing), разрешая запросы с определенных источников (доменов), все методы (GET, POST и т.д.) и заголовки, а также поддержку учетных данных (куки, авторизационные заголовки).
            // app.UseStaticFiles(new StaticFileOptions
            // {
            //     FileProvider = new PhysicalFileProvider(
            //             Path.Combine(Directory.GetCurrentDirectory(), @"C:\Users\DELL\Desktop\player\songs")),
            //     RequestPath = new PathString("/songs")
            // });
            app.UseRouting();
            //Включает маршрутизацию в приложении для определения, какой обработчик должен быть вызван для обработки входящего запроса.
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                //Применяет заголовки, перенаправленные обратными прокси и балансировщиками нагрузки, что важно для корректной работы при развертывании за прокси-серверами.
                ForwardedHeaders = ForwardedHeaders.All
            });
            app.UseAuthentication();
            //Включает аутентификацию в приложении, что позволяет определить пользователя на основе его запроса.
            app.UseAuthorization();
            //Включает авторизацию, определяя, имеет ли аутентифицированный пользователь доступ к конкретным ресурсам.
            app.UseEndpoints(endpoints =>
            {
                //Определяет конечные точки маршрутизации, включая контроллеры MVC и хабы SignalR.
                endpoints.MapControllers();
                endpoints.MapHub<PlayerClientHub>("/ws/player-client-hub");
            });

            app.UseSwagger(o =>
            {
                //Включает генерацию документации Swagger, позволяя изменять структуру путей в документе Swagger на основе регулярных выражений (например, преобразование CamelCase в snake-case).
                o.PreSerializeFilters.Add((document, _) =>
                {
                    var dictionary = document.Paths.ToDictionary(p => Regex.Replace(p.Key, "([a-z])([A-Z])", "$1-$2").ToLower(), p => p.Value);
                    var openApiPaths = new OpenApiPaths();

                    foreach (var (key, value) in dictionary)
                    {
                        openApiPaths.Add(key, value);
                    }

                    document.Paths = openApiPaths;
                });
            });
            app.UseSwaggerUI(
                options =>
                {
                    //Включает пользовательский интерфейс Swagger, позволяющий взаимодействовать с документированным API прямо в браузере. Конфигурирует UI для каждой версии API на основе информации от IApiVersionDescriptionProvider.
                    foreach (var description in app.ApplicationServices.GetRequiredService<IApiVersionDescriptionProvider>().ApiVersionDescriptions)
                    {
                        options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json",
                            description.GroupName.ToUpperInvariant());
                    }
                });

            //Configure(IApplicationBuilder app, ILogger<Startup> logger): Настраивает, как приложение будет обрабатывать запросы.
            // Использование промежуточного ПО (middleware) для логирования плохо форматированных запросов.
            // Настройка CORS для разрешения кросс-доменных запросов.
            // Настройка аутентификации и авторизации.
            // Настройка маршрутов и использование Swagger для документирования API.
            // Настройка конечных точек для контроллеров и SignalR хабов.
        }
    }

    public class SlugifyParameterTransformer : IOutboundParameterTransformer
    {
        //Класс SlugifyParameterTransformer используется для трансформации названий параметров в URL, превращая строки в стиле CamelCase в slug-case (например, "MyExample" станет "my-example"). Это полезно для создания более читабельных и SEO-дружелюбных URL.
        public string TransformOutbound(object value)
        {
            return value == null ? null : Regex.Replace(value.ToString()!, "([a-z])([A-Z])", "$1-$2").ToLower();
            //метод используется для преобразования параметров URL из одного формата в другой при формировании исходящих ссылок.
            //Если значение не null, метод преобразует его в строку (value.ToString()) и затем применяет регулярное выражение для замены. Регулярное выражение Regex.Replace(value.ToString()!, "([a-z])([A-Z])", "$1-$2") ищет паттерны, где строчная буква ([a-z]) непосредственно следует за заглавной буквой ([A-Z]), и вставляет между ними дефис (-). Например, "MyExample" превращается в "My-Example".
            //Следовательно, метод TransformOutbound используется для преобразования строк параметров маршрутизации из формата CamelCase в более читаемый и стандартный для URL snake-case.
        }
    }
    //В целом, этот класс Startup задает основную конфигурацию приложения и определяет, как оно будет взаимодействовать с внешним миром через HTTP-запросы.
}
