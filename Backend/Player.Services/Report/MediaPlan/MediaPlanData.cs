using System.Collections.Generic;
using Player.Domain;

namespace Player.Services.Report.MediaPlan
{
    public class ClientAdHistoryModel
    {
        public Client Client { get; set; }
        public IEnumerable<AdHistory> AdHistory { get; set; } = new List<AdHistory>();
    }
}