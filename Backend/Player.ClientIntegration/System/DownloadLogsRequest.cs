using System;

namespace Player.ClientIntegration.System
{
    public class DownloadLogsRequest
    {
        public Guid UserId { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public bool DbLogs { get; set; }
    }
}