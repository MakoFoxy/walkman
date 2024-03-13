using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Player.Domain;

namespace Player.DataAccess.EntityConfigurations
{
    public class MusicTrackConfiguration : IEntityTypeConfiguration<MusicTrack>
    {
        public void Configure(EntityTypeBuilder<MusicTrack> builder)
        {
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Name).IsRequired();
            builder.Property(e => e.Extension).IsRequired();
            builder.Property(e => e.Hash).IsRequired();
            builder.Property(e => e.Name).IsRequired();
            builder.Property(e => e.FilePath).IsRequired();
        }
    }
}
