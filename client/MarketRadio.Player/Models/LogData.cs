using Microsoft.Extensions.Logging;

namespace MarketRadio.Player.Models
{
    public class LogData
    {
        public LogLevel Level { get; set; }
        public object Data { get; set; } = null!;
    }
}