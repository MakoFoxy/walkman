using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MediatR;
using Player.BusinessLogic.Features.Clients.Models;
using Player.BusinessLogic.Features.Users.Validators;
using Player.DataAccess;
using Player.Domain;

namespace Player.BusinessLogic.Features.Clients
{
    public class Create
    {
        public class Validator : AbstractValidator<Command>
        {
            public Validator(
                IMapper mapper,
                UserUniqueValidator userUniqueValidator)
            {
                RuleFor(c => mapper.Map<Client>(c.ClientModel).User)
                    .SetValidator(userUniqueValidator);
            }
            //Validator - это класс, наследуемый от AbstractValidator<T>, предназначен для валидации команд создания клиента (Command). Он использует AutoMapper для преобразования ClientModel в сущность Client и применяет к пользовательскому объекту User внешний валидатор UserUniqueValidator, проверяющий уникальность пользователя.
        }
        
        public class Handler : IRequestHandler<Command>
        {
            private readonly PlayerContext _context;
            private readonly IMapper _mapper;

            public Handler(PlayerContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }
            
            public async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
            {
                _context.Clients.Add(_mapper.Map<Client>(request.ClientModel));
                await _context.SaveChangesAsync(cancellationToken);
                return Unit.Value;
                //Handler обрабатывает команду Command для создания нового клиента. Он использует контекст базы данных PlayerContext для добавления новой сущности Client, маппированной из ClientModel. Затем метод SaveChangesAsync асинхронно сохраняет изменения в базе данных. Возвращаемое значение Unit.Value является способом MediatR сообщить, что возвращать результат не требуется (эквивалентно void).
            }
        }

        public class Command : IRequest<Unit>
        {
            public ClientModel ClientModel { get; set; }
            ///Command является классом запроса, который используется для передачи данных клиента из пользовательского интерфейса или другого слоя приложения в логику обработки. Этот класс реализует интерфейс IRequest<Unit> из библиотеки MediatR, что позволяет его использовать с обработчиком Handler. ClientModel содержит данные клиента, которые нужно добавить в систему.
        }
    }

    //Весь код вместе формирует функциональную часть приложения, которая позволяет добавить нового клиента в систему, включая предварительную валидацию данных клиента и сохранение этих данных в базе.
}