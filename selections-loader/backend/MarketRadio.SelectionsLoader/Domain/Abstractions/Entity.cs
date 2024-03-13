using System;

namespace MarketRadio.SelectionsLoader.Domain.Abstractions
{
    public class Entity : IEntity
    {
        public Guid Id { get; set; }
    }
}