using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Player.DataAccess;
using Player.DataAccess.Extensions;
using Player.Domain;
using Player.Helpers.Extensions;
using Player.Services.Abstractions;

namespace Player.Services.PlaylistServices;

public class PlaylistGenerator : BasePlaylistGenerator
{
    private readonly ITrackLoader _trackLoader;
    private readonly IPlaylistLoadingCalculator _playlistLoadingCalculator;

    private int _maxAdvertBlockInSeconds;
        
    public PlaylistGenerator(
        PlayerContext context,
        ITrackLoader trackLoader,
        IPlaylistLoadingCalculator playlistLoadingCalculator) : base(context)
    {
        _trackLoader = trackLoader;
        _playlistLoadingCalculator = playlistLoadingCalculator;
    }

    public override async Task<PlaylistGeneratorResult> Generate(ObjectInfo objectInfo, DateTime date, CancellationToken cancellationToken = default)
    {
        var playlistEnvelope = await GetPlaylist(objectInfo, date);

        if (IsFreeDay(objectInfo, date))
        {
            if (playlistEnvelope.IsNew)
            {
                return NotGeneratedStatus();
            }

            return DeleteStatus(playlistEnvelope.Playlist);
        }

        _maxAdvertBlockInSeconds = objectInfo.MaxAdvertBlockInSeconds;

        var allAdverts = new List<Advert>();
            
        var oldAdvertsInPlaylist = playlistEnvelope.Playlist.Aderts
            .Select(ap => ap.Advert)
            .OrderBy(a => a.CreateDate)
            .ToList();
        allAdverts.AddRange(oldAdvertsInPlaylist);

        var distinctOldAdverts = playlistEnvelope.Playlist.Aderts
            .Select(ap => ap.Advert)
            .Distinct()
            .OrderBy(a => a.CreateDate)
            .ToList();
        DebugInfo.Add($"Adverts in playlists {string.Join(";", distinctOldAdverts.Select(a => a.Name))}");

        foreach (var oldAdvert in distinctOldAdverts)
        {
            var adTime = oldAdvert.AdTimes.Single(at => at.PlayDate == date && at.Object == objectInfo);

            if (adTime.RepeatCount == oldAdvertsInPlaylist.Count(a => a == oldAdvert))
            {
                continue;
            }

            for (var i = 0; i < adTime.RepeatCount - oldAdvertsInPlaylist.Count(a => a == oldAdvert); i++)
            {
                allAdverts.Add(oldAdvert);
            }
        }
            
        var newAdverts = await _context.GetValidTracksOn(date)
            .Include(a => a.TrackType)
            .Include(a => a.AdTimes)
            .Where(a => a.AdTimes.Any(at => at.Object == objectInfo))
            .Where(a => !distinctOldAdverts.Contains(a))
            .OrderBy(a => a.CreateDate)
            .ToListAsync(cancellationToken);

        DebugInfo.Add($"New adverts {string.Join(";", newAdverts.Select(a => a.Name))}");
        foreach (var newAdvert in newAdverts)
        {
            var adTime = newAdvert.AdTimes.Single(at => at.PlayDate == date && at.Object == objectInfo);

            for (var i = 0; i < adTime.RepeatCount; i++)
            {
                allAdverts.Add(newAdvert);
            }
        }

        var musicTracks = await _trackLoader.LoadForObject(objectInfo, cancellationToken);

        PopulateMusicTracks(playlistEnvelope.Playlist, objectInfo, musicTracks);
        var notFittedAdverts = PopulateAdverts(playlistEnvelope.Playlist, objectInfo, allAdverts);
        
        CalculatePlaylistLoading(objectInfo, playlistEnvelope);
            
        playlistEnvelope.Playlist.PlaylistInfos.Add(new PlaylistInfo
        {
            Info = string.Join(Environment.NewLine, DebugInfo),
            CreateDate = DateTime.Now,
        });
            
        return GeneratedStatus(playlistEnvelope.Playlist, DebugInfo, notFittedAdverts);
    }

