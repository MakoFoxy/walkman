using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Player.DataAccess;
using Player.DTOs;

namespace Player.BusinessLogic.Features.Selections
{
    public class Details
    {
        public class Handler : IRequestHandler<Query, SelectionModel>
        {
            private readonly PlayerContext _context;
            private readonly IMapper _mapper;

            public Handler(PlayerContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public Task<SelectionModel> Handle(Query request, CancellationToken cancellationToken)
            {
                return _context.Selections.Where(o => o.Id == request.Id)
                    .Select(s => new SelectionModel
                    {
                        Id = s.Id,
                        Name = s.Name,
                        DateBegin = s.DateBegin,
                        DateEnd = s.DateEnd,
                        IsPublic = s.IsPublic,
                        Tracks = s.MusicTracks.Select(mt => mt.MusicTrack).Select(mt => new TrackModel
                        {
                            Id = mt.Id,
                            Name = mt.Name,
                            Length = mt.Length.TotalSeconds,
                        }).ToList(),
                    })
                    .SingleAsync(cancellationToken);

                //Этот метод асинхронно извлекает из базы данных детальную информацию о подборке с указанным идентификатором. Возвращается объект SelectionModel, который включает в себя название подборки, даты начала и окончания, статус публичности и список треков, входящих в подборку.
            }
        }

        public class Query : IRequest<SelectionModel>
        {
            public Guid Id { get; set; }
            //Query - это класс, представляющий запрос на получение деталей подборки. Он содержит идентификатор конкретной подборки, детали которой необходимо получить.
        }

        public class SelectionModel : SimpleDto
        {
            public DateTimeOffset DateBegin { get; set; }
            public DateTimeOffset? DateEnd { get; set; }
            public bool IsPublic { get; set; }
            public List<TrackModel> Tracks { get; set; } = new();
            //SelectionModel - это модель данных для представления деталей подборки. Она включает информацию о временных рамках, доступности подборки и списка треков. 
        }

        public class TrackModel : SimpleDto        
        {
            public double Length { get; set; }
            //TrackModel используется для представления информации о каждом треке в подборке, включая длительность трека.
        }
    }
    //Этот код позволяет пользователям системы получать детальную информацию о музыкальных подборках, включая состав треков, что может использоваться для различных целей, например, для отображения информации на интерфейсе пользователя или для дальнейшего анализа и обработки.
}