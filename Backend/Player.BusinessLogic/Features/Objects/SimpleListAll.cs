using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Player.DataAccess;
using Player.Domain;
using Player.DTOs;
using Player.Services.Abstractions;

namespace Player.BusinessLogic.Features.Objects
{
    public class SimpleListAll
    {
        public class Handler : IRequestHandler<Query, List<ObjectDto>>
        // Это класс обработчика, который реализует интерфейс IRequestHandler из MediatR. Он отвечает за обработку входящих запросов типа Query и возвращает список объектов в формате ObjectDto.
        {
            private readonly PlayerContext _context;
            private readonly IUserManager _userManager;

            public Handler(PlayerContext context, IUserManager userManager)
            {
                _context = context;
                _userManager = userManager;
                // Конструктор инициализирует контекст данных _context и менеджер пользователей _userManager, которые передаются как зависимости. Эти зависимости используются для доступа к базе данных и информации о текущем пользователе.
            }

            public async Task<List<ObjectDto>> Handle(Query request, CancellationToken cancellationToken)
            {
                var query = _context.Objects.AsQueryable();

                var permissions = await _userManager.GetCurrentUserPermissions(cancellationToken);

                if (permissions.Any(p => p.Code == Permission.PartnerAccessToObject))
                {
                    var organization = await _userManager.GetUserOrganization(cancellationToken);

                    query = query.Where(o => _context.Organizations
                        .Where(org => org == organization)
                        .SelectMany(org => org.Clients)
                        .SelectMany(c => c.User.Objects)
                        .Select(uo => uo.Object)
                        .Contains(o));
                }

                if (permissions.Any(p => p.Code == Permission.AdminAccessObject))
                {
                    var user = await _userManager.GetCurrentUser(cancellationToken);

                    query = query.Where(o => _context.Users.Where(u => u == user)
                        .SelectMany(u => u.Objects)
                        .Select(uo => uo.Object)
                        .Contains(o));
                }

                return await query.Select(o => new ObjectDto
                {
                    Id = o.Id,
                    Name = o.Name,
                    Priority = o.Priority
                })
                    .OrderBy(o => o.Name)
                    .ToListAsync(cancellationToken);

                //Это основной метод обработчика. Он асинхронно извлекает данные о всех объектах из базы данных и фильтрует их в зависимости от разрешений текущего пользователя. Если у пользователя есть определенные разрешения (например, PartnerAccessToObject или AdminAccessObject), список объектов будет отфильтрован в соответствии с этими разрешениями.

                // Процесс фильтрации:
                //     Если пользователь имеет разрешение PartnerAccessToObject, выбираются только те объекты, которые связаны с его организацией.
                //     Если пользователь имеет разрешение AdminAccessObject, выбираются объекты, связанные непосредственно с этим пользователем.
            }
            // Когда пользователь делает запрос на получение списка объектов, система обрабатывает запрос с учетом его прав доступа и возвращает фильтрованный список объектов. Это позволяет разграничивать доступ к информации в системе и предоставлять данные в соответствии с уровнем доступа пользователя.
        }

        public class Query : IRequest<List<ObjectDto>>
        {
            // Query является пустым классом, который используется для инициации запроса на получение списка объектов. В данном случае он не содержит дополнительных данных, так как запрос не требует входных параметров.
        }

        public class ObjectDto : SimpleDto
        {
            public int Priority { get; set; }
            // ObjectDto представляет собой структуру данных (DTO), которая используется для передачи информации об объекте клиенту. Она содержит идентификатор объекта, его имя и приоритет.
        }
    }
}
// На основании запроса GET /api/v1/object/all, который был обработан в вашем веб-API, можно предположить, что в ответ клиент получит список объектов (List<SimpleListAll.ObjectDto>). Каждый объект будет содержать данные, определенные в ObjectDto.

// В контексте вашего приложения и визуального интерфейса, который вы прислали (форма добавления рекламы), запрос может быть использован для загрузки и отображения списка объектов, например, в выпадающем списке (dropdown), который позволит пользователям выбрать, к какому объекту относится добавляемая реклама.

// Поле "фильтр объектов" на форме, вероятно, позволяет пользователю отфильтровать или выбрать объекты из общего списка. Если в ObjectDto есть поля, такие как Id, Name и Priority, то в браузере может отобразиться что-то вроде:

// json

// [
//     {
//         "id": "идентификатор_объекта_1",
//         "name": "Название объекта 1",
//         "priority": 1
//     },
//     {
//         "id": "идентификатор_объекта_2",
//         "name": "Название объекта 2",
//         "priority": 2
//     },
//     // ... дальнейшие объекты
// ]

// Этот JSON затем может быть использован для отображения данных в соответствующих элементах пользовательского интерфейса. Например, когда пользователь перейдет на страницу http://localhost:8082/add-advert, фронтенд-часть приложения (написанная, возможно, на React, Angular, Vue или с использованием чистого JavaScript) может сделать запрос к API, получить список объектов и использовать эти данные для заполнения соответствующего поля формы.

// Также возможно, что этот запрос используется для инициализации формы, предоставляя список доступных объектов для выбора в соответствии с разрешениями текущего пользователя. Конкретные элементы формы, которые будут заполнены этими данными, зависят от реализации фронтенда и могут включать такие вещи, как текстовые поля, селекторы и другие вводные элементы.

// Обработчик SimpleListAll.Handler в вашем коде отвечает за выполнение запроса к базе данных для получения списка объектов. SQL-запрос, который вы привели, является результатом действий этого обработчика, который выполняет следующие шаги:

//     Определяет начальный запрос к таблице Objects в базе данных, используя контекст _context.

//     Получает текущие разрешения пользователя через метод _userManager.GetCurrentUserPermissions.

//     Применяет фильтрацию к запросу на основе этих разрешений:
//         Если пользователь имеет разрешение PartnerAccessToObject, запрос фильтруется, чтобы включить только объекты, связанные с организацией пользователя.
//         Если пользователь имеет разрешение AdminAccessObject, запрос фильтруется, чтобы включить только объекты, связанные непосредственно с пользователем.

//     Трансформирует результаты запроса в список объектов ObjectDto с полями Id, Name, и Priority.

//     Сортирует список объектов по имени с помощью OrderBy.

//     Выполняет запрос к базе данных асинхронно, преобразуя результаты в список с использованием метода ToListAsync.