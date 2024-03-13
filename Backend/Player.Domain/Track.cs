using System;
using Player.Domain.Base;

namespace Player.Domain
{
    public class Track : Entity
    {
        public string Name { get; set; }
        public string FilePath { get; set; }
        public string Extension { get; set; }
        public User Uploader { get; set; }
        public Guid UploaderId { get; set; }
        public TimeSpan Length { get; set; }
        public TrackType TrackType { get; set; }
        public Guid TrackTypeId { get; set; }
        public bool IsValid { get; set; }
        public string Hash { get; set; }
    }
}
