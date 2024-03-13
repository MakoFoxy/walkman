using System.Net.Http;
using System.Threading.Tasks;
using MarketRadio.SelectionsLoader.Models;
using Refit;

namespace MarketRadio.SelectionsLoader.Api
{
    public interface IUserApi
    {
        [Post("/api/v1/auth")]
        Task<HttpResponseMessage> Auth([Body] AuthModel authModel);

        [Get("/api/v1/auth/is-authenticated")]

        Task<HttpResponseMessage> IsAuthenticated([Header("Authorization")] string token);
        
        [Get("/api/v1/auth/renew")]
        Task<HttpResponseMessage> Renew([Header("Authorization")] string token);

        [Get("/api/v1/current-user/info")]
        Task<CurrentUserInfoResponse> GetCurrentUserInfo([Header("Authorization")] string token);
    }
}