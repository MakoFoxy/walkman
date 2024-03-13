using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Player.BusinessLogic.Features.Managers.Models;
using Player.DataAccess;
using Player.Domain;

namespace Player.BusinessLogic.Features.Managers
{
    public class Edit
    {
        public class Handler : IRequestHandler<Command>
        {
            private readonly PlayerContext _context;

            public Handler(PlayerContext context)
            {
                _context = context;
                //Конструктор класса Handler, который получает и сохраняет контекст базы данных PlayerContext для дальнейшего использования.
            }

            public async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
            {
                var managerModel = request.Manager;

                var manager = await _context.Managers.Where(m => m.Id == managerModel.Id)
                    .Include(m => m.User)
                    .ThenInclude(u => u.Objects)
                    .SingleAsync(cancellationToken);

                manager.User.FirstName = managerModel.FirstName;
                manager.User.LastName = managerModel.LastName;
                manager.User.SecondName = managerModel.SecondName;
                manager.User.Email = managerModel.Email;
                manager.User.PhoneNumber = managerModel.PhoneNumber;
                manager.User.RoleId = managerModel.Role.Id;

                if (!string.IsNullOrWhiteSpace(managerModel.Password))
                {
                    manager.User.Password = managerModel.Password;
                }

                var userObjectsForRemove = manager.User.Objects
                    .Where(uo => !managerModel.Objects.Select(o => o.Id).Contains(uo.ObjectId));

                _context.RemoveRange(userObjectsForRemove);

                foreach (var modelObject in managerModel.Objects)
                {
                    if (manager.User.Objects.Any(uo => uo.ObjectId == modelObject.Id))
                    {
                        continue;
                    }

                    manager.User.Objects.Add(new UserObjects
                    {
                        UserId = manager.UserId,
                        ObjectId = modelObject.Id
                    });
                }

                await _context.SaveChangesAsync(cancellationToken);
                return Unit.Value;

                //Асинхронный метод Handle принимает запрос Command, содержащий модель менеджера (ManagerModel), и токен отмены операции. Этот метод выполняет ряд операций по обновлению данных менеджера в базе данных.
                // Внутри метода:
                //     Получение текущего менеджера: Извлекается существующая запись менеджера из базы данных по идентификатору, предоставленному в запросе.
                //     Обновление данных менеджера и пользователя: Атрибуты объекта manager обновляются новыми значениями из managerModel.
                //     Обновление пароля: Если в модели предоставлен новый пароль, он обновляется для пользователя.
                //     Удаление несвязанных объектов пользователя: Удаляются связи с объектами (например, отделами или проектами), которые больше не связаны с пользователем.
                //     Добавление новых объектов: Если у пользователя появились новые связи с объектами, они добавляются в базу данных.
            }
        }

        public class Command : IRequest<Unit>
        {
            public ManagerModel Manager { get; set; }
            //Класс Command представляет запрос на редактирование и содержит ManagerModel, который включает всю необходимую информацию для обновления записи менеджера. Реализация интерфейса IRequest<Unit> указывает, что в ответ на эту команду не ожидается возвращаемое значение.
        }

        //Когда команда на редактирование отправляется через MediatR, соответствующий обработчик Handler вызывается с этой командой. Обработчик извлекает необходимую информацию, обновляет данные в базе и сохраняет изменения.

        // Этот процесс позволяет централизованно обрабатывать запросы на изменение данных менеджера в системе, обеспечивая их валидность, последовательность и сохранность.

    }
}