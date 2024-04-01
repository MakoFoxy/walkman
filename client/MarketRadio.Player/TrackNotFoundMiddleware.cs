using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace MarketRadio.Player
{//Класс TrackNotFoundMiddleware является пользовательским промежуточным программным обеспечением (middleware) для веб-приложения ASP.NET Core, предназначенным для логирования предупреждений, когда запрос к треку не может быть обработан из-за отсутствия файла. Давайте разберем его построчно.
    public class TrackNotFoundMiddleware
    {//TrackNotFoundMiddleware — класс промежуточного ПО для обработки ситуаций, когда трек не найден.
        private readonly RequestDelegate _next;
        private readonly ILogger<TrackNotFoundMiddleware> _logger;

        public TrackNotFoundMiddleware(RequestDelegate next, ILogger<TrackNotFoundMiddleware> logger)
        {
            _next = next;
            _logger = logger;
            //             RequestDelegate _next;: Делегат для следующего промежуточного ПО в конвейере.
            // ILogger<TrackNotFoundMiddleware> _logger;: Логгер для регистрации информации о неудачных запросах к трекам.
            // Конструктор инициализирует эти два поля, получая их через параметры.
        }

        public async Task InvokeAsync(HttpContext context)
        {//Асинхронно вызывается для каждого HTTP запроса.
            var tracksMask = "/tracks";//var tracksMask = "/tracks";: Указывает на маску пути, по которому доступны треки.

            var requestPath = context.Request.Path; //var requestPath = context.Request.Path;: Получает путь текущего запроса.
            if (requestPath.HasValue && requestPath.Value!.StartsWith(tracksMask))
            {//Если путь запроса начинается с /tracks, то метод делегирует обработку запроса следующему промежуточному ПО и после его выполнения проверяет статус ответа.
                await _next(context);//Вызов await _next(context); в контексте промежуточного ПО (middleware) для ASP.NET Core не гарантирует, что трек найден. Этот вызов просто передает обработку запроса следующему компоненту в конвейере промежуточного программного обеспечения или к конечной точке, которая обрабатывает запрос. Реальный результат обработки запроса становится известен только после выполнения этого вызова и зависит от логики обработки в последующих компонентах конвейера.
                if (context.Response.StatusCode == 404) //В случае, если статус ответа равен 404 (Не найдено), логирует предупреждение о том, что трек не найден.
                {
                    _logger.LogWarning("Track {Track} not found", requestPath.Value.Replace(tracksMask, ""));
                }
                return;
            }

            await _next(context);//Если путь не соответствует маске, запрос также передается следующему промежуточному ПО без дополнительной обработки.
        }
        //Это промежуточное ПО позволяет легко отслеживать ситуации, когда пользовательские запросы к трекам не удовлетворяются из-за их отсутствия, и логировать такие случаи для дальнейшего анализа или уведомления администрации. Это может быть полезно для мониторинга доступности контента или выявления ошибок в структуре хранения данных.
    }
}