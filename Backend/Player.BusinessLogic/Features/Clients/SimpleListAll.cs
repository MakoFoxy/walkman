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

namespace Player.BusinessLogic.Features.Clients
{
    public class SimpleListAll
    {
        public class Handler : IRequestHandler<Query, List<ClientDictionaryModel>>
        {
            private readonly PlayerContext _context;
            private readonly IMapper _mapper;

            public Handler(PlayerContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public Task<List<ClientDictionaryModel>> Handle(Query dictionaryRequest, CancellationToken cancellationToken)
            {
                // Метод Handle выполняет асинхронный запрос к базе данных для получения списка клиентов, сортируя их по имени пользователя (FirstName), и использует AutoMapper для проецирования сущностей клиентов в модели ClientDictionaryModel.
                return _context.Clients.OrderBy(c => c.User.FirstName)
                    .ProjectTo<ClientDictionaryModel>(_mapper.ConfigurationProvider)
                    .ToListAsync(cancellationToken);
            }
        }

        public class ClientDictionaryModel : SimpleDto
        {
        }

        public class Query : IRequest<List<ClientDictionaryModel>>
        {
        }
    }
    //В целом, этот код обеспечивает получение отсортированного списка всех клиентов из базы данных, проецируя данные сущностей на модели DTO с использованием AutoMapper, что упрощает передачу и использование этих данных в других частях приложения или при взаимодействии с внешними клиентами, такими как фронтенд.
}
