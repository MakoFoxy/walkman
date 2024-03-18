using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace Player.DataAccess
{
    public class DbContextTransactionPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        private readonly PlayerContext _context;

        public DbContextTransactionPipelineBehavior(PlayerContext context)
        {
            _context = context;
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            TResponse result;

            try
            {
                await _context.BeginTransactionAsync(cancellationToken);

                result = await next();

                await _context.CommitTransactionAsync(cancellationToken);
            }
            catch (Exception)
            {
                await _context.RollbackTransactionAsync(cancellationToken);
                throw;
            }

            return result;

            //             Этот метод реализует интерфейс IPipelineBehavior<TRequest, TResponse>, который определен в MediatR. Он оборачивает выполнение запроса в транзакцию базы данных.
            // TRequest и TResponse являются типами запроса и ответа соответственно, с которыми работает это поведение пайплайна.
            // CancellationToken cancellationToken предоставляется для поддержки асинхронной отмены операций.
            // RequestHandlerDelegate<TResponse> next — это делегат, который представляет следующий шаг в цепочке обработки запроса, то есть сам обработчик запроса.
        }
    }
}

//     Метод начинает с инициации транзакции в контексте базы данных с помощью вызова await _context.BeginTransactionAsync(cancellationToken).
//     Затем, он вызывает await next(), чтобы продолжить обработку запроса в пайплайне — фактически, это передает управление следующему обработчику или следующему поведению в пайплайне.
//     Если вызов next() успешно завершается (то есть запрос обработан без исключений), метод продолжает с подтверждением транзакции, вызывая await _context.CommitTransactionAsync(cancellationToken).
//     В случае возникновения исключения во время выполнения запроса, транзакция откатывается с помощью await _context.RollbackTransactionAsync(cancellationToken), и исключение перебрасывается дальше.
//     В конце, метод возвращает результат обработки запроса.

// Применение:

// Это поведение позволяет автоматически обернуть логику обработки каждого запроса, отправленного через MediatR, в транзакцию базы данных. Это обеспечивает, что все изменения в базе данных, выполненные в ходе обработки одного запроса, либо все вместе успешно сохраняются (коммитятся), либо, в случае ошибки, не применяются вовсе (откатываются). Это поведение важно для поддержания целостности данных.