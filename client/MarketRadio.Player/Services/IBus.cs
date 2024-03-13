using System;
using System.Threading.Tasks;
using MarketRadio.Player.Models;

namespace MarketRadio.Player.Services
{
    public interface IBus
    {
        Task ObjectUpdated(ObjectInfoDto @object);
        Task PlaylistLoaded(PlaylistDto playlist);
        Task PlaylistStarted();
        Task PlaylistUpdating();
        Task PlaylistUpdateFinished();
        Task TrackAdded(Guid trackId);
        Task CurrentTrackChanged(string trackUniqueId);
        Task CurrentVolumeChanged(int volume);
        Task PingCurrentVolume(int volume);
        Task OnlineStateChanged(bool isOnline);
        Task StopPlaying();
    }
}