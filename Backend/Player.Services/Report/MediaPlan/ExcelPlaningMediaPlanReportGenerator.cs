using System;
using System.Threading.Tasks;
using Player.Services.Report.Abstractions;

namespace Player.Services.Report.MediaPlan
{
    public class ExcelPlaningMediaPlanReportGenerator : BaseReportGenerator<PlaningMediaPlanReportModel>
    {
        protected override Task<string> LoadTemplate(PlaningMediaPlanReportModel model)
        {
            throw new NotImplementedException();
        }

        protected override Task<byte[]> GenerateAndInsertData(PlaningMediaPlanReportModel model)
        {
            throw new NotImplementedException();
        }

        protected override Task<string> GetReportName(PlaningMediaPlanReportModel model)
        {
            throw new NotImplementedException();
        }

        protected override Task AfterGenerate(PlaningMediaPlanReportModel model, GeneratorResult result)
        {
            throw new NotImplementedException();
        }

        public override ReportType ReportType => ReportType.Xlsx;
    }
}