using System;
using System.Linq.Expressions;
using Mapster;
using MarketRadio.SelectionsLoader.Models;
using Task = MarketRadio.SelectionsLoader.Domain.Task;

namespace MarketRadio.SelectionsLoader.Services.Abstractions.Mappers
{
    [Mapper]
    public interface ITaskMapper
    {
        Expression<Func<Task, TaskDto>> ProjectToDto { get; }
    }

    public class TaskMapperConfig : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<Task, TaskDto>()
                .Map(td => td.TaskType, t => Enum.Parse<TaskType>(t.TaskType.ToString()));
        }
    }
}