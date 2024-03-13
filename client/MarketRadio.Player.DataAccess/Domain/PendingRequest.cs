using System;
using MarketRadio.Player.DataAccess.Domain.Base;

namespace MarketRadio.Player.DataAccess.Domain
{
    public class PendingRequest : Entity
    {
        public required string HttpMethod { get; set; }
        public required string Url { get; set; }
        public required string Body { get; set; }
        public DateTime Date { get; set; }
    }
}