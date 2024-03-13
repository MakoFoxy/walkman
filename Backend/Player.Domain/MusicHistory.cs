using System;
using Player.Domain.Base;

namespace Player.Domain
{
    /// <summary>
    /// Дата и время воспроизведения музыкального трека на объекте
    /// </summary>
    public class MusicHistory : Entity
    {
        public MusicTrack MusicTrack { get; set; }
        public DateTime Date { get; set; }
        public ObjectInfo Object { get; set; }
    }
}