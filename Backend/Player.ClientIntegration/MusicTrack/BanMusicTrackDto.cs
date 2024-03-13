using System;

namespace Player.ClientIntegration.MusicTrack
{
    public class BanMusicTrackDto
    {
        public Guid MusicId { get; set; }
        public Guid ObjectId { get; set; }
        //BanMusicTrackDto — это объект данных, который отправляется клиентам через SignalR. Он содержит информацию о заблокированном треке и объекте.
    }
}