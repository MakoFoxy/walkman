using System;
using System.IO;
using System.IO.Compression;
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

namespace Player.BusinessLogic.Features.Selections
{
    public class CreateFromArchives
    {
        public class Handler : IRequestHandler<Command>
        {
            private readonly IConfiguration _configuration;
            private readonly IMusicTrackCreator _musicTrackCreator;
            private readonly PlayerContext _context;

            public Handler(
                IConfiguration configuration,
                IMusicTrackCreator musicTrackCreator,
                PlayerContext context
                )
            {
                _configuration = configuration;
                _musicTrackCreator = musicTrackCreator;
                _context = context;
            }

            public async Task<Unit> Handle(Command command, CancellationToken cancellationToken)
            {
                var basePath = _configuration.GetValue<string>("Player:SongsPath");

                var archives = Directory.GetFiles(basePath, "*.zip");

                var maxIndex = await _context.MusicTracks.OrderByDescending(mt => mt.Index).Select(mt => mt.Index)
                    .FirstOrDefaultAsync(cancellationToken);
                
                var musicTrackTypeId =
                    await _context.TrackTypes.Where(tt => tt.Code == TrackType.Music)
                        .Select(mt => mt.Id)
                        .SingleAsync(cancellationToken);

                var genreId = await _context.Genres.Select(g => g.Id).FirstAsync(cancellationToken);

                var currentUser = await _context.Users
                    .Where(u => u.Email == User.SystemUserEmail)
                    .SingleAsync(cancellationToken);

                foreach (var archive in archives)
                {
                    var selectionName = Path.GetFileNameWithoutExtension(archive);
                    ZipFile.ExtractToDirectory(archive, basePath);
                    var destinationDirectoryName = Directory.GetDirectories(basePath).Single();

                    var songFiles = Directory.GetFiles(destinationDirectoryName).OrderBy(sf => sf).ToList();
                    var songFileNames = songFiles.Select(Path.GetFileName).ToList();

                    foreach (var songFile in songFiles)
                    {
                        var resultDestination = Path.Combine(basePath, Path.GetFileName(songFile));

                        if (!File.Exists(resultDestination))
                        {
                            File.Move(songFile, resultDestination);
                        }
                    }

                    var newTracks = songFileNames.Select(f => new SimpleDto
                    {
                        Name = Path.GetFileName(f),
                    }).ToList();

                    var musicTracks = await _musicTrackCreator.CreateMusicTracks(new MusicTrackCreatorData
                    {
                        BasePath = basePath,
                        GenreId = genreId,
                        MaxIndex = maxIndex,
                        UserId = currentUser.Id,
                        MusicTrackTypeId = musicTrackTypeId,
                        Tracks = newTracks,
                    });

                    _context.MusicTracks.AddRange(musicTracks);
                    var selection = new Selection
                    {
                        Name = selectionName,
                        DateBegin = DateTimeOffset.Now.AddDays(-1),
                        IsPublic = true,
                    };
                    var index = 0;
                    selection.MusicTracks = musicTracks.Select(mt => new MusicTrackSelection
                    {
                        Index = index++,
                        Selection = selection,
                        MusicTrack = mt,
                    }).ToList();
                    _context.Selections.Add(selection);
                    Directory.Delete(destinationDirectoryName, true);
                }

                await _context.SaveChangesAsync(cancellationToken);
                return Unit.Value;
            }
        }

        public class Command : IRequest<Unit>
        {
        }
    }
}