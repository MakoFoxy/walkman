using System;
using System.Threading;
using System.Threading.Tasks;
using Player.DataAccess;
using Player.Domain;
using Player.Services.Abstractions;

namespace Player.Services
{
    public class PlayerTaskCreator : IPlayerTaskCreator
    //Этот код представляет собой класс PlayerTaskCreator, который реализует интерфейс IPlayerTaskCreator. Класс используется для создания задач генерации плейлиста в базе данных. Давайте разберем ключевые моменты этого класса:
    {
        private readonly PlayerContext _context; //В конструктор класса передается контекст базы данных PlayerContext, который затем сохраняется в приватной переменной _context для дальнейшего использования в методах класса.

        public PlayerTaskCreator(PlayerContext context)
        {
            _context = context;
        }

        public Task CreatePlaylistGenerationTask(Guid subjectId, CancellationToken token = default)
        {
            //Этот асинхронный метод создает новую задачу генерации плейлиста и добавляет ее в базу данных. Он принимает идентификатор subjectId, который, вероятно, является идентификатором сущности, для которой генерируется плейлист (например, пользователя или события). После добавления задачи в контекст базы данных метод возвращает Task, который представляет асинхронное сохранение изменений в базе данных с возможностью отмены через CancellationToken.
            _context.Tasks.Add(CreateTask(TaskType.PlaylistGeneration, subjectId));
            return _context.SaveChangesAsync(token);
        }

        public Task AddPlaylistGenerationTask(Guid subjectId, CancellationToken token = default)
        {
            //Этот метод также создает новую задачу генерации плейлиста и добавляет ее в базу данных, но, в отличие от CreatePlaylistGenerationTask, он не выполняет асинхронное сохранение изменений. Вместо этого он сразу возвращает Task.CompletedTask, что означает, что возвращаемая задача уже завершена. Это может быть использовано, если сохранение изменений в базе данных предполагается выполнить позже или если не требуется немедленное подтверждение добавления задачи в базу.
            _context.Tasks.Add(CreateTask(TaskType.PlaylistGeneration, subjectId));
            return Task.CompletedTask;
        }

        private PlayerTask CreateTask(TaskType type, Guid subjectId)
        {
            //Этот вспомогательный метод создает экземпляр PlayerTask с указанным типом задачи и идентификатором субъекта. RegisterDate устанавливается в текущее время. Этот метод используется внутри CreatePlaylistGenerationTask и AddPlaylistGenerationTask для создания объектов задач.
            return new PlayerTask
            {
                Type = type,
                RegisterDate = DateTimeOffset.Now,
                SubjectId = subjectId,
            };
        }
    }
    //В целом, класс PlayerTaskCreator обеспечивает интерфейс для создания задач в системе, что может быть частью большой системы управления плейлистами или медиаконтентом.
}