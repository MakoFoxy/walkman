using System;
using System.IO;

namespace Player.DTOs
{
    public class MusicFileModel
    {
        public Guid? MusicTrackId { get; set; }
        public string Name { get; set; }
        public Stream Stream { get; set; }
        public SimpleDto Genre { get; set; }
    }
}
