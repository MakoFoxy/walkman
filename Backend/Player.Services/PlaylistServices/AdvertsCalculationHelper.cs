using System;
using Player.Domain;

namespace Player.Services.PlaylistServices;

public static class AdvertsCalculationHelper
{
    public static readonly TimeSpan GapForClient = TimeSpan.FromSeconds(1);
    //    GapForClient: Это константа, обозначающая временной промежуток (задержку) между различными сегментами в плейлисте, здесь задана как 1 секунда. Этот зазор используется для обеспечения небольшой паузы между музыкальными и рекламными блоками.

    public static TimeSpan GetAdvertsBeginTime(ObjectInfo objectInfo)
    {
        var maxMusicBlockTimeInSeconds = GetMaxMusicBlockTimeInSeconds(objectInfo);

        // плейлист начиниается и заканчивается музыкой, делаем музыкальные блоки в начале и в конце
        var advertsBeginTime = objectInfo.BeginTime
            .Add(TimeSpan.FromSeconds(maxMusicBlockTimeInSeconds))
            .Add(GapForClient);

        return advertsBeginTime;
        //Принимает объект ObjectInfo, который содержит информацию о плейлисте, включая время начала (BeginTime).
        //Вычисляет и возвращает время начала рекламных блоков, которое рассчитывается путем добавления к времени начала объекта (BeginTime) максимального времени музыкального блока (в секундах) и временного зазора (GapForClient). Это гарантирует, что реклама начинается только после того, как музыкальный блок полностью проиграл свое время.
    }

    public static TimeSpan GetAdvertsEndTime(ObjectInfo objectInfo)
    //Также принимает объект ObjectInfo с информацией о времени окончания (EndTime).
    {
        var maxMusicBlockTimeInSeconds = GetMaxMusicBlockTimeInSeconds(objectInfo);

        // плейлист начиниается и заканчивается музыкой, делаем музыкальные блоки в начале и в конце
        var advertsEndTime = objectInfo.EndTime
            .Subtract(TimeSpan.FromSeconds(maxMusicBlockTimeInSeconds))
            .Subtract(GapForClient);

        return advertsEndTime;
        //Вычисляет и возвращает время окончания рекламных блоков, которое находится путем вычитания из времени окончания объекта длительности максимального музыкального блока и зазора. Это убедится, что рекламные блоки закончатся до начала заключительного музыкального блока.
    }

    public static int GetMaxMusicBlockTimeInSeconds(ObjectInfo objectInfo)
    {
        //Получает ту же структуру ObjectInfo.
        var maxAdvertBlockInSeconds = objectInfo.MaxAdvertBlockInSeconds;
        var maxMusicBlockTimeInSeconds = maxAdvertBlockInSeconds * 2;
        //Используется для расчета максимальной длительности музыкального блока на основе установленного максимального времени рекламного блока (MaxAdvertBlockInSeconds). В данном случае предполагается, что два музыкальных блока могут иметь длительность, равную удвоенному времени рекламного блока.
        // Возвращает максимально возможное время музыкального блока в секундах.
        return maxMusicBlockTimeInSeconds;
    }
    //В целом, этот помощник помогает правильно разместить рекламные и музыкальные блоки в рамках общего плейлиста, гарантируя, что рекламные блоки не перекрывают начало и конец музыкального вещания.
}