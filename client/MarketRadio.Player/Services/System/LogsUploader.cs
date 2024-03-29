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
{//Этот класс C# LogsUploader разработан для системы в предполагаемом приложении, связанном с воспроизведением медиа, возможно, музыкального или видеоплеера. Его основная функция - сбор и загрузка журналов на сервер. Класс хорошо структурирован и демонстрирует хорошие практики в программной инженерии, включая внедрение зависимостей, логирование и асинхронное программирование. Давайте разберем ключевые компоненты и функциональность этого класса:
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
            //Конструктор LogsUploader принимает четыре зависимости:
            // ISystemService: интерфейс, предоставляющий функционал для взаимодействия с системными службами, например, для отправки данных на сервер.
            // PlayerStateManager: компонент для управления состоянием медиаплеера, возможно, содержит информацию о текущем воспроизводимом медиа или настройках плеера.
            // ILogger<LogsUploader>: интерфейс для логирования, позволяющий регистрировать сообщения о ходе выполнения программы.
            // IApp: интерфейс, представляющий приложение, может использоваться для доступа к глобальным настройкам или состоянию приложения.
        }

        public async Task UploadLogs(DownloadLogsRequest downloadLogsRequest)
        {//Метод UploadLogs асинхронно выполняет процесс загрузки журналов на сервер. Он принимает один параметр:
            // Определение директории для загрузки логов: Создается путь к директории для загружаемых логов, используя базовый путь логов и добавляя к нему поддиректорию "upload".
            var uploadLogDir = Path.Combine(DefaultLocations.LogsPath, "upload");


            var fullLogsPath = Path.Combine(DefaultLocations.LogsPath, "app");  //    Определение полного пути к логам приложения: Создается полный путь к директории с логами приложения, соединяя базовый путь логов с поддиректорией "app".


            var files = Directory.GetFiles(fullLogsPath)
                .Select(f => new FileInfo(f))
                .Where(f => f.LastWriteTime.Date >= downloadLogsRequest.From && f.LastWriteTime.Date < downloadLogsRequest.To);//    Получение и фильтрация файлов логов: Получение всех файлов из директории логов приложения, преобразование путей файлов в объекты FileInfo, и фильтрация их по дате изменения, чтобы выбрать только те, которые попадают в заданный диапазон дат.

            if (downloadLogsRequest.DbLogs)
            {
                files = files.Where(f => f.Extension == ".db");
            }
            else
            {
                files = files.Where(f => f.Extension == ".txt");
            }//4-5. Условие для фильтрации по типу файла: Фильтрация файлов по их расширению, в зависимости от того, требуются ли логи базы данных (.db) или текстовые файлы (.txt), основываясь на параметре запроса.

            var filteredFiles = files.ToList(); //    Преобразование результатов фильтрации в список: Конвертация отфильтрованной последовательности файлов в список, чтобы облегчить последующую работу с ними.

            var archiveDirectory = Path.Combine(uploadLogDir,
                $"upload_{downloadLogsRequest.From:yyyy-MM-dd}_{downloadLogsRequest.To:yyyy-MM-dd}");

            if (Directory.Exists(archiveDirectory))
            {
                Directory.Delete(archiveDirectory, true);
            }

            Directory.CreateDirectory(archiveDirectory); //    Создание директории для временного хранения файлов перед архивацией: Определяется путь к директории, где будут временно храниться файлы для архивации, и создается эта директория.

            foreach (var file in filteredFiles)
            {
                File.Copy(file.FullName, Path.Combine(archiveDirectory, file.Name));
            } //    Копирование отфильтрованных файлов в директорию архива: Каждый отфильтрованный файл копируется в ранее созданную временную директорию.

            var fileName = $"log_archive_{_playerStateManager.Object?.NormalizedName}_{downloadLogsRequest.From:yyyy-MM-dd}_{downloadLogsRequest.To:yyyy-MM-dd}.zip"; //    Определение имени файла архива: Формируется имя файла архива, включающее нормализованное имя объекта из PlayerStateManager и диапазон дат.

            var fullArchivePath = Path.Combine(uploadLogDir, fileName); //    Создание полного пути к архиву: Определяется полный путь к файлу архива, соединяя путь к директории загрузки с именем файла архива.

            if (File.Exists(fullArchivePath))
            {
                File.Delete(fullArchivePath); //11-12. Удаление существующего архива, если он есть: Проверяется наличие файла архива по указанному пути, и если он существует, он удаляется.
            }

            _logger.LogInformation("Creating zip archive {FileName} from directory {Directory}", fullArchivePath, archiveDirectory);
            ZipFile.CreateFromDirectory(archiveDirectory, fullArchivePath);
            _logger.LogInformation("Created zip archive"); //Создание ZIP-архива: Логируется начало создания архива, после чего создается ZIP-архив из файлов в директории архива.

            AppendClientInfoToZip(fullArchivePath); //    Добавление информации о клиенте в ZIP-архив

            var archive = await File.ReadAllBytesAsync(fullArchivePath); // Чтение, отправка архива и логирование: Архив читается в массив байтов, который затем конвертируется в строку Base64 и отправляется на сервер. Процесс отправки логируется.
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
            Directory.Delete(archiveDirectory, true); //Очистка: Удаляются временный архив и директория, содержащая файлы для архивации, после успешной отправки.
            //Таким образом, данный метод организует процесс фильтрации, подготовки, архивации и отправки логов на сервер, а также обеспечивает очистку временных файлов после себя.
        }

        private void AppendClientInfoToZip(string fullArchivePath)
        {//Метод AppendClientInfoToZip предназначен для добавления дополнительной информации о клиенте в ZIP-архив с логами. Давайте подробно рассмотрим каждую строку этого метода:
            using var zipArchive = ZipFile.Open(fullArchivePath, ZipArchiveMode.Update); //Используя ZipFile.Open, метод открывает существующий ZIP-архив по указанному пути (fullArchivePath) в режиме ZipArchiveMode.Update, что позволяет добавлять новые файлы в архив.
            var entry = zipArchive.CreateEntry("_object_info.txt"); //Создается новая запись (файл) в архиве с именем _object_info.txt. Этот файл будет содержать информацию о клиенте.
            using var entryStream = entry.Open();
            using var streamWriter = new StreamWriter(entryStream); //Открытие потока для записи в файл: Через созданную запись открывается поток, который затем используется для создания объекта StreamWriter. StreamWriter предназначен для удобной записи текста в файл.
            var logsDirSize = new DirectoryInfo(DefaultLocations.LogsPath).GetDirectorySize().Bytes().Humanize("#.0");
            var dbFile = new DirectoryInfo(DefaultLocations.DatabasePath).GetFiles(DefaultLocations.DatabaseFileName).Single();
            var dbFileSize = dbFile.Length.Bytes().Humanize("#.0");
            var tracksDirSize = new DirectoryInfo(DefaultLocations.TracksPath).GetDirectorySize().Bytes().Humanize("#.0");
            //Получение информации о размерах директорий и файла: Рассчитывается размер директории с логами, файла базы данных и директории с треками. Размеры представляются в удобочитаемом формате с помощью метода Humanize.

            var pathRoot = Path.GetPathRoot(DefaultLocations.TracksPath);
            var freeSpace = DriveInfo.GetDrives().Single(d => d.Name == pathRoot).AvailableFreeSpace.Bytes().Humanize("#.0");
            //Получение свободного места на диске: Определяется корневая директория пути к трекам, а затем рассчитывается объем свободного места на диске в этой корневой директории, также в удобочитаемом формате.
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
            streamWriter.WriteLine($"Tracks directory size:{GetTabsCount()}{tracksDirSize}"); //Запись информации о клиенте в файл: С помощью StreamWriter записывается информация о клиенте, включая текущую дату и время, версию операционной системы, версию клиента, дату запуска клиента, идентификатор запуска клиента, уровень громкости, размеры директорий и свободное место на диске. 
        }
        //Для выравнивания используется метод GetTabsCount, добавляющий определенное количество табуляций.
        private string GetTabsCount(int additionalTabs = 0)
        {//    Метод GetTabsCount для форматирования вывода: Метод GetTabsCount принимает количество дополнительных табуляций (по умолчанию 0) и возвращает строку, состоящую из минимального количества табуляций (2) плюс дополнительные табуляции. Это используется для выравнивания текста в создаваемом файле информации.
            const int minTabsCount = 2;

            return string.Empty.PadLeft(minTabsCount + additionalTabs, '\t');
        }
        //В результате выполнения этого метода в архив с логами добавляется файл _object_info.txt, содержащий детализированную информацию о клиенте, что может быть полезно для диагностики и анализа проблем.
    }
}