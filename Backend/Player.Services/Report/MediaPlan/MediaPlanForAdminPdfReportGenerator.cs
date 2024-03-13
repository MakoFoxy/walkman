using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Player.Services.Report.Abstractions;
using Wkhtmltopdf.NetCore;

namespace Player.Services.Report.MediaPlan
{
    public class MediaPlanForAdminPdfReportGenerator : BaseReportGenerator<AdminMediaPlanPdfReportModel>
    {
        private readonly IGeneratePdf _generatePdf;
        
        public override ReportType ReportType => ReportType.Pdf;
        
        public MediaPlanForAdminPdfReportGenerator(IGeneratePdf generatePdf)
        {
            _generatePdf = generatePdf;
        }
        
        protected override async Task<string> LoadTemplate(AdminMediaPlanPdfReportModel model)
        {
            using var reader = File.OpenText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "report-templates/admin-media-plan-template.html"));
            return await reader.ReadToEndAsync();
        }

        protected override Task<byte[]> GenerateAndInsertData(AdminMediaPlanPdfReportModel model)
        {
            var html = Content.Replace("[body]", BuildBody(model));
            return Task.FromResult(_generatePdf.GetPDF(html));
        }

        protected override Task<string> GetReportName(AdminMediaPlanPdfReportModel model)
        {
            return Task.FromResult($"Админ {model.Object.Name} {DateTime.Today.AddDays(-1):ddMMyy}");
        }

        protected override Task AfterGenerate(AdminMediaPlanPdfReportModel model, GeneratorResult result)
        {
            return Task.CompletedTask;
        }

        private string BuildBody(AdminMediaPlanPdfReportModel model)
        {                
            var rows = new StringBuilder();
            var i = 1;
            
            foreach (var track in model.Tracks.All)
            {
                rows.Append("<tr>" +
                            $"<td>{i++}</td>" +
                            $"<td>{model.Tracks.All.Take(i).Count(t => t.Id == track.Id)}</td>" +
                            $"<td>{track.BeginTime:hh\\:mm\\:ss}</td>" +
                            $"<td>{track.Name}</td>" +
                            $"<td>{Math.Round(track.Length.TotalSeconds)}</td>" +
                            "</tr>");
            }

            return rows.ToString();
        }
    }
}