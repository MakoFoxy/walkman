using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using MarketRadio.Player.DataAccess;
using MarketRadio.Player.DataAccess.Domain;
using MarketRadio.Player.Services.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MarketRadio.Player.Services
{
    public class TrackService
    //Этот код описывает TrackService в пространстве имен MarketRadio.Player.Services, предназначенный для управления загрузкой треков. Сервис использует API для треков (ITrackApi), контекст базы данных (PlayerContext) и логгер (ILogger<TrackService>) для выполнения своих задач.
    {
        private readonly ITrackApi _trackApi;
        private readonly PlayerContext _context;
        private readonly ILogger<TrackService> _logger;

        public TrackService(ITrackApi trackApi, PlayerContext context, ILogger<TrackService> logger)
        {
            _trackApi = trackApi;
            _context = context;
            _logger = logger;
            //    Инициализирует сервис, сохраняя предоставленные экземпляры ITrackApi, PlayerContext и ILogger<TrackService> в приватных полях для дальнейшего использования в методах класса.
        }

        public async Task LoadTrackIfNeeded(Guid trackId)
        {
            var track = await _context.Tracks.SingleAsync(t => t.Id == trackId);
            await LoadTrackIfNeeded(track);//    Асинхронно проверяет, загружен ли уже трек с указанным идентификатором (trackId), и если нет, загружает его. Метод сначала извлекает трек из базы данных, а затем делегирует загрузку трека перегруженному методу LoadTrackIfNeeded(Track track).
        }

        public async Task LoadTrackIfNeeded(Track track)
        {//Основной метод, асинхронно проверяющий и загружающий трек, если это необходимо. Процесс включает в себя следующие шаги:
            if (!File.Exists(track.FilePath))//Проверка существования файла трека: Если файл трека не существует на диске, логгируется предупреждение, и трек загружается с использованием метода LoadTrack.
            {
                _logger.LogWarning("Track {@Track} not exists", track);
                await LoadTrack(track.Id, track.Type);
                return;
            }

            var currentHash = await CalculateTrackHash(track.FilePath);//Проверка хеш-суммы файла: Если файл существует, но его хеш-сумма не соответствует ожидаемой, логгируется предупреждение, и трек также перезагружается.

            if (currentHash != track.Hash)
            {
                _logger.LogWarning("Track hash not equals {@Track}", track);
                await LoadTrack(track.Id, track.Type);
            }
            else
            {//В противном случае, если файл существует и хеш-сумма совпадает, логгируется информационное сообщение о том, что трек в порядке.
                _logger.LogInformation("Track {@Track} ok", track);
            }
        }

        public async Task LoadTrack(Guid id, string trackType)
        {//Отвечает за асинхронную загрузку трека по идентификатору и типу трека. Загрузка пытается выполниться в цикле до тех пор, пока не будет успешно завершена, обрабатывая возможные исключения, например, связанные с потерей интернет-соединения:
            var trackIsLoaded = false;

            while (!trackIsLoaded)
            {
                try
                {
                    await LoadTrackInternal(id, trackType);
                    trackIsLoaded = true;

                }
                catch (Exception e)
                {//    В случае возникновения исключения, метод ждёт 10 секунд перед следующей попыткой загрузки, логгируя возникшую ошибку и предупреждение о времени ожидания до следующей попытки.
                    var waitTime = TimeSpan.FromSeconds(10);
                    _logger.LogError(e, "");
                    _logger.LogWarning(
                        "Track with id {Id} not loaded because internet connection was gone next attempt after {WaitTime}",
                        id, waitTime);
                    await Task.Delay(waitTime);
                }
            }
        }

        private async Task LoadTrackInternal(Guid id, string trackType)
        {
            _logger.LogInformation("Downloading track {TrackId} ...", id);

            var httpContent = await _trackApi.DownloadTrack(id, trackType);//        Загружает содержимое трека асинхронно через API, используя идентификатор и тип трека.
            var trackFileBytes = await httpContent.ReadAsByteArrayAsync();//        Преобразует загруженное содержимое трека в массив байтов.

            var track = await _context.Tracks.SingleAsync(t => t.Id == id);//    Извлекает информацию о треке из базы данных по идентификатору трека.

            var newHash = CalculateTrackHash(trackFileBytes); //    Вычисляет хеш-сумму для загруженного файла трека.
            if (track.Hash != newHash) //    Сравнивает новый хеш с сохраненным в базе данных. Если они не совпадают, логирует предупреждение.
            {
                _logger.LogWarning("Track {TrackId} with hash {Hash} not equal to downloaded track hash {NewHash}", track.Id,
                    track.Hash, newHash);
            }

            if (File.Exists(track.FilePath)) //    Удаляет существующий файл трека, если он уже существует.
            {
                File.Delete(track.FilePath);
            }

            await using (var file = File.Create(track.FilePath)) //    Создает новый файл трека и записывает в него загруженный массив байтов.
            {
                file.Write(trackFileBytes);
            }

            await _context.SaveChangesAsync(); //    Сохраняет изменения в базе данных.
            _logger.LogInformation("Downloaded track {TrackId}", id); //    Логирует успешное завершение загрузки трека.
        }

        public async Task<bool> CheckTrack(Guid id, string trackType, string hash)
        {
            var trackIsCorrect = await _trackApi.CheckTrack(id, trackType, hash);
            return trackIsCorrect.TrackIsCorrect; //    Проверяет корректность трека через API, используя его идентификатор, тип и хеш-сумму. Возвращает true, если трек корректен.
        }

        public async Task<bool> CheckTrack(Guid id, string trackType)
        {
            var trackPath = await _context.Tracks
                                            .Where(t => t.Id == id).Select(t => t.FilePath)
                                            .SingleAsync();

            return await CheckTrack(id, trackType, await CalculateTrackHash(trackPath)); //    Получает путь к файлу трека из базы данных, вычисляет его хеш и проверяет корректность трека через предыдущий метод CheckTrack.
        }

        public async Task<bool> CheckTrack(Guid id)
        {
            var trackType = await _context.Tracks.Where(t => t.Id == id)
                                                        .Select(t => t.Type)
                                                        .SingleAsync();
            return await CheckTrack(id, trackType); //    Получает тип трека из базы данных и делегирует проверку корректности предыдущему методу CheckTrack.
        }

        public string CalculateTrackHash(byte[] trackFileBytes)
        { //    Вычисляет хеш-сумму массива байтов файла трека, используя алгоритм SHA256, и возвращает полученное значение в виде строки.
            var hashString = new StringBuilder();

            using (var sha256 = SHA256.Create())
            {
                var hash = sha256.ComputeHash(trackFileBytes);

                foreach (var h in hash)
                {
                    hashString.Append($"{h:x2}");
                }
            }

            return hashString.ToString();
        }

        public async Task<string> CalculateTrackHash(string trackFilePath)
        {//    Асинхронно читает файл трека, преобразует его содержимое в массив байтов и вычисляет хеш-сумму через метод CalculateTrackHash(byte[] trackFileBytes). Если файл не существует, возвращает пустую строку.
            if (string.IsNullOrWhiteSpace(trackFilePath) || !File.Exists(trackFilePath))
            {
                return string.Empty;
            }

            var trackFileBytes = await File.ReadAllBytesAsync(trackFilePath);
            return CalculateTrackHash(trackFileBytes);
        }
        //Этот набор методов обеспечивает комплексное управление загрузкой и проверкой треков, а также расчетом их хеш-сумм, что является важной частью работы с медиаконтентом для обеспечения его целостности и актуальности.
    }
}