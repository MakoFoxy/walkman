using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Player.DataAccess;
using Player.DTOs;

namespace Player.BusinessLogic.Features.Selections
{
    public class Details
    {
        public class Handler : IRequestHandler<Query, SelectionModel>
        {
            private readonly PlayerContext _context;
            private readonly IMapper _mapper;

            public Handler(PlayerContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }
            
            public Task<SelectionModel> Handle(Query request, CancellationToken cancellationToken)
            {
                return _context.Selections.Where(o => o.Id == request.Id)
                    .Select(s => new SelectionModel
                    {
                        Id = s.Id,
                        Name = s.Name,
                        DateBegin = s.DateBegin,
                        DateEnd = s.DateEnd,
                        IsPublic = s.IsPublic,
                        Tracks = s.MusicTracks.Select(mt => mt.MusicTrack).Select(mt => new TrackModel
                        {
                            Id = mt.Id,
                            Name = mt.Name,
                            Length = mt.Length.TotalSeconds,
                        }).ToList(),
                    })
                    .SingleAsync(cancellationToken);
            }
        }

        public class Query : IRequest<SelectionModel>
        {
            public Guid Id { get; set; }
        }
        
        public class SelectionModel : SimpleDto
        {
            public DateTimeOffset DateBegin { get; set; }
            public DateTimeOffset? DateEnd { get; set; }
            public bool IsPublic { get; set; }
            public List<TrackModel> Tracks { get; set; } = new();
        }

        public class TrackModel : SimpleDto
        {
            public double Length { get; set; }
        }
    }
}