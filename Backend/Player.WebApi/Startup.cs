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

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            _env = env;
            Configuration = configuration;
            
            FFmpeg.SetExecutablesPath(configuration.GetValue<string>("Player:FFmpegPath"));
        }
        
        public void ConfigureServices(IServiceCollection services)
        {
            IdentityModelEventSource.ShowPII = true;

            var appSettings = Configuration["Player:Jwt:Key"];
            var key = Encoding.ASCII.GetBytes(appSettings);
            services.AddAuthentication(x =>
                {
                    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
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
                });
            
            services.AddAuthorization(options =>
            {
                var permissions = typeof(Permission).GetAllPublicConstantValues<string>();
                
                foreach(var permission in permissions) 
                {
                    options.AddPolicy(permission,
                        policy => policy.Requirements.Add(new PermissionRequirement(permission)));
                }
            });
            services.AddDbContext<PlayerContext>(builder =>
            {
                builder.UseNpgsql(Configuration.GetSection("Player:WalkmanConnectionString").Value);
            });

            services
                .AddFluentValidationAutoValidation()
                .AddFluentValidationClientsideAdapters()
                .AddValidatorsFromAssemblyContaining<Create.AdvertValidator>();

            services.AddControllers(option =>
                {
                    option.Conventions.Add(new RouteTokenTransformerConvention(
                        new SlugifyParameterTransformer()));
                })
                .AddNewtonsoftJson(option =>
                {
                    option.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
                });

            services.AddApiVersioning(options => { options.ReportApiVersions = true; });

            services.AddVersionedApiExplorer(
                options =>
                {
                    options.GroupNameFormat = "'v'VVV";
                    options.SubstituteApiVersionInUrl = true;
                });
            services.AddTransient<IConfigureOptions<SwaggerGenOptions>>(provider =>
                ActivatorUtilities.CreateInstance<ConfigureSwaggerOptions>(provider, "Web App Api"));
            
            services.AddSingleton<ITelegramConfiguration, TelegramConfiguration>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddMediatR(typeof(List.Handler).Assembly);
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(DbContextTransactionPipelineBehavior<,>));
            services.AddAutoMapper(typeof(List.Handler).Assembly, GetType().Assembly);
            services.AddSwaggerGen(
                options =>
                {
                    options.OperationFilter<SwaggerDefaultValues>();
                    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme {
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
                });

            services.AddWkhtmltopdf("wkhtmltopdf");

            services.AddRefitClient<IObjectApi>()
                .ConfigureHttpClient(client => client.BaseAddress = new Uri(Configuration.GetValue<string>("Player:ApiEndpoints:PublisherUrl")));
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.RegisterAssemblyTypes(typeof(UserManager).Assembly, typeof(ITokenGenerator).Assembly)
                .AsSelf()
                .AsImplementedInterfaces();
        }

        public void Configure(IApplicationBuilder app, ILogger<Startup> logger, IMapper mapper)
        {
            //mapper.ConfigurationProvider.AssertConfigurationIsValid();
            app.UseMiddleware<BadFormatLogMiddleware>();
            
            if (_env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseSerilogRequestLogging();

            app.UseRouting();
            app.UseAuthentication();//swaped
            app.UseAuthorization();//swaped

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
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
                });
            });
            app.UseSwaggerUI(
                options =>
                {
                    foreach (var description in app.ApplicationServices.GetRequiredService<IApiVersionDescriptionProvider>().ApiVersionDescriptions)
                    {
                        options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json",
                            description.GroupName.ToUpperInvariant());
                    }
                });
        }
    }

    public class SlugifyParameterTransformer : IOutboundParameterTransformer
    {
        public string TransformOutbound(object value)
        {
            return value == null ? null : Regex.Replace(value.ToString()!, "([a-z])([A-Z])", "$1-$2").ToLower();
        }
    }
}
