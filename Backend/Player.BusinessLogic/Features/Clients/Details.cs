using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Player.BusinessLogic.Features.Clients.Models;
using Player.DataAccess;

namespace Player.BusinessLogic.Features.Clients
{
    public class Details
    {
        public class Handler : IRequestHandler<Query, ClientModel>
        {
            private readonly PlayerContext _context;
            private readonly IMapper _mapper;

            public Handler(PlayerContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public Task<ClientModel> Handle(Query request, CancellationToken cancellationToken) =>
                _context.Clients.ProjectTo<ClientModel>(_mapper.ConfigurationProvider)
                    .SingleAsync(c => c.Id == request.Id, cancellationToken);
            //Метод Handle асинхронно извлекает данные клиента из базы данных, преобразуя их в ClientModel с использованием настроек AutoMapper, и возвращает одиночную запись, которая соответствует указанному идентификатору.
        }

        public class Query : IRequest<ClientModel>
        {
            public Guid Id { get; set; }
        }
    }
    //В итоге, этот блок кода реализует функциональность получения детальной информации о клиенте по идентификатору с использованием MediatR для разделения запросов и команд и AutoMapper для маппинга между сущностями базы данных и моделями передачи данных.
}
