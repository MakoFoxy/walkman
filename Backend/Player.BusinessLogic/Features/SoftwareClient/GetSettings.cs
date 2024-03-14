using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Player.DataAccess;

namespace Player.BusinessLogic.Features.SoftwareClient
{
    public class GetSettings
    {
        public class Handler : IRequestHandler<Request, string>
        //Handler – это класс обработчика запросов, который реализует интерфейс IRequestHandler из MediatR. Этот обработчик предназначен для обработки запросов типа Request и возвращает строку, представляющую настройки клиента.
        {
            private readonly PlayerContext _context;

            public Handler(PlayerContext context)
            {
                _context = context;
                //Конструктор класса Handler, принимает контекст базы данных PlayerContext, который используется для выполнения запросов к базе данных и извлечения данных о настройках клиента.
            }

            public async Task<string> Handle(Request request, CancellationToken cancellationToken)
            {
                return await _context.Objects.Where(o => o.Id == request.ObjectId)
                    .Select(o => o.ClientSettings)
                    .SingleOrDefaultAsync(cancellationToken);
                //Асинхронный метод, который обрабатывает запрос Request. Используя LINQ и Entity Framework Core, метод выполняет запрос к базе данных, чтобы получить строку настроек для объекта (клиента), указанного в запросе по ObjectId. Если соответствующая запись существует, возвращается строка настроек; если нет — возвращается null.
            }
        }

        public class Request : IRequest<string>
        {
            public Guid ObjectId { get; set; }
            //Класс запроса, содержащий ObjectId — идентификатор объекта (клиента), для которого необходимо получить настройки. Этот класс реализует интерфейс IRequest<TResponse>, где TResponse в данном случае — это строка (настройки клиента).
        }
    }
    //Этот код можно использовать в части API, которая позволяет клиентам или администраторам системы получать текущие настройки программного клиента по его идентификатору. Это может быть полезно для диагностики, удаленного управления или мониторинга состояния клиентских устройств.

    // Пример использования может включать запросы к API для получения и изменения настроек плееров, установленных в различных точках воспроизведения контента (например, в магазинах, кафе и т.д.).
}