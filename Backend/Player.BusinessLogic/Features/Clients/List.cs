using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Player.BusinessLogic.Features.Clients.Models;
using Player.DataAccess;
using Player.DataAccess.Extensions;
using Player.DTOs;

namespace Player.BusinessLogic.Features.Clients
{
    public class List
    {
        public class Handler : IRequestHandler<Query, BaseFilterResult<ClientModel>>
        {
            private readonly PlayerContext _context;
            private readonly IMapper _mapper;

            public Handler(PlayerContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public async Task<BaseFilterResult<ClientModel>> Handle(Query request, CancellationToken cancellationToken)
            {
                var result = new BaseFilterResult<ClientModel>
                {
                    Page = request.Filter.Page,
                    ItemsPerPage = request.Filter.ItemsPerPage
                };

                var query = _context.Clients.OrderBy(c => c.User.FirstName);

                result.Result = await query
                    .GetPagedQuery(result.Page, result.ItemsPerPage)
                    .ProjectTo<ClientModel>(_mapper.ConfigurationProvider)
                    .ToListAsync(cancellationToken);

                result.TotalItems = await query.CountAsync(cancellationToken);

                return result;
                //Handler обрабатывает запросы на получение списка клиентов. Он использует Entity Framework Core для запроса данных из контекста базы данных (_context.Clients) и AutoMapper для проецирования результатов запроса в список моделей ClientModel. Результаты пагинируются с использованием GetPagedQuery, а общее количество записей подсчитывается через CountAsync.
            }
        }

        public class Query : IRequest<BaseFilterResult<ClientModel>>
        {
            public BaseFilterModel Filter { get; set; }
        }
    }

    //BaseFilterResult<T> используется для возвращения результатов запроса, включая общее количество элементов (TotalItems), информацию о пагинации (Page и ItemsPerPage) и сами данные (Result — список моделей клиентов).

    //В итоге, этот код обеспечивает получение списка клиентов с применением пагинации и фильтрации, используя архитектурные принципы CQRS для разделения запросов и команд и AutoMapper для преобразования сущностей базы данных в модели передачи данных.
}
