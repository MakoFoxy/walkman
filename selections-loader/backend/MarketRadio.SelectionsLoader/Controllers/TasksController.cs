using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MarketRadio.SelectionsLoader.BackgroundServices;
using MarketRadio.SelectionsLoader.DataAccess;
using MarketRadio.SelectionsLoader.Models;
using MarketRadio.SelectionsLoader.Services;
using MarketRadio.SelectionsLoader.Services.Abstractions;
using MarketRadio.SelectionsLoader.Services.Abstractions.Mappers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MarketRadio.SelectionsLoader.Controllers
{
    [ApiController]
    [Route("/api/tasks")]
    public class TasksController : ControllerBase
    {
        private readonly DatabaseContext _context;
        private readonly ITaskMapper _taskMapper;
        private readonly ILoadingState _loadingState;

        public TasksController(
            DatabaseContext context, 
            ITaskMapper taskMapper,
            ILoadingState loadingState)
        {
            _context = context;
            _taskMapper = taskMapper;
            _loadingState = loadingState;
        }

        [HttpGet]
        public async Task<List<TaskDto>> Get()
        {
            var tasks = await _context.Tasks
                .OrderBy(t => t.IsFinished)
                .ThenBy(t => t.Priority)
                .Select(_taskMapper.ProjectToDto)
                .ToListAsync();
            
            foreach (var taskDto in tasks)
            {
                taskDto.Progress = _loadingState.GetProgress(taskDto.TaskObjectId)?.Percentage;
            }

            return tasks;
        }

        [HttpGet("statuses")]
        public List<LoadingProgress> GetStatuses()
        {
            return _loadingState.LoadingProgress;
        }

        [HttpPost("start-background-jobs")]
        public IActionResult StartBackgroundJobs()
        {
            TaskExecutorBackgroundService.StartService = true;
            return Ok();
        }
    }
}