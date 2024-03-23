using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Player.BusinessLogic.Features.Playlists;
using Player.DataAccess;
using Player.Domain;
using Player.DTOs;
using Player.Services;
using Player.Services.Abstractions;
using Player.Services.PlaylistServices;

namespace Player.Publisher.Controllers.v1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class PlaylistController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly PlayerContext _context;
        private readonly IPlaylistGenerator _playlistGenerator;
        private readonly IMediaPlanExcelReportCreator _mediaPlanExcelReportCreator;

        public PlaylistController(IMediator mediator,
            PlayerContext context,
            IPlaylistGenerator playlistGenerator,
            IMediaPlanExcelReportCreator mediaPlanExcelReportCreator)
        {
            //6-16. Конструктор: Здесь контроллер инициализируется с помощью необходимых сервисов: обработчика команд (Mediator), контекста базы данных (PlayerContext), генератора плейлистов и создателя отчетов по медиаплану.

            _mediator = mediator;
            _context = context;
            _playlistGenerator = playlistGenerator;
            _mediaPlanExcelReportCreator = mediaPlanExcelReportCreator;
        }

        [HttpGet]
        [Produces("application/json")]
        public async Task<ActionResult<GetPlaylistForObject.Response>> Get([FromQuery] GetPlaylistForObject.Query query, CancellationToken token) => Ok(await _mediator.Send(query, token));
        //18-19. GET endpoint для получения плейлистов: Возвращает плейлист для заданного объекта, используя MediatR для отправки запроса и получения ответа.
        //Таким образом, MediatR в данном контексте работает как посредник между контроллером (который получает HTTP запрос от клиента) и обработчиком (который выполняет бизнес-логику запроса).
        [HttpPost("run-generator")]
        public async Task<IActionResult> RunGenerator(Guid objectId, DateTime date, bool commit, bool onEmpty)
        {
            //В методе RunGenerator: если параметр commit установлен в true, новый плейлист сохраняется в базе данных, и изменения фиксируются (коммит транзакции). Если commit равен false, производится откат транзакции, и изменения не сохраняются.
            //В RunGenerator: возвращается отладочная информация, созданная во время генерации плейлиста.
            var dbContextTransaction = _context.Database.BeginTransaction();

            var objectInfo = _context.Objects.Single(o => o.Id == objectId);

            if (onEmpty)
            {
                var playlist = await _context.Playlists
                    .SingleOrDefaultAsync(p => p.Object.Id == objectId && p.PlayingDate == date);

                if (playlist != null)
                {
                    var deleted = await _context.Database.ExecuteSqlRawAsync($"select delete_playlist('{playlist.Id}')");

                    playlist = null;
                    playlist = await _context.Playlists
                        .SingleOrDefaultAsync(p => p.Object.Id == objectId && p.PlayingDate == date);
                }
            }

            var result = await _playlistGenerator.Generate(objectInfo, date);

            if (commit)
            {
                if (_context.Entry(result.Playlist).State == EntityState.Detached)
                {
                    _context.Playlists.Add(result.Playlist);
                }
                _context.SaveChanges();
                dbContextTransaction.Commit();
            }
            else
            {
                dbContextTransaction.Rollback();
            }

            return Ok(string.Join(Environment.NewLine, result.DebugInfo));
            //Метод RunGenerator:

            // Начало транзакции: Создается новая транзакция базы данных, что позволяет управлять последующими операциями как единым блоком.

            // Получение информации об объекте: Извлекается информация об объекте (например, о местоположении или устройстве) из базы данных по его идентификатору.

            // Условие на пустоту: Если флаг onEmpty установлен в true, ищется существующий плейлист для данного объекта на указанную дату. Если такой плейлист найден, он удаляется из базы данных, что обеспечивает возможность создать новый плейлист "с нуля".

            // Генерация плейлиста: Используя сервис генерации плейлистов, создается новый плейлист для объекта на заданную дату.

            // Фиксация изменений: Если флаг commit установлен в true, новый плейлист добавляется в базу данных и транзакция фиксируется (подтверждается). Если флаг commit не установлен, изменения откатываются.

            // Возвращение отладочной информации: Возвращает отладочную информацию о созданном плейлисте клиенту.
        }

        [HttpPost("run-temp-media-plan")]
        public async Task<IActionResult> RunTempMediaPlan(Guid objectId, DateTime date, bool onEmpty)
        {
            //В методе RunTempMediaPlan: после генерации плейлиста и создания отчета транзакция всегда откатывается. Это означает, что генерируемый плейлист не сохраняется в базе данных; он используется только для создания временного медиаплана.
            //В RunTempMediaPlan: возвращается сгенерированный Excel-файл медиаплана для скачивания.
            var dbContextTransaction = _context.Database.BeginTransaction();

            var objectInfo = _context.Objects.Single(o => o.Id == objectId);

            if (onEmpty)
            {
                var playlist = await _context.Playlists
                    .SingleOrDefaultAsync(p => p.Object.Id == objectId && p.PlayingDate == date);

                if (playlist != null)
                {
                    var deleted = await _context.Database.ExecuteSqlRawAsync($"select delete_playlist('{playlist.Id}')");

                    playlist = null;
                    playlist = await _context.Playlists
                        .SingleOrDefaultAsync(p => p.Object.Id == objectId && p.PlayingDate == date);
                }
            }

            var result = await _playlistGenerator.Generate(objectInfo, date);

            var model = new PlaylistModel
            {
                Object = new ObjectModel
                {
                    Id = objectInfo.Id,
                    Name = result.Playlist.Object.Name,
                    WorkTime = result.Playlist.Object.WorkTime,
                    EndTime = result.Playlist.Object.EndTime,
                },
                Tracks = result.Playlist.MusicTracks.OrderBy(mt => mt.PlayingDateTime).Select(m => new TrackModel
                {
                    Name = m.MusicTrack.Name,
                    TypeCode = m.MusicTrack.TrackType.Code,
                    Length = m.MusicTrack.Length,
                    StartTime = m.PlayingDateTime,
                }).ToList(),
                PlayDate = result.Playlist.PlayingDate
            };

            if (!model.Tracks.Any())
            {
                return NotFound();
            }

            var adverts = result.Playlist.Aderts.Select(m => new TrackModel
            {
                StartTime = m.PlayingDateTime,
                Name = m.Advert.Name,
                TypeCode = TrackType.Advert,
                Length = m.Advert.Length,
            }).ToList();

            model.Tracks.AddRange(adverts);
            model.Tracks = model.Tracks.OrderBy(t => t.StartTime).ToList();

            var report = _mediaPlanExcelReportCreator.Create(model);

            dbContextTransaction.Rollback();

            return File(report, "application/octet-stream", $"{objectInfo.Name}.xlsx");

            //             Начало транзакции: Аналогично методу RunGenerator, начинается новая транзакция базы данных.

            // Получение информации об объекте: Аналогично методу RunGenerator, извлекается информация об объекте из базы данных.

            // Условие на пустоту и удаление существующего плейлиста: Аналогично методу RunGenerator, если установлен флаг onEmpty, проверяется наличие существующего плейлиста и его возможное удаление.

            // Генерация плейлиста: Генерируется новый плейлист для объекта на заданную дату.

            // Создание модели плейлиста: Создается модель плейлиста, включая информацию об объекте и треках плейлиста.

            // Проверка на наличие треков: Если в плейлисте нет треков, возвращается HTTP статус NotFound.

            // Сборка отчета: С помощью сервиса создания отчетов формируется Excel-отчет по временному медиаплану.

            // Откат транзакции: Транзакция откатывается, поскольку это временный медиаплан, и изменения в базу данных сохранять не нужно.

            // Возврат файла отчета: Возвращается файл Excel-отчета клиенту для скачивания.
        }
    }
}

