using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Player.Domain;

namespace Player.DataAccess.EntityConfigurations
{
    public class MusicTrackPlaylistConfiguration : IEntityTypeConfiguration<MusicTrackPlaylist>
    {
        public void Configure(EntityTypeBuilder<MusicTrackPlaylist> builder)
        {
            builder.HasKey(e => e.Id);
        }
    }
}
