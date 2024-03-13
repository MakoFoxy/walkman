using System;

namespace MarketRadio.SelectionsLoader.Models
{
    public class TaskDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public bool IsFinished { get; set; }
        public int Priority { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? FinishDate { get; set; }
        public Guid TaskObjectId { get; set; }
        public TaskType TaskType { get; set; }

        public double? Progress { get; set; }
    }
    
    public enum TaskType
    {
        None,
        Selection,
        Track,
    }
}