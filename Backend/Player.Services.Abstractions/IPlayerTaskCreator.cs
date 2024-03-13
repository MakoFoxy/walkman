using System;
using System.Threading;
using System.Threading.Tasks;

namespace Player.Services.Abstractions
{
    public interface IPlayerTaskCreator
    {
        Task CreatePlaylistGenerationTask(Guid subjectId, CancellationToken token = default);
        Task AddPlaylistGenerationTask(Guid subjectId, CancellationToken token = default);
    }
}