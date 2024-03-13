using MarketRadio.SelectionsLoader.Domain;
using MarketRadio.SelectionsLoader.Models;
using MarketRadio.SelectionsLoader.Services.Abstractions.Mappers;
using TaskType = MarketRadio.SelectionsLoader.Models.TaskType;

namespace MarketRadio.PlaylistLoader.Services.Abstractions.Mappers
{
    public partial class TaskMapper : ITaskMapper
    {
        public System.Linq.Expressions.Expression<System.Func<Task, TaskDto>> ProjectToDto => p1 => new TaskDto()
        {
            Id = p1.Id,
            Name = p1.Name,
            IsFinished = p1.IsFinished,
            Priority = p1.Priority,
            CreateDate = p1.CreateDate,
            FinishDate = p1.FinishDate,
            TaskObjectId = p1.TaskObjectId,
            TaskType = System.Enum.Parse<TaskType>(p1.TaskType.ToString())
        };
    }
}