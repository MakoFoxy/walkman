using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using Player.DataAccess;
using Player.Domain;

namespace Player.BusinessLogic.Features.Playlists
{
    public class SendReport
    {
        public class Handler : IRequestHandler<Command, Response>
        {
            private readonly PlayerContext _context;
            private readonly ILogger<Handler> _logger;

            public Handler(PlayerContext context, ILogger<Handler> logger)
            {
                _context = context;
                _logger = logger;
                //Конструктор принимает две зависимости: контекст данных и логгер. Контекст данных используется для взаимодействия с базой данных, а логгер — для регистрации информации о процессе выполнения.

            }

            public async Task<Response> Handle(Command command, CancellationToken cancellationToken)
            {
                _logger.LogTrace("Report accepted: {@Report}", command);
                var entityEntry = _context.AdHistories.Add(new AdHistory
                {
                    AdvertId = command.AdvertId,
                    Start = command.Start,
                    End = command.End,
                    ObjectId = command.ObjectId
                });
                await _context.SaveChangesAsync(cancellationToken);

                return new Response
                {
                    ResponseId = entityEntry.Entity.Id
                };

                //                 Это основной метод обработчика. Он регистрирует в журнале информацию о полученном отчете (command), создает новый экземпляр AdHistory с данными из команды, добавляет его в базу данных и сохраняет изменения. В ответ возвращается идентификатор созданной записи.
                // Классы Command и Response:

                //     Command содержит данные, необходимые для создания записи в истории рекламы, включая идентификатор объекта (ObjectId), идентификатор рекламы (AdvertId), а также время начала (Start) и окончания (End) воспроизведения рекламы.
                //     Response возвращает идентификатор созданной записи (ResponseId) после успешного выполнения операции.

                // Логика работы:

                //     При получении команды на отправку отчета метод Handle регистрирует этот факт в системе логирования.
                //     Создается новый объект AdHistory с данными из команды.
                //     Этот объект добавляется в контекст данных, что готовит его к сохранению в базе данных.
                //     Вызывается SaveChangesAsync для фиксации изменений в базе данных.
                //     Создается и возвращается ответ, содержащий идентификатор новой записи в истории рекламы.
            }
        }

        public class Command : IRequest<Response>
        {
            public Guid ObjectId { get; set; }
            public Guid AdvertId { get; set; }
            public DateTime Start { get; set; }
            public DateTime End { get; set; }
        }

        public class Response
        {
            public Guid ResponseId { get; set; }
        }
        //Таким образом, код обеспечивает возможность отслеживания и анализа использования рекламных блоков в системе, сохраняя информацию о каждом показе рекламы.

    }
}