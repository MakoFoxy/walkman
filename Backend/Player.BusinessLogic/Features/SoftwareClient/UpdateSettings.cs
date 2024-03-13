using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Player.DataAccess;

namespace Player.BusinessLogic.Features.SoftwareClient
{
    public class UpdateSettings
    {
        public class Handler : IRequestHandler<Command>
        {
            private readonly PlayerContext _context;

            public Handler(PlayerContext context)
            {
                _context = context;
            }
            
            public async Task<Unit> Handle(Command command, CancellationToken cancellationToken)
            {
                var objectInfo = await _context.Objects.SingleAsync(o => o.Id == command.ObjectId, cancellationToken);
                objectInfo.ClientSettings = command.Settings;

                await _context.SaveChangesAsync(cancellationToken);
                return Unit.Value;
            }
        }

        public class Command : IRequest<Unit>
        {
            public Guid ObjectId { get; set; }
            public string Settings { get; set; }
        }
    }
}