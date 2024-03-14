using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Player.AuthorizationLogic;
using Player.DataAccess;
using Player.Domain;

namespace Player.BusinessLogic.Features.Users
{
    public class Authorize
    {
        public class Handler : IRequestHandler<Query, AuthorizeResult>
        {
            private readonly PlayerContext _context;
            private readonly ITokenGenerator _tokenGenerator;

            public Handler(PlayerContext context, ITokenGenerator tokenGenerator)
            {
                _context = context;
                _tokenGenerator = tokenGenerator;
                //Handler: Обработчик запроса авторизации, который реализует интерфейс IRequestHandler<Query, AuthorizeResult> из библиотеки MediatR. Обработчик использует контекст базы данных (PlayerContext) и генератор токенов (ITokenGenerator) для выполнения процедуры авторизации.

            }

            public async Task<AuthorizeResult> Handle(Query request, CancellationToken cancellationToken = default)
            {
                var user = await _context.Users
                    .Include(u => u.Role.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                    .SingleOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

                if (user?.Email == request.Email && user?.Password == request.Password)
                {
                    if (AllowToLogin(user))
                    {
                        return new AuthorizeResult
                        {
                            Token = _tokenGenerator.Generate(user),
                            IsSuccess = true
                            //В случае, если пользователь не найден или данные не совпадают, возвращается результат авторизации с флагом IsSuccess равным false (в данной реализации, пустой объект AuthorizeResult), что означает неуспешную авторизацию.
                        };
                    }
                }

                return new AuthorizeResult();

                //                 По полученным email и паролю из запроса, обработчик ищет пользователя в базе данных.
                // Если пользователь найден и его данные подтверждены (email и пароль совпадают), то проверяется, разрешен ли пользователю вход в систему (в данном случае, метод AllowToLogin всегда возвращает true, что означает, что вход разрешен всем пользователям, но этот механизм может быть изменен в соответствии с бизнес-требованиями).
                // Если условия авторизации удовлетворены, для пользователя генерируется токен, который возвращается в результате.
            }

            //TODO Понять нужно ли? Вход идет с мобилки, веба и клиента
            private bool AllowToLogin(User user)
            {
                return true;
                return user.Role.RolePermissions
                    .Any(rp => rp.Permission.Code == Permission.Login);
            }
        }

        public class AuthorizeResult
        {
            public string Token { get; set; }
            public bool IsSuccess { get; set; }
        }

        public class Query : IRequest<AuthorizeResult>
        {
            public string Email { get; set; }
            public string Password { get; set; }
        }
    }
}
