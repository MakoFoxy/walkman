using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Player.BusinessLogic.Features.Clients.Models;
using Player.DataAccess;
using Player.Domain;

namespace Player.BusinessLogic.Features.Clients
{
    public class Edit
    {
        public class Handler : IRequestHandler<Command>
        {
            private readonly PlayerContext _context;
            private readonly IMapper _mapper;

            public Handler(PlayerContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }
            
            public async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
            {
                var client = _mapper.Map<Client>(request.ClientModel);

                _context.Entry(client).State = EntityState.Modified;
                await _context.SaveChangesAsync(cancellationToken);
                return Unit.Value;
            }
        }

        public class Command : IRequest<Unit>
        {
            public ClientModel ClientModel { get; set; }
        }
    }
}