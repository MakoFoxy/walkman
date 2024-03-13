using System;

namespace Player.DTOs
{
    public class ObjectModel
    {
        public Guid Id { get; set; }
        public string Bin { get; set; }
        public string Name { get; set; }
        public TimeSpan EndTime { get; set; }
        public TimeSpan WorkTime { get; set; }
    }
}