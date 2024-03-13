namespace Player.ClientIntegration.System
{
    public class DownloadLogsResponse
    {
        public DownloadLogsRequest DownloadLogsRequest { get; set; }
        public ArchiveLogFile File { get; set; }
    }

    public class ArchiveLogFile
    {
        public string Body { get; set; }
        public string Name { get; set; }
    }
}