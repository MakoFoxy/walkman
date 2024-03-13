using System;
using System.Collections.Generic;
using System.Linq;
using Player.Domain;

namespace Player.Services.Report.Abstractions
{
    public interface IReportContainsTracks : IReportModel
    {
        Tracks Tracks { get; set; }
    }

    public class Tracks
    {
        public TrackEnvelope Music { get; set; } = new TrackEnvelope();
        public TrackEnvelope Adverts { get; set; } = new TrackEnvelope();
        
        public IList<Track> All
        {
            get
            {
                var tracks = new List<Track>();
                tracks.AddRange(Music.Tracks);
                tracks.AddRange(Adverts.Tracks);
                return tracks.DistinctBy(t => new {t.Id, t.BeginTime}).OrderBy(t => t.BeginTime).ToList();
            }
        }

        public void AddAdverts(ICollection<AdHistory> advertsHistory)
        {
            foreach (var advert in advertsHistory)
            {
                Adverts.Tracks.Add(Track.FromDomain(advert.Advert, advert.Start.TimeOfDay, true));
            }
        }
    }

    public class TrackEnvelope
    {
        public ICollection<Track> Tracks { get; set; } = new List<Track>();
        
        public double TotalMinutes => Tracks.Sum(t => t.Length.TotalMinutes);
        public int Count => Tracks.Count;
        public int UniqueCount => Tracks.DistinctBy(t => t.Id).Count();
    }
    
    public class Track
    {
        public Guid Id { get; set; }
        public TimeSpan BeginTime { get; set; }
        public string Name { get; set; }
        public TimeSpan Length { get; set; }
        public bool IsAdvert { get; set; }

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