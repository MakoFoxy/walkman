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

            //             Класс Handler в Player.BusinessLogic.Features.AdvertTypes предназначен для обработки запросов, которые извлекают информацию о типах рекламы из базы данных, используя Entity Framework Core и AutoMapper. AdvertTypeModel используется для формирования данных, которые будут отправлены клиенту. Свойство Code в AdvertTypeModel предполагает, что для каждого типа рекламы, помимо базовой информации (как в SimpleDto), будет также предоставлен уникальный код.

            // На основе данной структуры и логики класса Handler данные, которые могут быть выведены в браузер для каждого типа рекламы, включают:

            //     Идентификатор (как часть SimpleDto, который, вероятно, включает в себя свойство Id).
            //     Название (также как часть SimpleDto, предполагается, что есть свойство Name).
            //     Код типа рекламы (Code), который является уникальным строковым идентификатором для типа рекламы.

            // Таким образом, когда контроллер AdvertTypesController получает GET-запрос, он через медиатор отправляет Query, который обрабатывается в Handler. Handler выполняет запрос к базе данных для извлечения типов рекламы, преобразует полученные сущности AdvertType в список AdvertTypeModel и возвращает его.
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
