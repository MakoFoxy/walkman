using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Player.DataAccess;

namespace Player.BusinessLogic.Features.Adverts
{
    public class ListInObjectOnSelectedDate
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
                //Определение асинхронного метода Handle, который принимает параметры запроса Query и токен отмены CancellationToken, и возвращает данные о рекламных объявлениях для конкретного объекта и даты.
                var adverts = await _context.Playlists
                    .Where(p => p.Object.Id == query.ObjectId && p.PlayingDate == query.Date)
                    .SelectMany(p => p.Aderts.Select(a => a.Advert))
                    .Distinct()
                    .Select(a => new AdvertsInObjectResponse
                    {
                        Id = a.Id,
                        Name = a.Name,
                        Begin = a.AdLifetimes.Single(al => al.DateBegin <= query.Date && al.DateEnd >= query.Date).DateBegin,
                        End = a.AdLifetimes.Single(al => al.DateBegin <= query.Date && al.DateEnd >= query.Date).DateEnd,
                        RepeatCount = a.AdTimes.Single(at => at.PlayDate == query.Date && at.Object.Id == query.ObjectId).RepeatCount,
                    })
                    .OrderBy(a => a.Name)
                    .ToListAsync(cancellationToken);

                return new Response
                {
                    Adverts = adverts
                    //Создается новый объект Response, содержащий список рекламных объявлений adverts, и возвращается как результат работы метода.
                };

                //Формируется и выполняется запрос к базе данных для получения уникальных рекламных объявлений, которые были запланированы в указанном объекте (ObjectId) в определенную дату (Date). Информация извлекается, формируется в объекты AdvertsInObjectResponse и сортируется по имени.
            }
        }

        public class Query : IRequest<Response>
        {
            public Guid ObjectId { get; set; }
            public DateTime Date { get; set; }
        }

        public class Response
        {
            public List<AdvertsInObjectResponse> Adverts { get; set; } = new List<AdvertsInObjectResponse>();
        }

        public class AdvertsInObjectResponse
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public DateTime Begin { get; set; }
            public DateTime End { get; set; }
            public int RepeatCount { get; set; }
        }

        //Здесь определены структуры для запроса (Query) и ответа (Response). Query содержит информацию для фильтрации данных: ObjectId и Date. Response включает в себя список ответов AdvertsInObjectResponse, каждый из которых содержит детали о рекламном объявлении.

        //В итоге, этот код позволяет получить отфильтрованный список рекламных объявлений, активных в определенном объекте на выбранную дату, что может быть использовано, например, для составления отчетов или планирования рекламных кампаний.

    }
}