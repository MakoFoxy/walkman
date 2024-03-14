using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Player.DataAccess;
using Player.DTOs;

namespace Player.BusinessLogic.Features.TrackTypes
{
    public class List
    {
        public class Handler : IRequestHandler<Query, List<TrackTypeModel>>
        {
            //Это класс, который обрабатывает входящие запросы на получение списка типов треков. Он реализует интерфейс IRequestHandler<Query, List<TrackTypeModel>>, что означает, что он принимает Query и возвращает List<TrackTypeModel>.
            private readonly PlayerContext _context;
            private readonly IMapper _mapper;

            public Handler(PlayerContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public Task<List<TrackTypeModel>> Handle(Query request, CancellationToken cancellationToken = default)
            {
                return _context.TrackTypes.ProjectTo<TrackTypeModel>(_mapper.ConfigurationProvider).ToListAsync(cancellationToken);
                //Это метод, в котором происходит обработка запроса. В данном контексте, он извлекает типы треков из базы данных с использованием Entity Framework Core, автоматически проецирует их в DTO (объекты передачи данных) TrackTypeModel с использованием AutoMapper, а затем возвращает их в виде списка. Эта операция асинхронна и использует ToListAsync для асинхронного выполнения запроса к базе данных.
            }
        }

        public class TrackTypeModel : SimpleDto
        {
            //Это модель, представляющая объект передачи данных для типов треков. Она наследует SimpleDto, который, как правило, включает стандартные свойства, такие как Id и Name. Эта модель показывает, как каждый тип трека должен отображаться или представляться за пределами контекста базы данных или внутренней логики.
        }

        public class Query : IRequest<List<TrackTypeModel>>

        {
            //Это простой класс, представляющий запрос на получение списка типов треков. В MediatR запросы или команды являются просто контейнерами данных без какого-либо поведения. Здесь этот класс не содержит никаких свойств, потому что для получения списка типов треков не требуются конкретные параметры.
        }
    }
}
