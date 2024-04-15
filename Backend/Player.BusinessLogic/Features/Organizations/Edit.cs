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
    {//    Сигнатура метода: System.Threading.Tasks.Task1[Microsoft.AspNetCore.Mvc.IActionResult] Put(Player.BusinessLogic.Features.Organizations.Models.OrganizationModel, System.Threading.CancellationToken)`
    // Этот метод обрабатывает запросы PUT для обновления данных организации в базе данных. Принимает модель OrganizationModel, содержащую новые или измененные данные организации, и обновляет соответствующие записи в базе данных.
    // Возвращает статус код 200 (OK), если обновление прошло успешно, подтверждая успешное выполнение операции.
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

// В вашем коде реализован метод Handle, который обновляет данные организации и связанных с ней клиентов в базе данных. Давайте разберём его по шагам:

//     Получение существующей организации: Сначала метод находит организацию по её идентификатору (Id) из базы данных. Для этого используется запрос к базе данных с применением метода .Where() и последующей загрузкой связанных с организацией клиентов и их объектов с помощью .Include() и .ThenInclude().

//     Обновление данных организации: Затем метод обновляет основные атрибуты организации (адрес, банк, БИН и т.д.) на основе данных, полученных из OrganizationModel.

//     Обработка новых клиентов: Если в OrganizationModel есть клиенты, которых ещё нет в базе данных (идентификатор клиента равен Guid.Empty), для каждого такого клиента создаётся новый экземпляр User, который затем добавляется к организации.

//     Обновление существующих клиентов: Для клиентов, уже существующих в базе данных, метод обновляет их информацию (e-mail, пароль, роль и т.д.) на основе данных из OrganizationModel. Также обновляются связанные с клиентами объекты. Если новый пароль не предоставлен, сохраняется текущий пароль пользователя.

//     Сохранение изменений: В конце метод сохраняет все изменения в базе данных с помощью SaveChangesAsync().

// Ваше понимание процесса верное. Метод берёт данные из OrganizationModel, обновляет сущность Organization в базе данных и соответствующим образом обрабатывает связанных с организацией клиентов (User) и их объекты. После выполнения этих шагов, обновлённая информация организации будет доступна через соответствующие запросы к API, например, через OrganizationsController.Get, который вернёт актуализированные данные об организации.
// Да, ваше понимание верное. Давайте еще раз пройдемся по ключевым моментам для ясности:

//     Для новых клиентов:
//         Если в OrganizationModel присутствуют клиенты, которые еще не сохранены в базе данных (их Id равен Guid.Empty), то для каждого такого клиента создается новый экземпляр User.
//         Этот новый экземпляр User затем заполняется данными из ClientModel, включая создание связей с объектами через UserObjects.
//         Новый клиент (Client) связывается с текущей организацией и добавляется в список клиентов организации.

//     Для существующих клиентов:
//         Если ClientModel относится к уже существующему клиенту (его Id не равен Guid.Empty), метод находит этого клиента в базе данных (originalClient).
//         Данные этого существующего клиента обновляются на основе информации из ClientModel. Это включает в себя обновление информации пользователя (User) и его связанных объектов.
//         Если для клиента предоставлен новый пароль, он обновляется; в противном случае сохраняется текущий пароль.
//         Связи между клиентом и его объектами также обновляются в соответствии с данными ClientModel.

//     Обновление данных организации:
//         Основные данные организации, такие как адрес, банковские реквизиты, БИН и т.д., обновляются на основе данных из OrganizationModel.

//     Сохранение изменений:
//         После обработки всех клиентов и обновления данных организации все изменения сохраняются в базе данных.

// Этот процесс обеспечивает гибкое управление данными организации и ее клиентов, позволяя одновременно добавлять новых клиентов и обновлять информацию о существующих.