using System;
using MarketRadio.Player.Helpers;

namespace MarketRadio.Player.Models
{
    public class ObjectInfoDto
    {
        public Guid Id { get; set; }
        public string Bin { get; set; } = null!;
        public string Name { get; set; } = null!;
        public TimeSpan BeginTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public DayOfWeek[] FreeDays { get; set; } = Array.Empty<DayOfWeek>();
        public SettingsDto Settings { get; set; } = null!;

        public string NormalizedName => PathHelper.ToSafeName(Name.Replace(" ", "_").ToLower());
    }
}