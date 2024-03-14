using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Player.Services.Abstractions;

namespace Player.BusinessLogic.Features.Users
{
    public class PermissionList
    {
        public class Handler : IRequestHandler<Query, Response>
        {
            private readonly IUserManager _userManager;
            //IUserManager: Сервис, используемый для получения данных о текущем пользователе и его разрешениях. Метод GetCurrentUserPermissions используется для получения списка разрешений, ассоциированных с текущим пользователем.

            public Handler(IUserManager userManager)
            {
                _userManager = userManager;
                //Handler: Класс, реализующий интерфейс IRequestHandler<Query, Response>. Этот класс отвечает за обработку запроса на получение списка разрешений текущего пользователя. В его конструкторе инжектируется зависимость IUserManager, которая предоставляет методы для работы с пользователями и их разрешениями.
            }

            public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
            {
                var permission = await _userManager.GetCurrentUserPermissions(cancellationToken);

                return new Response
                {
                    Permissions = permission.Select(p => p.Code).ToList(),
                };
                //             Процесс работы функции следующий:

                // Когда MediatR получает запрос Query, он направляет его к соответствующему обработчику Handler.
                // Внутри обработчика вызывается метод GetCurrentUserPermissions из IUserManager для извлечения списка разрешений, ассоциированных с текущим авторизованным пользователем.
                // Полученный список разрешений преобразуется в список кодов разрешений (каждый код разрешения представляет собой строку).
                // Список кодов разрешений упаковывается в объект Response и возвращается вызывающему коду.
            }
        }

        public class Query : IRequest<Response>
        {
            //Query: Пустой класс запроса, который служит триггером для выполнения операции. В этом случае, он не несёт никакой дополнительной информации.
        }

        public class Response
        {
            public List<string> Permissions { get; set; } = new();
            //Response: Класс ответа, который возвращает обработчик. Содержит список строковых идентификаторов разрешений (Permissions), которые присвоены текущему пользователю.
        }
    }
    //Таким образом, эта функция позволяет текущему пользователю узнать, какие у него есть разрешения в системе, что может быть полезно для управления доступом к различным функциям приложения.
}