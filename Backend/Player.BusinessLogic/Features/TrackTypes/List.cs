using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Player.DataAccess;
using Player.DTOs;

namespace Player.BusinessLogic.Features.TrackTypes
{
    public class List
    {
        public class Handler : IRequestHandler<Query, List<TrackTypeModel>>
        {
            private readonly PlayerContext _context;
            private readonly IMapper _mapper;

            public Handler(PlayerContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public Task<List<TrackTypeModel>> Handle(Query request, CancellationToken cancellationToken = default)
            {
                return _context.TrackTypes.ProjectTo<TrackTypeModel>(_mapper.ConfigurationProvider).ToListAsync(cancellationToken);
            }
        }

        public class TrackTypeModel : SimpleDto
        {
        }

        public class Query : IRequest<List<TrackTypeModel>>
        {
        }
    }
}
