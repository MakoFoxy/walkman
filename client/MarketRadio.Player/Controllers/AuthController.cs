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
        private readonly IUserApi _userApi;
        private readonly PlayerContext _context;

        public AuthController(IUserApi userApi, PlayerContext context)
        {
            _userApi = userApi;
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody]AuthModel authModel)
        {
            var response = await _userApi.Auth(authModel);

            switch (response.StatusCode)
            {
                case HttpStatusCode.Unauthorized:
                    return Unauthorized();
                case HttpStatusCode.InternalServerError:
                    return StatusCode(500);
                default:
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

        [HttpGet("is-authorized")]
        public async Task<IActionResult> IsAuthorized()
        {
            var userSettings = await _context.UserSettings.SingleOrDefaultAsync(us => us.Key == UserSetting.Token);

            if (userSettings == null)
            {
                return Unauthorized();
            }

            var response = await _userApi.IsAuthenticated($"Bearer {userSettings.Value}");

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    return Unauthorized();
                }
            }

            return Ok();
        }
    }
}