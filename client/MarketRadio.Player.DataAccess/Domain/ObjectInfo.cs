using System;
using MarketRadio.Player.DataAccess.Domain.Base;

namespace MarketRadio.Player.DataAccess.Domain
{
    public class ObjectInfo : Entity
    {
        public string Bin { get; set; } = null!;
        public string Name { get; set; } = null!;
        public TimeSpan BeginTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public DayOfWeek[] FreeDays { get; set; } = Array.Empty<DayOfWeek>();
        public required City City { get; set; }
        public Settings? Settings { get; set; }
    }
}