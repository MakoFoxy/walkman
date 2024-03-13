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
    public class Edit
    {
        public class Handler : IRequestHandler<Command>
        {
            private readonly PlayerContext _context;
            private readonly ITelegramMessageSender _telegramMessageSender;
            private readonly IMapper _mapper;

            public Handler(PlayerContext context, ITelegramMessageSender telegramMessageSender, IMapper mapper)
            {
                _context = context;
                _telegramMessageSender = telegramMessageSender;
                _mapper = mapper;
            }
            
            public async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
            {
                var model = request.Model;
                var selection = await _context.Selections
                    .Include(s => s.MusicTracks)
                    .SingleAsync(s => s.Id == model.Id, cancellationToken);
                selection.Name = model.Name;
                selection.DateBegin = model.DateBegin;
                selection.DateEnd = model.DateEnd;
                selection.IsPublic = request.Model.IsPublic;
                selection.MusicTracks.Clear();

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