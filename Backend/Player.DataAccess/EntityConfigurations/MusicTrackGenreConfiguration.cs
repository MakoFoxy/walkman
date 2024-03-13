using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Player.Domain;

namespace Player.DataAccess.EntityConfigurations
{
    public class MusicTrackGenreConfiguration : IEntityTypeConfiguration<MusicTrackGenre>
    {
        public void Configure(EntityTypeBuilder<MusicTrackGenre> builder)
        {
            builder.HasKey(e => e.Id);
        }
    }
}
