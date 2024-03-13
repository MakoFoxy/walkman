using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Player.BusinessLogic.Features.Organizations.Models;
using Player.DataAccess;
using Player.DTOs;

namespace Player.BusinessLogic.Features.Organizations
{
    public class Details
    {
        public class Handler : IRequestHandler<Query, OrganizationModel>
        {
            private readonly PlayerContext _context;

            public Handler(PlayerContext context)
            {
                _context = context;
                //Конструктор класса принимает контекст базы данных PlayerContext, который используется для запроса информации организации.
            }

            public Task<OrganizationModel> Handle(Query request, CancellationToken cancellationToken)
            {
                return _context.Organizations.Where(m => m.Id == request.Id)
                    .Select(o => new OrganizationModel
                    {
                        Id = o.Id,
                        Name = o.Name,
                        Bin = o.Bin,
                        Address = o.Address,
                        Bank = o.Bank,
                        Iik = o.Iik,
                        Phone = o.Phone,
                        Clients = o.Clients.Select(c => new OrganizationModel.ClientModel
                        {
                            Id = c.Id,
                            Email = c.User.Email,
                            Objects = c.User.Objects.Select(ob => new SimpleDto
                            {
                                Id = ob.Object.Id,
                                Name = ob.Object.Name
                            }),
                            Role = new SimpleDto
                            {
                                Id = c.User.Role.Id,
                                Name = c.User.Role.Name
                            },
                            FirstName = c.User.FirstName,
                            LastName = c.User.LastName,
                            SecondName = c.User.SecondName,
                            PhoneNumber = c.User.PhoneNumber,
                        })
                    })
                    .SingleAsync(cancellationToken);
                //Этот асинхронный метод обрабатывает запрос Query, извлекая из базы данных детальную информацию организации по заданному Id. Он возвращает модель OrganizationModel, содержащую всю необходимую информацию об организации, включая её клиентов и связанные с ними объекты.
                // Процесс запроса:

                //     Сначала происходит поиск организации по идентификатору.
                //     Затем извлекается информация об организации и её клиентах, включая связанные объекты и роли пользователей.
                //     Информация преобразуется в модель OrganizationModel, которая включает в себя данные как о самой организации, так и о её клиентах.
            }
        }

        public class Query : IRequest<OrganizationModel>
        {
            public Guid Id { get; set; }
            //Query представляет собой запрос, который содержит Id организации, информацию о которой нужно получить.
        }
        //         Класс OrganizationModel:

        // Это модель данных, предназначенная для передачи информации об организации и её клиентах. Она включает поля, такие как Id, Name, Address, информацию о банковских данных и контактные данные, а также список моделей клиентов (ClientModel), каждый из которых содержит информацию о клиенте и связанных с ним объектах.

        // В итоге, этот код обеспечивает возможность получения полной информации об организации, что может быть использовано в пользовательском интерфейсе или для других внутренних нужд системы.
    }
}