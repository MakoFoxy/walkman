using System.Net.Http;
using System.Threading.Tasks;
using MarketRadio.Player.Models;
using Refit;

namespace MarketRadio.Player.Services.Http
{
    public interface IUserApi
    {
        [Post("/api/v1/auth")] //    authModel: Объект, содержащий учётные данные пользователя, такие как email пользователя и пароль.
        Task<HttpResponseMessage> Auth([Body] AuthModel authModel); //Возвращаемое значение: Task<HttpResponseMessage> — асинхронный таск, возвращающий HTTP-ответ от сервера. Ответ может содержать информацию о статусе аутентификации, включая успешность операции и токен доступа.

        [Get("/api/v1/auth/is-authenticated")] //    Цель: Проверка, аутентифицирован ли пользователь.

        Task<HttpResponseMessage> IsAuthenticated([Header("Authorization")] string token); //    token: Токен доступа, используемый для верификации аутентификации пользователя.Возвращаемое значение: Task<HttpResponseMessage> — проверка статуса аутентификации пользователя на основе предоставленного токена.

        [Get("/api/v1/auth/renew")] //Обновление токена доступа пользователя.
        Task<HttpResponseMessage> Renew([Header("Authorization")] string token); //асинхронный таск, возвращающий HTTP-ответ, содержащий новый токен доступа или информацию о неудаче обновления.

        [Get("/api/v1/current-user/info")] //Получение информации о текущем аутентифицированном пользователе.
        Task<CurrentUserInfoResponse> GetCurrentUserInfo([Header("Authorization")] string token); //асинхронный таск, возвращающий объект CurrentUserInfoResponse, который содержит детальную информацию о пользователе, включая имя, электронную почту и другие данные профиля.
    }
    //Интерфейс IUserApi позволяет разработчикам абстрагироваться от деталей реализации HTTP-запросов и сосредоточиться на бизнес-логике приложения, связанной с аутентификацией и управлением пользователями. Использование Refit облегчает создание типизированных клиентов для взаимодействия с веб-сервисами, делая код чище и удобнее для поддержки.
}