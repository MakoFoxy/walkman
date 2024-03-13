using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Player.Domain;

namespace Player.DataAccess.EntityConfigurations
{
    public class AdTimeConfiguration : IEntityTypeConfiguration<AdTime>
    {
        public void Configure(EntityTypeBuilder<AdTime> builder)
        {
            builder.HasKey(e => e.Id);
        }
    }
}
