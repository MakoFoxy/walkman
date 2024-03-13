using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Player.DataAccess;
using Player.DTOs;
using Player.Services.Abstractions;

namespace Player.BusinessLogic.Features.Adverts
{
    public class CheckPossibility //Определен как контейнер для вложенных классов, которые выполняют логику проверки возможности размещения рекламы в плейлистах.
    {
        public class Handler : IRequestHandler<Query, Response> //Этот класс реализует интерфейс IRequestHandler из MediatR, что делает его обработчиком запросов. Он предназначен для обработки запросов типа Query и возвращает ответ типа Response.
        {
            private readonly PlayerContext _context;
            private readonly IPlaylistLoadingCalculator _playlistLoadingCalculator;

            public Handler(
                PlayerContext context,
                IPlaylistLoadingCalculator playlistLoadingCalculator)
            {
                _context = context;
                _playlistLoadingCalculator = playlistLoadingCalculator;
            }

            public async Task<Response> Handle(Query query, CancellationToken cancellationToken)
            {
                var playlists = await _context.Playlists
                    .Include(p => p.Object)
                    .Include(p => p.Aderts)
                    .ThenInclude(a => a.Advert)
                    .Where(p => p.PlayingDate >= query.DateBegin && p.PlayingDate <= query.DateEnd)
                    .Where(p => query.Objects.Contains(p.Object.Id))
                    .ToListAsync(cancellationToken);

                //    Извлекает плейлисты из базы данных для заданного периода (DateBegin до DateEnd) и списка объектов (Objects).
                //Для каждого плейлиста рассчитывает переполнение(количество лишних повторений рекламы), используя сервис _playlistLoadingCalculator.Это делается на основе длины рекламного блока(AdvertLength) и количества его повторений(RepeatCount).
                //Если для плейлиста вычисляется переполнение(overflowCount > 0), информация о проблеме добавляется в ответ(Response).

                var advertLength = TimeSpan.FromSeconds(query.AdvertLength);

                //Response предназначен для хранения ответа обработчика, который включает в себя список проблем (Problems), связанных с переполнением плейлистов.
                var response = new Response();

                foreach (var playlist in playlists)
                {
                    var overflowCount = _playlistLoadingCalculator.GetOverflowCount(playlist, advertLength, query.RepeatCount);

                    if (overflowCount > 0)
                    {
                        response.Problems.Add(new ProblemInObject
                        {
                            Object = new SimpleDto
                            {
                                Id = playlist.Object.Id,
                                Name = playlist.Object.Name,
                            },
                            Overflow = overflowCount,
                            Date = playlist.PlayingDate,
                        });
                        //ProblemInObject Определяет структуру проблемы, включая сам объект (плейлист), количество переполнений и дату, когда проблема возникла.
                    }
                }

                response.Problems = response.Problems.OrderBy(p => p.Date).ToList();

                return response;
            }
        }

        public class Query : IRequest<Response>
        {
            public double AdvertLength { get; set; }
            public int RepeatCount { get; set; }
            public DateTime DateBegin { get; set; }
            public DateTime DateEnd { get; set; }
            [FromQuery(Name = "objects[]")]
            public List<Guid> Objects { get; set; } = new();
        }

        public class Response
        {
            public List<ProblemInObject> Problems { get; set; } = new();
        }

        public class ProblemInObject
        {
            public SimpleDto Object { get; set; }
            public int Overflow { get; set; }
            public DateTime Date { get; set; }
        }
    }
}

// Этот фрагмент кода предназначен для проверки возможности размещения рекламных блоков в плейлистах для определенных объектов и дат. Он выявляет потенциальные проблемы, такие как переполнение плейлиста (когда планируется слишком много повторений рекламы), и возвращает список этих проблем. Это может быть полезно для планирования рекламных кампаний и оптимизации загрузки плейлистов.