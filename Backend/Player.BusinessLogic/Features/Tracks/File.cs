using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Player.DataAccess;
using Player.Domain;

namespace Player.BusinessLogic.Features.Tracks
{
    public class File
    {
        public class Handler : IRequestHandler<Query, Response>
        {
            private readonly PlayerContext _context;
            private readonly IConfiguration _configuration;


            public Handler(PlayerContext context, IConfiguration configuration)
            {
                _context = context;
                _configuration = configuration;
            }

            public async Task<Response> Handle(Query query, CancellationToken cancellationToken)
            {
                string basePath;

                Track track;

                switch (query.TrackType)
                {
                    case TrackType.Advert:
                    {
                        track = await _context.Adverts.Where(a => a.Id == query.TrackId).SingleAsync(cancellationToken);
                        basePath = _configuration.GetValue<string>("Player:AdvertsPath");
                        break;
                    }
                    case TrackType.Music:
                    {
                        track = await _context.MusicTracks.Where(m => m.Id == query.TrackId).SingleAsync(cancellationToken);
                        basePath = _configuration.GetValue<string>("Player:SongsPath");
                        break;
                    }
                    default:
                        throw new ArgumentException(nameof(query.TrackType));
                }
                
                return new Response
                {
                    FileStream = System.IO.File.OpenRead(Path.Combine(basePath, track.FilePath)),
                    FileName = track.Name + track.Extension
                };
            }
        }

        public class Query : IRequest<Response>
        {
            public string TrackType { get; set; }
            public Guid TrackId { get; set; }
        }

        public class Response
        {
            public Stream FileStream { get; set; }
            public string FileName { get; set; }
        }
    }
}
