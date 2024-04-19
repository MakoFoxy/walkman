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

                var manager = await _context.Managers.Where(m => m.Id == managerModel.Id) //Используя Entity Framework, код выполняет поиск менеджера по идентификатору managerModel.Id.
                    .Include(m => m.User)
                    .ThenInclude(u => u.Objects)
                    .SingleAsync(cancellationToken);  //Загружает связанного пользователя (User) и связанные объекты (Objects) с помощью методов Include и ThenInclude.

                manager.User.FirstName = managerModel.FirstName;
                manager.User.LastName = managerModel.LastName;
                manager.User.SecondName = managerModel.SecondName;
                manager.User.Email = managerModel.Email;
                manager.User.PhoneNumber = managerModel.PhoneNumber;
                manager.User.RoleId = managerModel.Role.Id; //Обновляет основные атрибуты пользователя, такие как имя, фамилия, отчество, электронная почта и номер телефона, используя данные из managerModel. Обновляет роль пользователя, если она предоставлена в запросе.

                if (!string.IsNullOrWhiteSpace(managerModel.Password))
                {
                    manager.User.Password = managerModel.Password; //    Если в запросе предоставлен пароль и он не пуст, обновляет пароль пользователя.
                }

                var userObjectsForRemove = manager.User.Objects
                    .Where(uo => !managerModel.Objects.Select(o => o.Id).Contains(uo.ObjectId));
                //    Определяет, какие связи с объектами (UserObjects) необходимо удалить, основываясь на том, что их идентификаторы не включены в список идентификаторов объектов, предоставленных в managerModel.
                _context.RemoveRange(userObjectsForRemove); // удалит по id из массива ненужные UserObjects и удалить с RemoveRange     Удаляет эти объекты из контекста базы данных с помощью RemoveRange.


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
                        //    Для каждого объекта из managerModel, который еще не связан с пользователем, добавляет новую связь, создавая экземпляр UserObjects и добавляя его в контекст базы данных.
                    });
                }

                await _context.SaveChangesAsync(cancellationToken);
                return Unit.Value; //    Возвращает Unit.Value, что является стандартным способом указания на успешное выполнение команды в CQRS без возвращения конкретного результата.

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