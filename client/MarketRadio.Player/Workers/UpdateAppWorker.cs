using System;
using System.Threading;
using System.Threading.Tasks;
using ElectronNET.API;
using ElectronNET.API.Entities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MarketRadio.Player.Workers
{
    public class UpdateAppWorker : PlayerBackgroundServiceBase
    {
        private readonly PlayerStateManager _stateManager;
        private readonly ILogger<UpdateAppWorker> _logger;
        private readonly IHostEnvironment _env;
        private bool _updateDownloading;
        private bool _updateDownloaded;

        public UpdateAppWorker(PlayerStateManager stateManager, 
            ILogger<UpdateAppWorker> logger,
            IHostEnvironment env) : base(stateManager)
        {
            _stateManager = stateManager;
            _logger = logger;
            _env = env;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (_env.IsDevelopment())
            {
                return;
            }
            
            await Task.WhenAll(WaitForObject(stoppingToken), WaitForPlaylist(stoppingToken));
            
            _logger.LogInformation("UpdateAppWorker start");
            
            Electron.AutoUpdater.AutoDownload = false;
            Electron.AutoUpdater.OnUpdateAvailable += OnUpdateAvailable;
            Electron.AutoUpdater.OnDownloadProgress += OnDownloadProgress;
            Electron.AutoUpdater.OnUpdateDownloaded += OnUpdateDownloaded;
            Electron.AutoUpdater.OnError += OnError;

            //После полноценного запуска ждем еще минуту чтобы не мешать скачиванию треков
            await Task.Delay(TimeSpan.FromMinutes(1));

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Update check start");
                    await DoWork();
                    _logger.LogInformation("Update check end");
                }
                catch (Exception e)
                {
                    _logger.LogError(e,"Update check error");
                }
                await Task.Delay(TimeSpan.FromMinutes(30));
            }
        }

        private void OnError(string error)
        {
            _logger.LogError("Error in downloading update {Error}", error);
            _updateDownloading = false;
        }

        private async void OnUpdateDownloaded(UpdateInfo updateInfo)
        {
            _updateDownloading = false;
            _updateDownloaded = true;
            _logger.LogInformation("Update downloaded");
            
            while (NowTheWorkingTime)
            {
                await Task.Delay(TimeSpan.FromSeconds(10));
            }
            
            Electron.AutoUpdater.QuitAndInstall();
        }

        private void OnDownloadProgress(ProgressInfo progressInfo)
        {
        }

        private async void OnUpdateAvailable(UpdateInfo updateInfo)
        {
            _logger.LogInformation("Update available");
            while (_stateManager.PlaylistIsDownloading)
            {
                await Task.Delay(TimeSpan.FromSeconds(10));
            }

            _updateDownloading = true;
            await Electron.AutoUpdater.DownloadUpdateAsync();
        }

        private async Task DoWork()
        {
            if (_updateDownloading || _updateDownloaded)
            {
                return;
            }
            
            await Electron.AutoUpdater.CheckForUpdatesAsync();
        }
    }
}