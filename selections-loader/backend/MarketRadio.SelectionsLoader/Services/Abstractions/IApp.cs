namespace MarketRadio.SelectionsLoader.Services.Abstractions
{
    public interface IApp
    {
        string Version { get; }
        string ProductName { get; }
        string RunId { get; }
    }
}