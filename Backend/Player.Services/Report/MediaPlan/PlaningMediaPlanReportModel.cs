using System;
using System.Collections.Generic;
using Player.Services.Report.Abstractions;

namespace Player.Services.Report.MediaPlan
{
    public class PlaningMediaPlanReportModel : IReportContainsClient, IReportContainsTracks
    {
        public IEnumerable<ObjectHistoryModel> ObjectHistoryModels { get; set; } = new List<ObjectHistoryModel>();
        public DateTime ReportBegin { get; set; }
        public DateTime ReportEnd { get; set; }
        public DateTime Date { get; set; }
        public Client Client { get; set; }
        public Tracks Tracks { get; set; }
    }
}