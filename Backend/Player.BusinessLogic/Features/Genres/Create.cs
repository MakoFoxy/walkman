using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Player.DataAccess;
using Player.Domain;

namespace Player.BusinessLogic.Features.Genres
{
    public class Create
    {
        public class Handler : IRequestHandler<Command, Guid>
        {
            private readonly PlayerContext _context;

            public Handler(PlayerContext context)
            {
                _context = context;
            }

            public async Task<Guid> Handle(Command command, CancellationToken cancellationToken)
            {
                var genre = new Genre
                {
                    Name = command.Name
                };
                _context.Genres.Add(genre);
                await _context.SaveChangesAsync(cancellationToken);
                return genre.Id;
                //Метод Handle асинхронно обрабатывает команду создания нового жанра. Он создает новый экземпляр Genre, добавляет его в контекст базы данных и сохраняет изменения. Возвращается идентификатор (Guid) новосозданного жанра.
            }
        }

        public class Command : IRequest<Guid>
        {
            public string Name { get; set; }
            //Command - это DTO (Data Transfer Object), который используется для передачи данных в запросе. Он реализует интерфейс IRequest из MediatR, указывая, что в ответ на эту команду ожидается Guid. В данном случае, Name — это имя создаваемого жанра.
        }
    }
    //Этот код демонстрирует использование паттерна CQRS для разделения бизнес-логики и представления в приложении, где логика создания нового жанра музыки или видео изолирована в специальном обработчике команд.
}