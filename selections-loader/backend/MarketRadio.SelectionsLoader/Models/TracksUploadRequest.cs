using System.Collections.Generic;

namespace MarketRadio.SelectionsLoader.Models
{
    public class TracksUploadRequest
    {
        public List<string> Paths { get; set; } = new();
        public List<SimpleDto> Genres { get; set; } = new();
    }
}