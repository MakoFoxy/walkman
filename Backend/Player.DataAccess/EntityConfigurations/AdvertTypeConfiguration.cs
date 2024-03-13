using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Player.Domain;

namespace Player.DataAccess.EntityConfigurations
{
    public class AdvertTypeConfiguration : IEntityTypeConfiguration<AdvertType>
    {
        public void Configure(EntityTypeBuilder<AdvertType> builder)
        {
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Code).IsRequired();
            builder.Property(e => e.Name).IsRequired();
        }
    }
}