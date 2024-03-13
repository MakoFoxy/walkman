using System.Collections.Generic;
using MarketRadio.SelectionsLoader.Domain.Abstractions;

namespace MarketRadio.SelectionsLoader.Domain
{
    public class Genre : Entity
    {
        public string Name { get; set; }
        public ICollection<Track> Tracks { get; set; } = new List<Track>();
    }
}