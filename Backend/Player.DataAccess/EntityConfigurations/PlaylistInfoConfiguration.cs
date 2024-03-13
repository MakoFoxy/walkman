using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Player.Domain;

namespace Player.DataAccess.EntityConfigurations
{
    public class PlaylistInfoConfiguration : IEntityTypeConfiguration<PlaylistInfo>
    {
        public void Configure(EntityTypeBuilder<PlaylistInfo> builder)
        {
            builder.Property(pi => pi.Info).IsRequired();
            builder.ToTable("PlaylistInfos");
        }
    }
}