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
        }
    }
}