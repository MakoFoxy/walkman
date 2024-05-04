using System;
using System.Collections.Generic;
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
using System.IO;
using System.IO.Compression;

namespace Player.BusinessLogic.Features.Objects
{
    public class PlaningMediaPlanReport
    {
        public class Handler : IRequestHandler<Query, Response>
        {
            private readonly PlayerContext _context;
            private readonly IMediaPlanExcelReportCreator _mediaPlanExcelReportCreator;
            private List<TrackModel> adverts;


            public Handler(PlayerContext context, IMediaPlanExcelReportCreator mediaPlanExcelReportCreator)
            {
                _context = context;
                _mediaPlanExcelReportCreator = mediaPlanExcelReportCreator;
                // Конструктор инициализирует контекст данных и сервис создания отчетов Excel, которые передаются как зависимости.
            }

            public async Task<Response> Handle(Query query, CancellationToken cancellationToken)
            {
                var playlists = await _context.Playlists
                    .Include(p => p.Object)
                    .Include(p => p.MusicTracks).ThenInclude(mtp => mtp.MusicTrack).ThenInclude(mt => mt.TrackType)
                    .Include(p => p.Aderts).ThenInclude(ap => ap.Advert).ThenInclude(a => a.TrackType)
                    .Where(p => p.Object.Id == query.ObjectId && p.PlayingDate == query.Date)
                    .ToListAsync(cancellationToken);

                if (!playlists.Any())
                {
                    return new Response { IsSuccess = false };
                }

                List<byte[]> files = new List<byte[]>();
                string fileName = "";

                foreach (var playlist in playlists)
                {
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
                        PlayDate = playlist.PlayingDate,
                    };
                    adverts = playlist.Aderts.Select(m => new TrackModel
                    {
                        StartTime = m.PlayingDateTime,
                        Name = m.Advert.Name,
                        TypeCode = TrackType.Advert,
                        Length = m.Advert.Length,
                    }).ToList();

                    model.Tracks.AddRange(adverts);
                    model.Tracks = model.Tracks.OrderBy(t => t.StartTime).ToList();

                    var fileContent = _mediaPlanExcelReportCreator.Create(model);
                    files.Add(fileContent);  // Сохраняем содержимое файла для каждого отчета
                    fileName = $"{query.Date.ToString("d", CultureInfo.CurrentCulture)}_{playlist.Object.Name}_{DateTime.Now}.xlsx";  // Пример формирования имени файла
                }

                if (files.Count == 1)
                {
                    // Если создан только один файл, возвращаем его
                    return new Response { File = files[0], FileName = fileName, IsSuccess = true };
                }
                else
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                        {
                            for (int i = 0; i < files.Count; i++)
                            {
                                var zipEntry = archive.CreateEntry($"Playlist_{i + 1}.xlsx", CompressionLevel.Fastest);
                                using (var zipStream = zipEntry.Open())
                                {
                                    zipStream.Write(files[i], 0, files[i].Length);
                                }
                            }
                        }

                        return new Response
                        {
                            File = memoryStream.ToArray(),
                            FileName = $"Playlists_{query.Date.ToString("yyyyMMdd")}.zip",
                            IsSuccess = true
                        };
                    }
                }
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
