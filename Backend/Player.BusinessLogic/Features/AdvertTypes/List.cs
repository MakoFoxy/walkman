using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Player.DataAccess;
using Player.DTOs;

namespace Player.BusinessLogic.Features.AdvertTypes
{
    public class List
    {
        public class Handler : IRequestHandler<Query, List<AdvertTypeModel>>
        {
            private readonly PlayerContext _context;
            private readonly IMapper _mapper;

            public Handler(PlayerContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public Task<List<AdvertTypeModel>> Handle(Query request, CancellationToken cancellationToken = default)
            {
                return _context.AdvertTypes
                    .ProjectTo<AdvertTypeModel>(_mapper.ConfigurationProvider)
                    .ToListAsync(cancellationToken);
                //Асинхронный метод Handle, который обрабатывает входящий запрос, выполняет запрос к таблице AdvertTypes в базе данных, преобразует результаты в список моделей AdvertTypeModel с использованием настроек маппинга AutoMapper и возвращает их. Использование ProjectTo позволяет автоматически преобразовать результаты запроса к нужному типу.
            }
        }

        public class AdvertTypeModel : SimpleDto
        {
            public string Code { get; set; }
            //Определение модели данных AdvertTypeModel, которая наследуется от SimpleDto (не показана, но предположительно содержит основные свойства, такие как Id и Name). Дополнительно модель содержит свойство Code, представляющее уникальный код типа рекламы.
        }

        public class Query : IRequest<List<AdvertTypeModel>>
        {
        }
    }

    //В целом, этот код позволяет получить список всех типов рекламы из базы данных и преобразовать его в список моделей для использования в приложении, используя архитектурные принципы CQRS и AutoMapper для разделения логики запросов от логики команд и упрощения процесса преобразования данных.
}
