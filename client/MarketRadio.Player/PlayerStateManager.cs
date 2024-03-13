using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AudioSwitcher.AudioApi;
using AudioSwitcher.AudioApi.CoreAudio;
using ElectronNET.API;
using MarketRadio.Player.DataAccess.Domain;
using MarketRadio.Player.Models;
using MarketRadio.Player.Services;
using MarketRadio.Player.Services.LiveConnection;
using Microsoft.Extensions.DependencyInjection;
using Player.ClientIntegration;
using Player.ClientIntegration.Object;

namespace MarketRadio.Player
{
    public class PlayerStateManager
    {
        private readonly Bus _bus;
        private readonly IServiceProvider _serviceProvider;
        private readonly IAudioController _audioController;

        public PlayerStateManager(Bus bus, IServiceProvider serviceProvider, IAudioController audioController)
        {
            _bus = bus;
            _serviceProvider = serviceProvider;
            _audioController = audioController;
        }

        public PlaylistDto? Playlist { get;  private set; }
        public bool PlaylistIsDownloading { get; set; }
        public ObjectInfoDto? Object { get; set; }
        public TrackDto? CurrentTrack { get; private set; }
        public bool IsOnline { get; set; }
        public List<Guid> BannedTracks { get; private set; } = new();

        public TrackDto? NextTrack
        {
            get
            {
                if (Playlist == null)
                {
                    return null;
                }
                
                return Playlist.Tracks
                    .Where(p => p.PlayingDateTime > CurrentTrack?.PlayingDateTime)
                    .MinBy(p => p.PlayingDateTime);
            }
        }

        public BrowserWindow? CurrentWindow { get; set; }
        
        public Task UpdateObject(ObjectInfoDto @object)
        {
            Object = @object;
            return _bus.ObjectUpdated(@object);
        }

        public Task ChangeMasterVolume(int volume)
        {
            _audioController.DefaultPlaybackDevice.Volume = volume;
            return Task.CompletedTask;
        }
        
        public int GetMasterVolume()
        {
            return Convert.ToInt32(_audioController.DefaultPlaybackDevice.Volume);
        }

        public async Task ChangeCurrentTrack(TrackDto track)
        {
            CurrentTrack = track;
            await _bus.CurrentTrackChanged(track.UniqueId);
            //TODO не хороший код, надо избавиться и изменить архитектуру
            using var scope = _serviceProvider.CreateScope();
            var serverLiveConnection = scope.ServiceProvider.GetRequiredService<ServerLiveConnection>();
            
            await serverLiveConnection.CurrentTrackResponse(new OnlineObjectInfo
            {
                Date = DateTime.Now,
                CurrentTrack = track.UniqueId,
                ObjectId = Object!.Id,
                SecondsFromStart = 0,
            });
        }
        
        public async Task ChangePlaylist(PlaylistDto playlist)
        {
            await _bus.PlaylistLoaded(playlist);
            Playlist = playlist;
        }

        public async Task ChangeOnlineState(bool isOnline)
        {
            await _bus.OnlineStateChanged(isOnline);
            IsOnline = isOnline;
        }

        public async Task ChangeObjectVolume(ObjectVolumeChanged volumeChanged)
        {
            if (CurrentTrack == null)
            {
                return;
            }

            if (CurrentTrack.Type == Track.Advert)
            {
                await _bus.CurrentVolumeChanged(volumeChanged.AdvertVolume);
            }
            else
            {
                await _bus.CurrentVolumeChanged(volumeChanged.MusicVolume);
            }
        }
    }
}