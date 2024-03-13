using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Player.Domain;

namespace Player.DataAccess.EntityConfigurations
{
    public class AdLifetimeConfiguration : IEntityTypeConfiguration<AdLifetime>
    {
        public void Configure(EntityTypeBuilder<AdLifetime> builder)
        {
            builder.HasKey(e => e.Id);
        }
    }
}