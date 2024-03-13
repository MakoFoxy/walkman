using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Player.DataAccess;

namespace Player.WebApi.Controllers.v1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class MigrateController : ControllerBase
    {
        private readonly PlayerContext _context;

        public MigrateController(PlayerContext context)
        {
            _context = context;
        }
        
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var pendingMigrations = await _context.Database.GetPendingMigrationsAsync();
            await _context.Database.MigrateAsync();
            return Ok(pendingMigrations);
        }
    }
}