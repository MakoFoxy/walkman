using MarketRadio.Player.DataAccess.Domain.Base;

namespace MarketRadio.Player.DataAccess.Domain
{
    public class UserSetting : Entity
    {
        public required string Key { get; set; }
        public required string Value { get; set; }

        public const string Token = nameof(Token);
        public const string Email = nameof(Email);
    }
}