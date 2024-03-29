using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using MarketRadio.Player.DataAccess;
using MarketRadio.Player.DataAccess.Domain;
using MarketRadio.Player.Models;
using MarketRadio.Player.Services.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Player.ClientIntegration;
using Refit;

namespace MarketRadio.Player.Services
{
    public class PlaylistService
    //Этот код представляет класс PlaylistService, который является частью более крупной системы управления медиаплеером. PlaylistService отвечает за отправку отчётов о воспроизведении треков (например, музыки или рекламы) на сервер для дальнейшей обработки. Давайте рассмотрим ключевые компоненты и процесс работы этого метода.
    {
        private readonly PlayerContext _context;
        private readonly IPlaylistApi _playlistApi;
        private readonly PlayerStateManager _stateManager;
        private readonly TrackService _trackService;
        private readonly Bus _bus;
        private readonly ILogger<PlaylistService> _logger;

        public PlaylistService(PlayerContext context,
            IPlaylistApi playlistApi,
            PlayerStateManager stateManager,
            TrackService trackService,
            Bus bus,
            ILogger<PlaylistService> logger)
        {
            _context = context;
            _playlistApi = playlistApi;
            _stateManager = stateManager;
            _trackService = trackService;
            _bus = bus;
            _logger = logger;
            //Конструктор инициализирует PlaylistService через внедрение зависимостей, предоставляя ему доступ к контексту приложения (PlayerContext), API для работы с плейлистами (IPlaylistApi), менеджеру состояния плеера (PlayerStateManager), сервису треков (TrackService), шине сообщений (Bus) и логгеру (ILogger<PlaylistService>).
        }

        public async Task<ReportSendingStatus> SendReport(PlaybackResultDto report)
        {//Асинхронный метод SendReport принимает отчёт о воспроизведении report и выполняет следующие действия:
            var trackReport = new TrackReport
            {
                AdvertId = report.TrackId,
                ObjectId = _stateManager.Object!.Id,
                Start = report.StartTime ?? DateTime.MinValue,
                End = report.EndTime ?? DateTime.MinValue,
                //Формирование отчёта о треке (TrackReport): Создаётся новый объект TrackReport, заполняемый данными из report. Важными полями являются AdvertId, ObjectId, Start и End, указывающие на идентификатор трека, идентификатор объекта, время начала и окончания воспроизведения соответственно.
            };

            try
            {
                _context.PlaybackResults.Add(new PlaybackResult
                {
                    PlaylistId = report.PlaylistId,
                    Status = report.Status,
                    AdditionalInfo = report.AdditionalInfo,
                    TrackId = report.TrackId,
                    StartTime = report.StartTime ?? DateTime.Now,
                    EndTime = report.EndTime
                    //Добавление результатов воспроизведения в контекст и сохранение: Добавляет информацию о воспроизведении (PlaybackResult) в контекст для последующего сохранения в базе данных. Здесь важно, что время начала StartTime устанавливается в текущее время, если не указано иное.
                });
                await _context.SaveChangesAsync();

                if (report.Status != PlaybackStatus.Ok)
                {
                    return ReportSendingStatus.TrackErrorNotSent;
                    //Обработка статуса воспроизведения: Если статус воспроизведения отличен от Ok, возвращается статус ReportSendingStatus.TrackErrorNotSent, что означает, что отчёт не отправлен из-за ошибки воспроизведения.
                }

                var trackType = _stateManager.Playlist?.Tracks.FirstOrDefault(t => t.Id == report.TrackId)?.Type;

                if (string.IsNullOrWhiteSpace(trackType))
                {
                    trackType = await _context.Tracks.Where(t => t.Id == report.TrackId).Select(t => t.Type).SingleAsync();
                }

                if (trackType != Track.Advert)
                {
                    return ReportSendingStatus.TrackOkNotSent;//Определение типа трека: Получает тип трека либо из менеджера состояния плеера, либо из контекста, если он не найден в менеджере. Если трек не является рекламой (Track.Advert), возвращается статус ReportSendingStatus.TrackOkNotSent.
                }

                _logger.LogInformation("Sending report to server with report {@Report} started", report);

                await _playlistApi.SendTrackReport(trackReport);//Отправка отчёта на сервер: Логирует начало отправки отчёта, отправляет отчёт о треке на сервер через _playlistApi, логирует успешную отправку и возвращает статус ReportSendingStatus.Sent.

                _logger.LogInformation("Sending report to server with report {@Report} success", report);
                return ReportSendingStatus.Sent;
            }
            catch (ApiException e)
            {//Обработка исключений: В случае возникновения ApiException, логирует неудачу отправки отчёта, добавляет информацию о неудачной отправке (PendingRequest) в контекст для повторной попытки в будущем и возвращает статус ReportSendingStatus.SendingError.
                _logger.LogInformation("Sending report to server with report {@Report} failed", report);
                _context.PendingRequest.Add(new PendingRequest
                {
                    Url = e.Uri!.ToString(),
                    HttpMethod = e.HttpMethod.Method,
                    Body = JsonConvert.SerializeObject(trackReport),
                    Date = DateTime.Now
                });
                await _context.SaveChangesAsync();
                return ReportSendingStatus.SendingError;
                //Ключевым моментом является обработка исключений: если при отправке отчёта возникает ошибка, информация о неудачной отправке сохраняется в базе данных для возможной повторной отправки, что обеспечивает устойчивость системы к временным сбоям связи или другим проблемам, предотвращая потерю данных о воспроизведении.
            }
        }

        public async Task<PlaylistDto> LoadPlaylist(DateTime on)
        { //Метод LoadPlaylist асинхронно загружает плейлист для указанной даты, используя транзакцию базы данных для обеспечения целостности данных в случае возникновения ошибок. Давайте разберем каждую строку этого кода:
            await using var tx = await _context.Database.BeginTransactionAsync(); //Инициируется асинхронная транзакция базы данных. Это гарантирует, что все операции в рамках загрузки плейлиста будут выполнены как единая транзакция.
            try
            {//В блоке try асинхронно вызывается внутренний метод LoadPlaylistInternal, который загружает плейлист для указанной даты. Если метод выполнен успешно, транзакция подтверждается с помощью CommitAsync, и результат возвращается.
                var loadPlaylistInternal = await LoadPlaylistInternal(on);
                await tx.CommitAsync();
                return loadPlaylistInternal;
            }
            catch (Exception e)
            {//В случае исключения логируется ошибка, транзакция откатывается (RollbackAsync), и исключение перебрасывается дальше. Это обеспечивает отмену всех изменений в базе данных, сделанных в рамках транзакции, если произошла ошибка.
                _logger.LogError(e, "");
                await tx.RollbackAsync();
                throw;
            }
        }

        private async Task<PlaylistDto> LoadPlaylistInternal(DateTime on, int retryCount = 0)
        {
            if (_stateManager.Object == null)
            {//Проверяется, задан ли объект в StateManager. Если нет, генерируется исключение InvalidOperationException.
                throw new InvalidOperationException(nameof(_stateManager.Object));
            }

            _logger.LogInformation("Loading playlist on {Date}", on); //Логирование начала загрузки плейлиста на указанную дату.
            var playlistFromDb = await _context.Playlists
                .Include(p => p.PlaylistTracks)
                .ThenInclude(pt => pt.Playlist)
                .Include(p => p.PlaylistTracks)
                .ThenInclude(pt => pt.Track)
                .SingleOrDefaultAsync(p => p.Date == on); ////Асинхронное получение плейлиста из базы данных для заданной даты, включая связанные треки плейлиста и сами треки.ы

            PlaylistWrapper playlistWrapper;
            try
            {//Попытка асинхронного получения плейлиста через API, используя идентификатор объекта и дату.
                playlistWrapper = await _playlistApi.GetPlaylist(_stateManager.Object.Id, on);

                if (!playlistWrapper.Playlist.Tracks.Any())
                {
                    return playlistWrapper.Playlist;//Если в полученном плейлисте нет треков, возвращается пустой плейлист.
                }
            }
            catch (HttpRequestException e)
            {//Обработка различных типов исключений, таких как HttpRequestException (ошибка соединения) и другие исключения. В случае HttpRequestException:    Логируется ошибка.
             // Если количество попыток меньше заданного максимума, производится задержка и рекурсивный вызов метода с измененной датой и увеличенным счетчиком попыток.
                _logger.LogError(e, "Internet error");

                if (retryCount > 10)
                {
                    throw new Exception("Server error or internet connection error");
                }

                await Task.Delay(1000);
                return await LoadPlaylistInternal(on.AddDays(-1), retryCount + 1);
            }
            catch (Exception e)
            {//Если есть плейлист из базы данных, он преобразуется в DTO и возвращается.
                _logger.LogError(e, "");

                if (playlistFromDb != null)
                {
                    var playlistToPlaylistDto = PlaylistToPlaylistDto(playlistFromDb);
                    await _stateManager.ChangePlaylist(playlistToPlaylistDto);
                    return playlistToPlaylistDto;
                }

                if (retryCount > 10)
                {//Если достигнуто максимальное количество попыток, генерируется исключение.
                    throw new Exception("Server error or internet connection error");
                }

                return await LoadPlaylistInternal(on.AddDays(-1), retryCount + 1);
            }

            PlaylistDto playlistDto; //Объявляется переменная для DTO плейлиста.

            if (playlistFromDb == null)
            {
                playlistDto = await CreatePlaylist(playlistWrapper.Playlist, on);
            }
            else
            {
                playlistDto = await UpdatePlaylist(playlistWrapper.Playlist, playlistFromDb, on);
            } //Если плейлиста нет в базе данных, он создается; если есть — обновляется. В обоих случаях результат присваивается playlistDto.

            if (on == DateTime.Today)
            {
                await _stateManager.ChangePlaylist(playlistDto);
            } //Если дата плейлиста — текущий день, производится смена плейлиста в StateManager.

            return playlistDto;
            //Этот метод реализует логику загрузки и обновления плейлиста с обработкой ошибок и попытками восстановления при ошибках связи, обеспечивая устойчивость системы к временным проблемам.
        }

        public async Task DownloadPlaylistTracks(Guid playlistId)
        {//Этот код относится к функциональности загрузки и обновления плейлистов в системе управления медиаконтентом. Давайте рассмотрим детали реализации.
            var tracks = await _context.Playlists
                .Include(p => p.PlaylistTracks)
                .ThenInclude(p => p.Track)
                .Where(p => p.Id == playlistId)
                .SelectMany(p => p.PlaylistTracks)
                .ToListAsync(); //Извлечение треков из базы данных: Сначала из базы данных выбираются все треки указанного плейлиста. Это делается с помощью LINQ-запроса, который включает в себя связанные данные PlaylistTracks и Track, фильтруя плейлисты по идентификатору и преобразуя результат в список с ToListAsync().

            await DownloadPlaylistTracks(tracks); //Вызов асинхронной загрузки треков: Затем вызывается перегруженный метод DownloadPlaylistTracks с извлеченными треками плейлиста.
        }

        public async Task DownloadPlaylistTracks(IEnumerable<PlaylistTrack> tracks)
        {//Этот метод обрабатывает коллекцию треков плейлиста, загружая их и оповещая систему о добавленных треках.
            var orderedTracks = tracks.OrderBy(pt => pt.PlayingDateTime).ToList(); //Сортировка треков по дате воспроизведения: Треки сортируются по времени воспроизведения.
            var adverts = orderedTracks.Where(pt => pt.Track.Type == Track.Advert)
                                                        .Select(pt => pt.Track)
                                                        .DistinctBy(t => t.Id);//Фильтрация и группировка рекламных треков: Выбираются рекламные треки, убираются дубликаты по идентификатору.

            var now = DateTime.Now;
            var actualMusic = orderedTracks
                .Where(pt => pt.PlayingDateTime >= now)
                .Where(pt => pt.Track.Type == Track.Music)
                .Select(pt => pt.Track)
                .DistinctBy(t => t.Id);

            var notActualMusic = orderedTracks
                .Where(pt => pt.PlayingDateTime < now)
                .Where(pt => pt.Track.Type == Track.Music)
                .Select(pt => pt.Track)
                .DistinctBy(t => t.Id);
            //Разделение музыкальных треков на актуальные и неактуальные: Музыкальные треки фильтруются на предмет их актуальности (в зависимости от текущего времени), после чего также убираются дубликаты.
            var formattedTracks = new List<Track>();//Формирование итогового списка треков: Все отфильтрованные и упорядоченные треки объединяются в один список без дубликатов.
            formattedTracks.AddRange(adverts);
            formattedTracks.AddRange(actualMusic);
            formattedTracks.AddRange(notActualMusic);
            formattedTracks = formattedTracks.DistinctBy(t => t.Id).ToList();

            foreach (var track in formattedTracks)
            {
                await _trackService.LoadTrackIfNeeded(track);
                await _bus.TrackAdded(track.Id); //Загрузка треков и оповещение системы: Для каждого трека в итоговом списке вызывается метод LoadTrackIfNeeded для загрузки трека, если это необходимо, и метод TrackAdded шины сообщений для оповещения системы о добавлении трека.
            }
        }

        private async Task<PlaylistDto> UpdatePlaylist(PlaylistDto playlist, Playlist playlistFromDb, DateTime on)
        {//Этот метод обновляет плейлист в базе данных на основе полученных данных.
            await InsertNewTracks(playlist);//Вставка новых треков: Сначала вставляются новые треки в базу данных.

            _context.Remove(playlistFromDb); //Удаление старого плейлиста: Старый плейлист удаляется из базы данных.

            var newPlaylist = new Playlist //Создание и добавление нового плейлиста: Создается новый плейлист с актуальными данными и треками, после чего он добавляется в базу данных.
            {
                Id = playlist.Id,
                Date = on,
                Overloaded = playlist.Overloaded,
                PlaylistTracks = playlist.Tracks.Select(t => new PlaylistTrack
                {
                    PlaylistId = playlist.Id,
                    TrackId = t.Id,
                    PlayingDateTime = t.PlayingDateTime
                }).ToList(),
            };

            _context.Playlists.Add(newPlaylist);
            await _context.SaveChangesAsync(); //Сохранение изменений: Изменения сохраняются в базе данных.
            return playlist; //Возврат обновленного плейлиста: Возвращается DTO обновленного плейлиста.
        }

        private async Task<PlaylistDto> CreatePlaylist(PlaylistDto playlist, DateTime on)
        { //Этот метод создает новый плейлист на основе предоставленных данных и сохраняет его в базе данных.
            await InsertNewTracks(playlist); //Вставка новых треков: Сначала вставляются новые треки в базу данных, как и в методе UpdatePlaylist.
            _context.Playlists.Add(new Playlist
            { //Добавление нового плейлиста в базу данных: Создается новый плейлист с данными и треками, после чего он добавляется в базу данных.
                Id = playlist.Id,
                Date = on,
                Overloaded = playlist.Overloaded,
                PlaylistTracks = playlist.Tracks.Select(t => new PlaylistTrack
                {
                    PlaylistId = playlist.Id,
                    TrackId = t.Id,
                    PlayingDateTime = t.PlayingDateTime
                }).ToList()
            });
            await _context.SaveChangesAsync(); //Сохранение изменений: Изменения сохраняются в базе данных.

            return playlist; //Возврат созданного плейлиста: Возвращается DTO созданного плейлиста.
        }

        private async Task InsertNewTracks(PlaylistDto playlist)
        {//Метод для вставки новых треков в базу данных. Он получает список новых треков, убирает дубликаты и добавляет их в базу данных. Этот метод предполагает использование другого метода GetNewTracks, который извлекает новые треки из DTO плейлиста.
            var newTracks = await GetNewTracks(playlist);
            _context.Tracks.AddRange(newTracks.DistinctBy(t => t.Id));
        }

        private async Task<List<Track>> GetNewTracks(PlaylistDto playlist)
        {//Этот метод асинхронно определяет и извлекает новые треки из предоставленного DTO плейлиста для последующего добавления в базу данных.
            var existedTracks = await _context.Tracks
                .Where(t => playlist.Tracks.Select(pt => pt.Id).Contains(t.Id))
                .ToListAsync(); //Получение существующих треков из базы данных: Сначала из базы данных выбираются треки, которые уже присутствуют в ней и одновременно указаны в DTO плейлиста. Это делается с помощью LINQ-запроса, где playlist.Tracks.Select(pt => pt.Id).Contains(t.Id) проверяет наличие идентификатора трека из плейлиста среди идентификаторов треков в базе данных.

            var newTracksId = playlist.Tracks.Select(t => t.Id)
                .Except(existedTracks.Select(et => et.Id))
                .ToList(); //Определение новых треков: Затем идентификаторы треков из DTO плейлиста, которых нет среди существующих в базе данных, определяются с помощью операции Except. Это позволяет получить список идентификаторов треков, которые необходимо добавить в базу данных.

            return playlist.Tracks.Where(t => newTracksId.Contains(t.Id)).Select(t => new Track
            {//Создание объектов Track для новых треков: Для каждого нового трека (определяемого по его идентификатору) создается новый объект Track, заполненный данными из DTO плейлиста. Это включает в себя идентификатор трека, его хэш, длительность, имя и тип.
                Id = t.Id,
                Hash = t.Hash,
                Length = t.Length,
                Name = t.Name,
                Type = t.Type
            }
            ).ToList(); //Возврат списка новых треков: В конце метод возвращает список объектов Track, представляющих новые треки для добавления в базу данных.
        }

        private PlaylistDto PlaylistToPlaylistDto(Playlist playlist)
        {//Этот метод преобразует сущность Playlist из базы данных в объект PlaylistDto, который может использоваться в приложении для представления данных плейлиста.
            return new PlaylistDto
            {//Создание объекта PlaylistDto: Создается новый объект DTO с заполнением основных полей из сущности Playlist, таких как идентификатор плейлиста, дата, и флаг Overloaded.
                Id = playlist.Id,
                Date = playlist.Date,
                Overloaded = playlist.Overloaded,
                Tracks = playlist.PlaylistTracks.Select(pt => new TrackDto
                {
                    Id = pt.TrackId,
                    Name = pt.Track.Name,
                    Hash = pt.Track.Hash,
                    Length = pt.Track.Length,
                    Type = pt.Track.Type,
                    PlayingDateTime = pt.PlayingDateTime
                }).ToList()
                //Преобразование треков плейлиста: playlist.PlaylistTracks.Select(pt => new TrackDto {...}) преобразует связанные с плейлистом треки (PlaylistTracks) в DTO треков (TrackDto). Для каждого трека плейлиста заполняются такие поля, как идентификатор, имя, хэш, длительность, тип и дата/время воспроизведения. Это позволяет получить полную информацию о треках в плейлисте.
            };//Возврат DTO плейлиста: В конце метод возвращает сформированный объект PlaylistDto, содержащий всю необходимую информацию о плейлисте и его треках для дальнейшего использования в приложении.
        }
       //методы вместе обеспечивают механизмы для эффективного управления данными плейлистов в приложении, включая определение и добавление новых треков в базу данных и преобразование данных плейлистов для использования в различных частях приложения.
    }
}