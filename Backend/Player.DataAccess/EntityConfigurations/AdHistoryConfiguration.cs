using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Player.Domain;

namespace Player.DataAccess.EntityConfigurations
{
    public class AdHistoryConfiguration : IEntityTypeConfiguration<AdHistory>
    {
        public void Configure(EntityTypeBuilder<AdHistory> builder)
        {
            builder.HasKey(e => e.Id);
        }
    }
}
