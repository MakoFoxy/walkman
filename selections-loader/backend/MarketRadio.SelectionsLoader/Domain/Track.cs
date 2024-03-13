using System.Collections.Generic;
using MarketRadio.SelectionsLoader.Domain.Abstractions;

namespace MarketRadio.SelectionsLoader.Domain
{
    public class Track : Entity
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public bool Uploaded { get; set; }
        public bool UploadInProgress { get; set; }
        public double Length { get; set; }
        public ICollection<Genre> Genres { get; set; } = new List<Genre>();
    }
}