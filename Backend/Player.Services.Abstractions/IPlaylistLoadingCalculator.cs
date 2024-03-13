using System;
using Player.Domain;

namespace Player.Services.Abstractions
{
    public interface IPlaylistLoadingCalculator
    {
        bool IsOverloaded(Playlist playlist);
        double GetLoading(ObjectInfo objectInfo, Playlist playlist);
        int GetOverflowCount(Playlist playlist, TimeSpan advertLength, int repeatCount);
    }
}