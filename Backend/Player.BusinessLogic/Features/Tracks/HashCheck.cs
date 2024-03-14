using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Player.DataAccess;
using Player.Domain;

namespace Player.BusinessLogic.Features.Tracks
{
    public class HashCheck
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
                IQueryable<Track> tracksQuery = query.TrackType switch
                {
                    TrackType.Advert => _context.Adverts.AsQueryable(),
                    TrackType.Music => _context.MusicTracks.AsQueryable(),
                    _ => throw new ArgumentException(nameof(query.TrackType))
                };

                var hash = await tracksQuery.Where(a => a.Id == query.TrackId)
                    .Select(a => a.Hash)
                    .SingleAsync(cancellationToken);

                return new Response
                {
                    TrackIsCorrect = hash == query.Hash
                };

                //Метод принимает запрос Query и возвращает ответ Response. Внутри метода создается LINQ запрос к базе данных для выбора треков в зависимости от их типа (TrackType). Далее из этого набора выбирается трек с идентификатором TrackId, и для него извлекается хеш. Сравнивается хеш из базы данных с хешем, переданным в запросе (query.Hash). Возвращается ответ, содержащий результат сравнения хешей.
            }
        }

        public class Query : IRequest<Response>
        {
            public string TrackType { get; set; }
            public Guid TrackId { get; set; }
            public string Hash { get; set; }
            //Представляет собой запрос на проверку хеша трека. Содержит тип трека (TrackType), идентификатор трека (TrackId) и хеш для проверки (Hash).
        }

        public class Response
        {
            public bool TrackIsCorrect { get; set; }
            //Представляет ответ на запрос. Содержит булево значение TrackIsCorrect, которое указывает, совпадает ли хеш трека с переданным хешем.
        }
    }
    //Этот функционал позволяет клиентской части приложения проверить, соответствует ли хеш трека, хранящегося на сервере, хешу, который имеется у клиента. Это может быть использовано для валидации целостности файлов перед проигрыванием или скачиванием.
}
