using System;

namespace Player.Services.Report.Abstractions
{
    public interface IReportModel
    {
        DateTime ReportBegin { get; set; }
        DateTime ReportEnd { get; set; }
        DateTime Date { get; set; }
    }

    public class GeneratorResult
    {
        public string FileName { get; set; }
        public string FileType { get; set; }
        public byte[] Report { get; set; }
    }
}