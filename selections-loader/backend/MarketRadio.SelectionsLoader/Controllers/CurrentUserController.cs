using System.Linq;
using System.Threading.Tasks;
using MarketRadio.SelectionsLoader.Api;
using MarketRadio.SelectionsLoader.DataAccess;
using MarketRadio.SelectionsLoader.Domain;
using MarketRadio.SelectionsLoader.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace MarketRadio.SelectionsLoader.Controllers
{
    [ApiController]
    [Route("/api/current-user")]
    public class CurrentUserController : ControllerBase
    {
        private readonly DatabaseContext _context;
        private readonly IUserApi _userApi;

        public CurrentUserController(DatabaseContext context, IUserApi userApi)
        {
            _context = context;
            _userApi = userApi;
        }

        [HttpGet]
        public async Task<CurrentUserInfoDto> Get()
        {
            var settings = await _context.Settings.SingleOrDefaultAsync(us => us.Key == Settings.Email);

            if (settings != null)
            {
                return JsonConvert.DeserializeObject<CurrentUserInfoDto>(settings.Value);
            }

            var token = await _context.Settings
                .Where(us => us.Key == Settings.Token)
                .Select(us => us.Value)
                .SingleAsync();
                
            var currentUserInfo = await _userApi.GetCurrentUserInfo($"Bearer {token}");

            settings = new Settings
            {
                Key = Settings.Email,
                Value = JsonConvert.SerializeObject(currentUserInfo.CurrentUserInfo),
            };
            _context.Settings.Add(settings);
            return currentUserInfo.CurrentUserInfo;
        }
    }
}