using System;

namespace MarketRadio.SelectionsLoader.Models
{
    public class SelectionFromFolderUploadRequest
    {
        public string FullPath { get; set; }
        public DateTime DateBegin { get; set; }
        public DateTime? DateEnd { get; set; }
        public SimpleDto Genre { get; set; }
    }
}