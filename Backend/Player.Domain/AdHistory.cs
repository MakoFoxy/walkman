using System;
using Player.Domain.Base;

namespace Player.Domain
{
    /// <summary>
    /// Дата и объект, в котором была проиграна реклама
    /// </summary>
    public class AdHistory : Entity
    {
        public Advert Advert { get; set; }
        public Guid AdvertId { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public ObjectInfo Object { get; set; }
        public Guid ObjectId { get; set; }
    }
}
