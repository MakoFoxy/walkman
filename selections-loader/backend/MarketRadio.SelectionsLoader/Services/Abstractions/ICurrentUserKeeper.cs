namespace MarketRadio.SelectionsLoader.Services.Abstractions
{
    public interface ICurrentUserKeeper
    {
        string Token { get; }
        void SetToken(string token);
    }
}