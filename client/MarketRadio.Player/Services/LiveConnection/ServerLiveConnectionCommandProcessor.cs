using System;
using System.Threading.Tasks;
using MarketRadio.Player.DataAccess;
using MarketRadio.Player.DataAccess.Domain;
using MarketRadio.Player.Models;
using MarketRadio.Player.Services.System;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Player.ClientIntegration;
using Player.ClientIntegration.MusicTrack;
using Player.ClientIntegration.Object;
using Player.ClientIntegration.System;

namespace MarketRadio.Player.Services.LiveConnection
{
    public class ServerLiveConnectionCommandProcessor
    {
        private readonly ILogger<ServerLiveConnectionCommandProcessor> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly PlayerStateManager _stateManager;
        private readonly LogsUploader _logsUploader;
        private HubConnection _connection = null!;

        public ServerLiveConnectionCommandProcessor(
            ILogger<ServerLiveConnectionCommandProcessor> logger,
            IServiceProvider serviceProvider,
            PlayerStateManager stateManager,
            LogsUploader logsUploader)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _stateManager = stateManager;
            _logsUploader = logsUploader;
        }

        public void Subscribe(HubConnection connection)
        {
            _connection = connection;
            
            _connection.On<BanMusicTrackDto>(OnlineEvents.MusicBanned, MusicBanAccepted);
            _connection.On<ObjectVolumeChanged>(OnlineEvents.ObjectVolumeChanged, ObjectVolumeChangeAccepted);
            _connection.On<DownloadLogsRequest>(OnlineEvents.DownloadLogs, DownloadLogsAccepted);
            _connection.On(OnlineEvents.CurrentTrackRequest, CurrentTrackRequest);
            _connection.On<ObjectInfo>(OnlineEvents.ObjectInfoReceived, ObjectInfoAccepted);
        }

        private async Task ObjectInfoAccepted(ObjectInfo info)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<PlayerContext>();
            var objectInfo = await context.ObjectInfos.SingleAsync();

            objectInfo.Bin = info.Bin;
            objectInfo.City = info.City;
            objectInfo.Name = info.Name;
            objectInfo.Settings = info.Settings ?? objectInfo.Settings;
            objectInfo.BeginTime = info.BeginTime;
            objectInfo.EndTime = info.EndTime;
            objectInfo.FreeDays = info.FreeDays;

            await context.SaveChangesAsync();

            await _stateManager.UpdateObject(new ObjectInfoDto
            {
                Id = objectInfo.Id,
                Bin = objectInfo.Bin,
                Name = objectInfo.Name,
                BeginTime = objectInfo.BeginTime,
                EndTime = objectInfo.EndTime,
                FreeDays = objectInfo.FreeDays,
                Settings = new SettingsDto
                {
                    MusicVolume = objectInfo.Settings!.MusicVolumeComputed,
                    SilentTime = objectInfo.Settings.SilentTime,
                    AdvertVolume = objectInfo.Settings.AdvertVolumeComputed,
                    IsOnTop = objectInfo.Settings.IsOnTop,
                }
            });
        }

        private Task CurrentTrackRequest()
        {
            var onlineObjectInfo = new OnlineObjectInfo
            {
                Date = DateTime.Now,
                ObjectId = _stateManager.Object!.Id,
            };

            if (_stateManager.CurrentTrack != null)
            {
                onlineObjectInfo.CurrentTrack = _stateManager.CurrentTrack.UniqueId;
                onlineObjectInfo.SecondsFromStart = (int)onlineObjectInfo.Date.Subtract( _stateManager.CurrentTrack.PlayingDateTime).TotalSeconds;
            }
            
            // return _connection.SendAsync(OnlineEvents.CurrentTrackResponse, JsonConvert.SerializeObject(onlineObjectInfo));
            return _connection.SendAsync(OnlineEvents.CurrentTrackResponse, onlineObjectInfo);
        }

        private async Task ObjectVolumeChangeAccepted(ObjectVolumeChanged objectVolumeChanged)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<PlayerContext>();
            var objectInfo = await context.ObjectInfos.SingleAsync();
            objectInfo.Settings!.AdvertVolumeComputed[objectVolumeChanged.Hour] = objectVolumeChanged.AdvertVolume;
            objectInfo.Settings.MusicVolumeComputed[objectVolumeChanged.Hour] = objectVolumeChanged.MusicVolume;
            await context.SaveChangesAsync();

            _stateManager.Object!.Settings.AdvertVolume = objectInfo.Settings.AdvertVolumeComputed;
            _stateManager.Object!.Settings.MusicVolume = objectInfo.Settings.MusicVolumeComputed;
            await _stateManager.ChangeObjectVolume(objectVolumeChanged);
        }

        private async Task MusicBanAccepted(BanMusicTrackDto ban)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<PlayerContext>();
            context.BanLists.Add(new BanList
            {
                ObjectId = ban.ObjectId,
                MusicTrackId = ban.MusicId,
            });
            await context.SaveChangesAsync();
            _stateManager.BannedTracks.Add(ban.MusicId);
        }
        
        private async Task DownloadLogsAccepted(DownloadLogsRequest downloadLogsModel)
        {
            _logger.LogInformation("DownloadLogs accepted");
            try
            {
                await _logsUploader.UploadLogs(downloadLogsModel);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "");
                return;
            }

            _logger.LogInformation("DownloadLogs have been sent to the server");
        }
    }
}