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
