using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Autofac;
using AutoMapper;
using FluentValidation;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Player.DataAccess;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using Player.Helpers;
using Player.Services;
using Player.Services.Abstractions;
using Swashbuckle.AspNetCore.SwaggerGen;
using Wkhtmltopdf.NetCore;
using Xabe.FFmpeg;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Player.AuthorizationLogic;
using Player.BusinessLogic.Features.Adverts;
using Player.Domain;
using Player.Helpers.ApiInterfaces.PublisherApiInterfaces;
using Player.Helpers.Middlewares;
using Refit;
using Serilog;
using List = Player.BusinessLogic.Features.ActivityTypes.List;

[assembly: ApiConventionType(typeof(DefaultApiConventions))]
namespace Player.WebApi
{
    public class Startup
    {
        private readonly IWebHostEnvironment _env;
        public IConfiguration Configuration { get; }
        // IConfiguration configuration: Интерфейс для доступа к настройкам приложения (например, appsettings.json).
        // IWebHostEnvironment env: Предоставляет информацию о веб-окружении, в котором выполняется приложение.
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            _env = env;
            Configuration = configuration;

            FFmpeg.SetExecutablesPath(configuration.GetValue<string>("Player:FFmpegPath")); //    Устанавливает путь к исполняемым файлам FFmpeg, используемым для обработки мультимедиа.
        }

        public void ConfigureServices(IServiceCollection services)
        { //Этот метод используется для добавления сервисов в контейнер DI и настройки конфигурации:
            IdentityModelEventSource.ShowPII = true; //    Разрешает отображение личной идентифицирующей информации в логах для упрощения отладки.

            var appSettings = Configuration["Player:Jwt:Key"];
            var key = Encoding.ASCII.GetBytes(appSettings); //    Извлекает секретный ключ JWT из конфигурации и преобразует его в массив байтов.
            services.AddAuthentication(x =>
                {
                    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                }) //    Настраивает аутентификацию с использованием JWT токенов.
                .AddJwtBearer(x =>
                {
                    x.RequireHttpsMetadata = false;
                    x.SaveToken = true;
                    x.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = false,
                        ValidateAudience = false
                    };
                }); //    Настраивает параметры проверки JWT токенов.

            services.AddAuthorization(options =>
            {
                var permissions = typeof(Permission).GetAllPublicConstantValues<string>();

                foreach (var permission in permissions)
                {
                    options.AddPolicy(permission,
                        policy => policy.Requirements.Add(new PermissionRequirement(permission)));
                }
            }); //    Добавляет политики авторизации для каждого разрешения, определенного в классе Permission.
            services.AddDbContext<PlayerContext>(builder =>
            {
                builder.UseNpgsql(Configuration.GetSection("Player:WalkmanConnectionString").Value);
            }); //    Регистрирует контекст базы данных PlayerContext для использования с PostgreSQL.

            services
                .AddFluentValidationAutoValidation()
                .AddFluentValidationClientsideAdapters()
                .AddValidatorsFromAssemblyContaining<Create.AdvertValidator>(); //    Включает автоматическую валидацию с использованием FluentValidation и регистрирует все валидаторы в сборке.

            services.AddControllers(option =>
                {
                    option.Conventions.Add(new RouteTokenTransformerConvention(
                        new SlugifyParameterTransformer()));
                }) 
                .AddNewtonsoftJson(option =>
                {
                    option.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
                }); //    Добавляет контроллеры, настраивает маршрутизацию и сериализацию JSON.

            services.AddApiVersioning(options => { options.ReportApiVersions = true; }); //    Включает версионирование API.

            services.AddVersionedApiExplorer(
                options =>
                {
                    options.GroupNameFormat = "'v'VVV";
                    options.SubstituteApiVersionInUrl = true;
                }); //    Настраивает подробности версионирования API для Swagger.
            services.AddTransient<IConfigureOptions<SwaggerGenOptions>>(provider =>
                ActivatorUtilities.CreateInstance<ConfigureSwaggerOptions>(provider, "Web App Api"));

