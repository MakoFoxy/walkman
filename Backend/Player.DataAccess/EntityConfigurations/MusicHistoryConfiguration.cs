using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Player.Domain;

namespace Player.DataAccess.EntityConfigurations
{
    public class MusicHistoryConfiguration : IEntityTypeConfiguration<MusicHistory>
    {
        public void Configure(EntityTypeBuilder<MusicHistory> builder)
        {
            builder.HasKey(e => e.Id);
        }
    }
}