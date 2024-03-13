using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Player.DataAccess;
using Player.Services;
using Player.Services.Abstractions;
using Player.Services.Report.Abstractions;
using Player.Services.Report.MediaPlan;
using Wkhtmltopdf.NetCore;

namespace Player.ReportSender
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<PlayerContext>(builder =>
                builder.UseNpgsql(_configuration.GetValue<string>("Player:WalkmanConnectionString")));
            services.AddTransient<IReportGenerator<AdminMediaPlanPdfReportModel>, MediaPlanForAdminPdfReportGenerator>();
            services.AddTransient<IReportGenerator<ClientMediaPlanPdfReportModel>, MediaPlanForClientPdfReportGenerator>();
            services.AddTransient<IReportGenerator<PartnerMediaPlanPdfReportModel>, MediaPlanForPartnerPdfReportGenerator>();
            services.AddSingleton<ITelegramConfiguration, TelegramConfiguration>();
            services.AddTransient<ITelegramMessageSender, TelegramMessageSender>();
            services.AddWkhtmltopdf("wkhtmltopdf");
            services.AddHostedService<ManagerMediaPlanWorker>();
            services.AddHostedService<ClientMediaPlanWorker>();
            services.AddHostedService<PartnerMediaPlanWorker>();
        }

        public void Configure(IApplicationBuilder app, ILogger<Startup> logger)
        {
        }
    }
}