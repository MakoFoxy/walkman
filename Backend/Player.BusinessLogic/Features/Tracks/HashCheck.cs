using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Player.DataAccess;
using Player.Domain;

namespace Player.BusinessLogic.Features.Tracks
{
    public class HashCheck
    {
        public class Handler : IRequestHandler<Query, Response>
        {
            private readonly PlayerContext _context;

            public Handler(PlayerContext context)
            {
                _context = context;
            }

            public async Task<Response> Handle(Query query, CancellationToken cancellationToken)
            {
                IQueryable<Track> tracksQuery = query.TrackType switch
                {
                    TrackType.Advert => _context.Adverts.AsQueryable(),
                    TrackType.Music => _context.MusicTracks.AsQueryable(),
                    _ => throw new ArgumentException(nameof(query.TrackType))
                };
                
                var hash = await tracksQuery.Where(a => a.Id == query.TrackId)
                    .Select(a => a.Hash)
                    .SingleAsync(cancellationToken);

                return new Response
                {
                    TrackIsCorrect = hash == query.Hash
                };
            }
        }

        public class Query : IRequest<Response>
        {
            public string TrackType { get; set; }
            public Guid TrackId { get; set; }
            public string Hash { get; set; }
        }

        public class Response
        {
            public bool TrackIsCorrect { get; set; }
        }
    }
}
