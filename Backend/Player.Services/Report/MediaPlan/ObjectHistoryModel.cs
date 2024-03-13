using System.Collections.Generic;
using Player.Domain;

namespace Player.Services.Report.MediaPlan
{
    public class ObjectHistoryModel
    {
        public ObjectInfo Object { get; set; }
        public IEnumerable<AdHistory> History { get; set; } = new List<AdHistory>();
    }
}