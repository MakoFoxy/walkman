using System;
using MarketRadio.SelectionsLoader.Domain.Abstractions;

namespace MarketRadio.SelectionsLoader.Domain
{
    public class Task : Entity
    {
        public string Name { get; set; }
        public bool IsFinished { get; set; }
        public int Priority { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? FinishDate { get; set; }
        public Guid TaskObjectId { get; set; }
        public TaskType TaskType { get; set; }
    }

    public enum TaskType
    {
        None,
        Selection,
        Track,
    }
}