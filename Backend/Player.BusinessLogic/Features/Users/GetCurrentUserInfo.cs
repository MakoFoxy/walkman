using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Player.ClientIntegration;
using Player.Services.Abstractions;

namespace Player.BusinessLogic.Features.Users
{
    public class GetCurrentUserInfo
    {
        public class Handler : IRequestHandler<Query, Response>
        {
            private readonly IUserManager _userManager;

            public Handler(IUserManager userManager)
            {
                _userManager = userManager;
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
            }
        }

        public class Query : IRequest<Response>
        {
        }

        public class Response
        {
            public CurrentUserInfoDto CurrentUserInfo { get; set; }
        }
    }
}