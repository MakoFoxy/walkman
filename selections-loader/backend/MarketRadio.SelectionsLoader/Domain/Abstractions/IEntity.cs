using System;

namespace MarketRadio.SelectionsLoader.Domain.Abstractions
{
    public interface IEntity
    {
        Guid Id { get; set; }
    }
}