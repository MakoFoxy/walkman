using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Player.Domain;

namespace Player.DataAccess.EntityConfigurations
{
    public class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
    {
        public void Configure(EntityTypeBuilder<Organization> builder)
        {
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Address).IsRequired();
            builder.Property(e => e.Bank).IsRequired();
            builder.Property(e => e.Bin).IsRequired();
            builder.Property(e => e.Iik).IsRequired();
            builder.Property(e => e.Phone).IsRequired();
            builder.Property(e => e.Name).IsRequired();
        }
    }
}