using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Player.BusinessLogic.Features.Managers.Models;
using Player.DataAccess;
using Player.DTOs;

namespace Player.BusinessLogic.Features.Managers
{
    public class Details
    {
        public class Handler : IRequestHandler<Query, ManagerModel>
        {
            private readonly PlayerContext _context;

            public Handler(PlayerContext context)
            {
                _context = context;
            }

            public Task<ManagerModel> Handle(Query request, CancellationToken cancellationToken)
            {
                return _context.Managers.Where(m => m.Id == request.Id)
                    .Select(m => new ManagerModel
                    {
                        Id = m.Id,
                        LastName = m.User.LastName,
                        FirstName = m.User.FirstName,
                        SecondName = m.User.SecondName,
                        Role = new SimpleDto
                        {
                            Id = m.User.Role.Id,
                            Name = m.User.Role.Name
                        },
                        PhoneNumber = m.User.PhoneNumber,
                        Email = m.User.Email,
                        Objects = m.User.Objects.Select(o => new SimpleDto
                        {
                            Id = o.Object.Id,
                            Name = o.Object.Name
                        })
                        //Запрос к контексту базы данных для выбора одной записи менеджера по идентификатору. Информация о менеджере преобразуется в объект ManagerModel, который включает детали менеджера и связанные с ним данные, такие как личные данные и объекты, с которыми он ассоциируется.
                    })
                    .SingleAsync(cancellationToken);
            }
        }

        public class Query : IRequest<ManagerModel>
        {
            public Guid Id { get; set; }
            //Класс Query содержит данные для запроса - в данном случае, это идентификатор менеджера. Он реализует интерфейс IRequest с типом возвращаемого ответа ManagerModel.
        }
        //В итоге, этот блок кода позволяет извлекать из базы данных детализированную информацию о конкретном менеджере для ее последующего отображения или использования в бизнес-логике приложения.
    }
}