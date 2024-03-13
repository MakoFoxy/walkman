using System;
using System.Collections.Generic;
using Player.Services.Report.Abstractions;
using Object = Player.Services.Report.Abstractions.Object;

namespace Player.Services.Report.MediaPlan;

public class PartnerMediaPlanPdfReportModel : IReportContainsClient
{
    public IEnumerable<ObjectHistoryModel> ObjectHistoryModels { get; set; } = new List<ObjectHistoryModel>();
    public DateTime ReportBegin { get; set; }
    public DateTime ReportEnd { get; set; }
    public DateTime Date { get; set; }
    public Object Object { get; set; }
    public Client Client { get; set; }
}