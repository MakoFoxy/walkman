using System;

namespace Player.DTOs
{
    public class ObjectDto
    {
        public Guid Id { get; set; }
        public string BeginTime { get; set; }
        public string EndTime { get; set; }
        public int Attendance { get; set; }
        public string ActualAddress { get; set; }
        public string LegalAddress { get; set; }
        public SimpleDto ActivityType { get; set; }
        public string Name { get; set; }
        public string Geolocation { get; set; }
        public SimpleDto ServiceCompany { get; set; }
        public SimpleDto City { get; set; }
        public double Area { get; set; }
        public int RentersCount { get; set; }
        public string Bin { get; set; }
        public DayOfWeek[] FreeDays { get; set; } = Array.Empty<DayOfWeek>();
    }
}