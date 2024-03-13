using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Player.BusinessLogic.Features.Managers.Models;
using Player.BusinessLogic.Features.Users.Validators;
using Player.DataAccess;
using Player.Domain;

namespace Player.BusinessLogic.Features.Managers
{
    public class Create
    {
        public class Validator : AbstractValidator<Command>
        {
            public Validator(
                IMapper mapper,
                UserUniqueValidator userUniqueValidator)
            {
                RuleFor(c => mapper.Map<Manager>(c.Manager).User)
                    .SetValidator(userUniqueValidator);
            }
            // Этот класс наследуется от AbstractValidator<T> из библиотеки FluentValidation. Он определяет правила валидации для объекта Command. Здесь используется AutoMapper для преобразования ManagerModel в сущность Manager, а затем для связанного с менеджером пользователя (User) применяется внешний валидатор UserUniqueValidator, который проверяет уникальность пользователя (например, по его email или username).
        }

        public class Handler : IRequestHandler<Command>
        {
            private readonly PlayerContext _context;
            private readonly IMapper _mapper;

            public Handler(
                PlayerContext context,
                IMapper mapper
                )
            {
                _context = context;
                _mapper = mapper;
            }

            public async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
            {
                var manager = _mapper.Map<Manager>(request.Manager);
                foreach (var modelObject in request.Manager.Objects)
                {
                    manager.User.Objects.Add(new UserObjects
                    {
                        ObjectId = modelObject.Id,
                        User = manager.User
                    });
                }
                _context.Entry(manager.User.Role).State = EntityState.Unchanged;
                _context.Managers.Add(manager);
                await _context.SaveChangesAsync(cancellationToken);
                return Unit.Value;
            }
            //Handler обрабатывает создание нового менеджера. Он использует PlayerContext для работы с базой данных и IMapper для преобразования DTO в доменные объекты. В методе Handle создается новый объект Manager из переданного DTO ManagerModel, затем добавляются связанные объекты и устанавливается состояние связанной роли как Unchanged, чтобы предотвратить ее повторное создание. Новый менеджер добавляется в контекст и сохраняется в базе данных.
        }

        public class Command : IRequest<Unit>
        {
            public ManagerModel Manager { get; set; }
        }
        //В целом, код демонстрирует добавление нового менеджера в систему с предварительной валидацией и преобразованием данных. Он использует MediatR для обработки команды создания, FluentValidation для проверки данных и AutoMapper для маппинга между моделями и сущностями.ы
    }
}