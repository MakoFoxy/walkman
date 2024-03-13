using System;

namespace MarketRadio.SelectionsLoader.Models
{
    public class Error
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime CreateDate { get; set; }
        public string Text { get; set; }
        public object Metadata { get; set; }
    }
}