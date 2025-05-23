﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Player.DataAccess;
using Player.DataAccess.Extensions;
using Player.Domain;
using Player.DTOs;

namespace Player.BusinessLogic.Features.Music
{
    public class List
    {
        public class Handler : IRequestHandler<Query, MusicFilterResult>
        {
            private readonly PlayerContext _context;
            private readonly IMapper _mapper;

            public Handler(PlayerContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public async Task<MusicFilterResult> Handle(Query request, CancellationToken cancellationToken)
            {
                var result = new MusicFilterResult
                { //оздается объект MusicFilterResult, который будет хранить результаты запроса, включая информацию о пагинации и фильтрах (номер страницы, количество элементов на странице, ID жанра и ID подборки).
                    Page = request.Filter?.Page,
                    ItemsPerPage = request.Filter?.ItemsPerPage,
                    GenreId = request.Filter?.GenreId,
                    SelectionId = request.Filter?.SelectionId
                };

                var query = _context.MusicTracks
                    .Where(mt => mt.TrackType.Code == TrackType.Music); //Исходный запрос к таблице MusicTracks фильтруется так, чтобы включать только те треки, которые относятся к типу Music.

                if (result.GenreId != Guid.Empty && result.GenreId.HasValue) //Если задан фильтр по жанру (GenreId), запрос дополнительно фильтруется по наличию жанра с соответствующим ID.
                {
                    query = query.Where(mt => mt.Genres.Any(g => g.Genre.Id == result.GenreId));
                }

                if (result.SelectionId != Guid.Empty && result.SelectionId.HasValue) //Если задан фильтр по подборке (SelectionId), запрос дополнительно фильтруется по наличию подборки с соответствующим ID и результаты сортируются по индексу треков в подборке.
                {
                    query = query.Where(mt => mt.Selections.Any(s => s.Selection.Id == result.SelectionId))
                        .OrderBy(mt => mt.Selections.Select(s => s.Index));
                }

                result.Result = await query
                    .GetPagedQuery(result.Page, result.ItemsPerPage) //Запрос выполняется с использованием пагинации (GetPagedQuery), что позволяет вернуть только требуемый диапазон результатов на запрашиваемой странице.
                    .ProjectTo<MusicModel>(_mapper.ConfigurationProvider).ToListAsync(cancellationToken); //Результаты запроса преобразуются в список объектов MusicModel с помощью AutoMapper (ProjectTo).
                result.TotalItems = await query.CountAsync(cancellationToken); //    Выполняется подсчет общего количества треков, соответствующих заданным фильтрам, что позволяет клиенту корректно реализовать пагинацию на пользовательском интерфейсе.

                return result; //    Возвращается объект MusicFilterResult, содержащий отфильтрованный и постранично организованный список музыкальных треков, а также общее количество треков, удовлетворяющих условиям фильтрации.

                //Это основная логика обработки запроса на получение музыки. В методе Handle формируется LINQ запрос к базе данных, который может включать фильтрацию по жанру и подборкам (селекциям). Затем результаты фильтруются и преобразуются в модели MusicModel, а также происходит пагинация результатов. В конце метода возвращается объект MusicFilterResult, содержащий отфильтрованный и страницы список треков, а также общее количество треков, удовлетворяющих критериям фильтрации.
            }
        }

        public class MusicModel
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public TimeSpan Length { get; set; }
            public string FilePath { get; set; }
            public ICollection<SimpleDto> Genres { get; set; } = new List<SimpleDto>();
        }

        public class Query : IRequest<MusicFilterResult>
        {
            public MusicFilterModel Filter { get; set; }
        }

        public class MusicFilterModel : BaseFilterModel
        {
            public Guid GenreId { get; set; }
            public Guid SelectionId { get; set; }
        }

        public class MusicFilterResult : BaseFilterResult<MusicModel>
        {
            public Guid? GenreId { get; set; }
            public Guid? SelectionId { get; set; }
        }
        //MusicFilterResult наследуется от BaseFilterResult<MusicModel>, что позволяет использовать обобщённые методы и свойства для управления результатами запросов с учётом пагинации и количества элементов. MusicFilterModel используется в Query для передачи параметров фильтрации в запросе.
        //    MusicModel определяет структуру данных музыкального трека, который будет возвращен в ответе.
        // Query представляет собой запрос, который включает в себя параметры фильтрации MusicFilterModel.
        // MusicFilterModel содержит критерии для фильтрации музыкальных треков, такие как GenreId и SelectionId.
        // MusicFilterResult — это класс, который содержит результат запроса, включая отфильтрованный список музыкальных треков и информацию для пагинации (общее количество треков, номер страницы и количество элементов на странице).
    }
    //     Пользователь отправляет запрос на получение списка музыкальных треков, возможно, с указанием фильтров по жанру или подборке.
    // Система, используя класс Handler, обрабатывает запрос: выполняет запрос к базе данных с учетом указанных фильтров, применяет пагинацию и преобразует результаты в формат MusicModel.
    // Система возвращает ответ MusicFilterResult, который содержит необходимую информацию о музыкальных треках и общее количество найденных треков, соответствующих условиям фильтрации.
}
