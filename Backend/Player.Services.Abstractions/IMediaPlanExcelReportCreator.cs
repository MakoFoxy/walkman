using Player.DTOs;

namespace Player.Services.Abstractions
{
    public interface IMediaPlanExcelReportCreator
    {
        byte[] Create(PlaylistModel playlist);
    }
}
