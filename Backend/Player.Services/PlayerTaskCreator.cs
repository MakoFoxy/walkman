using System;
using System.Threading;
using System.Threading.Tasks;
using Player.DataAccess;
using Player.Domain;
using Player.Services.Abstractions;

namespace Player.Services
{
    public class PlayerTaskCreator : IPlayerTaskCreator
    {
        private readonly PlayerContext _context;

        public PlayerTaskCreator(PlayerContext context)
        {
            _context = context;
        }
        
        public Task CreatePlaylistGenerationTask(Guid subjectId, CancellationToken token = default)
        {
            _context.Tasks.Add(CreateTask(TaskType.PlaylistGeneration, subjectId));
            return _context.SaveChangesAsync(token);
        }

        public Task AddPlaylistGenerationTask(Guid subjectId, CancellationToken token = default)
        {
            _context.Tasks.Add(CreateTask(TaskType.PlaylistGeneration, subjectId));
            return Task.CompletedTask;
        }
        
        private PlayerTask CreateTask(TaskType type, Guid subjectId)
        {
            return new PlayerTask
            {
                Type = type,
                RegisterDate = DateTimeOffset.Now,
                SubjectId = subjectId,
            };
        }
    }
}