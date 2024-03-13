using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Player.DataAccess;
using Player.Domain;
using Player.DTOs;
using Player.Services.Abstractions;

namespace Player.BusinessLogic.Features.Objects
{
    public class PlaningMediaPlanReport
    {
        public class Handler : IRequestHandler<Query, Response>
        {
            private readonly PlayerContext _context;
            private readonly IMediaPlanExcelReportCreator _mediaPlanExcelReportCreator;

            public Handler(PlayerContext context, IMediaPlanExcelReportCreator mediaPlanExcelReportCreator)
            {
                _context = context;
                _mediaPlanExcelReportCreator = mediaPlanExcelReportCreator;
                // Конструктор инициализирует контекст данных и сервис создания отчетов Excel, которые передаются как зависимости.
            }

            public async Task<Response> Handle(Query query, CancellationToken cancellationToken)
            {
                var playlist = await _context.Playlists
                    .Include(p => p.Object)
                    .Where(p => p.Object.Id == query.ObjectId && p.PlayingDate == query.Date)
                    .SingleOrDefaultAsync(cancellationToken);

                if (playlist == null)
                {
                    return new Response();
                }

                await _context.Entry(playlist).Collection(p => p.MusicTracks)
                    .Query()
                    .Include(mtp => mtp.MusicTrack)
                    .ThenInclude(mt => mt.TrackType)
                    .LoadAsync();

                await _context.Entry(playlist).Collection(p => p.Aderts)
                    .Query()
                    .Include(ap => ap.Advert)
                    .ThenInclude(mt => mt.TrackType)
                    .LoadAsync();

                var model = new PlaylistModel
                {
                    Object = new ObjectModel
                    {
                        Id = query.ObjectId,
                        Name = playlist.Object.Name,
                        WorkTime = playlist.Object.WorkTime,
                        EndTime = playlist.Object.EndTime,
                    },
                    Tracks = playlist.MusicTracks.OrderBy(mt => mt.PlayingDateTime).Select(m => new TrackModel
                    {
                        Name = m.MusicTrack.Name,
                        TypeCode = m.MusicTrack.TrackType.Code,
                        Length = m.MusicTrack.Length,
                        StartTime = m.PlayingDateTime,
                    }).ToList(),
                    PlayDate = playlist.PlayingDate
                };

                if (!model.Tracks.Any())
                {
                    return new Response();
                }

                var adverts = playlist.Aderts.Select(m => new TrackModel
                {
                    StartTime = m.PlayingDateTime,
                    Name = m.Advert.Name,
                    TypeCode = TrackType.Advert,
                    Length = m.Advert.Length,
                }).ToList();

                model.Tracks.AddRange(adverts);
                model.Tracks = model.Tracks.OrderBy(t => t.StartTime).ToList();

                return new Response
                {
                    File = _mediaPlanExcelReportCreator.Create(model),
                    FileName = $"{query.Date.ToString("d", CultureInfo.CurrentCulture)}_{playlist.Object.Name}_{DateTime.Now}.xlsx",
                    IsSuccess = true,
                };
                //             Процесс создания отчёта:

                // Запускается запрос на создание отчета с заданными параметрами.
                // Выполняется выборка данных о плейлисте из базы данных.
                // Если плейлист найден, загружаются дополнительные данные о треках и рекламных блоках.
                // На основе этих данных формируется модель плейлиста для отчета, включая информацию о треках и времени их воспроизведения.
                // Сервис IMediaPlanExcelReportCreator создает отчет Excel на основе сформированной модели.
                // Возвращается ответ, содержащий сгенерированный файл, имя файла и статус операции.
                // Этот метод обрабатывает запрос на создание отчёта медиаплана. Он асинхронно извлекает информацию о плейлисте для заданного объекта и даты. Загружает связанные с плейлистом музыкальные треки и рекламные блоки. Затем формирует модель данных для отчета и использует сервис IMediaPlanExcelReportCreator для создания файла Excel.
            }
        }

        public class Response
        {
            public byte[] File { get; set; }
            public string FileName { get; set; }
            public bool IsSuccess { get; set; }
        }

        public class Query : IRequest<Response>
        {
            public Guid ObjectId { get; set; }
            public DateTime Date { get; set; }
        }
        //         Response содержит результат создания отчета: сам файл в виде массива байтов (File), имя файла (FileName) и флаг успешного выполнения операции (IsSuccess).
        // Query представляет запрос с параметрами для создания отчета: идентификатор объекта (ObjectId) и дата (Date).
    }
    // Код обеспечивает возможность генерации детальных отчетов о расписании воспроизведения музыкальных и рекламных треков в конкретном объекте на заданную дату, что важно для планирования и анализа медиаконтента.
}
