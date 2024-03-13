using System;
using System.Linq;
using Player.Domain;
using Player.Services.Abstractions;
using Player.Services.PlaylistServices;

namespace Player.Services
{
    public class PlaylistLoadingCalculator : IPlaylistLoadingCalculator
    {
        public bool IsOverloaded(Playlist playlist)
        {
            return playlist.Aderts.Any(ap =>
                ap.Advert.AdTimes.First().RepeatCount >
                ap.Playlist.Aderts.Count(app => app.Advert == ap.Advert));
        }

        public double GetLoading(ObjectInfo objectInfo, Playlist playlist)
        {
            var advertLengthInPlaylist = GetAdvertLengthInPlaylist(playlist);
            var maxAdvertLength = GetMaxAdvertLength(objectInfo);

            return Math.Round(advertLengthInPlaylist * 100 / maxAdvertLength, 2);
        }

        public int GetOverflowCount(Playlist playlist, TimeSpan advertLength, int repeatCount)
        {
            var maxAdvertLength = GetMaxAdvertLength(playlist.Object);
            var advertLengthInPlaylist = GetAdvertLengthInPlaylist(playlist);

            if (advertLengthInPlaylist + TimeSpan.FromSeconds(advertLength.TotalSeconds * repeatCount) <= maxAdvertLength)
            {
                return 0;
            }

            for (var i = 1; i <= repeatCount; i++)
            {
                if (advertLengthInPlaylist + TimeSpan.FromSeconds(advertLength.TotalSeconds * i) >= maxAdvertLength)
                {
                    return repeatCount - (i - 1);
                }
            }

            return repeatCount;
        }

        private TimeSpan GetAdvertLengthInPlaylist(Playlist playlist)
        {
            return TimeSpan.FromMinutes(playlist.Aderts.Select(ap => ap.Advert.Length.TotalMinutes).Sum());
        }
        
        private TimeSpan GetMaxAdvertLength(ObjectInfo objectInfo)
        {
            var advertsBeginTime = AdvertsCalculationHelper.GetAdvertsBeginTime(objectInfo);
            var advertsEndTime = AdvertsCalculationHelper.GetAdvertsEndTime(objectInfo);
            
            var advertsTime = advertsEndTime - advertsBeginTime;

            return advertsTime * 0.3;
        }
    }
}