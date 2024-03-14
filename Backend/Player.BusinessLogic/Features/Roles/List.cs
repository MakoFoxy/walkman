using System;
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

namespace Player.BusinessLogic.Features.Roles
{
    public class List
    {
        public class Handler : IRequestHandler<Query, List<RoleModel>>
        {
            private readonly PlayerContext _context;
            private readonly IMapper _mapper;

            public Handler(PlayerContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
                //Handler – это класс обработчика, реализующий интерфейс IRequestHandler из MediatR. Он принимает запрос типа Query и возвращает список ролей в формате RoleModel. В его конструкторе используются контекст данных PlayerContext для доступа к базе данных и маппер IMapper для преобразования сущностей базы данных в модели данных.
            }

            public Task<List<RoleModel>> Handle(Query request, CancellationToken cancellationToken = default)
            {
                var query = _context.Roles.AsQueryable();

                query = request.Filter switch
                {
                    RoleFilter.All => query,
                    RoleFilter.Admin => query.Where(r => r.IsAdminRole),
                    RoleFilter.Client => query.Where(r => !r.IsAdminRole),
                    _ => throw new ArgumentOutOfRangeException(nameof(request.Filter))
                };

                return query.ProjectTo<RoleModel>(_mapper.ConfigurationProvider).ToListAsync(cancellationToken);
                //Этот метод асинхронно обрабатывает запрос на получение списка ролей, фильтруя их в зависимости от значения request.Filter. Метод использует AutoMapper для проецирования данных из сущностей в модели.
                // Фильтрация:

                //     RoleFilter.All: Возвращает все роли.
                //     RoleFilter.Admin: Возвращает только роли с пометкой администратора (IsAdminRole).
                //     RoleFilter.Client: Возвращает роли, не являющиеся ролями администраторов.
            }
        }

        public class RoleModel : SimpleDto
        {
        }

        public enum RoleFilter
        {
            All,
            Admin,
            Client
        }

        public class Query : IRequest<List<RoleModel>>
        {
            public RoleFilter Filter { get; set; }
        }
        //         RoleModel: Модель данных, представляющая роль. Она наследуется от SimpleDto, что, вероятно, включает идентификатор и название.
        // Query: Класс запроса, содержащий фильтр, который определяет, какие роли нужно возвращать.
        //RoleFilter – это перечисление, определяющее доступные фильтры для списка ролей.
    }
    //Этот код позволяет пользователям системы запросить список ролей с учетом определенного фильтра, что может быть полезно для управления доступом пользователей к различным функциям системы.
}
