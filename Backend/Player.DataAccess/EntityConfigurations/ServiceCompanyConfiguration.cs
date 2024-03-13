using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Player.Domain;

namespace Player.DataAccess.EntityConfigurations
{
    public class ServiceCompanyConfiguration : IEntityTypeConfiguration<ServiceCompany>
    {
        public void Configure(EntityTypeBuilder<ServiceCompany> builder)
        {
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Name).IsRequired();
        }
    }
}