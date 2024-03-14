using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Player.DataAccess;

namespace Player.BusinessLogic.Features.SoftwareClient
{
    public class UpdateSettings
    {
        public class Handler : IRequestHandler<Command>
        {
            private readonly PlayerContext _context;

            public Handler(PlayerContext context)
            {
                _context = context;
                //Конструктор класса Handler принимает контекст базы данных PlayerContext, который используется для выполнения запросов к базе данных.
            }

            public async Task<Unit> Handle(Command command, CancellationToken cancellationToken)
            {
                var objectInfo = await _context.Objects.SingleAsync(o => o.Id == command.ObjectId, cancellationToken);
                objectInfo.ClientSettings = command.Settings;

                await _context.SaveChangesAsync(cancellationToken);
                return Unit.Value;
                //Это асинхронный метод, который обрабатывает команду Command. Метод находит в базе данных объект (клиент) по его идентификатору, обновляет его настройки клиента строкой Settings, полученной из команды, и сохраняет изменения в базе данных.
            }
        }

        public class Command : IRequest<Unit>
        {
            public Guid ObjectId { get; set; }
            public string Settings { get; set; }
            //Command — класс команды, который передает данные, необходимые для обновления настроек клиента. Включает в себя ObjectId (идентификатор объекта, настройки которого необходимо обновить) и Settings (новые настройки в виде строки).
        }
    }
    //Этот код может быть использован для обновления настроек клиента, например, в системах управления мультимедийным контентом или в системах централизованного контроля устройств. Администраторы или автоматические системы могут отправлять команды на обновление настроек для определенных клиентских устройств, и эти изменения будут мгновенно применены и сохранены для последующего использования.
}