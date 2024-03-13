using System;
using MarketRadio.SelectionsLoader.Domain.Abstractions;

namespace MarketRadio.SelectionsLoader.Domain
{
    public class TrackInSelection : Entity
    {
        public Track Track { get; set; }
        public Guid TrackId { get; set; }
        public Selection Selection { get; set; }
        public Guid SelectionId { get; set; }
        public int Order { get; set; }
    }
}