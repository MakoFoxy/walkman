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
    // Класс BadFormatLogMiddleware:

    // Назначение: Это middleware предназначено для логирования тел запросов, которые привели к HTTP-ответам с кодами статуса 400 (Bad Request) или 415 (Unsupported Media Type), что часто связано с неверным форматом тела запроса.
    // Конструктор: Принимает RequestDelegate (следующий шаг в конвейере middleware) и ILogger<BadFormatLogMiddleware> для логирования. Эти зависимости инжектируются через механизм внедрения зависимостей ASP.NET Core.
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

            if (new List<int> { 400, 415 }.Contains(context.Response.StatusCode))
            {
                if (new List<string> { HttpMethods.Post, HttpMethods.Put }.Contains(request.Method))
                {
                    var clientVersion = context.Request.Headers["X-Client-Version"];

                    if (clientVersion.Any())
                    {
                        _logger.LogWarning("Bad format {Code} {Body} {Version}", context.Response.StatusCode, body, clientVersion[0]);
                    }
                    else
                    {
                        _logger.LogWarning("Bad format {Code} {Body}", context.Response.StatusCode, body);
                    }
                }
            }

            //         Логика работы:
            // Вначале включается буферизация для тела запроса, чтобы можно было прочитать тело несколько раз.
            // Содержимое тела запроса считывается и сохраняется в переменную body. После чтения позиция в потоке тела запроса сбрасывается в начало, чтобы не нарушить чтение тела в последующих обработчиках.
            // Вызывается следующий обработчик в конвейере middleware.
            // После выполнения всех последующих middleware проверяется статус-код ответа. Если он соответствует 400 или 415, и метод запроса является POST или PUT, производится логирование.
            // В лог записывается статус-код ответа, тело запроса и, если доступна, версия клиента из заголовков запроса.

        }
    }
}