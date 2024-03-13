using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Humanizer;
using MarketRadio.Player.Helpers;
using MarketRadio.Player.Services.Http;
using Microsoft.Extensions.Logging;
using Player.ClientIntegration.System;

namespace MarketRadio.Player.Services.System
{
    public class LogsUploader
    {
        private readonly ISystemService _systemService;
        private readonly PlayerStateManager _playerStateManager;
        private readonly ILogger<LogsUploader> _logger;
        private readonly IApp _app;

        public LogsUploader(
            ISystemService systemService,
            PlayerStateManager playerStateManager,
            ILogger<LogsUploader> logger,
            IApp app)
        {
            _systemService = systemService;
            _playerStateManager = playerStateManager;
            _logger = logger;
            _app = app;
        }

        public async Task UploadLogs(DownloadLogsRequest downloadLogsRequest)
        {
            var uploadLogDir = Path.Combine(DefaultLocations.LogsPath, "upload");

            var fullLogsPath = Path.Combine(DefaultLocations.LogsPath, "app");
            var files = Directory.GetFiles(fullLogsPath)
                .Select(f => new FileInfo(f))
                .Where(f => f.LastWriteTime.Date >= downloadLogsRequest.From && f.LastWriteTime.Date < downloadLogsRequest.To);

            if (downloadLogsRequest.DbLogs)
            {
                files = files.Where(f => f.Extension == ".db");
            }
            else
            {
                files = files.Where(f => f.Extension == ".txt");
            }

            var filteredFiles = files.ToList();

            var archiveDirectory = Path.Combine(uploadLogDir,
                $"upload_{downloadLogsRequest.From:yyyy-MM-dd}_{downloadLogsRequest.To:yyyy-MM-dd}");

            if (Directory.Exists(archiveDirectory))
            {
                Directory.Delete(archiveDirectory, true);
            }

            Directory.CreateDirectory(archiveDirectory);

            foreach (var file in filteredFiles)
            {
                File.Copy(file.FullName, Path.Combine(archiveDirectory, file.Name));
            }

            var fileName = $"log_archive_{_playerStateManager.Object?.NormalizedName}_{downloadLogsRequest.From:yyyy-MM-dd}_{downloadLogsRequest.To:yyyy-MM-dd}.zip";

            var fullArchivePath = Path.Combine(uploadLogDir, fileName);

            if (File.Exists(fullArchivePath))
            {
                File.Delete(fullArchivePath);
            }
            
            _logger.LogInformation("Creating zip archive {FileName} from directory {Directory}", fullArchivePath, archiveDirectory);
            ZipFile.CreateFromDirectory(archiveDirectory, fullArchivePath);
            _logger.LogInformation("Created zip archive");

            AppendClientInfoToZip(fullArchivePath);

            var archive = await File.ReadAllBytesAsync(fullArchivePath);
            _logger.LogInformation("Sending zip archive");
            var _ = await _systemService.SendLogsToServer(new DownloadLogsResponse
            {
                DownloadLogsRequest = downloadLogsRequest,
                File = new ArchiveLogFile
                {
                    Body = Convert.ToBase64String(archive),
                    Name = fileName
                }
            });
            _logger.LogInformation("Sent zip archive");
            File.Delete(fullArchivePath);
            Directory.Delete(archiveDirectory, true);
        }

        private void AppendClientInfoToZip(string fullArchivePath)
        {
            using var zipArchive = ZipFile.Open(fullArchivePath, ZipArchiveMode.Update);
            var entry = zipArchive.CreateEntry("_object_info.txt");
            using var entryStream = entry.Open();
            using var streamWriter = new StreamWriter(entryStream);
            var logsDirSize = new DirectoryInfo(DefaultLocations.LogsPath).GetDirectorySize().Bytes().Humanize("#.0");
            var dbFile = new DirectoryInfo(DefaultLocations.DatabasePath).GetFiles(DefaultLocations.DatabaseFileName).Single();
            var dbFileSize = dbFile.Length.Bytes().Humanize("#.0");
            var tracksDirSize = new DirectoryInfo(DefaultLocations.TracksPath).GetDirectorySize().Bytes().Humanize("#.0");
            var pathRoot = Path.GetPathRoot(DefaultLocations.TracksPath);
            var freeSpace = DriveInfo.GetDrives().Single(d => d.Name == pathRoot).AvailableFreeSpace.Bytes().Humanize("#.0");

            streamWriter.WriteLine($"Date:{GetTabsCount(4)}{DateTime.Now:yyyy-MM-dd_HH:mm:ss}");
            streamWriter.WriteLine($"OS:{GetTabsCount(5)}{Environment.OSVersion}");
            streamWriter.WriteLine($"Client version:{GetTabsCount(2)}{_app.Version}");
            streamWriter.WriteLine($"Client start date:{GetTabsCount(1)}{_app.StartDate:yyyy-MM-dd_HH:mm:ss}");
            streamWriter.WriteLine($"Client run id:{GetTabsCount(2)}{_app.RunId}");
            streamWriter.WriteLine($"Master volume:{GetTabsCount(2)}{_playerStateManager.GetMasterVolume()}");
            streamWriter.WriteLine($"Free disk size:{GetTabsCount(2)}{freeSpace}");
            streamWriter.WriteLine($"Logs directory path:{GetTabsCount()}{DefaultLocations.LogsPath}");
            streamWriter.WriteLine($"Logs directory size:{GetTabsCount()}{logsDirSize}");
            streamWriter.WriteLine($"DB file path:{GetTabsCount(2)}{DefaultLocations.DatabasePath}");
            streamWriter.WriteLine($"DB file size:{GetTabsCount(2)}{dbFileSize}");
            streamWriter.WriteLine($"Tracks directory path:{GetTabsCount()}{DefaultLocations.TracksPath}");
            streamWriter.WriteLine($"Tracks directory size:{GetTabsCount()}{tracksDirSize}");
        }

        private string GetTabsCount(int additionalTabs = 0)
        {
            const int minTabsCount = 2;

            return string.Empty.PadLeft(minTabsCount + additionalTabs, '\t');
        }
    }
}