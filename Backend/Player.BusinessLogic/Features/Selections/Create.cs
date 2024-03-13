using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Player.DataAccess;
using Player.Domain;
using Player.Services.Abstractions;
using UpdateSelectionModel = Player.BusinessLogic.Features.Selections.Models.UpdateSelectionModel;

namespace Player.BusinessLogic.Features.Selections
{
    public class Create
    {
        public class Handler : IRequestHandler<Command>
        {
            private readonly PlayerContext _context;
            private readonly IMapper _mapper;
            private readonly ITelegramMessageSender _telegramMessageSender;
            private readonly IUserManager _userManager;

            public Handler(
                PlayerContext context, 
                IMapper mapper,
                ITelegramMessageSender telegramMessageSender,
                IUserManager userManager)
            {
                _context = context;
                _mapper = mapper;
                _telegramMessageSender = telegramMessageSender;
                _userManager = userManager;
            }
            
            public async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
            {
                //TODO Вынести в валидацию
                if (await _context.Selections.AnyAsync(s => s.Name.ToLower() == request.Model.Name.ToLower()))
                {
                    throw new Exception("Selection exists");
                }
                
                var user = await _userManager.GetCurrentUser(cancellationToken);

                var organization = await _context.Organizations
                    .Where(o => o.Clients.Select(c => c.User).Contains(user))
                    .SingleOrDefaultAsync(cancellationToken);
                
                var model = request.Model;
                
                var selection = new Selection
                {
                    Name = model.Name,
                    DateBegin = request.Model.DateBegin,
                    DateEnd = request.Model.DateEnd,
                    Organization = organization,
                    IsPublic = request.Model.IsPublic,
                };

                for (var i = 0; i < model.Tracks.Count; i++)
                {
                    var musicTrackSelection = new MusicTrackSelection
                    {
                        Index = i,
                        MusicTrackId = model.Tracks[i],
                        Selection = selection,
                    };
                
                    selection.MusicTracks.Add(musicTrackSelection);
                }

                if (model.ObjectId.HasValue)
                {
                    selection.Objects.Add(new ObjectSelection
                    {
                        ObjectId = model.ObjectId.Value,
                        Selection = selection
                    });
                }

                _context.Selections.Add(selection);
                await _context.SaveChangesAsync(cancellationToken);
                return Unit.Value;
            }
        }

        public class Command : IRequest<Unit>
        {
            public UpdateSelectionModel Model { get; set; }
        }
    }
}