// Метод RunGenerator:

// Этот метод предназначен для генерации плейлиста для определенного объекта (objectId) на указанную дату (date). Параметр commit указывает, следует ли сохранить результаты в базе данных.

//     Начинается с начала транзакции для обеспечения целостности данных (dbContextTransaction = _context.Database.BeginTransaction();).

//     Загружает информацию об объекте по objectId.

//     Если параметр onEmpty установлен в true, метод проверяет, существует ли уже плейлист для данного объекта и даты. Если плейлист существует, он удаляется, чтобы можно было создать новый. Это гарантирует, что генерируется "свежий" плейлист, если пользователь хочет перегенерировать его для той же даты.

//     Вызывает сервис _playlistGenerator для создания нового плейлиста.

//     Если commit равен true, новый плейлист добавляется в контекст базы данных и сохраняется, а транзакция подтверждается. Если commit равен false, транзакция откатывается, и изменения не сохраняются в базе данных.

//     Возвращает отладочную информацию о генерации плейлиста.

// Метод RunTempMediaPlan:

// Этот метод используется для создания временного медиаплана (отчета) для плейлиста определенного объекта на указанную дату.

//     Аналогично начинает с транзакции для управления изменениями данных.

//     Загружает информацию об объекте и проверяет существование плейлиста для данного объекта и даты, удаляя его при необходимости, если onEmpty установлен в true.

//     Генерирует новый плейлист для объекта на указанную дату.

//     Создает модель плейлиста для отчета, включая информацию об объекте, дате плейлиста и списка треков.

//     Если в сгенерированном плейлисте нет треков, возвращает статус NotFound.

//     Генерирует отчет в формате Excel на основе сгенерированных данных плейлиста с помощью сервиса _mediaPlanExcelReportCreator.

//     Откатывает транзакцию, так как это временная операция, и возвращает файл отчета.

// В обоих методах параметр onEmpty контролирует, следует ли удалять существующий плейлист перед генерацией нового. RunGenerator используется для создания или обновления плейлиста в базе данных, в то время как RunTempMediaPlan создает временный отчет (медиаплан) для плейлиста без его сохранения в базе данных.