using System;
using System.Collections.Generic;

namespace MarketRadio.SelectionsLoader.Models
{
    public class SelectionDto : SimpleDto
    {
        public bool IsPublic { get; set; }
        public bool Created { get; set; }
        public DateTime DateBegin { get; set; }
        public DateTime? DateEnd { get; set; }
        public List<TrackInSelectionDto> Tracks { get; set; } = new();
    }
}