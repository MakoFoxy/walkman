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
    {
        private readonly PlayerContext _context;
        private readonly IUserApi _userApi;

        public CurrentUserController(PlayerContext context, IUserApi userApi)
        {
            _context = context;
            _userApi = userApi;
        }

        [HttpGet]
        public async Task<CurrentUserInfoDto> Get()
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            var userSetting = await _context.UserSettings.SingleOrDefaultAsync(us => us.Key == UserSetting.Email);

            if (userSetting != null)
            {
                return JsonConvert.DeserializeObject<CurrentUserInfoDto>(userSetting.Value)!;
            }

            var token = await _context.UserSettings
                .Where(us => us.Key == UserSetting.Token)
                .Select(us => us.Value)
                .SingleAsync();
                
            var currentUserInfo = await _userApi.GetCurrentUserInfo($"Bearer {token}");

            userSetting = new UserSetting
            {
                Key = UserSetting.Email,
                Value = JsonConvert.SerializeObject(currentUserInfo.CurrentUserInfo),
            };
            _context.UserSettings.Add(userSetting);
            await transaction.CommitAsync();

            return currentUserInfo.CurrentUserInfo;
        }
    }
}