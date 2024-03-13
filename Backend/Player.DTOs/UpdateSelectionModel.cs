using System;
using System.Collections.Generic;

namespace Player.DTOs
{
    public class UpdateSelectionModel : SimpleDto
    {
        public IList<Guid> Tracks { get; set; } = new List<Guid>();
    }
}
