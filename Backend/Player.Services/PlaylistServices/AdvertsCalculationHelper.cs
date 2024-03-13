using System;
using Player.Domain;

namespace Player.Services.PlaylistServices;

public static class AdvertsCalculationHelper
{
    public static readonly TimeSpan GapForClient = TimeSpan.FromSeconds(1);

    public static TimeSpan GetAdvertsBeginTime(ObjectInfo objectInfo)
    {
        var maxMusicBlockTimeInSeconds = GetMaxMusicBlockTimeInSeconds(objectInfo);

        // плейлист начиниается и заканчивается музыкой, делаем музыкальные блоки в начале и в конце
        var advertsBeginTime = objectInfo.BeginTime
            .Add(TimeSpan.FromSeconds(maxMusicBlockTimeInSeconds))
            .Add(GapForClient);
            
        return advertsBeginTime;
    }

    public static TimeSpan GetAdvertsEndTime(ObjectInfo objectInfo)
    {
        var maxMusicBlockTimeInSeconds = GetMaxMusicBlockTimeInSeconds(objectInfo);

        // плейлист начиниается и заканчивается музыкой, делаем музыкальные блоки в начале и в конце
        var advertsEndTime = objectInfo.EndTime
            .Subtract(TimeSpan.FromSeconds(maxMusicBlockTimeInSeconds))
            .Subtract(GapForClient);
            
        return advertsEndTime;
    }

    public static int GetMaxMusicBlockTimeInSeconds(ObjectInfo objectInfo)
    {
        var maxAdvertBlockInSeconds = objectInfo.MaxAdvertBlockInSeconds;
        var maxMusicBlockTimeInSeconds = maxAdvertBlockInSeconds * 2;
        return maxMusicBlockTimeInSeconds;
    }
}