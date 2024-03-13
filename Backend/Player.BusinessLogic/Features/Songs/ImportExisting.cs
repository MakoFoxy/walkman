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
using Player.DTOs;
using Player.Services.Abstractions;

namespace Player.BusinessLogic.Features.Songs
{
    public class ImportExisting
    {
        public class Handler : IRequestHandler<Command>
        {
            private readonly PlayerContext _context;
            private readonly IConfiguration _configuration;
            private readonly IUserManager _userManager;
            private readonly IMusicTrackCreator _musicTrackCreator;

            public Handler(PlayerContext context,
                IConfiguration configuration,
                IUserManager userManager,
                IMusicTrackCreator musicTrackCreator)
            {
                _context = context;
                _configuration = configuration;
                _userManager = userManager;
                _musicTrackCreator = musicTrackCreator;
            }

            public async Task<Unit> Handle(Command command, CancellationToken cancellationToken)
            {
                var basePath = _configuration.GetValue<string>("Player:SongsPath");

                var files = Directory.GetFiles(basePath, "*.mp3", SearchOption.AllDirectories);
                var fileNames = files.Select(f => (string)Path.GetFileNameWithoutExtension(f));

                var tracksInDb = await _context.MusicTracks
                    .Select(mt => mt.Name)
                    .ToListAsync(cancellationToken);

                var newTracks = fileNames
                    .Where(fn => !tracksInDb.Contains(Path.GetFileNameWithoutExtension(fn)))
                    .Select(t => new SimpleDto
                    {
                        Name = t,
                    })
                    .ToList();
                
                var maxIndex = await _context.MusicTracks.OrderByDescending(mt => mt.Index).Select(mt => mt.Index)
                    .FirstOrDefaultAsync(cancellationToken);
                
                var musicTrackTypeId =
                    await _context.TrackTypes.Where(tt => tt.Code == TrackType.Music)
                        .Select(mt => mt.Id)
                        .SingleAsync(cancellationToken);
                
                var currentUser = await _userManager.GetCurrentUser(cancellationToken);

                var musicTracks = await _musicTrackCreator.CreateMusicTracks(new MusicTrackCreatorData
                {
                    Tracks = newTracks,
                    MaxIndex = maxIndex,
                    BasePath = basePath,
                    MusicTrackTypeId = musicTrackTypeId,
                    GenreId = command.GenreId,
                    UserId = currentUser.Id,
                });
                
                _context.MusicTracks.AddRange(musicTracks);
                await _context.SaveChangesAsync(cancellationToken);

                return Unit.Value;
            }
        }
        
        public class Command : IRequest<Unit>
        {
            public Guid GenreId { get; set; }
        }
    }
}