using System;
using System.Collections.Generic;
using Player.DTOs;

namespace Player.BusinessLogic.Features.Adverts.Models
{
    public class BaseAdvertModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public IEnumerable<SimpleDto> Objects { get; set; } = new List<SimpleDto>();
        public DateTime CreateDate { get; set; }
    }
}