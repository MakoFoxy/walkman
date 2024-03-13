using System;

namespace Player.Reminder
{
    public class ServiceSettings
    {
        public int SelectionExpiredDays { get; set; }
        public int MaxPlaylistGenerationTime { get; set; }
        public int PlaylistGenerationCheckPeriod { get; set; }
        public TimeSpan WakeUpTime { get; set; }
    }
}