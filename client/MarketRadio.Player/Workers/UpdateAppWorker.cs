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
    {//Код представляет собой фоновую службу UpdateAppWorker, наследуемую от базового класса PlayerBackgroundServiceBase, предназначенную для автоматического обновления приложения. Давайте подробно разберем, как она работает. 
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
            //             PlayerStateManager _stateManager: Управляет состоянием плеера, включая информацию о текущем плейлисте и загрузке обновлений.
            // ILogger<UpdateAppWorker> _logger: Предоставляет функции логирования для отслеживания событий и ошибок, связанных с процессом обновления.
            // IHostEnvironment _env: Информация о среде выполнения приложения, используется для определения, находится ли приложение в режиме разработки.
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (_env.IsDevelopment()) //Проверка Среды: Если приложение запущено в среде разработки (_env.IsDevelopment()), служба завершает выполнение, предотвращая ненужные обновления во время разработки.
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
            //Инициализация: Регистрация обработчиков событий обновления Electron.AutoUpdater, таких как доступность обновлений (OnUpdateAvailable), прогресс скачивания (OnDownloadProgress), завершение скачивания (OnUpdateDownloaded) и ошибки (OnError). Эти обработчики управляют процессом обновления и логированием.
            await Task.Delay(TimeSpan.FromMinutes(1));

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Update check start");
                    await DoWork();//Периодическая Проверка: Служба циклически каждые 30 минут выполняет проверку на наличие обновлений (DoWork), если предыдущее скачивание не запущено и не завершено.
                    _logger.LogInformation("Update check end");
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Update check error");
                }
                await Task.Delay(TimeSpan.FromMinutes(30));
            }
        }

        private void OnError(string error)//OnError: Логирует ошибки скачивания обновления и сбрасывает флаг скачивания (_updateDownloading).
        {
            _logger.LogError("Error in downloading update {Error}", error);
            _updateDownloading = false;
        }

        private async void OnUpdateDownloaded(UpdateInfo updateInfo)//OnUpdateDownloaded: После скачивания обновления ожидает окончания рабочего времени для установки обновления (Electron.AutoUpdater.QuitAndInstall), предполагая минимизацию влияния на воспроизведение медиа.
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
        {//OnDownloadProgress: Может использоваться для отображения прогресса скачивания, хотя в предоставленном коде тело метода пусто.
        }

        private async void OnUpdateAvailable(UpdateInfo updateInfo)
        {//OnUpdateAvailable: Когда обновление доступно, ожидает завершения загрузки текущего плейлиста (_stateManager.PlaylistIsDownloading), прежде чем начать скачивание обновления.
            _logger.LogInformation("Update available");
            while (_stateManager.PlaylistIsDownloading)
            {
                await Task.Delay(TimeSpan.FromSeconds(10));
            }

            _updateDownloading = true;
            await Electron.AutoUpdater.DownloadUpdateAsync();
        }

        private async Task DoWork()
        {//    Вызывает проверку наличия обновлений (Electron.AutoUpdater.CheckForUpdatesAsync), если в данный момент не происходит скачивание или установка обновления.
            if (_updateDownloading || _updateDownloaded)
            {
                return;
            }

            await Electron.AutoUpdater.CheckForUpdatesAsync();
            //Эта фоновая служба важна для поддержания приложения актуальным и безопасным, автоматически загружая и устанавливая обновления за пределами рабочего времени или когда это наименее влияет на функциональность воспроизведения. Использование Electron.AutoUpdater предполагает, что приложение разработано с использованием фреймворка Electron, что является популярным выбором для создания кросс-платформенных настольных приложений.
        }
    }
}