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
                            Token =_tokenGenerator.Generate(user),
                            IsSuccess = true
                        };
                    }
                }
                
                return new AuthorizeResult();
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
