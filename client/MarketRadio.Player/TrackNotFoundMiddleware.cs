using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace MarketRadio.Player
{
    public class TrackNotFoundMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TrackNotFoundMiddleware> _logger;

        public TrackNotFoundMiddleware(RequestDelegate next, ILogger<TrackNotFoundMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var tracksMask = "/tracks";

            var requestPath = context.Request.Path;
            if (requestPath.HasValue && requestPath.Value!.StartsWith(tracksMask))
            {
                await _next(context);
                if (context.Response.StatusCode == 404)
                {
                    _logger.LogWarning("Track {Track} not found", requestPath.Value.Replace(tracksMask, ""));
                }
                return;
            }

            await _next(context);
        }
    }
}