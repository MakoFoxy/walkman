using System;

namespace Player.DTOs
{
    public class TrackModel
    {
        public string TypeCode { get; set; }
        public string Name { get; set; }
        public TimeSpan Length { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}