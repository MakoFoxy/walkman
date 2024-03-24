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
    //DebugInfo: Список строк, используемый для хранения информации для отладки в процессе генерации плейлиста.
    protected readonly PlayerContext _context;
    //_context: Экземпляр контекста базы данных PlayerContext, который позволяет взаимодействовать с базой данных.
    protected BasePlaylistGenerator(PlayerContext context)
    {
        _context = context;
        //    Принимает контекст базы данных (PlayerContext) и инициализирует поле _context.
    }

    public abstract Task<PlaylistGeneratorResult> Generate(ObjectInfo objectInfo, DateTime date, CancellationToken cancellationToken = default);
    //Абстрактный метод для генерации плейлиста на основе информации об объекте и конкретной даты.
    public abstract Task<PlaylistGeneratorResult> Generate(Guid objectId, DateTime date, CancellationToken cancellationToken = default);
    //Альтернативный абстрактный метод для генерации плейлиста, использующий идентификатор объекта вместо объекта информации.
    [Pure]
    protected PlaylistGeneratorResult NotGeneratedStatus()
    {
        return new()
        {
            Status = PlaylistGeneratorStatus.NotGenerated
            //Возвращает результат генерации с статусом NotGenerated, указывающим, что плейлист не был сгенерирован.
        };
    }

    [Pure]
    protected PlaylistGeneratorResult DeleteStatus(Playlist playlist)
    {
        return new()
        {
            Status = PlaylistGeneratorStatus.Delete,
            Playlist = playlist,
            //Возвращает результат генерации с статусом Delete, указывающим на необходимость удаления плейлиста.
        };
    }

    [Pure]
    protected PlaylistGeneratorResult GeneratedStatus(
        Playlist playlist,
        ICollection<string> debugInfo,
        ICollection<Advert> notFittedAdverts) //содержащим сгенерированный плейлист, информацию для отладки и список реклам, которые не подошли.
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
        //Проверяет, является ли указанная дата днём, когда объект не работает (выходной).
    }

    protected async Task<PlaylistEnvelope> GetPlaylist(ObjectInfo objectInfo, DateTime date)
    {
        //Пытается найти плейлист для указанного объекта и даты. Если плейлист не найден, создает новый. Загружает связанные данные, такие как рекламные и музыкальные треки, из базы данных.
        var playlist = await _context.Playlists
            .Where(p => p.Object == objectInfo && p.PlayingDate == date)
            .SingleOrDefaultAsync();
        //Эта строка делает запрос к базе данных для поиска плейлиста, который соответствует указанному объекту (ObjectInfo) и дате (date). Если такой плейлист найден, он будет загружен из базы данных; в противном случае переменная playlist будет равна null
        if (playlist == null)
        //Этот блок кода выполняется, если плейлист не найден в базе данных, т.е. он еще не был создан для данного объекта на указанную дату. В этом случае:
        {
            DebugInfo.Add("New playlist"); //DebugInfo.Add("New playlist");: Добавляется запись в список отладочной информации, указывающая, что создается новый плейлист.
            return new PlaylistEnvelope
            {
                //Возвращается новый объект PlaylistEnvelope, содержащий вновь созданный плейлист с уникальным идентификатором, информацией об объекте, датой воспроизведения и текущей датой создания. IsNew устанавливается в true, что означает, что это новый плейлист.
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
        //DebugInfo.Add("Existing playlist");: Если плейлист уже существует (т.е. он был найден в базе данных), добавляется запись о том, что работа будет продолжена с существующим плейлистом.
        DebugInfo.Add("Existing playlist");
        await _context.Entry(playlist)
            .Collection(p => p.Aderts)
            .Query()
            .Include(ap => ap.Advert.TrackType)
            .LoadAsync();
        //await _context.Entry(playlist)...LoadAsync();: Загружает из базы данных дополнительные детали для существующего плейлиста, включая информацию о рекламных блоках (Aderts) и музыкальных треках (MusicTracks). Для каждого рекламного блока загружается тип трека. Это нужно, чтобы полностью воссоздать структуру плейлиста со всеми зависимостями.
        var adverts = playlist.Aderts.Select(ap => ap.Advert).ToList();
        //Извлекает все рекламные объявления из плейлиста в список для дальнейшей работы с ними.
        await _context.AdTimes.Where(at => adverts.Contains(at.Advert)).LoadAsync();
        //Загружает из базы данных времена воспроизведения для всех рекламных блоков, присутствующих в плейлисте.
        await _context.Entry(playlist)
            .Collection(p => p.MusicTracks)
            .Query().Include(mt => mt.MusicTrack.TrackType)
            .LoadAsync();
        //Возвращается объект PlaylistEnvelope, содержащий существующий плейлист. IsNew устанавливается в false, указывая, что это существующий плейлист, а не новый.
        return new PlaylistEnvelope
        {
            Playlist = playlist,
            IsNew = false
            //Возвращаемый GetPlaylist объект PlaylistEnvelope представляет собой "конверт" или контейнер для плейлиста, который также указывает, является ли этот плейлист новым или уже существующим. Это помогает определить, следует ли сохранять новый плейлист в базу данных или обновлять существующий.
        };
    }
    //В целом, этот базовый класс определяет общие методы и свойства, которые могут быть использованы в конкретных реализациях генераторов плейлистов для различных сценариев или требований.
    //В общем, этот код обрабатывает проверку наличия плейлиста для определенного объекта и даты и подготавливает его к дальнейшему использованию, загружая все необходимые данные из базы данных.
}