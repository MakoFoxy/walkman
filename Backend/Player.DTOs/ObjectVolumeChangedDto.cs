using System;

namespace Player.DTOs
{
    public class ObjectVolumeChangedDto
    {
        public Guid ObjectId { get; set; }
        public int Hour { get; set; }
        public int AdvertVolume { get; set; }
        public int MusicVolume { get; set; }
    }
}