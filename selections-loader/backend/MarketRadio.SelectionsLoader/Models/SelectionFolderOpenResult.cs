using System.Collections.Generic;

namespace MarketRadio.SelectionsLoader.Models
{
    public class SelectionFolderOpenResult
    {
        public string FullPath { get; set; }
        public string SelectionName { get; set; }
        public ICollection<string> Tracks { get; set; } = new List<string>();
    }
}