    private List<Advert> PopulateAdverts(Playlist playlist, ObjectInfo objectInfo, ICollection<Advert> adverts)
    {        
        playlist.Aderts.Clear();
        var maxAdvertBlockInSecondsWithSmallGap = _maxAdvertBlockInSeconds + 3;
        var maxMusicBlockTimeInSeconds = AdvertsCalculationHelper.GetMaxMusicBlockTimeInSeconds(objectInfo);

        // плейлист начиниается и заканчивается музыкой, делаем музыкальные блоки в начале и в конце
        var advertsBeginTime = AdvertsCalculationHelper.GetAdvertsBeginTime(objectInfo);
        var advertsEndTime = AdvertsCalculationHelper.GetAdvertsEndTime(objectInfo);

        var currentTime = advertsBeginTime;

        var remainingAdverts = adverts.ToList();

        while (currentTime <= advertsEndTime)
        {
            var advertBlock = new List<Advert>();

            if (!remainingAdverts.Any())
            {
                currentTime = currentTime.RoundUp()
                    .Add(TimeSpan.FromSeconds(maxMusicBlockTimeInSeconds))
                    .Add(AdvertsCalculationHelper.GapForClient);
                continue;
            }

            foreach (var remainingAdvert in remainingAdverts)
            {
                var possibleBlockLength = advertBlock.Sum(ab => ab.Length.TotalSeconds) +
                                          remainingAdvert.Length.TotalSeconds;

                if (possibleBlockLength > maxAdvertBlockInSecondsWithSmallGap)
                {
                    continue;
                }

                if (!advertBlock.Contains(remainingAdvert))
                {
                    advertBlock.Add(remainingAdvert);
                }
            }

            foreach (var advert in advertBlock)
            {
                var advertPlaylist = new AdvertPlaylist
                {
                    Playlist = playlist,
                    PlayingDateTime = playlist.PlayingDate.Add(currentTime),
                    Advert = advert
                };
                playlist.Aderts.Add(advertPlaylist);
                DebugInfo.Add($"Advert {advert.Name} - {advertPlaylist.PlayingDateTime:HH:mm:ss}");
                currentTime += advert.Length.RoundUp().Add(AdvertsCalculationHelper.GapForClient);
            }

            var totalBlockLength = TimeSpan.FromSeconds(advertBlock.Sum(ab => ab.Length.TotalSeconds));
            DebugInfo.Add($"Block length = {totalBlockLength}");

            currentTime = currentTime.RoundUp()
                .Add(TimeSpan.FromSeconds(maxMusicBlockTimeInSeconds))
                .Add(AdvertsCalculationHelper.GapForClient);

            foreach (var advert in advertBlock)
            {
                remainingAdverts.Remove(advert);
            }
        }

        return remainingAdverts;
    }

    private void PopulateMusicTracks(Playlist playlist, ObjectInfo objectInfo, ICollection<MusicTrack> musicTracks)
    {
        playlist.MusicTracks.Clear();
        var currentTime = objectInfo.BeginTime;

        using IEnumerator<MusicTrack> musicTrackEnumerator = musicTracks.GetEnumerator();

        // todo проверить 24 часа
        while (currentTime <= objectInfo.EndTime)
        {
            var currentTrack = GetCurrentMusicTrack(musicTrackEnumerator);

            var musicTrackPlaylist = new MusicTrackPlaylist
            {
                Playlist = playlist,
                MusicTrack =  currentTrack,
                PlayingDateTime = playlist.PlayingDate.Add(currentTime),
            };
            playlist.MusicTracks.Add(musicTrackPlaylist);
            DebugInfo.Add($"MusicTrack {currentTrack.Name} - {musicTrackPlaylist.PlayingDateTime:HH:mm:ss}");

            currentTime += currentTrack.Length.RoundUp().Add(AdvertsCalculationHelper.GapForClient);
        }

        MusicTrack GetCurrentMusicTrack(IEnumerator<MusicTrack> enumerator)
        {
            if (enumerator.MoveNext())
            {
                return enumerator.Current;
            }
            
            enumerator.Reset();
            return GetCurrentMusicTrack(enumerator);
        }
    }

    public override async Task<PlaylistGeneratorResult> Generate(Guid objectId, DateTime date, CancellationToken cancellationToken = default)
    {
        var objectInfo = await _context.Objects.SingleAsync(o => o.Id == objectId, cancellationToken);
        return await Generate(objectInfo, date, cancellationToken);
    }

    private void CalculatePlaylistLoading(ObjectInfo objectInfo, PlaylistEnvelope playlistEnvelope)
    {
        playlistEnvelope.Playlist.Loading = _playlistLoadingCalculator.GetLoading(objectInfo, playlistEnvelope.Playlist);
        playlistEnvelope.Playlist.UniqueAdvertsCount = playlistEnvelope.Playlist.Aderts.Select(a => a.Advert).Distinct().Count();
        playlistEnvelope.Playlist.AdvertsCount = playlistEnvelope.Playlist.Aderts.Select(a => a.Advert).Count();

        playlistEnvelope.Playlist.Overloaded = _playlistLoadingCalculator.IsOverloaded(playlistEnvelope.Playlist);

        DebugInfo.Add(
            $"Loading\t{playlistEnvelope.Playlist.Loading}\t" +
            $"UniqueAdvertsCount\t{playlistEnvelope.Playlist.UniqueAdvertsCount}\t" +
            $"AdvertsCount\t{playlistEnvelope.Playlist.AdvertsCount}\t" +
            $"Overloaded\t{playlistEnvelope.Playlist.Overloaded}");
    }
}