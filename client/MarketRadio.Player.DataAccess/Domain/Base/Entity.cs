using System;

namespace MarketRadio.Player.DataAccess.Domain.Base
{
    public class Entity : IEntity
    {
        public Guid Id { get; set; }
    }
}