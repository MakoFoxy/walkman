using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Player.DataAccess;
using Player.Domain;

namespace Player.BusinessLogic.Features.Tracks
{
    public class TracksInObjectList
    {
        public class Handler : IRequestHandler<Query, List<TrackInObject>>
        //Это класс, который обрабатывает запросы на получение списка треков в объекте. Он реализует интерфейс IRequestHandler<Query, List<TrackInObject>>, что означает, что он принимает запрос Query и возвращает список треков List<TrackInObject>.
        {
            private readonly PlayerContext _context;

            public Handler(PlayerContext context)
            {
                _context = context;
            }

            public async Task<List<TrackInObject>> Handle(Query query, CancellationToken cancellationToken)
            {
                var musicTracks = await _context.Playlists
                    .Where(p => p.Object.Id == query.ObjectId && p.PlayingDate == query.Date.Date)
                    .SelectMany(p => p.MusicTracks)
                    .Select(mt => new TrackInObject
                    {
                        Id = mt.MusicTrack.Id,
                        Type = TrackType.Music,
                        Name = mt.MusicTrack.Name,
                        PlayingDateTime = mt.PlayingDateTime,
                    })
                    .ToListAsync(cancellationToken);

                var adverts = await _context.Playlists
                    .Where(p => p.Object.Id == query.ObjectId && p.PlayingDate == query.Date.Date)
                    .SelectMany(p => p.Aderts)
                    .Select(mt => new TrackInObject
                    {
                        Id = mt.Advert.Id,
                        Type = TrackType.Advert,
                        Name = mt.Advert.Name,
                        PlayingDateTime = mt.PlayingDateTime,
                    })
                    .ToListAsync(cancellationToken);

                var trackInPlaylistDtos = new List<TrackInObject>();
                trackInPlaylistDtos.AddRange(musicTracks);
                trackInPlaylistDtos.AddRange(adverts);

                return trackInPlaylistDtos;
                //Метод, который выполняет логику обработки запроса. В этом случае он выполняет запросы к базе данных через Entity Framework для извлечения музыкальных треков (musicTracks) и рекламных треков (adverts), которые запланированы к воспроизведению в определенной дате (query.Date) и принадлежат определенному объекту (query.ObjectId). Затем он объединяет оба списка в один и возвращает его.
                //    При получении запроса Query с идентификатором объекта и датой, обработчик Handler запросит из базы данных два списка: музыкальные треки и рекламные треки, запланированные к воспроизведению в этот день в данном объекте.
                //             Полученные списки треков объединяются в один список trackInPlaylistDtos.
                // Возвращается этот объединенный список как результат выполнения запроса.
            }
        }

        public class Query : IRequest<List<TrackInObject>>
        {
            public Guid ObjectId { get; set; }
            public DateTime Date { get; set; }
            //Класс запроса, который содержит данные, необходимые для выполнения запроса: ObjectId (идентификатор объекта) и Date (дата плейлиста).
        }

        public class TrackInObject
        {
            public Guid Id { get; set; }
            public string Type { get; set; }
            public string Name { get; set; }
            public DateTime PlayingDateTime { get; set; }
            //DTO (Data Transfer Object), который используется для представления информации о треке в контексте объекта. Содержит поля, такие как Id, Type, Name и PlayingDateTime, которые описывают уникальные характеристики каждого трека в плейлисте.
        }
    }
    //Этот функционал используется, например, для отображения списка треков, запланированных к воспроизведению в определенной локации на конкретную дату, что может быть полезно для управления медиа-контентом в коммерческих или общественных пространствах.
}