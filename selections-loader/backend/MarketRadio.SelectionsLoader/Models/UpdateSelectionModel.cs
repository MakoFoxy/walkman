using System;
using System.Collections.Generic;

namespace MarketRadio.SelectionsLoader.Models
{
    public class UpdateSelectionModel
    {
        public Guid? Id { get; set; }
        public string Name { get; set; }
        public DateTime DateBegin { get; set; }
        public DateTime? DateEnd { get; set; }
        public bool IsPublic { get; set; }
        public IList<Guid> Tracks { get; set; } = new List<Guid>();
    }
}