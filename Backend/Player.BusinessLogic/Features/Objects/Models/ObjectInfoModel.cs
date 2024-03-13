using System;
using System.Collections.Generic;
using Player.DTOs;

namespace Player.BusinessLogic.Features.Objects.Models
{
    public class ObjectInfoModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public TimeSpan BeginTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int Attendance { get; set; }
        public string ActualAddress { get; set; }
        public string LegalAddress { get; set; }
        public SimpleDto ActivityType { get; set; }
        public SimpleDto City { get; set; }
        public string Geolocation { get; set; }
        public SimpleDto ServiceCompany { get; set; }
        public double Area { get; set; }
        public int RentersCount { get; set; }
        public string Bin { get; set; }
        public int Priority { get; set; }
        public bool IsOnline { get; set; }
        public ResponsiblePersonModel ResponsiblePersonOne { get; set; }
        public ResponsiblePersonModel ResponsiblePersonTwo { get; set; }
        public DayOfWeek[] FreeDays { get; set; } = Array.Empty<DayOfWeek>();
        public ICollection<SimpleDto> Selections { get; set; } = new List<SimpleDto>();
        
        public int MaxAdvertBlockInSeconds { get; set; }

        public class ResponsiblePersonModel
        {
            public string ComplexName { get; set; }
            public string Phone { get; set; }
            public string Email { get; set; }
        }
    }
}