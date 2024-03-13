using System;
using System.Collections.Generic;

namespace MarketRadio.SelectionsLoader.Models
{
    public class TrackDto : SimpleDto
    {
        public TimeSpan Length { get; set; }
        public bool Uploaded { get; set; }
        public bool UploadInProgress { get; set; }
        public List<SimpleDto> Genres { get; set; } = new();
    }
}