            services.AddSingleton<ITelegramConfiguration, TelegramConfiguration>(); //    Регистрирует конфигурацию Telegram и доступ к HTTP контексту как singleton.
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>(); //    Регистрирует конфигурацию Telegram и доступ к HTTP контексту как singleton.
            services.AddMediatR(typeof(List.Handler).Assembly); //    Включает MediatR для CQRS и медиаторных шаблонов.
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(DbContextTransactionPipelineBehavior<,>));
            services.AddAutoMapper(typeof(List.Handler).Assembly, GetType().Assembly); //    Регистрирует AutoMapper для автоматического маппинга между объектами. Это используется для преобразования данных между слоями приложения, например, между сущностями базы данных и моделями DTO.
            services.AddSwaggerGen(
                options =>
                {
                    options.OperationFilter<SwaggerDefaultValues>();
                    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                    {
                        In = ParameterLocation.Header,
                        Description = "Please insert JWT with Bearer into field",
                        Name = "Authorization",
                        Type = SecuritySchemeType.ApiKey
                    });
                    options.AddSecurityRequirement(new OpenApiSecurityRequirement {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                }
                            },
                            new string[] { }
                        }
                    });
                }); //    Настраивает Swagger для документирования API. Это включает в себя добавление определения безопасности для JWT и требование безопасности, чтобы Swagger UI предлагал ввод токена при тестировании защищенных эндпоинтов.

            services.AddWkhtmltopdf("wkhtmltopdf"); //    Добавляет поддержку wkhtmltopdf, инструмента для конвертации HTML в PDF, который можно использовать для создания отчетов или экспорта данных.

            services.AddRefitClient<IObjectApi>()
                .ConfigureHttpClient(client => client.BaseAddress = new Uri(Configuration.GetValue<string>("Player:ApiEndpoints:PublisherUrl"))); //    Регистрирует HTTP-клиент для взаимодействия с внешним API через Refit, библиотеку для вызова REST API в .NET. IObjectApi - это интерфейс, определяющий методы для взаимодействия с API, а PublisherUrl - базовый адрес внешнего сервиса, указанный в настройках.
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.RegisterAssemblyTypes(typeof(UserManager).Assembly, typeof(ITokenGenerator).Assembly)
                .AsSelf()
                .AsImplementedInterfaces(); //Этот код используется в контексте Autofac (расширенной системы управления зависимостями для .NET). Он регистрирует все типы в сборках, где находятся UserManager и ITokenGenerator, так, чтобы они были доступны для внедрения зависимостей как по своим интерфейсам, так и по собственным классам.
        }

        public void Configure(IApplicationBuilder app, ILogger<Startup> logger, IMapper mapper)
        {
            //mapper.ConfigurationProvider.AssertConfigurationIsValid();
            app.UseMiddleware<BadFormatLogMiddleware>(); //Добавляет пользовательское промежуточное ПО (middleware) BadFormatLogMiddleware в конвейер HTTP-запросов.

            if (_env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                //Если приложение запущено в разработочном окружении, используется страница исключений разработчика для подробного отображения ошибок.
            }
            app.UseSerilogRequestLogging(); //Включает логирование запросов с использованием Serilog.

            app.UseRouting(); //Активирует маршрутизацию запросов.
            app.UseAuthentication();//swaped Включает механизмы аутентификации и авторизации.
            app.UseAuthorization();//swaped Включает механизмы аутентификации и авторизации.

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers(); //Настройка конечных точек, для маршрутизации запросов к контроллерам.
            });

            app.UseSwagger(o =>
            {
                o.PreSerializeFilters.Add((document, _) =>
                {
                    var dictionary = document.Paths.ToDictionary(p => Regex.Replace(p.Key, "([a-z])([A-Z])", "$1-$2").ToLower(), p => p.Value);
                    var openApiPaths = new OpenApiPaths();

                    foreach (var (key, value) in dictionary)
                    {
                        openApiPaths.Add(key, value);
                    }

                    document.Paths = openApiPaths;
                }); //Настройка Swagger, включая фильтр предсериализации, который изменяет пути API в документе Swagger, применяя к ним kebab-case (разделение слов дефисами).
            });
            app.UseSwaggerUI(
                options =>
                {
                    foreach (var description in app.ApplicationServices.GetRequiredService<IApiVersionDescriptionProvider>().ApiVersionDescriptions)
                    {
                        options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json",
                            description.GroupName.ToUpperInvariant());
                    }
                }); //Настройка UI для Swagger, где для каждой версии API создается своя конечная точка в пользовательском интерфейсе Swagger.
        }
    }

    public class SlugifyParameterTransformer : IOutboundParameterTransformer
    {
        public string TransformOutbound(object value)
        {
            return value == null ? null : Regex.Replace(value.ToString()!, "([a-z])([A-Z])", "$1-$2").ToLower();
            //Это трансформер параметров для маршрутизации, который преобразует строки из CamelCase (например, "OneTwoThree") в kebab-case (например, "one-two-three"), что часто используется в URL. Это делается путем вставки дефиса между строчной и прописной буквами и приведением всей строки к нижнему регистру. Этот метод возвращается для изменения исходящих параметров маршрутов.
        }
    }
}
