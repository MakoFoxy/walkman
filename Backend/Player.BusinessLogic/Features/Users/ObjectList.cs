using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Player.DataAccess;
using Player.DTOs;
using Player.Services.Abstractions;

namespace Player.BusinessLogic.Features.Users
{
    public class ObjectList
    {
        public class Handler : IRequestHandler<Query, Response>
        {
            private readonly IUserManager _userManager;
            //IUserManager: Сервис, предоставляющий функционал для работы с информацией о пользователях. В данном случае, используется метод GetCurrentUser, который возвращает текущего авторизованного пользователя из контекста приложения.
            private readonly PlayerContext _context;
            //PlayerContext: Контекст базы данных, предоставляющий доступ к данным приложения. Используется для запроса информации из базы данных.

            public Handler(IUserManager userManager, PlayerContext context)
            {
                _userManager = userManager;
                _context = context;
                //Handler: Класс, реализующий интерфейс IRequestHandler<Query, Response>. Он отвечает за обработку запроса. В его конструкторе инжектируются зависимости IUserManager, который используется для работы с пользователями, и PlayerContext, который представляет контекст базы данных.

            }

            public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
            {
                var currentUser = await _userManager.GetCurrentUser(cancellationToken);

                var objects = await _context.Users.Where(u => u == currentUser)
                    .SelectMany(u => u.Objects)
                    .Select(uo => new SimpleDto
                    {
                        Id = uo.Object.Id,
                        Name = uo.Object.Name,
                    })
                    .ToListAsync(cancellationToken);

                return new Response
                {
                    Objects = objects,
                };
                //Процесс работы функции следующий:

                // При получении запроса Query, MediatR направляет его обработчику Handler.
                // Обработчик использует IUserManager для получения информации о текущем пользователе.
                // Затем он делает запрос к базе данных через PlayerContext, чтобы получить список объектов, связанных с этим пользователем.
                // Информация об объектах преобразуется в список SimpleDto и возвращается в виде объекта Response.
            }
        }

        public class Query : IRequest<Response>
        {
            //Query: Класс запроса, который передаётся в обработчик. В данном случае, он не содержит данных и служит просто триггером для операции.
        }

        public class Response
        {
            public List<SimpleDto> Objects { get; set; } = new();
            //Response: Класс ответа, который возвращает обработчик. Содержит список объектов, связанных с текущим пользователем. Каждый объект представлен в виде SimpleDto, который содержит идентификатор и имя объекта.
        }
    }
    //Таким образом, эта функция позволяет текущему пользователю получить информацию обо всех объектах, с которыми он ассоциирован, в удобном и структурированном формате.
}