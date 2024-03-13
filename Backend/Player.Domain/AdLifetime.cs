using System;
using Player.Domain.Base;

namespace Player.Domain
{
    /// <summary>
    /// Дата начала и окончания действия рекламы
    /// </summary>
    public class AdLifetime : Entity
    {
        public Advert Advert { get; set; }
        public DateTime DateBegin { get; set; }
        public DateTime DateEnd { get; set; }
        public bool InArchive { get; set; }
    }
}