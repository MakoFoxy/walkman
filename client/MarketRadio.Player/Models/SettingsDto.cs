using System;

namespace MarketRadio.Player.Models
{
    public class SettingsDto
    {
        public int[] AdvertVolume { get; set; } = Array.Empty<int>();
        public int[] MusicVolume { get; set; } = Array.Empty<int>();
        public bool IsOnTop { get; set; }
        public int SilentTime { get; set; }
    }
}