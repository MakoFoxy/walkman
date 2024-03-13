using System;
using Player.Services.Report.Abstractions;
using Object = Player.Services.Report.Abstractions.Object;

namespace Player.Services.Report.MediaPlan
{
    public class AdminMediaPlanPdfReportModel : IReportContainsObject, IReportContainsTracks
    {
        public DateTime ReportBegin { get; set; }
        public DateTime ReportEnd { get; set; }
        public DateTime Date { get; set; }
        public Object Object { get; set; }
        public Tracks Tracks { get; set; }
    }
}