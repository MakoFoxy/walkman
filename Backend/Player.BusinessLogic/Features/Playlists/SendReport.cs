using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using Player.DataAccess;
using Player.Domain;

namespace Player.BusinessLogic.Features.Playlists
{
    public class SendReport
    {
        public class Handler : IRequestHandler<Command, Response>
        {
            private readonly PlayerContext _context;
            private readonly ILogger<Handler> _logger;

            public Handler(PlayerContext context, ILogger<Handler> logger)
            {
                _context = context;
                _logger = logger;
            }

            public async Task<Response> Handle(Command command, CancellationToken cancellationToken)
            {
                _logger.LogTrace("Report accepted: {@Report}", command);
                var entityEntry = _context.AdHistories.Add(new AdHistory
                {
                    AdvertId = command.AdvertId,
                    Start = command.Start,
                    End = command.End,
                    ObjectId = command.ObjectId
                });
                await _context.SaveChangesAsync(cancellationToken);
                
                return new Response
                {
                    ResponseId = entityEntry.Entity.Id
                };
            }
        }

        public class Command : IRequest<Response>
        {
            public Guid ObjectId { get; set; }
            public Guid AdvertId { get; set; }
            public DateTime Start { get; set; }
            public DateTime End { get; set; }
        }

        public class Response
        {
            public Guid ResponseId { get; set; }
        }
    }
}
