using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Player.Domain;

namespace Player.DataAccess.EntityConfigurations
{
    public class AdvertPlaylistConfiguration : IEntityTypeConfiguration<AdvertPlaylist>
    {
        public void Configure(EntityTypeBuilder<AdvertPlaylist> builder)
        {
            builder.HasKey(e => e.Id);
        }
    }
}
