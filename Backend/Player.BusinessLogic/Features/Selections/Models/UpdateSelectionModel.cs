using System;
using System.Collections.Generic;
using Player.DTOs;

namespace Player.BusinessLogic.Features.Selections.Models
{
    public class UpdateSelectionModel
    {
        public Guid? Id { get; set; }
        public string Name { get; set; }
        public DateTimeOffset DateBegin { get; set; }
        public DateTimeOffset? DateEnd { get; set; }
        public bool IsPublic { get; set; }
        public IList<Guid> Tracks { get; set; } = new List<Guid>();

        public Guid? ObjectId { get; set; }
    }
}