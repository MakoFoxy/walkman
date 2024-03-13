using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Player.BusinessLogic.Features.Organizations.Models;
using Player.BusinessLogic.Features.Users.Validators;
using Player.DataAccess;
using Player.Domain;

namespace Player.BusinessLogic.Features.Organizations
{
    public class Create
    {
        public class Validator : AbstractValidator<OrganizationModel>
        {
            public Validator(
                IMapper mapper,
                UserUniqueValidator userUniqueValidator)
            {
                // todo Не работает
                RuleForEach(c => mapper.Map<List<Client>>(c.Clients).Select(c => c.User))
                    .NotEmpty()
                    .NotNull()
                    .OverridePropertyName("Clients")
                    .SetValidator(userUniqueValidator);
            }
        }

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

                var organization = new Organization
                {
                    Id = Guid.NewGuid(),
                    Address = model.Address,
                    Bank = model.Bank,
                    Bin = model.Bin,
                    Iik = model.Iik,
                    Name = model.Name,
                    Phone = model.Phone,
                };

                foreach (var clientModel in model.Clients)
                {
                    var user = new User
                    {
                        Id = Guid.NewGuid(),
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

                _context.Organizations.Add(organization);
                await _context.SaveChangesAsync(cancellationToken);

                return Unit.Value;
                //Метод Handle асинхронно обрабатывает команду на создание организации. Он преобразует модель организации из запроса в сущность Organization и добавляет ее в базу данных, включая создание связанных с организацией клиентов (User) и связь между клиентами и объектами через UserObjects.

                //Процесс создания организации и клиентов:

                // Инициализируется новая сущность Organization с данными из OrganizationModel.
                // Для каждого клиента в model.Clients создается новый User, заполняются его поля, и он добавляется к организации.
                // Если у клиента есть связанные объекты, они также добавляются через связь UserObjects.
                // Организация с клиентами добавляется в контекст базы данных и сохраняется.
            }
            // Этот код позволяет реализовать создание организации с учетом всех связанных с ней клиентов и их объектов, обеспечивая целостность данных и правильность их структурирования в базе данных. Валидация данных перед сохранением помогает предотвратить возможные ошибки и конфликты.
        }

        public class Command : IRequest<Unit>
        {
            public OrganizationModel OrganizationModel { get; set; }
        }
    }


}