using System;

namespace Player.ClientIntegration.Object
{
    public class OnlineObjectInfo
    {
        public Guid ObjectId { get; set; }
        public string CurrentTrack { get; set; }
        public DateTime Date { get; set; }
        public int SecondsFromStart { get; set; }
    }
}