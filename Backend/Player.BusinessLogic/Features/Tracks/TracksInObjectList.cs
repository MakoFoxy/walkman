using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Player.DataAccess;
using Player.Domain;

namespace Player.BusinessLogic.Features.Tracks
{
    public class TracksInObjectList
    {
        public class Handler : IRequestHandler<Query, List<TrackInObject>>
        {
            private readonly PlayerContext _context;

            public Handler(PlayerContext context)
            {
                _context = context;
            }

            public async Task<List<TrackInObject>> Handle(Query query, CancellationToken cancellationToken)
            {
                var musicTracks = await _context.Playlists
                    .Where(p => p.Object.Id == query.ObjectId && p.PlayingDate == query.Date.Date)
                    .SelectMany(p => p.MusicTracks)
                    .Select(mt => new TrackInObject
                    {
                        Id = mt.MusicTrack.Id, 
                        Type = TrackType.Music,
                        Name = mt.MusicTrack.Name,
                        PlayingDateTime = mt.PlayingDateTime,
                    })
                    .ToListAsync(cancellationToken);

                var adverts = await _context.Playlists
                    .Where(p => p.Object.Id == query.ObjectId && p.PlayingDate == query.Date.Date)
                    .SelectMany(p => p.Aderts)
                    .Select(mt => new TrackInObject
                    {
                        Id = mt.Advert.Id,
                        Type = TrackType.Advert,
                        Name = mt.Advert.Name,
                        PlayingDateTime = mt.PlayingDateTime,
                    })
                    .ToListAsync(cancellationToken);

                var trackInPlaylistDtos = new List<TrackInObject>();
                trackInPlaylistDtos.AddRange(musicTracks);
                trackInPlaylistDtos.AddRange(adverts);

                return trackInPlaylistDtos;
            }
        }

        public class Query : IRequest<List<TrackInObject>>
        {
            public Guid ObjectId { get; set; }
            public DateTime Date { get; set; }
        }
        
        public class TrackInObject
        {
            public Guid Id { get; set; }
            public string Type { get; set; }
            public string Name { get; set; }
            public DateTime PlayingDateTime { get; set; }
        }
    }
}