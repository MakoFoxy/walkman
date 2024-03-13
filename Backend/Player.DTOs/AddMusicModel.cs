using Microsoft.AspNetCore.Http;

namespace Player.DTOs
{
    public class AddMusicModel
    {
        public IFormCollection MusicFiles  { get; set; }
    }
}
