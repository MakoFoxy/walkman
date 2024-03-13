using System; // Импорт базового пространства имен для общих целей, например, для работы с типом Guid.
using System.Linq; // Подключение функционала LINQ для упрощения запросов к коллекциям.
using System.Threading.Tasks; // Импорт пространства имен для асинхронного программирования.
using Microsoft.AspNetCore.Authorization; // Использование функционала авторизации ASP.NET Core.
using Microsoft.EntityFrameworkCore; // Импорт Entity Framework Core для работы с базами данных.
using Microsoft.Extensions.Logging; // Импорт для работы с логгированием.
using Player.DataAccess; // Подключение пространства имен, связанного с доступом к данным в проекте Player.

namespace Player.AuthorizationLogic // Определение пространства имен, содержащего логику авторизации.
{
    public class PermissionHandler : AuthorizationHandler<PermissionRequirement> // Объявление класса PermissionHandler, который является обработчиком авторизации для проверки прав на основе PermissionRequirement.
    {
        private readonly PlayerContext _context; // Объявление приватного поля для контекста базы данных.
        private readonly ILogger<PermissionHandler> _logger; // Объявление приватного поля для логгера.

        public PermissionHandler(PlayerContext context, ILogger<PermissionHandler> logger) // Конструктор класса с внедрением зависимостей.
        {
            _context = context; // Присваивание контекста базы данных.
            _logger = logger; // Присваивание объекта логгера.
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement) // Асинхронный метод для обработки требований авторизации.
        {
            if (context.User.Identity?.Name == null) // Проверка, аутентифицирован ли пользователь.
            {
                _logger.LogWarning("Unauthorized request"); // Логгирование предупреждения о неавторизованном запросе.
                context.Fail(); // Указание на неудачное выполнение требования авторизации.
                return; // Прекращение выполнения метода.
            }

            var userId = Guid.Parse(context.User.Identity.Name); // Получение идентификатора пользователя из контекста авторизации.
            var hasPermission = await _context.Users.AnyAsync(u => // Асинхронная проверка наличия у пользователя необходимого разрешения.
                u.Id == userId && u.Role.RolePermissions
                    .Any(rp => rp.Permission.Code == requirement.Permission)); // Проверка соответствия роли пользователя требуемому разрешению.

            if (hasPermission) // Если у пользователя есть разрешение.
            {
                context.Succeed(requirement); // Указание на успешное выполнение требования авторизации.
            }
            else // Если у пользователя нет разрешения.
            {
                _logger.LogWarning("Permission denied for user {UserId} permission {Permission}", userId, requirement.Permission); // Логгирование предупреждения об отказе в доступе.
                context.Fail(); // Указание на неудачное выполнение требования авторизации.
            }
        }
    }
}
// Этот класс PermissionHandler реализует специфичную для приложения логику авторизации, проверяя, есть ли у аутентифицированного пользователя необходимое разрешение для выполнения определенного действия, определенного в PermissionRequirement. Если разрешение есть, авторизация проходит успешно. В противном случае доступ отклоняется, и в систему логирования выводится соответствующее предупреждение.