using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Player.DataAccess;
using Player.Domain;
using Player.Services.Abstractions;

namespace Player.Services.PlaylistServices;

public abstract class BasePlaylistGenerator : IPlaylistGenerator
{
    protected readonly List<string> DebugInfo = [];

    protected readonly PlayerContext _context;

    protected BasePlaylistGenerator(PlayerContext context)
    {
        _context = context;
    }
    
    public abstract Task<PlaylistGeneratorResult> Generate(ObjectInfo objectInfo, DateTime date, CancellationToken cancellationToken = default);
    public abstract Task<PlaylistGeneratorResult> Generate(Guid objectId, DateTime date, CancellationToken cancellationToken = default);

    [Pure]
    protected PlaylistGeneratorResult NotGeneratedStatus()
    {
        return new()
        {
            Status = PlaylistGeneratorStatus.NotGenerated
        };
    }

    [Pure]
    protected PlaylistGeneratorResult DeleteStatus(Playlist playlist)
    {
        return new()
        {
            Status = PlaylistGeneratorStatus.Delete,
            Playlist = playlist,
        };
    }
    
    [Pure]
    protected PlaylistGeneratorResult GeneratedStatus(
        Playlist playlist, 
        ICollection<string> debugInfo,
        ICollection<Advert> notFittedAdverts)
    {
        return new()
        {
            Status = PlaylistGeneratorStatus.Generated,
            Playlist = playlist,
            DebugInfo = debugInfo,
            NotFittedAdverts = notFittedAdverts,
        };
    }
    
    protected bool IsFreeDay(ObjectInfo objectInfo, DateTime date)
    {
        return objectInfo.FreeDays.Any(fd => fd == date.DayOfWeek);
    }
    
    protected async Task<PlaylistEnvelope> GetPlaylist(ObjectInfo objectInfo, DateTime date)
    {
        var playlist = await _context.Playlists
            .Where(p => p.Object == objectInfo && p.PlayingDate == date)
            .SingleOrDefaultAsync();

        if (playlist == null)
        {
            DebugInfo.Add("New playlist");
            return new PlaylistEnvelope
            {
                Playlist = new Playlist
                {
                    Id = Guid.NewGuid(),
                    Object = objectInfo,
                    PlayingDate = date,
                    CreateDate = DateTime.Now,
                },
                IsNew = true
            };
        }

        DebugInfo.Add("Existing playlist");
        await _context.Entry(playlist)
            .Collection(p => p.Aderts)
            .Query()
            .Include(ap => ap.Advert.TrackType)
            .LoadAsync();
        
        var adverts = playlist.Aderts.Select(ap => ap.Advert).ToList();
        await _context.AdTimes.Where(at => adverts.Contains(at.Advert)).LoadAsync();
        
        await _context.Entry(playlist)
            .Collection(p => p.MusicTracks)
            .Query().Include(mt => mt.MusicTrack.TrackType)
            .LoadAsync();

        return new PlaylistEnvelope
        {
            Playlist = playlist,
            IsNew = false
        };
    }
}