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
//Этот код представляет собой генератор плейлистов, который создает плейлисты для объектов в определенные даты. Вот детальное объяснение каждой части кода
{
    private readonly ITrackLoader _trackLoader;
    private readonly IPlaylistLoadingCalculator _playlistLoadingCalculator;

    private int _maxAdvertBlockInSeconds;

    public PlaylistGenerator(
        PlayerContext context,
        ITrackLoader trackLoader,
        IPlaylistLoadingCalculator playlistLoadingCalculator) : base(context)
    //    PlayerContext context: Контекст базы данных для доступа к данным.
    // ITrackLoader trackLoader: Загрузчик треков, который предоставляет функционал для загрузки музыкальных треков.
    // IPlaylistLoadingCalculator playlistLoadingCalculator: Калькулятор загрузки плейлиста, который вычисляет загрузку плейлиста и определяет, перегружен ли он.
    {
        _trackLoader = trackLoader;
        _playlistLoadingCalculator = playlistLoadingCalculator;
    }

    public override async Task<PlaylistGeneratorResult> Generate(ObjectInfo objectInfo, DateTime date, CancellationToken cancellationToken = default)
    {
        var playlistEnvelope = await GetPlaylist(objectInfo, date); //Сначала получает информацию о плейлисте, вызывая метод GetPlaylist. Если на указанную дату это выходной, плейлист не генерируется. Метод начинается с асинхронного получения информации о плейлисте с использованием метода GetPlaylist. Эта информация включает в себя текущий состав плейлиста и возможно другие данные, такие как статус, является ли плейлист новым и т.д.

        if (IsFreeDay(objectInfo, date))
        {//Затем проверяется, является ли указанная дата выходным днем для данного объекта с помощью метода IsFreeDay. Если это так и плейлист является новым, возвращается статус "Не сгенерировано". Если плейлист не новый, выполняется его удаление через DeleteStatus.
            if (playlistEnvelope.IsNew)
            {
                return NotGeneratedStatus();
            }

            return DeleteStatus(playlistEnvelope.Playlist);
        }

        _maxAdvertBlockInSeconds = objectInfo.MaxAdvertBlockInSeconds;
        //Определяет максимальную длину блока рекламы из информации объекта.
        var allAdverts = new List<Advert>();
        //Загружает все рекламы из базы данных, которые должны быть воспроизведены на данную дату.
        var oldAdvertsInPlaylist = playlistEnvelope.Playlist.Aderts
            .Select(ap => ap.Advert)
            .OrderBy(a => a.CreateDate)
            .ToList();
        allAdverts.AddRange(oldAdvertsInPlaylist); //Далее устанавливается максимальная длительность блока рекламы. Загружаются все рекламные блоки, уже присутствующие в плейлисте. Они сортируются по дате создания и добавляются в список allAdverts.

        var distinctOldAdverts = playlistEnvelope.Playlist.Aderts //Сначала создается список distinctOldAdverts, который содержит уникальные рекламные блоки, уже существующие в плейлисте. Эти рекламные блоки выбираются, делаются уникальными (без дубликатов), сортируются по дате создания и сохраняются в список.
            .Select(ap => ap.Advert)
            .Distinct()
            .OrderBy(a => a.CreateDate)
            .ToList();
        DebugInfo.Add($"Adverts in playlists {string.Join(";", distinctOldAdverts.Select(a => a.Name))}");
       
        foreach (var oldAdvert in distinctOldAdverts)
        { //Затем проходится цикл по каждой уникальной рекламе в distinctOldAdverts. Для каждой рекламы извлекается информация о времени показа на указанную дату и для данного объекта (objectInfo). Проверяется, совпадает ли запланированное количество повторений (RepeatCount) с количеством уже включенных в плейлист экземпляров этой рекламы. Если не совпадает, разница между запланированным и фактическим количеством добавляется в общий список allAdverts.
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
        DebugInfo.Add($"New adverts {string.Join(";", newAdverts.Select(a => a.Name))}"); //Далее загружаются новые рекламные блоки, которые допустимы к показу на данную дату и для данного объекта, но которых еще нет в плейлисте (distinctOldAdverts). Рекламные блоки включаются в запрос с необходимыми включениями связанных данных (Include), и затем список загружается асинхронно.

        foreach (var newAdvert in newAdverts)
        {//Для каждой новой рекламы также проверяется количество повторений на указанную дату, и эта реклама добавляется в allAdverts нужное количество раз, в соответствии с запланированным количеством показов.
            var adTime = newAdvert.AdTimes.Single(at => at.PlayDate == date && at.Object == objectInfo);

            for (var i = 0; i < adTime.RepeatCount; i++)
            {
                allAdverts.Add(newAdvert);
            }
        }
        //Загружает музыкальные треки для объекта.

        var musicTracks = await _trackLoader.LoadForObject(objectInfo, cancellationToken);

        PopulateMusicTracks(playlistEnvelope.Playlist, objectInfo, musicTracks);
        var notFittedAdverts = PopulateAdverts(playlistEnvelope.Playlist, objectInfo, allAdverts);

        //Распределяет рекламные и музыкальные треки по плейлисту, учитывая заданные ограничения по времени и правила.
        CalculatePlaylistLoading(objectInfo, playlistEnvelope);

        playlistEnvelope.Playlist.PlaylistInfos.Add(new PlaylistInfo
        {
            Info = string.Join(Environment.NewLine, DebugInfo),
            CreateDate = DateTime.Now,
        });

        return GeneratedStatus(playlistEnvelope.Playlist, DebugInfo, notFittedAdverts); //Возвращает результат генерации, который включает в себя сам плейлист и информацию о рекламах, которые не уместились в плейлист.
    }

    private List<Advert> PopulateAdverts(Playlist playlist, ObjectInfo objectInfo, ICollection<Advert> adverts)
    //Этот код отвечает за распределение рекламных блоков в плейлисте с учетом заданных временных параметров и правил
    {
        playlist.Aderts.Clear(); //Очищает текущие рекламные блоки в плейлисте.
        var maxAdvertBlockInSecondsWithSmallGap = _maxAdvertBlockInSeconds + 3; //var maxAdvertBlockInSecondsWithSmallGap = _maxAdvertBlockInSeconds + 3;: Задает максимально допустимую продолжительность рекламного блока с небольшим запасом времени (3 секунды) для учета возможных интервалов между рекламами.
        var maxMusicBlockTimeInSeconds = AdvertsCalculationHelper.GetMaxMusicBlockTimeInSeconds(objectInfo); //var maxMusicBlockTimeInSeconds = AdvertsCalculationHelper.GetMaxMusicBlockTimeInSeconds(objectInfo);: Получает максимальное время музыкального блока для данного объекта.

        // плейлист начиниается и заканчивается музыкой, делаем музыкальные блоки в начале и в конце
        var advertsBeginTime = AdvertsCalculationHelper.GetAdvertsBeginTime(objectInfo);
        var advertsEndTime = AdvertsCalculationHelper.GetAdvertsEndTime(objectInfo);
        //Определяют время начала и окончания воспроизведения рекламных блоков в плейлисте, оставляя время для музыкальных блоков в начале и в конце.
        //Распределяет рекламные блоки в плейлисте, соблюдая максимальное время их длительности и интервалы между музыкальными блоками.
        var currentTime = advertsBeginTime; //Устанавливает текущее время воспроизведения рекламы на время начала воспроизведения рекламных блоков.

        var remainingAdverts = adverts.ToList(); //Создает список оставшихся для распределения реклам.

        while (currentTime <= advertsEndTime)
        {
            var advertBlock = new List<Advert>(); //Создает новый временный список для рекламного блока.


            if (!remainingAdverts.Any()) //Проверка if (!remainingAdverts.Any()): Если не осталось реклам для распределения, пропускает время до следующего музыкального блока. 
            {
                currentTime = currentTime.RoundUp()
                    .Add(TimeSpan.FromSeconds(maxMusicBlockTimeInSeconds))
                    .Add(AdvertsCalculationHelper.GapForClient);
                continue;
            }

            foreach (var remainingAdvert in remainingAdverts) //В цикле foreach: Перебирает оставшиеся рекламы, проверяя, можно ли добавить рекламу в текущий блок без превышения максимально допустимой длительности.
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
                    //После распределения реклам в блок, обновляет currentTime, добавляя длительность рекламных блоков и переходит к следующему временному интервалу.
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

        return remainingAdverts; //Возвращает список реклам, которые не удалось включить в плейлист.
        //return remainingAdverts;: Возвращает список реклам, которые не были включены в плейлист. Эти рекламы либо будут использованы позже, либо не найдут своего места в плейлисте из-за временных ограничений.
    }

    private void PopulateMusicTracks(Playlist playlist, ObjectInfo objectInfo, ICollection<MusicTrack> musicTracks)
    //Этот код отвечает за заполнение плейлиста музыкальными треками
    {
        if (!musicTracks.Any())
        {
            DebugInfo.Add("No music tracks available.");
            return; // Выход, если треков нет.
        }
        playlist.MusicTracks.Clear(); //Эта строка удаляет все существующие музыкальные треки из плейлиста, чтобы начать распределение заново.
        var currentTime = objectInfo.BeginTime; //var currentTime = objectInfo.BeginTime;: Устанавливает начальное время воспроизведения музыкальных треков равным времени начала работы объекта (например, открытия магазина или ресторана).

        using IEnumerator<MusicTrack> musicTrackEnumerator = musicTracks.GetEnumerator(); //Создает перечислитель для коллекции музыкальных треков, чтобы последовательно итерировать по ним.
        musicTrackEnumerator.MoveNext(); // Перемещаемся к первому треку перед началом цикла
        // todo проверить 24 часа
        while (currentTime <= objectInfo.EndTime) //Цикл продолжается до тех пор, пока текущее время не превысит время окончания работы объекта.
        {
            var currentTrack = GetCurrentMusicTrack(musicTrackEnumerator); //Вызывает метод, который получает следующий музыкальный трек из коллекции. Если треки закончились, перечислитель начинается сначала.
            if (currentTrack == null)
            {
                DebugInfo.Add("Reached end of music tracks.");
                break; // Выход из цикла, если треков больше нет
            }
            var musicTrackPlaylist = new MusicTrackPlaylist
            {
                Playlist = playlist,
                MusicTrack = currentTrack,
                PlayingDateTime = playlist.PlayingDate.Add(currentTime), //Создает новый экземпляр MusicTrackPlaylist, который связывает текущий музыкальный трек с плейлистом и временем его воспроизведения.
            };
            playlist.MusicTracks.Add(musicTrackPlaylist); //Добавляет созданный музыкальный блок плейлиста к списку музыкальных треков плейлиста.
            DebugInfo.Add($"MusicTrack {currentTrack.Name} - {musicTrackPlaylist.PlayingDateTime:HH:mm:ss}"); //Добавляет информацию о музыкальном треке и его времени воспроизведения в список отладочной информации.

            currentTime += currentTrack.Length.RoundUp().Add(AdvertsCalculationHelper.GapForClient); //Обновляет текущее время, добавляя к нему длительность текущего музыкального трека и зазор между треками. Это гарантирует, что между концом одного трека и началом следующего будет время на зазор и возможную рекламу.
        }

        MusicTrack GetCurrentMusicTrack(IEnumerator<MusicTrack> enumerator)
        //Эта функция GetCurrentMusicTrack используется для получения текущего музыкального трека из коллекции музыкальных треков:
        {
            if (!enumerator.MoveNext())
            //if (enumerator.MoveNext()): Проверяет, существует ли следующий элемент в коллекции (в данном случае в списке музыкальных треков). Метод MoveNext перемещает перечислитель к следующему элементу в коллекции.

            {
                enumerator.Reset(); //enumerator.Reset();: Если в коллекции не осталось элементов (метод MoveNext вернул false), метод Reset сбрасывает перечислитель к началу коллекции, так что он снова указывает на место перед первым элементом коллекции.
                enumerator.MoveNext(); // Переместиться к первому элементу после сброса
                // return enumerator.Current;
                // //return enumerator.Current;: Если следующий элемент существует (метод MoveNext вернул true), метод возвращает этот текущий музыкальный трек. Свойство Current содержит текущий элемент в коллекции, на который указывает перечислитель.
            }
            return enumerator.Current;
            //return enumerator.Current;: Если следующий элемент существует (метод MoveNext вернул true), метод возвращает этот текущий музыкальный трек. Свойство Current содержит текущий элемент в коллекции, на который указывает перечислитель.
            // enumerator.Reset(); //enumerator.Reset();: Если в коллекции не осталось элементов (метод MoveNext вернул false), метод Reset сбрасывает перечислитель к началу коллекции, так что он снова указывает на место перед первым элементом коллекции.
            // return GetCurrentMusicTrack(enumerator); //return GetCurrentMusicTrack(enumerator);: После сброса перечислителя метод рекурсивно вызывает себя, чтобы начать перебор с начала коллекции и получить первый музыкальный трек после сброса. Это гарантирует, что музыкальные треки будут повторно использоваться в плейлисте, если все треки уже были проиграны.
        }
    }

    public override async Task<PlaylistGeneratorResult> Generate(Guid objectId, DateTime date, CancellationToken cancellationToken = default) //public override async Task<PlaylistGeneratorResult> Generate(...): Это асинхронный метод, переопределенный из базового класса, который запускает процесс генерации плейлиста для конкретного объекта на определенную дату.
    {
        var objectInfo = await _context.Objects.SingleAsync(o => o.Id == objectId, cancellationToken); //Запрашивает из базы данных полную информацию об объекте по его ID.
        return await Generate(objectInfo, date, cancellationToken); //Вызывает метод генерации плейлиста, передавая полученные данные об объекте, дату и токен отмены.
    }

    private void CalculatePlaylistLoading(ObjectInfo objectInfo, PlaylistEnvelope playlistEnvelope)
    //Этот код относится к сервису для генерации плейлистов и вычисления их загрузки в системе управления медиа-контентом. Давайте разберем каждую строку:
    {
        playlistEnvelope.Playlist.Loading = _playlistLoadingCalculator.GetLoading(objectInfo, playlistEnvelope.Playlist); //Вычисляет общую загрузку плейлиста на основании анализа содержимого и установок объекта.
        playlistEnvelope.Playlist.UniqueAdvertsCount = playlistEnvelope.Playlist.Aderts.Select(a => a.Advert).Distinct().Count(); //Считает количество уникальных рекламных блоков в плейлисте.
        playlistEnvelope.Playlist.AdvertsCount = playlistEnvelope.Playlist.Aderts.Select(a => a.Advert).Count(); //Считает общее количество рекламных блоков в плейлисте.

        playlistEnvelope.Playlist.Overloaded = _playlistLoadingCalculator.IsOverloaded(playlistEnvelope.Playlist); //Определяет, перегружен ли плейлист на основе его содержимого.

        DebugInfo.Add(
            $"Loading\t{playlistEnvelope.Playlist.Loading}\t" +
            $"UniqueAdvertsCount\t{playlistEnvelope.Playlist.UniqueAdvertsCount}\t" +
            $"AdvertsCount\t{playlistEnvelope.Playlist.AdvertsCount}\t" +
            $"Overloaded\t{playlistEnvelope.Playlist.Overloaded}"); //Добавляет информацию о загрузке плейлиста в список отладочной информации для дальнейшего анализа или логирования.
    }
    //Каждая строка в этом коде помогает определить и записать характеристики генерируемого плейлиста, включая его загрузку, количество рекламы и музыкальных треков, а также проверяет, не перегружен ли плейлист с точки зрения общего времени звучания и количества рекламных блоков.
}