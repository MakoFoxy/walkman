using System;
using System.IO;
using System.Threading.Tasks;
using MarketRadio.Player.DataAccess;
using MarketRadio.Player.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
//В этом фрагменте кода представлен TrackController, контроллер ASP.NET Core, который используется для обработки запросов, связанных с аудио треками в вашем приложении:
namespace MarketRadio.Player.Controllers
{
    [ApiController] //[ApiController]: Указывает, что этот класс является контроллером API.
    [Route("api/[controller]")] //[Route("api/[controller]")]: Определяет маршрутизацию к API контроллера. [controller] будет заменено на track, таким образом базовый маршрут для этого контроллера будет /api/track.
    public class TrackController : ControllerBase
    { //public class TrackController : ControllerBase: Определяет класс TrackController, который наследует от ControllerBase, предоставляя ему набор функциональных возможностей для обработки HTTP запросов.
        private readonly PlayerContext _context;

        public TrackController(PlayerContext context)
        {
            _context = context;
            //             private readonly PlayerContext _context;: Определение и инициализация контекста данных, который предполагается использовать для доступа к трекам в базе данных.
            // public TrackController(PlayerContext context): Конструктор класса, который принимает контекст PlayerContext и инициализирует им внутреннее поле _context.
        }

        [HttpGet] //[HttpGet]: Этот атрибут указывает, что метод Get будет обрабатывать HTTP GET запросы. Однако здесь есть ошибка: используется [FromRoute], но в маршруте нет параметра для {id}. Правильно было бы изменить строку маршрута на что-то вроде [HttpGet("{id}")] для корректной работы.
        public async Task<IActionResult> Get([FromRoute] Guid id)
        { //public async Task<IActionResult> Get([FromRoute] Guid id): Асинхронный метод, который принимает идентификатор трека, ищет трек в базе данных и возвращает файл трека клиенту.
            var track = await _context.Tracks.AsNoTracking().SingleAsync(t => t.Id == id); //var track = await _context.Tracks.AsNoTracking().SingleAsync(t => t.Id == id);: Асинхронно извлекает трек из базы данных без отслеживания изменений (для улучшения производительности). Если трек с таким ID не найден, будет выброшено исключение.
            var filePath = Path.Combine(DefaultLocations.TracksPath, track.UniqueName); //var filePath = Path.Combine(DefaultLocations.TracksPath, track.UniqueName);: Создает полный путь к файлу трека, комбинируя базовый путь к трекам с уникальным именем файла трека.
            var stream = System.IO.File.OpenRead(filePath); //var stream = System.IO.File.OpenRead(filePath);: Открывает файловый поток для чтения указанного файла.
            return File(stream, "audio/mp3"); //return File(stream, "audio/mp3");: Возвращает файл в формате MP3 клиенту, что позволяет воспроизводить аудио напрямую через HTTP ответ.
        }
    }
    //Чтобы код работал правильно, необходимо убедиться, что в маршруте присутствует параметр {id} для метода Get, иначе приложение не сможет корректно извлечь и использовать идентификатор трека из URL запроса.
}