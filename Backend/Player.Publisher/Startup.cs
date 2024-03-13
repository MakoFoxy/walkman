using System;
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
        private readonly IWebHostEnvironment _env;

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            _env = env;
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {            
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
            
            services.Configure<PlaylistGeneratorConfiguration>(
                Configuration.GetSection("Player:Configurations:PlaylistGenerator"));
            services.AddDbContext<PlayerContext>(builder =>
            {
                builder.UseNpgsql(Configuration.GetSection("Player:WalkmanConnectionString").Value);

                if (_env.IsDevelopment())
                {
                    builder.EnableSensitiveDataLogging();
                }
            });
            
            services.AddControllers(option =>
                {
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

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddMediatR(typeof(List.Handler).Assembly);
            services.AddAutoMapper(typeof(List.Handler).Assembly, GetType().Assembly);
            services.AddSwaggerGen(
                options => { options.OperationFilter<SwaggerDefaultValues>(); options.CustomSchemaIds(x => x.FullName); });

            services.AddSignalR().AddJsonProtocol(options =>
            {
                options.PayloadSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            });
            services.AddRefitClient<IObjectApi>()
                .ConfigureHttpClient(client => client.BaseAddress = new Uri(Configuration.GetValue<string>("Player:ApiEndpoints:WebAppUrl")));
            services.AddCors();
            services.AddHostedService<ObjectOnlineStatusSyncWorker>();
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.RegisterAssemblyTypes(typeof(UserManager).Assembly).AsSelf().AsImplementedInterfaces();
        }

        public void Configure(IApplicationBuilder app, ILogger<Startup> logger)
        {
            app.UseMiddleware<BadFormatLogMiddleware>();
            
            if (_env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors(builder => builder.AllowAnyMethod()
                .WithOrigins("http://localhost:8080", "https://dev.909.kz", "https://909.kz")
                .AllowCredentials()
                .AllowAnyHeader());

            app.UseRouting();
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.All
            });
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<PlayerClientHub>("/ws/player-client-hub");
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
