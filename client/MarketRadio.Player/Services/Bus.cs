using System;
using System.Threading.Tasks;
using ElectronNET.API;
using MarketRadio.Player.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace MarketRadio.Player.Services
{
    public class Bus : Hub<IBus>
    {
        private readonly ILogger<Bus> _logger;

        private string? _connectionId;

        public Bus(ILogger<Bus> logger)
        {
            _logger = logger;
        }

        public override Task OnConnectedAsync()
        {
            _connectionId = Context.ConnectionId;
            _logger.LogInformation("Loading playlist on frontend with {Id} connected to bus", _connectionId);
            return base.OnConnectedAsync();
        }

        public Task ObjectUpdated(ObjectInfoDto @object)
        {
            if (_connectionId == null)
                return Task.CompletedTask;

            return Clients.Client(_connectionId).ObjectUpdated(@object);
        }

        public Task PlaylistLoaded(PlaylistDto playlist)
        {
            if (_connectionId == null)
                return Task.CompletedTask;

            return Clients.Client(_connectionId).PlaylistLoaded(playlist);
        }

        public Task StopPlaying()
        {
            if (_connectionId == null)
                return Task.CompletedTask;

            return Clients.Client(_connectionId).StopPlaying();
        }

        public Task PlaylistUpdating()
        {
            if (_connectionId == null)
                return Task.CompletedTask;

            return Clients.Client(_connectionId).PlaylistUpdating();
        }

        public Task PlaylistUpdateFinished()
        {
            if (_connectionId == null)
                return Task.CompletedTask;

            return Clients.Client(_connectionId).PlaylistUpdateFinished();
        }

        public Task TrackAdded(Guid trackId)
        {
            if (_connectionId == null)
                return Task.CompletedTask;

            return Clients.Client(_connectionId).TrackAdded(trackId);
        }

        public Task CurrentTrackChanged(string trackUniqueId)
        {
            if (_connectionId == null)
                return Task.CompletedTask;

            return Clients.Client(_connectionId).CurrentTrackChanged(trackUniqueId);
        }

        public Task CurrentVolumeChanged(int volume)
        {
            if (_connectionId == null)
                return Task.CompletedTask;

            return Clients.Client(_connectionId).CurrentVolumeChanged(volume);
        }

        public Task PingCurrentVolume(int volume)
        {
            if (_connectionId == null)
                return Task.CompletedTask;

            return Clients.Client(_connectionId).PingCurrentVolume(volume);
        }

        public Task OnlineStateChanged(bool isOnline)
        {
            if (_connectionId == null)
                return Task.CompletedTask;

            return Clients.Client(_connectionId).OnlineStateChanged(isOnline);
        }
    }
}