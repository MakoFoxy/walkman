namespace Player.Services.Report.Abstractions
{
    public interface IReportContainsClient : IReportModel
    {
        Client Client { get; set; }
    }

    public class Client
    {
        public string Name { get; set; }
    }
}