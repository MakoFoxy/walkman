using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Player.ClientIntegration;
using Player.DataAccess;
using Player.Domain;

namespace Player.BusinessLogic.Features.Playlists
{
    public class GetPlaylistForObject
    {
        public class Handler : IRequestHandler<Query, Response>
        {
            private readonly PlayerContext _context;

            public Handler(PlayerContext context)
            {
                _context = context;
            }

            public async Task<Response> Handle(Query query, CancellationToken cancellationToken)
            {
                query.Date = query.Date.Date;

                var playlist = await _context.Playlists.AsQueryable()
                    .Where(p => p.Object.Id == query.ObjectId)
                    .Where(p => p.PlayingDate == query.Date)
                    .Select(p => new PlaylistInternal
                    {
                        Id = p.Id,
                        Date = p.PlayingDate,
                        MusicTracks = p.MusicTracks.Select(mt => new TrackDto
                        {
                            Id = mt.MusicTrack.Id,
                            Type = TrackType.Music,
                            Name = mt.MusicTrack.Name + mt.MusicTrack.Extension,
                            Hash = mt.MusicTrack.Hash,
                            Length = mt.MusicTrack.Length.TotalSeconds,
                            PlayingDateTime = mt.PlayingDateTime,
                        }),
                        Adverts = p.Aderts.Select(a => new TrackDto
                        {
                            Id = a.Advert.Id,
                            Type = TrackType.Advert,
                            PlayingDateTime = a.PlayingDateTime,
                            Name = a.Advert.Name + a.Advert.Extension,
                            Hash = a.Advert.Hash,
                            Length = a.Advert.Length.TotalSeconds
                        }),
                        Overloaded = p.Overloaded
                    }
                    ).SingleOrDefaultAsync(cancellationToken);

                if (playlist == null)
                {
                    return new Response
                    {
                        Playlist = new Playlist
                        {
                            Date = query.Date,
                        },
                    };
                }

                var response = new Response
                {
                    Playlist = new Playlist
                    {
                        Id = playlist.Id,
                        Overloaded = playlist.Overloaded,
                        Date = query.Date,
                    }
                };

                response.Playlist.Tracks.AddRange(playlist.Adverts);
                response.Playlist.Tracks.AddRange(playlist.MusicTracks);

                return response;
                //                 Метод Handle асинхронно обрабатывает запрос, получая данные о плейлисте для заданного объекта и даты. Если плейлист существует, метод извлекает информацию о музыкальных треках и рекламе в плейлисте, а также статус перегрузки (Overloaded).
                // Классы Response, Query, Playlist, PlaylistInternal и TrackDto:

                //     Response содержит экземпляр класса Playlist, который включает в себя детали плейлиста.
                //     Query представляет запрос на получение плейлиста, содержащий идентификатор объекта и дату.
                //     Playlist и PlaylistInternal используются для хранения информации о плейлисте, включая треки и флаг перегрузки.
                //     TrackDto представляет собой модель данных для треков в плейлисте, содержащую информацию как о музыкальных треках, так и о рекламных блоках.

                // Логика обработки:

                //     Дата запроса преобразуется к началу дня, чтобы сравнение дат было корректным.
                //     Производится запрос к базе данных для поиска плейлиста для заданного объекта и даты.
                //     Если плейлист найден, извлекается информация о треках и рекламных блоках, и они добавляются в ответ.
                //     Если плейлист не найден, возвращается пустой ответ с указанием даты.
            }
            // Код обеспечивает возможность получения полного состава плейлиста для объекта на определенную дату, что важно для управления контентом и рекламой в различных точках воспроизведения.
        }

        private class PlaylistInternal
        {
            public Guid Id { get; set; }
            public IEnumerable<TrackDto> MusicTracks { get; set; }
            public IEnumerable<TrackDto> Adverts { get; set; }
            public bool Overloaded { get; set; }
            public DateTime Date { get; set; }
        }

        public class Playlist
        {
            public Guid Id { get; set; } = Guid.Empty;
            public List<TrackDto> Tracks { get; set; } = new List<TrackDto>();
            public bool Overloaded { get; set; }
            public DateTime Date { get; set; }
        }

        public class Response
        {
            public Playlist Playlist { get; set; }
        }

        public class Query : IRequest<Response>
        {
            public Guid ObjectId { get; set; }
            public DateTime Date { get; set; }
        }
    }
}
