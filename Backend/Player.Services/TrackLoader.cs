using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Player.DataAccess;
using Player.Domain;
using Player.Helpers.Extensions;
using Player.Services.Abstractions;

namespace Player.Services
{
    public class TrackLoader : ITrackLoader
    {
        private readonly PlayerContext _context;

        public TrackLoader(PlayerContext context)
        {
            _context = context;
        }
        
        public async Task<ICollection<MusicTrack>> LoadForObject(ObjectInfo objectInfo, CancellationToken cancellationToken = default)
        {
            var now = DateTimeOffset.Now;
            var bannedTracks = await _context.BannedMusicInObject
                .Where(bm => bm.Object.Id == objectInfo.Id)
                .Select(bm => bm.MusicTrack)
                .ToListAsync(cancellationToken);
            
            var selections = await _context.Selections
                .Include(s => s.MusicTracks)
                .ThenInclude(mts => mts.MusicTrack)
                .ThenInclude(mt => mt.TrackType)
                .Where(s => s.Objects.Select(o => o.Object).Contains(objectInfo))
                .OrderByDescending(s => s.DateEnd)
                .Where(s => s.MusicTracks.Any())
                .ToListAsync(cancellationToken);

            if (!selections.Any())
            {
                var musicTracks = await _context.MusicTracks
                    .Include(mt => mt.TrackType)
                    .Where(mt => mt.TrackType.Code == TrackType.Music)
                    .Where(mt => !bannedTracks.Contains(mt))
                    .ToListAsync(cancellationToken);

                return musicTracks.Randomize();
            }

            var filteredSelections = selections
                .Where(s => s.DateBegin <= now && (s.DateEnd == null || s.DateEnd > now))
                .ToList();

            if (filteredSelections.Any())
            {
                selections = filteredSelections;
            }

            var resultedSelections = new List<Selection>(selections);

            while (TracksLengthInSelection(resultedSelections, bannedTracks) < objectInfo.WorkTime.TotalSeconds)
            {
                resultedSelections.AddRange(selections);
            }

            return resultedSelections
                .SelectMany(rs => rs.MusicTracks)
                .Select(mts => mts.MusicTrack)
                .Randomize()
                .ToList();
        }

        private static double TracksLengthInSelection(List<Selection> resultedSelections, List<MusicTrack> bannedTracks)
        {
            return resultedSelections.SelectMany(s => s.MusicTracks).Where(mt => !bannedTracks.Contains(mt.MusicTrack)).Sum(mts => mts.MusicTrack.Length.TotalSeconds);
        }

        public Task<ICollection<MusicTrack>> LoadForObject(ObjectInfo objectInfo, TimeSpan allTracksLength, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<ICollection<MusicTrack>> LoadForObject(TimeSpan allTracksLength, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}