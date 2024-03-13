using System;
using System.Collections.Generic;

namespace Player.Domain
{
    public class Advert : Track
    {
        public DateTime CreateDate { get; set; }
        
        /// <summary>
        /// Список дней и количеств выходов рекламы на объекте
        /// </summary>
        public ICollection<AdTime> AdTimes { get; set; } = new List<AdTime>();

        /// <summary>
        /// Список дат начала и окончания действия рекламы
        /// </summary>
        public ICollection<AdLifetime> AdLifetimes { get; set; } = new List<AdLifetime>();

        /// <summary>
        /// Список дат и объектов, в котором была проиграна реклама
        /// </summary>
        public ICollection<AdHistory> AdHistories { get; set; } = new List<AdHistory>();

        /// <summary>
        /// Плейлисты, в которых пристутсвует реклама
        /// </summary>
        public ICollection<AdvertPlaylist> Playlists { get; set; } = new List<AdvertPlaylist>();

        public Organization Organization { get; set; }
        public Guid OrganizationId { get; set; }
        public AdvertType AdvertType { get; set; }
        public Guid? AdvertTypeId { get; set; }
    }
}
