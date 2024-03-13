using System;

namespace MarketRadio.Player.DataAccess.Domain.Base
{
    public interface IEntity
    {
        public Guid Id { get; set; }
    }
}