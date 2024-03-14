using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Player.ClientIntegration;
using Player.Services.Abstractions;

namespace Player.BusinessLogic.Features.Users
{
    public class GetCurrentUserInfo
    //CurrentUserInfoDto: DTO, который содержит информацию о текущем пользователе. В данном случае, он содержит только поле Email, но может быть расширен другими полями по мере необходимости.
    {
        public class Handler : IRequestHandler<Query, Response>
        {
            private readonly IUserManager _userManager;
            //IUserManager: Сервис, используемый для получения данных текущего пользователя. Это абстракция, позволяющая извлекать данные пользователя из источника (обычно это база данных, но может быть и что-то иное в зависимости от реализации). В данном случае, метод GetCurrentUser сервиса используется для получения экземпляра текущего пользователя.
            public Handler(IUserManager userManager)
            {
                _userManager = userManager;
                //Handler: Это класс обработчика, который реализует интерфейс IRequestHandler<Query, Response>, предоставляемый библиотекой MediatR. Задача обработчика – получить информацию о текущем пользователе и вернуть её в формате, ожидаемом клиентом. Внутри обработчика используется сервис IUserManager, предназначенный для работы с данными пользователя.
            }

            public async Task<Response> Handle(Query query, CancellationToken cancellationToken)
            {
                var currentUser = await _userManager.GetCurrentUser(cancellationToken);

                return new Response
                {
                    CurrentUserInfo = new CurrentUserInfoDto
                    {
                        Email = currentUser.Email,
                    },
                };
                //             Процесс работы функции выглядит следующим образом:

                // MediatR получает запрос Query.
                // MediatR передаёт запрос Query обработчику Handler.
                // Обработчик использует IUserManager для получения текущего пользователя.
                // После получения пользователя, обработчик формирует и возвращает объект Response, который содержит информацию о пользователе в виде CurrentUserInfoDto.
            }
        }

        public class Query : IRequest<Response>
        {
            //Query: Класс запроса, который в данном случае не содержит полей и служит просто как сигнал для MediatR о необходимости выполнения определённой операции (получение информации о текущем пользователе).
        }

        public class Response
        {
            public CurrentUserInfoDto CurrentUserInfo { get; set; }
            //Response: Класс ответа, содержащий информацию о пользователе. В данном случае, в ответе содержится объект CurrentUserInfoDto, который включает в себя необходимые данные пользователя (например, email). Этот объект представляет собой DTO (Data Transfer Object) - паттерн для передачи данных между подсистемами приложения.
        }
    }
}