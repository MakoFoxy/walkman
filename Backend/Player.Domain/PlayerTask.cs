using System;
using Player.Domain.Base;

namespace Player.Domain
{
    public class PlayerTask : Entity
    {
        public TaskType Type { get; set; }
        public DateTimeOffset RegisterDate { get; set; }
        public DateTimeOffset? FinishDate { get; set; }
        public bool IsFinished { get; set; }
        public Guid SubjectId { get; set; }
    }

    public enum TaskType
    {
        None,
        PlaylistGeneration,
    }
}