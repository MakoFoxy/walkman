using MarketRadio.SelectionsLoader.Domain.Abstractions;

namespace MarketRadio.SelectionsLoader.Domain
{
    public class Settings : Entity
    {
        public string Key { get; set; }
        public string Value { get; set; }
        
        public const string Email = nameof(Email);
        public const string Token = nameof(Token);
    }
}