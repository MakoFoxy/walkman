using System;
using Player.Domain.Base;

namespace Player.Domain
{
    public class CheapPlaylistTemplate : Entity
    {
        public int Index { get; set; }
        public ObjectInfo ObjectInfo { get; set; }
        public Guid ObjectInfoId { get; set; }
        public DateTime DateBegin { get; set; }
        public DateTime? DateEnd { get; set; }
    }
}