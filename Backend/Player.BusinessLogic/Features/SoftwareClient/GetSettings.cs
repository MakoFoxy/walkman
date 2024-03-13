using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Player.DataAccess;

namespace Player.BusinessLogic.Features.SoftwareClient
{
    public class GetSettings
    {
        public class Handler : IRequestHandler<Request, string>
        {
            private readonly PlayerContext _context;

            public Handler(PlayerContext context)
            {
                _context = context;
            }
            
            public async Task<string> Handle(Request request, CancellationToken cancellationToken)
            {
                return await _context.Objects.Where(o => o.Id == request.ObjectId)
                    .Select(o => o.ClientSettings)
                    .SingleOrDefaultAsync(cancellationToken);
            }
        }

        public class Request : IRequest<string>
        {
            public Guid ObjectId { get; set; }
        }
    }
}