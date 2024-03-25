using System;
using System.Linq;
using Player.Domain;
using Player.Services.Abstractions;
using Player.Services.PlaylistServices;

namespace Player.Services
{
    public class PlaylistLoadingCalculator : IPlaylistLoadingCalculator
    { //Этот код представляет собой часть системы управления плейлистами, в частности класс PlaylistLoadingCalculator, реализующий интерфейс IPlaylistLoadingCalculator. Этот класс предназначен для расчета загрузки плейлиста рекламой и определения перегружен ли плейлист. Разберем функциональность методов этого класса:
        public bool IsOverloaded(Playlist playlist)
        { //Определяет, превышено ли количество повторений рекламы в плейлисте. Он проверяет, есть ли реклама в плейлисте, количество повторений которой превышает общее количество ее размещений в этом плейлисте. Это служит индикатором перегрузки плейлиста рекламой.
            return playlist.Aderts.Any(ap =>
                ap.Advert.AdTimes.First().RepeatCount >
                ap.Playlist.Aderts.Count(app => app.Advert == ap.Advert));
        }

        public double GetLoading(ObjectInfo objectInfo, Playlist playlist)
        {//Вычисляет общую загрузку плейлиста рекламой в процентах. Сначала он получает общую продолжительность рекламы в плейлисте (advertLengthInPlaylist) и максимально допустимую продолжительность рекламы для данного объекта (maxAdvertLength). Затем вычисляется отношение общей продолжительности рекламы к максимально допустимой, результат умножается на 100 и округляется до двух знаков после запятой.
            var advertLengthInPlaylist = GetAdvertLengthInPlaylist(playlist);
            var maxAdvertLength = GetMaxAdvertLength(objectInfo);

            return Math.Round(advertLengthInPlaylist * 100 / maxAdvertLength, 2);
        }

        public int GetOverflowCount(Playlist playlist, TimeSpan advertLength, int repeatCount)
        { //Определяет количество повторений рекламы, которые не помещаются в плейлист без превышения максимальной допустимой продолжительности рекламы. Если добавление всех повторений рекламы в плейлист не превышает максимально допустимую продолжительность, возвращается 0. В противном случае метод вычисляет, начиная с какого повторения общая длительность рекламы превышает допустимый предел, и возвращает количество повторений, которые не укладываются в этот предел.
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
            return TimeSpan.FromMinutes(playlist.Aderts.Select(ap => ap.Advert.Length.TotalMinutes).Sum()); //GetAdvertLengthInPlaylist вычисляет общую длительность всех рекламных блоков в плейлисте.
        }

        private TimeSpan GetMaxAdvertLength(ObjectInfo objectInfo)
        { //GetMaxAdvertLength определяет максимально допустимую длительность рекламы для данного объекта, исходя из общего времени, отведенного для рекламы (как правило, это 30% от общего времени рекламных блоков, доступных объекту).
            var advertsBeginTime = AdvertsCalculationHelper.GetAdvertsBeginTime(objectInfo);
            var advertsEndTime = AdvertsCalculationHelper.GetAdvertsEndTime(objectInfo);

            var advertsTime = advertsEndTime - advertsBeginTime;

            return advertsTime * 0.3;
        }
    }
    //Эти методы помогают управлять рекламными блоками в медийных плейлистах, оптимизируя их длительность и количество в соответствии с установленными правилами и ограничениями.
}