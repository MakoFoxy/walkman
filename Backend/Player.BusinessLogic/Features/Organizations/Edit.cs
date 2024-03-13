using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Player.BusinessLogic.Features.Organizations.Models;
using Player.DataAccess;
using Player.Domain;

namespace Player.BusinessLogic.Features.Organizations
{
    public class Edit
    {
        public class Handler : IRequestHandler<Command>
        {
            private readonly PlayerContext _context;

            public Handler(PlayerContext context)
            {
                _context = context;
            }

            public async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
            {

                var model = request.OrganizationModel;

                var organization = await _context.Organizations.Where(o => o.Id == model.Id)
                    .Include(o => o.Clients)
                    .ThenInclude(c => c.User)
                    .ThenInclude(u => u.Objects)
                    .SingleAsync(cancellationToken);

                organization.Address = model.Address;
                organization.Bank = model.Bank;
                organization.Bin = model.Bin;
                organization.Iik = model.Iik;
                organization.Name = model.Name;
                organization.Phone = model.Phone;

                var newClients = model.Clients.Where(c => c.Id == Guid.Empty).ToList();

                if (newClients.Any())
                {
                    foreach (var clientModel in newClients)
                    {
                        var user = new User
                        {
                            Id = clientModel.Id,
                            Email = clientModel.Email,
                            Password = clientModel.Password,
                            RoleId = clientModel.Role.Id,
                            FirstName = clientModel.FirstName,
                            LastName = clientModel.LastName,
                            PhoneNumber = clientModel.PhoneNumber,
                            SecondName = clientModel.SecondName,
                        };
                        foreach (var clientModelObject in clientModel.Objects)
                        {
                            user.Objects.Add(new UserObjects
                            {
                                ObjectId = clientModelObject.Id,
                                UserId = user.Id
                            });
                        }

                        var client = new Client
                        {
                            Organization = organization,
                            User = user
                        };
                        organization.Clients.Add(client);
                    }
                }

                foreach (var client in model.Clients.Except(newClients))
                {
                    var originalClient = organization.Clients.Single(c => c.Id == client.Id);
                    var user = originalClient.User;

                    user.Email = client.Email;
                    user.Password = string.IsNullOrWhiteSpace(client.Password) ? user.Password : client.Password;
                    user.RoleId = client.Role.Id;
                    user.FirstName = client.FirstName;
                    user.LastName = client.LastName;
                    user.PhoneNumber = client.PhoneNumber;
                    user.SecondName = client.SecondName;
                    user.Objects.Clear();

                    foreach (var clientModelObject in client.Objects)
                    {
                        user.Objects.Add(new UserObjects
                        {
                            ObjectId = clientModelObject.Id,
                            UserId = user.Id
                        });
                    }
                }

                await _context.SaveChangesAsync(cancellationToken);
                return Unit.Value;

                //                 Асинхронный метод обрабатывает запрос на редактирование организации. Он извлекает организацию из базы данных по Id, обновляет ее данные и сохраняет изменения. Метод также обрабатывает добавление новых клиентов и обновление существующих, включая связанные с ними объекты.
                // Процесс обновления организации и клиентов:

                //     Находится организация по Id и загружается ее текущее состояние вместе с клиентами и связанными объектами.
                //     Обновляются поля организации, используя данные из OrganizationModel.
                //     Добавляются новые клиенты, если они есть в модели (newClients).
                //     Для каждого нового клиента создается экземпляр User, связанные объекты добавляются к пользователю, и клиент связывается с организацией.
                //     Обновляются данные существующих клиентов: их личная информация и связанные объекты. Если пароль клиента не предоставлен, сохраняется текущий пароль.
                //     Вносятся изменения в базу данных и сохраняются.
            }
        }

        public class Command : IRequest<Unit>
        {
            public OrganizationModel OrganizationModel { get; set; }
            //Этот класс является DTO (Data Transfer Object) для передачи данных организации, которую необходимо обновить. OrganizationModel содержит все необходимые данные для обновления, включая информацию о клиентах и их связанных объектах.
        }
    }
    // Этот код обеспечивает функциональность для обновления данных организации вместе с информацией о связанных с ней клиентах и их объектах. Это важно для поддержания актуальности информации об организациях в системе и управления связями между организациями, их клиентами и объектами.
}