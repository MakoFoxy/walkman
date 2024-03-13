using Player.Domain;

namespace Player.Services.PlaylistServices;

public class PlaylistEnvelope
{
    public Playlist Playlist { get; set; }
    public bool IsNew { get; set; }
}