using System;
using System.Collections.Generic;
using System.IO;

namespace Player.DTOs
{
    public class MusicFileModel
    {
        public Guid? MusicTrackId { get; set; }
        public string Name { get; set; }
        public Stream Stream { get; set; }
        public SimpleDto Genre { get; set; }

        public IReadOnlyCollection<SimpleDto> Select(Func<object, SimpleDto> value)
        {
            throw new NotImplementedException();
        }

    }
}
