using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Player.DataAccess;
using Player.DTOs; //DTO (Data Transfer Object)
//Эти строки кода подключают необходимые пространства имен и пакеты для работы функционала. В частности, используются AutoMapper для сопоставления сущностей и DTO, MediatR для реализации шаблона CQRS, EntityFrameworkCore для взаимодействия с базой данных и другие вспомогательные пространства имен.
namespace Player.BusinessLogic.Features.ActivityTypes
{
    public class List
    {
        //Этот класс является контейнером для вложенных классов, которые реализуют логику получения списка типов деятельности. В нем определены классы Handler, ActivityTypeModel и Query.
        public class Handler : IRequestHandler<Query, List<ActivityTypeModel>>
        {
            private readonly PlayerContext _context;
            private readonly IMapper _mapper;

            public Handler(PlayerContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public Task<List<ActivityTypeModel>> Handle(Query request,
                CancellationToken cancellationToken = default)
            {
                return _context.ActivityTypes
                    .ProjectTo<ActivityTypeModel>(_mapper.ConfigurationProvider)
                    .ToListAsync(cancellationToken);
            }

            // Это вложенный класс, который реализует интерфейс IRequestHandler из пакета MediatR. Это означает, что Handler обрабатывает запросы типа Query и возвращает список типов деятельности в формате ActivityTypeModel.

            //В конструкторе Handler принимает две зависимости:

            //  _context: Контекст базы данных, через который осуществляется доступ к данным.
            //_mapper: Объект AutoMapper, используемый для сопоставления сущностей с DTO.

            //Метод Handle асинхронно извлекает из контекста базы данных список типов деятельности, преобразуя их в формат ActivityTypeModel с использованием AutoMapper, и возвращает результат.

        }

        public class ActivityTypeModel : SimpleDto
        {

            //Это класс DTO (Data Transfer Object), используемый для передачи данных о типах деятельности из API в клиент. Он наследуется от SimpleDto, что предполагает, что может содержать некоторые базовые свойства, общие для различных DTO.
        }

        public class Query : IRequest<List<ActivityTypeModel>>
        {

            //Этот класс представляет запрос, который обрабатывается Handler. Он реализует интерфейс IRequest из пакета MediatR. В данном случае запрос не содержит дополнительных данных и используется только для того, чтобы инициировать получение списка типов деятельности.
        }
    }
}
