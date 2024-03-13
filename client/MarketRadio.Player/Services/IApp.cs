using System;

namespace MarketRadio.Player.Services
{
    public interface IApp
    {
        string Version { get; }
        string ProductName { get; }
        string RunId { get; }
        DateTime StartDate { get; }
    }
}