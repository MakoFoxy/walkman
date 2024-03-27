using System.Linq;
using System.Threading.Tasks;
using MarketRadio.Player.DataAccess;
using MarketRadio.Player.DataAccess.Domain;
using MarketRadio.Player.Services.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Player.ClientIntegration;

namespace MarketRadio.Player.Controllers
{
    [ApiController]
    [Route("api/current-user")]
    public class CurrentUserController : ControllerBase
    //Этот код на C# представляет собой контроллер с именем CurrentUserController в пространстве имен MarketRadio.Player.Controllers, предназначенный для сервиса API ASP.NET Core. Контроллер управляет получением информации о текущем пользователе. Вот детальное описание его компонентов и функциональности:
    {
        private readonly PlayerContext _context; //PlayerContext: Контекст базы данных (предполагается, что это контекст Entity Framework Core), используемый для доступа и изменения настроек пользователя в базе данных приложения.
        private readonly IUserApi _userApi; //IUserApi: Интерфейс, вероятно, общается с внешним сервисом или другой частью приложения для получения информации о пользователях.

        public CurrentUserController(PlayerContext context, IUserApi userApi)
        {
            _context = context;
            _userApi = userApi;
            //    Конструктор принимает PlayerContext context и IUserApi userApi в качестве параметров. Они используются для взаимодействия с базой данных и API пользователей соответственно.
        }

        [HttpGet] //[HttpGet]: Этот атрибут указывает, что действие отвечает на HTTP GET запросы. Поскольку в маршруте не указаны параметры или строки запроса, он прослушивает запросы, сделанные непосредственно к базовому маршруту, определенному на уровне контроллера (api/current-user).
        public async Task<CurrentUserInfoDto> Get()
        {//Метод асинхронно извлекает настройку электронной почты пользователя из базы данных. Если эта настройка существует, он десериализует сохраненное значение (которое ожидается в формате JSON) в объект CurrentUserInfoDto и возвращает его.
            await using var transaction = await _context.Database.BeginTransactionAsync();
            var userSetting = await _context.UserSettings.SingleOrDefaultAsync(us => us.Key == UserSetting.Email);

            if (userSetting != null)
            {
                return JsonConvert.DeserializeObject<CurrentUserInfoDto>(userSetting.Value)!;
            }

            var token = await _context.UserSettings
                .Where(us => us.Key == UserSetting.Token)
                .Select(us => us.Value)
                .SingleAsync();  //Если настройка электронной почты отсутствует, метод затем извлекает токен пользователя из настроек, использует его для получения текущей информации о пользователе через IUserApi, сохраняет новые данные о пользователе в настройках и фиксирует транзакцию.

            var currentUserInfo = await _userApi.GetCurrentUserInfo($"Bearer {token}");

            userSetting = new UserSetting
            {
                Key = UserSetting.Email,
                Value = JsonConvert.SerializeObject(currentUserInfo.CurrentUserInfo),
            };
            _context.UserSettings.Add(userSetting);
            await transaction.CommitAsync();

            return currentUserInfo.CurrentUserInfo; //После получения информации о текущем пользователе и её сохранения, метод возвращает эти данные в формате CurrentUserInfoDto.
        }
    }
    //Этот контроллер обеспечивает базовую функциональность для получения информации о текущем пользователе, включая проверку наличия необходимых данных в настройках и обращение к внешнему API для обновления этой информации при необходимости.
}