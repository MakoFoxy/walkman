using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Player.Domain;

namespace Player.DataAccess.EntityConfigurations
{
    public class ObjectSelectionConfiguration : IEntityTypeConfiguration<ObjectSelection>
    {
        public void Configure(EntityTypeBuilder<ObjectSelection> builder)
        {
            builder.HasKey(e => e.Id);
        }
    }
}
