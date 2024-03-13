using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Player.Helpers.Middlewares
{
    public class BadFormatLogMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<BadFormatLogMiddleware> _logger;

        public BadFormatLogMiddleware(RequestDelegate next, ILogger<BadFormatLogMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }
 
        public async Task InvokeAsync(HttpContext context)
        {
            var request = context.Request;
            request.EnableBuffering();
            string body;

            using (var reader = new StreamReader(
                context.Request.Body,
                Encoding.UTF8,
                false,
                1024,
                true))
            {
                body = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0;
            }

            await _next(context);
            
            if (new List<int>{400, 415}.Contains(context.Response.StatusCode))
            {
                if (new List<string>{HttpMethods.Post, HttpMethods.Put}.Contains(request.Method))
                {
                    var clientVersion = context.Request.Headers["X-Client-Version"];

                    if (clientVersion.Any())
                    {
                        _logger.LogWarning("Bad format {Code} {Body} {Version}",context.Response.StatusCode, body, clientVersion[0]);                        
                    }
                    else
                    {
                        _logger.LogWarning("Bad format {Code} {Body}",context.Response.StatusCode, body);
                    }
                }
            }
            
        }
    }
}