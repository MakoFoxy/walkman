using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Player.Domain;

namespace Player.DataAccess.EntityConfigurations
{
    public class MusicTrackSelectionConfiguration : IEntityTypeConfiguration<MusicTrackSelection>
    {
        public void Configure(EntityTypeBuilder<MusicTrackSelection> builder)
        {
            builder.HasKey(e => e.Id);
        }
    }
}
