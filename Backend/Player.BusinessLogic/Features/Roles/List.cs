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

namespace Player.BusinessLogic.Features.Roles
{
    public class List
    {
        public class Handler : IRequestHandler<Query, List<RoleModel>>
        {
            private readonly PlayerContext _context;
            private readonly IMapper _mapper;

            public Handler(PlayerContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public Task<List<RoleModel>> Handle(Query request, CancellationToken cancellationToken = default)
            {
                var query = _context.Roles.AsQueryable();

                query = request.Filter switch
                {
                    RoleFilter.All => query,
                    RoleFilter.Admin => query.Where(r => r.IsAdminRole),
                    RoleFilter.Client => query.Where(r => !r.IsAdminRole),
                    _ => throw new ArgumentOutOfRangeException(nameof(request.Filter))
                };
                
                return query.ProjectTo<RoleModel>(_mapper.ConfigurationProvider).ToListAsync(cancellationToken);
            }
        }

        public class RoleModel : SimpleDto
        {
        }

        public enum RoleFilter
        {
            All,
            Admin,
            Client
        }

        public class Query : IRequest<List<RoleModel>>
        {
            public RoleFilter Filter { get; set; }
        }
    }
}
