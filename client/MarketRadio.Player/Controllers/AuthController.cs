using System.Data;
using System.Net;
using System.Threading.Tasks;
using MarketRadio.Player.DataAccess;
using MarketRadio.Player.DataAccess.Domain;
using MarketRadio.Player.Models;
using MarketRadio.Player.Services.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace MarketRadio.Player.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserApi _userApi; //IUserApi: Интерфейс, представляющий API, связанный с пользователями. Вероятно, используется для общения с сервисом управления пользователями для аутентификации пользователей и получения информации о пользователе.
        private readonly PlayerContext _context; //PlayerContext: Контекст (предположительно контекст базы данных), используемый для доступа и изменения настроек пользователя в базе данных приложения.

        public AuthController(IUserApi userApi, PlayerContext context)
        {
            _userApi = userApi;
            _context = context;
            //    Конструктор принимает IUserApi userApi и PlayerContext context в качестве параметров и присваивает их приватным только для чтения полям. Эти зависимости, вероятно, внедряются через контейнер IoC (Inversion of Control), который управляет внедрением зависимостей.
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] AuthModel authModel)
        {//    В действии Login используются транзакции базы данных для обеспечения атомарности изменений в настройках пользователя. Это означает, что если какая-либо часть процесса не удастся, изменения могут быть откачены для поддержания целостности данных.
            var response = await _userApi.Auth(authModel); //Принимает объект AuthModel из тела запроса, который, вероятно, содержит информацию для аутентификации, такую как имя email и password.

            //Использует _userApi для аутентификации пользователя. В зависимости от кода состояния ответа, возвращает либо статус неавторизованного доступа, либо внутреннюю ошибку сервера, или продолжает процесс входа.
            switch (response.StatusCode)
            {
                case HttpStatusCode.Unauthorized:
                    return Unauthorized();
                case HttpStatusCode.InternalServerError:
                    return StatusCode(500);
                default:
                    //Если аутентификация успешна, начинается транзакция, удаляются существующие настройки пользователя и добавляются новые настройки, включая токен и электронную почту. Наконец, транзакция подтверждается и возвращается статус Ok.
                    {
                        await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
                        await _context.UserSettings.ExecuteDeleteAsync();
                        var token = await response.Content.ReadAsStringAsync();
                        _context.UserSettings.Add(new UserSetting
                        {
                            Key = UserSetting.Token,
                            Value = token,
                        });

                        var currentUserInfo = await _userApi.GetCurrentUserInfo($"Bearer {token}");

                        _context.UserSettings.Add(new UserSetting
                        {
                            Key = UserSetting.Email,
                            Value = JsonConvert.SerializeObject(currentUserInfo.CurrentUserInfo),
                        });
                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();
                        return Ok();
                    }
            }
        }

        [HttpGet("is-authorized")] //[HttpGet("is-authorized")]: Атрибут, указывающий, что это действие обрабатывает HTTP GET запросы и может быть доступно через URL api/Auth/is-authorized.
        public async Task<IActionResult> IsAuthorized()
        {
            var userSettings = await _context.UserSettings.SingleOrDefaultAsync(us => us.Key == UserSetting.Token);

            if (userSettings == null)
            {
                return Unauthorized(); //Проверяет наличие настройки пользователя с токеном. Если ее нет, возвращает статус неавторизованного доступа.
            }

            var response = await _userApi.IsAuthenticated($"Bearer {userSettings.Value}");

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    return Unauthorized(); //Проверяет наличие настройки пользователя с токеном. Если ее нет, возвращает статус неавторизованного доступа.
                }
            }

            return Ok(); //Если токен существует, пытается проверить действительность токена с помощью _userApi. В зависимости от ответа возвращает либо статус неавторизованного доступа, либо статус Ok, что означает, что токен все еще действителен.     
            
            //Контроллер должным образом обрабатывает различные HTTP коды состояния. Возвращает Unauthorized() для HttpStatusCode.Unauthorized, и StatusCode(500) для HttpStatusCode.InternalServerError.
        }
    }
    //Этот контроллер обеспечивает базовую функциональность, необходимую для аутентификации пользователя, включая вход в систему и проверку авторизации пользователя.
}