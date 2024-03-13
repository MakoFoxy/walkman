using System;
using MarketRadio.Player.DataAccess.Domain.Base;

namespace MarketRadio.Player.DataAccess.Domain
{
    public class BanList : Entity
    {
        public Guid MusicTrackId { get; set; }
        public Guid ObjectId { get; set; }
    }
}