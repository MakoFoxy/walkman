using System;
using System.Collections.Generic;
using System.Linq;
using Player.Domain;

namespace Player.Services.Report.Abstractions
{
    // Интерфейс, определяющий модель отчета, включающую музыкальные треки
    public interface IReportContainsTracks : IReportModel
    {
        Tracks Tracks { get; set; }
    }

    // Класс, представляющий коллекции музыкальных и рекламных треков
    public class Tracks
    {
        // Контейнер для музыкальных треков
        public TrackEnvelope Music { get; set; } = new TrackEnvelope();
        // Контейнер для рекламных треков
        public TrackEnvelope Adverts { get; set; } = new TrackEnvelope();

        // Объединенный список всех треков, удаляющий дубликаты и упорядочивающий по времени начала
        public IList<Track> All
        {
            get
            {
                var tracks = new List<Track>();
                tracks.AddRange(Music.Tracks);
                tracks.AddRange(Adverts.Tracks);
                return tracks.DistinctBy(t => new { t.Id, t.BeginTime }).OrderBy(t => t.BeginTime).ToList(); // Этот вызов метода выбирает уникальные треки из списка tracks, основываясь на комбинации Id и BeginTime каждого трека. Это означает, что если в списке будут два трека с одинаковыми Id и BeginTime, то в итоговый список попадет только один из них. Метод DistinctBy используется для удаления дубликатов из списка, когда нужно учитывать несколько свойств объекта для определения уникальности.
            }
        }

        // Метод для добавления рекламных треков на основе исторических данных
        public void AddAdverts(ICollection<AdHistory> advertsHistory)
        {
            foreach (var advert in advertsHistory)
            {
                Adverts.Tracks.Add(Track.FromDomain(advert.Advert, advert.Start.TimeOfDay, true));
            }
        }
    }

    // Класс, представляющий коллекцию треков с дополнительными метриками
    public class TrackEnvelope
    {
        public ICollection<Track> Tracks { get; set; } = new List<Track>();

        // Общая длительность всех треков в минутах
        public double TotalMinutes => Tracks.Sum(t => t.Length.TotalMinutes);
        // Общее количество треков
        public int Count => Tracks.Count;
        // Количество уникальных треков по Id
        public int UniqueCount => Tracks.DistinctBy(t => t.Id).Count();
    }

    // Класс, представляющий одиночный трек (музыкальный или рекламный)
    public class Track
    {
        public Guid Id { get; set; }
        public TimeSpan BeginTime { get; set; }
        public string Name { get; set; }
        public TimeSpan Length { get; set; }
        public bool IsAdvert { get; set; }

        // Фабричный метод для создания объекта Track из доменного трека, со временем и флагом рекламы
        public static Track FromDomain(Domain.Track track, TimeSpan beginTime, bool isAdvert)
        {
            return new Track
            {
                Id = track.Id,
                Name = track.Name,
                Length = track.Length,
                BeginTime = beginTime,
                IsAdvert = isAdvert
            };
        }
    }
}
