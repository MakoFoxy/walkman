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

            public Handler(IUserManager userManager)
            {
                _userManager = userManager;
            }
            
            public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
            {
                var permission = await _userManager.GetCurrentUserPermissions(cancellationToken);

                return new Response
                {
                    Permissions = permission.Select(p => p.Code).ToList(),
                };
            }
        }

        public class Query : IRequest<Response>
        {
        }
        
        public class Response
        {
            public List<string> Permissions { get; set; } = new();
        }
    }
}