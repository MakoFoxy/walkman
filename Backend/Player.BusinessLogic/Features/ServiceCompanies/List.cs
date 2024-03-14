using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Player.DataAccess;
using Player.DTOs;

namespace Player.BusinessLogic.Features.ServiceCompanies
{
    public class List
    {
        public class Handler : IRequestHandler<Query, List<ServiceCompanyModel>>
        {
            private readonly PlayerContext _context;
            private readonly IMapper _mapper;

            public Handler(PlayerContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
                //Handler — класс обработчика запроса, который реализует интерфейс IRequestHandler из MediatR. Он обрабатывает запрос Query и возвращает список моделей ServiceCompanyModel. В конструктор класса инжектируются контекст данных PlayerContext и маппер IMapper для доступа к базе данных и трансформации сущностей в модели соответственно.
            }

            public Task<List<ServiceCompanyModel>> Handle(Query request, CancellationToken cancellationToken = default)
            {
                return _context.ServiceCompanies.ProjectTo<ServiceCompanyModel>(_mapper.ConfigurationProvider).ToListAsync(cancellationToken);
                //Асинхронный метод, который выполняет запрос к базе данных для получения списка всех сервисных компаний. Он использует AutoMapper для проецирования результатов запроса из сущностей базы данных в модели ServiceCompanyModel. Метод возвращает список сервисных компаний после выполнения асинхронного запроса.
            }
        }

        public class ServiceCompanyModel : SimpleDto
        {
            //    ServiceCompanyModel представляет модель данных сервисной компании. В данном контексте он наследуется от SimpleDto, что может включать в себя базовые свойства, такие как идентификатор и название.

        }

        public class Query : IRequest<List<ServiceCompanyModel>>
        {
            // Query является пустым классом запроса, используемым для инициализации операции получения списка компаний. Поскольку он не содержит дополнительных полей или свойств, этот запрос возвращает все доступные сервисные компании без фильтрации.
        }
    }
}
