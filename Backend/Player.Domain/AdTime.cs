using System;
using Player.Domain.Base;

namespace Player.Domain
{
    /// <summary>
    /// День и количество выходов рекламы на объекте
    /// </summary>
    public class AdTime : Entity
    {
        public Advert Advert { get; set; }
        public Guid AdvertId { get; set; }
        public DateTime PlayDate { get; set; }
        public int RepeatCount { get; set; }
        public ObjectInfo Object { get; set; }
        public Guid ObjectId { get; set; }
    }
}
