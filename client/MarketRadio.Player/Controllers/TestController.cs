using System;
using System.Threading.Tasks;
using MarketRadio.Player.Services.System;
using Microsoft.AspNetCore.Mvc;
using Player.ClientIntegration.System;

namespace MarketRadio.Player.Controllers
{
#if DEBUG //    #if DEBUG: Этот контроллер будет включен в сборку только во время отладки. В продакшн-сборке этот контроллер отсутствует, что помогает предотвратить несанкционированный доступ к методам тестирования.
    [ApiController] //[ApiController]: Указывает, что класс является контроллером API.
    [Route("/api/[controller]")] //[Route("/api/[controller]")]: Определяет маршрутизацию для контроллера. [controller] автоматически заменяется на имя класса контроллера без слова "Controller", так что в данном случае базовый URL будет /api/test.
    public class TestController : ControllerBase
    { //public class TestController : ControllerBase: Определяет класс TestController, который наследует функционал от ControllerBase.
        private readonly LogsUploader _logsUploader;

        public TestController(LogsUploader logsUploader)
        {
            _logsUploader = logsUploader;
            //    private readonly LogsUploader _logsUploader;: Определение зависимости, которая будет инжектирована через конструктор. LogsUploader - это, вероятно, сервис для загрузки логов.
            // public TestController(LogsUploader logsUploader): Конструктор, который принимает экземпляр LogsUploader и инициализирует приватное поле _logsUploader.
        }

        [HttpPost] //[HttpPost]: Атрибут, указывающий, что метод Upload обрабатывает HTTP POST запросы. Поскольку здесь не указан путь, он будет обрабатывать запросы, поступающие непосредственно на /api/test.
        public async Task<IActionResult> Upload()
        {
            await _logsUploader.UploadLogs(new DownloadLogsRequest
            {
                From = DateTime.Now.AddYears(-1),
                To = DateTime.Now,
            });
            return Ok();
            //public async Task<IActionResult> Upload(): Метод Upload, который асинхронно вызывает метод UploadLogs у _logsUploader, передавая новый запрос на загрузку логов с датами "от" и "до", охватывающими последний год. После выполнения загрузки метод возвращает статус Ok, указывающий на успешное выполнение запроса.
        }
    }
#endif
    //Этот контроллер может использоваться для тестирования и отладки функционала загрузки логов без воздействия на основной код или доступ пользователей в продакшн-среде.
}