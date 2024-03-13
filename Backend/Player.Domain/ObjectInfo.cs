using System;
using System.Collections.Generic;
using Player.Domain.Base;

namespace Player.Domain
{
    public class ObjectInfo : Entity
    {
        public TimeSpan BeginTime { get; set; }
        public TimeSpan EndTime { get; set; }

        public TimeSpan WorkTime =>
            EndTime > BeginTime ? EndTime - BeginTime : TimeSpan.FromHours(24) - BeginTime + EndTime;

        public int Attendance { get; set; }
        public string ActualAddress { get; set; }
        public string LegalAddress { get; set; }
        public ActivityType ActivityType { get; set; }
        public Guid ActivityTypeId { get; set; }
        public string Name { get; set; }
        public ICollection<Image> Images { get; set; } = new List<Image>();
        public string Geolocation { get; set; }
        public ServiceCompany ServiceCompany { get; set; }
        public double Area { get; set; }
        public int RentersCount { get; set; }
        public string Bin { get; set; }
        public ICollection<Interval> Intervals { get; set; } = new List<Interval>();
        public double SilentPercent { get; set; }
        public int SilentBlockInterval { get; set; } = 4;

        public City City { get; set; }
        public Guid CityId { get; set; }
        public bool IsOnline { get; set; }
        public DateTime LastOnlineTime { get; set; }
        public string Version { get; set; }

        /// <summary>
        /// Музыкальные подборки, подобранные для текущего объекта
        /// </summary>
        public ICollection<ObjectSelection> Selections { get; set; } = new List<ObjectSelection>();
        public string ClientSettings { get; set; }

        public DayOfWeek[] FreeDays { get; set; } = new DayOfWeek[0];
        public ICollection<Playlist> Playlists { get; set; } = new List<Playlist>();
        public ResponsiblePerson ResponsiblePersonOne { get; set; }
        public ResponsiblePerson ResponsiblePersonTwo { get; set; }
        public int Priority { get; set; }

        public ICollection<BannedMusicInObject> BannedMusicInObjects { get; set; } = new List<BannedMusicInObject>();

        public int MaxAdvertBlockInSeconds { get; set; }

        public class ResponsiblePerson
        {
            public string ComplexName { get; set; }
            public string Phone { get; set; }
            public string Email { get; set; }
        }
    }
}
