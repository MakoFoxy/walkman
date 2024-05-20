using System;
using System.Collections.Generic;
using System.IO;
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
        var playlistEnvelope = await GetPlaylist(objectInfo, date); //GetPlaylist: Метод асинхронно извлекает данные плейлиста для указанного объекта (objectInfo) и даты (date). Это может включать информацию о текущем составе плейлиста и его статусе (новый или существующий).

        if (IsFreeDay(objectInfo, date))
        {//    IsFreeDay: Проверяет, является ли указанная дата выходным днем для объекта. Если да, то действия зависят от статуса плейлиста:
         // NotGeneratedStatus: Возвращает статус, что плейлист не сгенерирован для новых плейлистов.
         // DeleteStatus: Удаляет плейлист, если он не новый.
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
            .ToList(); //Собирает и сортирует существующие в плейлисте рекламные блоки по дате создания.
        allAdverts.AddRange(oldAdvertsInPlaylist); //Рекламы добавляются в общий список allAdverts.

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

        var musicTracks = await _trackLoader.LoadForObject(objectInfo, cancellationToken); //Метод LoadForObject асинхронно загружает музыкальные треки, которые должны быть включены в плейлист для данного объекта (objectInfo). Используется cancellationToken для возможности отмены операции, если это потребуется. Загруженные треки сохраняются в переменной musicTracks.

        var logFilePath = @"C:\Logs\log.txt"; // Путь к файлу лога

        PopulateMusicTracks(playlistEnvelope.Playlist, objectInfo, musicTracks, logFilePath); //Метод PopulateMusicTracks используется для добавления загруженных музыкальных треков в плейлист. Он принимает текущий плейлист, информацию об объекте и список музыкальных треков, и распределяет их в плейлисте согласно заданным правилам и ограничениям.
        var notFittedAdverts = PopulateAdverts(playlistEnvelope.Playlist, objectInfo, allAdverts); //PopulateAdverts распределяет рекламные блоки из списка allAdverts по плейлисту. Метод возвращает список реклам, которые не удалось вместить в плейлист (notFittedAdverts). Это может быть связано с ограничениями по времени, количеству повторений или другими правилами.

        //Распределяет рекламные и музыкальные треки по плейлисту, учитывая заданные ограничения по времени и правила.
        CalculatePlaylistLoading(objectInfo, playlistEnvelope); //Метод CalculatePlaylistLoading выполняет финальный расчет загрузки плейлиста, учитывая все рекламные и музыкальные треки, уже распределенные в плейлисте. Он проверяет, соответствует ли итоговая композиция плейлиста всем требованиям и ограничениям.


        // WriteToLogFile(playlistEnvelope.Playlist, logFilePath, objectInfo, musicTracks);

        playlistEnvelope.Playlist.PlaylistInfos.Add(new PlaylistInfo
        {//Здесь добавляется информационный блок в плейлист, который содержит детали процесса генерации (собранные в DebugInfo) и дату создания этой информации. Это может быть полезно для аудита или отладки процесса генерации плейлиста.
            Info = string.Join(Environment.NewLine, DebugInfo),
            CreateDate = DateTime.Now,
        });

        return GeneratedStatus(playlistEnvelope.Playlist, DebugInfo, notFittedAdverts); //Возвращает результат генерации, который включает в себя сам плейлист и информацию о рекламах, которые не уместились в плейлист. Метод GeneratedStatus формирует и возвращает результат генерации плейлиста, который включает сам плейлист, отладочную информацию и список реклам, которые не уместились в плейлист. Это финальный этап процесса, когда пользователь или другие части системы получают готовый продукт с необходимыми метаданными.
    }

    private MusicTrack GetCurrentMusicTrack(object musicTrackEnumerator)
    {
        throw new NotImplementedException();
    }


    private List<Advert> PopulateAdverts(Playlist playlist, ObjectInfo objectInfo, ICollection<Advert> adverts)
    //Этот код отвечает за распределение рекламных блоков в плейлисте с учетом заданных временных параметров и правил
    {
        playlist.Aderts.Clear(); //Очищает текущие рекламные блоки в плейлисте. Эта строка удаляет все текущие рекламные блоки из плейлиста, подготавливая его к новой генерации рекламного контента.
        var maxAdvertBlockInSecondsWithSmallGap = _maxAdvertBlockInSeconds + 3; //var maxAdvertBlockInSecondsWithSmallGap = _maxAdvertBlockInSeconds + 3;: Задает максимально допустимую продолжительность рекламного блока с небольшим запасом времени (3 секунды) для учета возможных интервалов между рекламами.  устанавливает максимальную длительность рекламного блока, добавляя 3 секунды для учета возможных интервалов между рекламами.
        var maxMusicBlockTimeInSeconds = AdvertsCalculationHelper.GetMaxMusicBlockTimeInSeconds(objectInfo); //var maxMusicBlockTimeInSeconds = AdvertsCalculationHelper.GetMaxMusicBlockTimeInSeconds(objectInfo);: Получает максимальное время музыкального блока для данного объекта. определяет максимальное время для музыкальных блоков для данного объекта.

        // плейлист начиниается и заканчивается музыкой, делаем музыкальные блоки в начале и в конце
        var advertsBeginTime = AdvertsCalculationHelper.GetAdvertsBeginTime(objectInfo);
        var advertsEndTime = AdvertsCalculationHelper.GetAdvertsEndTime(objectInfo);
        //Определяют время начала и окончания воспроизведения рекламных блоков в плейлисте, оставляя время для музыкальных блоков в начале и в конце. Эти методы определяют начальное и конечное время воспроизведения рекламных блоков в плейлисте, обеспечивая наличие музыкальных блоков в начале и в конце плейлиста.
        //Распределяет рекламные блоки в плейлисте, соблюдая максимальное время их длительности и интервалы между музыкальными блоками.
        var currentTime = advertsBeginTime; //Устанавливает текущее время воспроизведения рекламы на время начала воспроизведения рекламных блоков. устанавливает начальное время для размещения первого рекламного блока.

        var remainingAdverts = adverts.ToList(); //Создает список оставшихся для распределения реклам. создает список всех реклам, которые нужно разместить в плейлисте.

        while (currentTime <= advertsEndTime)
        {//Цикл продолжается до тех пор, пока текущее время не превысит время окончания рекламных блоков. В каждой итерации создается новый список для рекламного блока.
            var advertBlock = new List<Advert>(); //Создает новый временный список для рекламного блока.

            //Эта строка инициализирует новый список advertBlock, который будет использоваться для временного хранения реклам, выбранных для включения в текущий рекламный блок плейлиста. Этот список наполняется в процессе обработки и проверки каждой рекламы на соответствие заданным критериям длительности и временных рамок.

            if (!remainingAdverts.Any()) //Проверка if (!remainingAdverts.Any()): Если не осталось реклам для распределения, пропускает время до следующего музыкального блока.  !remainingAdverts.Any(): Проверяет, пуст ли список remainingAdverts. Если да, то это означает, что все доступные рекламы уже распределены или их не было с самого начала.
            {
                currentTime = currentTime.RoundUp() //currentTime.RoundUp(): Возможно округляет текущее время до следующего полного интервала (например, до начала следующей минуты), чтобы избежать перекрытия с музыкальными блоками.
                    .Add(TimeSpan.FromSeconds(maxMusicBlockTimeInSeconds)) //.Add(TimeSpan.FromSeconds(maxMusicBlockTimeInSeconds)): Добавляет максимально допустимое время музыкального блока к текущему времени, устанавливая интервал до начала следующего возможного рекламного блока.
                    .Add(AdvertsCalculationHelper.GapForClient); //.Add(AdvertsCalculationHelper.GapForClient): Добавляет дополнительный временной интервал, который может быть использован для технического перерыва или других целей, обеспечивающих плавный переход между блоками.
                continue;
            }

            foreach (var remainingAdvert in remainingAdverts) //В цикле foreach: Перебирает оставшиеся рекламы, проверяя, можно ли добавить рекламу в текущий блок без превышения максимально допустимой длительности.
            { //Определение длительности рекламного блока: Используется лямбда-выражение для подсчета общей длительности всех реклам в текущем блоке advertBlock и проверяется, укладывается ли добавление новой рекламы remainingAdvert в установленные временные рамки (maxAdvertBlockInSecondsWithSmallGap).
                var possibleBlockLength = advertBlock.Sum(ab => ab.Length.TotalSeconds) +
                                          remainingAdvert.Length.TotalSeconds;

                if (possibleBlockLength > maxAdvertBlockInSecondsWithSmallGap)
                {//Условие пропуска: Если добавление рекламы превысит максимально допустимую длительность, текущая реклама пропускается.
                    continue;
                }

                if (!advertBlock.Contains(remainingAdvert))
                { //Добавление рекламы: Если реклама не превышает ограничения и еще не содержится в текущем блоке, она добавляется в advertBlock.
                    advertBlock.Add(remainingAdvert);
                }
            }

            foreach (var advert in advertBlock)
            { //Создание объекта AdvertPlaylist: Каждая реклама из блока регистрируется в плейлисте с указанием точного времени начала воспроизведения, которое вычисляется на основе текущего времени currentTime.
                var advertPlaylist = new AdvertPlaylist
                { //Обновление времени: После добавления рекламы в плейлист, currentTime увеличивается на длительность рекламы и дополнительный интервал, заданный в AdvertsCalculationHelper.GapForClient.
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
            DebugInfo.Add($"Block length = {totalBlockLength}"); //Подсчет общей длительности блока: Вычисляется и регистрируется в DebugInfo для отладки и мониторинга.

            currentTime = currentTime.RoundUp()
                .Add(TimeSpan.FromSeconds(maxMusicBlockTimeInSeconds))
                .Add(AdvertsCalculationHelper.GapForClient); //Округление времени и добавление музыкального блока: currentTime округляется и увеличивается на максимально допустимое время музыкального блока, обеспечивая интервал между рекламными блоками.

            foreach (var advert in advertBlock)
            {
                remainingAdverts.Remove(advert); //Удаление использованных реклам: Рекламы, которые были добавлены в плейлист, удаляются из списка оставшихся реклам.
            }
        }

        return remainingAdverts; //Возвращает список реклам, которые не удалось включить в плейлист.
        //return remainingAdverts;: Возвращает список реклам, которые не были включены в плейлист. Эти рекламы либо будут использованы позже, либо не найдут своего места в плейлисте из-за временных ограничений.
    }

    // private void WriteToLogFile(Playlist playlist, string logFilePath, ObjectInfo objectInfo, ICollection<MusicTrack> musicTracks)
    // {
    //     using IEnumerator<MusicTrack> musicTrackEnumerator = musicTracks.GetEnumerator(); //Создает перечислитель для коллекции музыкальных треков, чтобы последовательно итерировать по ним.
    //     musicTrackEnumerator.MoveNext(); // Перемещаемся к первому треку перед началом цикла
    //     // todo проверить 24 часа    
    //     // Открываем или создаем файл лога
    //     using (StreamWriter writer = new StreamWriter(logFilePath))
    //     {
    //         var currentTime = objectInfo.BeginTime;
    //         while (currentTime <= objectInfo.EndTime)
    //         {
    //             var currentTrack = GetCurrentMusicTrack(musicTrackEnumerator);
    //             if (currentTrack == null)
    //             {
    //                 DebugInfo.Add("Reached end of music tracks.");
    //                 break;
    //             }
    //             var musicTrackPlaylist = new MusicTrackPlaylist
    //             {
    //                 Playlist = playlist,
    //                 MusicTrack = currentTrack,
    //                 PlayingDateTime = playlist.PlayingDate.Add(currentTime),
    //             };

    //             // Выводим в консоль currentTime и PlayingDateTime
    //             Console.WriteLine($"Current Time: {currentTime}, PlayingDateTime: {musicTrackPlaylist.PlayingDateTime}");

    //             // Записываем в лог-файл
    //             writer.WriteLine($"Current Time: {currentTime}, PlayingDateTime: {musicTrackPlaylist.PlayingDateTime}");

    //             currentTime = currentTime.Add(currentTrack.Length); // Переходим к следующему времени
    //         }
    //     }
    //     MusicTrack GetCurrentMusicTrack(IEnumerator<MusicTrack> enumerator)
    //     //Эта функция GetCurrentMusicTrack используется для получения текущего музыкального трека из коллекции музыкальных треков:
    //     {
    //         if (!enumerator.MoveNext())
    //         //if (enumerator.MoveNext()): Проверяет, существует ли следующий элемент в коллекции (в данном случае в списке музыкальных треков). Метод MoveNext перемещает перечислитель к следующему элементу в коллекции.

    //         {
    //             enumerator.Reset(); //enumerator.Reset();: Если в коллекции не осталось элементов (метод MoveNext вернул false), метод Reset сбрасывает перечислитель к началу коллекции, так что он снова указывает на место перед первым элементом коллекции.
    //             enumerator.MoveNext(); // Переместиться к первому элементу после сброса
    //             // return enumerator.Current;
    //             // //return enumerator.Current;: Если следующий элемент существует (метод MoveNext вернул true), метод возвращает этот текущий музыкальный трек. Свойство Current содержит текущий элемент в коллекции, на который указывает перечислитель.
    //         }
    //         return enumerator.Current;
    //         //return enumerator.Current;: Если следующий элемент существует (метод MoveNext вернул true), метод возвращает этот текущий музыкальный трек. Свойство Current содержит текущий элемент в коллекции, на который указывает перечислитель.
    //         // enumerator.Reset(); //enumerator.Reset();: Если в коллекции не осталось элементов (метод MoveNext вернул false), метод Reset сбрасывает перечислитель к началу коллекции, так что он снова указывает на место перед первым элементом коллекции.
    //         // return GetCurrentMusicTrack(enumerator); //return GetCurrentMusicTrack(enumerator);: После сброса перечислителя метод рекурсивно вызывает себя, чтобы начать перебор с начала коллекции и получить первый музыкальный трек после сброса. Это гарантирует, что музыкальные треки будут повторно использоваться в плейлисте, если все треки уже были проиграны.
    //     }

    // }

    private void PopulateMusicTracks(Playlist playlist, ObjectInfo objectInfo, ICollection<MusicTrack> musicTracks, string logFilePath)
    //Этот код отвечает за заполнение плейлиста музыкальными треками
    {
        if (!musicTracks.Any())
        {
            DebugInfo.Add("No music tracks available.");
            return; // Выход, если треков нет.
        }
        playlist.MusicTracks.Clear(); //Эта строка удаляет все существующие музыкальные треки из плейлиста, чтобы начать распределение заново.

        using IEnumerator<MusicTrack> musicTrackEnumerator = musicTracks.GetEnumerator(); //Создает перечислитель для коллекции музыкальных треков, чтобы последовательно итерировать по ним.
        musicTrackEnumerator.MoveNext(); // Перемещаемся к первому треку перед началом цикла
                                         // todo проверить 24 часа
        using (StreamWriter writer = new StreamWriter(logFilePath, true))
        {
            var currentTime = objectInfo.BeginTime; //var currentTime = objectInfo.BeginTime;: Устанавливает начальное время воспроизведения музыкальных треков равным времени начала работы объекта (например, открытия магазина или ресторана).
            int playlistNumber = 1; // начальное значение счетчика
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

                Console.WriteLine($"Current Time: {currentTime}, PlayingDateTime: {musicTrackPlaylist.PlayingDateTime}, MusicTrack: {musicTrackPlaylist.MusicTrack.FilePath}");

                // Записываем в лог-файл
                writer.WriteLine($"Current Time: {currentTime}, PlayingDateTime: {musicTrackPlaylist.PlayingDateTime}, MusicTrack: {musicTrackPlaylist.MusicTrack.FilePath}");

                playlist.MusicTracks.Add(musicTrackPlaylist); //Добавляет созданный музыкальный блок плейлиста к списку музыкальных треков плейлиста.
                DebugInfo.Add($"MusicTrack {currentTrack.Name} - {musicTrackPlaylist.PlayingDateTime:HH:mm:ss}"); //Добавляет информацию о музыкальном треке и его времени воспроизведения в список отладочной информации.

                currentTime += currentTrack.Length.RoundUp().Add(AdvertsCalculationHelper.GapForClient); //Обновляет текущее время, добавляя к нему длительность текущего музыкального трека и зазор между треками. Это гарантирует, что между концом одного трека и началом следующего будет время на зазор и возможную рекламу.
                playlistNumber++; // увеличиваем счетчик для следующего плейлиста
            }
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
        playlistEnvelope.Playlist.UniqueAdvertsCount = playlistEnvelope.Playlist.Aderts.Select(a => a.Advert).Distinct().Count();
        //Считает количество уникальных рекламных блоков в плейлисте.
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

// Функция PopulateMusicTracks

// Эта функция отвечает за добавление музыкальных треков в плейлист. В ней происходит следующее:

//     Определение времени воспроизведения: Для каждого музыкального трека определяется точное время его воспроизведения на основе текущего состояния плейлиста и предыдущих треков или рекламных блоков.
//     Создание записей: Для каждого трека создается объект, например, MusicTrackPlaylist, который включает в себя ссылку на сам трек, на плейлист, к которому он относится, и время его воспроизведения.
//     Добавление в плейлист: Обновленные данные о треках добавляются в общий плейлист, который затем может быть использован для воспроизведения или сохранен для последующего использования.

// Функция PopulateAdverts

// Функция PopulateAdverts аналогична функции PopulateMusicTracks, но работает с рекламными блоками:

//     Определение времени воспроизведения: Как и в случае с музыкальными треками, для каждой рекламы определяется время ее воспроизведения. Это время зависит от предыдущих элементов в плейлисте и должно учитывать различные ограничения, такие как максимальная продолжительность рекламных блоков и необходимость музыкальных интервалов.
//     Создание записей: Создается объект AdvertPlaylist, который включает в себя рекламу, плейлист и точное время воспроизведения.
//     Добавление в плейлист и обработка оставшихся реклам: Рекламы, которые успешно прошли проверку, добавляются в плейлист. Те рекламы, которые не уместились по времени или другим параметрам, сохраняются в список, который возвращается из функции. Этот список можно использовать для корректировки планирования или переноса реклам на другое время или в другой плейлист.