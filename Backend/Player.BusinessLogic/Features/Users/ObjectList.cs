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
            private readonly PlayerContext _context;

            public Handler(IUserManager userManager, PlayerContext context)
            {
                _userManager = userManager;
                _context = context;
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
            }
        }

        public class Query : IRequest<Response>
        {
        }
        
        public class Response
        {
            public List<SimpleDto> Objects { get; set; } = new();
        }
    }
}