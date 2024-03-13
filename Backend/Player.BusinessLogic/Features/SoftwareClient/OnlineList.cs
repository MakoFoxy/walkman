using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Player.BusinessLogic.Hubs;
using Player.DataAccess;
using Player.DTOs;

namespace Player.BusinessLogic.Features.SoftwareClient
{
    public class OnlineList
    {
        public class Handler : IRequestHandler<Query, List<OnlineClient>>
        {
            private readonly PlayerContext _context;
            private readonly IMapper _mapper;

            public Handler(PlayerContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }
            
            public async Task<List<OnlineClient>> Handle(Query request, CancellationToken cancellationToken)
            {
                var connectedClients = PlayerClientHub.ConnectedClients.Select(cc => cc.Id).ToList();
                var onlineClients = await _context.Objects.Where(o => connectedClients.Contains(o.Id))
                    .Select(o => new OnlineClient
                    {
                        Id = o.Id,
                        Name = o.Name
                    })
                    .ToListAsync(cancellationToken);
                
                foreach (var onlineClient in onlineClients)
                {
                    var connectedClient = PlayerClientHub.ConnectedClients.First(cc => cc.Id == onlineClient.Id);
                    onlineClient.IpAddress = connectedClient.IpAddress;
                    onlineClient.Version = connectedClient.Version;
                }

                return onlineClients;
            }
        }

        public class Query : IRequest<List<OnlineClient>>
        {
            
        }

        public class OnlineClient : SimpleDto
        {
            public string IpAddress { get; set; }
            public string Version { get; set; }
        }
    }
}