using System;
using Player.Domain.Base;

namespace Player.Domain
{
    public class AdvertPlaylist : Entity
    {
        public Advert Advert { get; set; }
        public Guid AdvertId { get; set; }
        public Playlist Playlist { get; set; }
        public Guid PlaylistId { get; set; }
        public DateTime PlayingDateTime { get; set; }

        public override string ToString()
        {
            return $"Advert = {Advert.Name}, PlayingDate = {PlayingDateTime:HH:mm:ss dd.MM.yyyy}";
        }
    }
}
