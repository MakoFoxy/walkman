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
                var playlists = await _context.Playlists //Используется Entity Framework для получения данных из таблицы Playlists.
                    .Include(p => p.Object)
                    .Include(p => p.Aderts)
                    .ThenInclude(a => a.Advert)
                    .Where(p => p.PlayingDate >= query.DateBegin && p.PlayingDate <= query.DateEnd) //Отбираются плейлисты, которые соответствуют заданному временному интервалу (DateBegin до DateEnd).
                    .Where(p => query.Objects.Contains(p.Object.Id)) //Отбор плейлистов осуществляется по списку объектов (каждый объект имеет уникальный идентификатор), переданному в запросе.
                    .ToListAsync(cancellationToken); //В запрос включены связанные данные об объектах (Object) и рекламах (Adverts), а также подробности рекламных блоков (Advert).

                //    Извлекает плейлисты из базы данных для заданного периода (DateBegin до DateEnd) и списка объектов (Objects).
                //Для каждого плейлиста рассчитывает переполнение(количество лишних повторений рекламы), используя сервис _playlistLoadingCalculator.Это делается на основе длины рекламного блока(AdvertLength) и количества его повторений(RepeatCount).
                //Если для плейлиста вычисляется переполнение(overflowCount > 0), информация о проблеме добавляется в ответ(Response).

                var advertLength = TimeSpan.FromSeconds(query.AdvertLength); //    Преобразование длительности рекламного блока из секунд в объект TimeSpan.

                //Response предназначен для хранения ответа обработчика, который включает в себя список проблем (Problems), связанных с переполнением плейлистов.
                var response = new Response(); //Создается экземпляр Response, который будет хранить результаты обработки.

                foreach (var playlist in playlists)
                {
                    var overflowCount = _playlistLoadingCalculator.GetOverflowCount(playlist, advertLength, query.RepeatCount); //Для каждого плейлиста рассчитывается количество переполнений (лишних повторений рекламы) с помощью сервиса _playlistLoadingCalculator. Этот расчет учитывает длину рекламного блока и количество его повторений.

                    if (overflowCount > 0) //Если для плейлиста выявляется переполнение (overflowCount > 0), информация о проблеме добавляется в список проблем в ответе. Проблема включает в себя идентификатор объекта, его название, количество переполнений и дату плейлиста.
                    {
                        response.Problems.Add(new ProblemInObject
                        {//В этом фрагменте кода происходит добавление новой проблемы, связанной с переполнением плейлиста, в список проблем ответа (response.Problems). Каждый объект проблемы (ProblemInObject) содержит информацию о конкретном плейлисте и о том, насколько он перегружен:
                            Object = new SimpleDto //Object: Это объект SimpleDto, который хранит информацию об объекте плейлиста, включая его Id и Name. Эти данные получаются непосредственно из объекта плейлиста (playlist.Object).
                            {
                                Id = playlist.Object.Id,
                                Name = playlist.Object.Name,
                            },
                            Overflow = overflowCount, //Overflow: Количество, показывающее, насколько количество повторений рекламы в данном плейлисте превышает допустимый предел. Это значение вычисляется в другой части кода и передается в качестве overflowCount.
                            Date = playlist.PlayingDate, //Date: Дата, на которую запланировано воспроизведение плейлиста (playlist.PlayingDate). Это позволяет идентифицировать, когда именно возникла проблема переполнения.
                            // Добавление происходит с помощью метода Add списка Problems,
                            // что означает,
                            // что каждая обнаруженная проблема будет последовательно записываться в этот список,
                            // и вся эта информация вернется в ответе на запрос.Этот подход позволяет систематически собирать и представлять данные о проблемах переполнения для дальнейшего анализа и принятия мер.
                        });
                        //ProblemInObject Определяет структуру проблемы, включая сам объект (плейлист), количество переполнений и дату, когда проблема возникла.
                    }
                }

                response.Problems = response.Problems.OrderBy(p => p.Date).ToList(); //    Список проблем сортируется по дате возникновения для удобства анализа и отчетности.

                return response; //    Возвращает объект Response, содержащий упорядоченный список проблем с переполнениями плейлистов, что помогает администраторам системы принимать меры для оптимизации распределения рекламных блоков и предотвращения избыточной загрузки плейлистов.
            }
        }

        public class Query : IRequest<Response> //Query: Класс запроса, который реализует интерфейс IRequest<Response>. Он предназначен для передачи данных в запросе, включая:
        {
            public double AdvertLength { get; set; } // Длина рекламного блока в секундах.
            public int RepeatCount { get; set; } //Количество повторений рекламного блока.
            public DateTime DateBegin { get; set; } //Даты начала периода, для которого нужно получить информацию.
            public DateTime DateEnd { get; set; } //Даты окончания периода, для которого нужно получить информацию.
            [FromQuery(Name = "objects[]")]
            public List<Guid> Objects { get; set; } = new(); //Objects: Список идентификаторов объектов (Guid), для которых требуется проверка. Атрибут [FromQuery(Name = "objects[]")] указывает, что данные будут извлечены из параметров запроса URL.
        }

        public class Response //Response: Класс ответа, содержащий список проблем, обнаруженных в процессе запроса. Problems инициализируется как новый список объектов ProblemInObject.
        {
            public List<ProblemInObject> Problems { get; set; } = new(); //ProblemInObject: Описывает конкретную проблему с переполнением плейлиста. Содержит:
        }

        public class ProblemInObject
        {
            public SimpleDto Object { get; set; } //Object: Данные объекта в виде SimpleDto, который включает идентификатор и название объекта.
            public int Overflow { get; set; } //Overflow: Количество лишних повторений рекламы, превышающее допустимое значение.
            public DateTime Date { get; set; } //Date: Дата, на которую рассчитано переполнение.
        }
        //Эти классы используются в рамках API для обработки запросов на проверку возможности размещения рекламных блоков в плейлистах, учитывая заданные параметры. Ответ включает в себя список потенциальных проблем, что позволяет пользователям API принимать информированные решения о корректировке рекламных кампаний или плейлистов.
    }
}

// Этот фрагмент кода предназначен для проверки возможности размещения рекламных блоков в плейлистах для определенных объектов и дат. Он выявляет потенциальные проблемы, такие как переполнение плейлиста (когда планируется слишком много повторений рекламы), и возвращает список этих проблем. Это может быть полезно для планирования рекламных кампаний и оптимизации загрузки плейлистов.