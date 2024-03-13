using MarketRadio.SelectionsLoader.Services.Abstractions;

namespace MarketRadio.SelectionsLoader.Services
{
    public class CurrentUserKeeper : ICurrentUserKeeper
    {
        private string _token = "";

        public string Token => _token;

        public void SetToken(string token)
        {
            _token = $"Bearer {token}";
        }
    }
}