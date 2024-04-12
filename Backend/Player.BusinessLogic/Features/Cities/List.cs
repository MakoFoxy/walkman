using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Player.DataAccess;
using Player.DTOs;

namespace Player.BusinessLogic.Features.Cities
{
    public class List
    {
        public class Handler : IRequestHandler<Query, List<CityModel>>
        {
            private readonly PlayerContext _context;
            private readonly IMapper _mapper;

            public Handler(PlayerContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public Task<List<CityModel>> Handle(Query request, CancellationToken cancellationToken)
            {
                return _context.Cities
                    .ProjectTo<CityModel>(_mapper.ConfigurationProvider)
                    .ToListAsync(cancellationToken);
                //Это определение асинхронного метода Handle, который обрабатывает входящие запросы на получение списка городов. Запрос к базе данных формируется через контекст _context.Cities, и результаты проецируются непосредственно в список моделей CityModel при помощи AutoMapper. Затем список асинхронно загружается и возвращается как результат метода.
            }
        }

        public class Query : IRequest<List<CityModel>>
        {
            //Определение класса Query, который является пустым, поскольку для получения списка городов дополнительные данные в запросе не требуются. Query реализует интерфейс IRequest, который указывает на тип возвращаемого ответа List<CityModel>.
        }

        public class CityModel : SimpleDto
        {
            //CityModel представляет собой модель данных для города. Она наследуется от SimpleDto, который предполагается содержит основные свойства (в коде не показано, что входит в SimpleDto, но обычно это базовые свойства, такие как Id и Name). CityModel может быть расширена, чтобы включать дополнительные свойства, специфичные для городов.
        }
    }
    //В целом, этот код предназначен для получения и отправки списка городов из базы данных в стандартизированном формате через асинхронный запрос, используя стандартные паттерны и библиотеки в .NET Core приложении.
}

// Клиентский Запрос: Клиент делает HTTP GET запрос на /api/v1/cities. Этот запрос обрабатывается контроллером CitiesController.

// Аутентификация и Авторизация: Прежде чем обработать запрос, система проверяет, аутентифицирован ли пользователь и имеет ли он право ReadAllCities, используя для аутентификации JWT. Если проверка проходит успешно, процесс продолжается.

// Обработка запроса через MediatR: Контроллер использует MediatR для отправки запроса List.Query соответствующему обработчику. Этот запрос не содержит дополнительных данных, поскольку для получения списка городов специфичные данные не требуются.

// Извлечение данных из базы: Обработчик List.Handler выполняет запрос к базе данных через Entity Framework Core, используя контекст PlayerContext. Он асинхронно извлекает данные о городах, хранящиеся в таблице Cities.

// Преобразование данных: Используя AutoMapper, извлеченные данные о городах преобразуются в список объектов CityModel, который представляет собой структурированное представление информации о городах, пригодное для передачи клиенту.

// Отправка ответа: Преобразованный список объектов CityModel возвращается контроллером в качестве ответа на запрос клиента.

// Клиент получает данные: В итоге клиент получает ответ в формате JSON, содержащий структурированный список городов, который можно использовать в пользовательском интерфейсе для отображения информации о доступных городах.