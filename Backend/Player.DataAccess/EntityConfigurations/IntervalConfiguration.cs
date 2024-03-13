using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Player.Domain;

namespace Player.DataAccess.EntityConfigurations
{
    public class IntervalConfiguration : IEntityTypeConfiguration<Interval>
    {
        public void Configure(EntityTypeBuilder<Interval> builder)
        {
            builder.HasKey(e => e.Id);
        }
    }
}
