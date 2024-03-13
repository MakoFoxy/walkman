using System;

namespace Player.ClientIntegration.Object
{
    public class ObjectVolumeChanged
    {
        public Guid ObjectId { get; set; }
        public int Hour { get; set; }
        public int AdvertVolume { get; set; }
        public int MusicVolume { get; set; }
    }
}