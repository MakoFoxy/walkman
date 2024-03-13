using System;
using System.IO;
using System.Threading.Tasks;
using MarketRadio.Player.DataAccess;
using MarketRadio.Player.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MarketRadio.Player.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TrackController : ControllerBase
    {
        private readonly PlayerContext _context;

        public TrackController(PlayerContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromRoute] Guid id)
        {
            var track = await _context.Tracks.AsNoTracking().SingleAsync(t => t.Id == id);
            var filePath = Path.Combine(DefaultLocations.TracksPath, track.UniqueName);
            var stream = System.IO.File.OpenRead(filePath);
            return File(stream, "audio/mp3");
        }
    }
}