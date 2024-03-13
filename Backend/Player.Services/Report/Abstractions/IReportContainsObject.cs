using System;

namespace Player.Services.Report.Abstractions
{
    public interface IReportContainsObject : IReportModel
    {
        Object Object { get; set; }
    }

    public class Object
    {
        public string Name { get; set; }
        public TimeSpan BeginTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }
}