using System;
using System.Linq;
using Innofactor.EfCoreJsonValueConverter;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage;
using Player.Domain;

namespace Player.DataAccess.EntityConfigurations
{
    public class ObjectInfoConfiguration : IEntityTypeConfiguration<ObjectInfo>
    {
        private readonly DbContext _context;

        public ObjectInfoConfiguration(DbContext context)
        {
            _context = context;
        }
        
        public void Configure(EntityTypeBuilder<ObjectInfo> builder)
        {
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Bin).IsRequired();
            builder.Property(e => e.Name).IsRequired();

            if (_context.Database.IsNpgsql())
            {
                builder.Property(oi => oi.FreeDays)
                    .IsRequired()
                    .HasDefaultValueSql("array[]::varchar[]")
                    .HasPostgresArrayConversion(week => week.ToString(), s => Enum.Parse<DayOfWeek>(s));
            }
            else
            {
                builder.Property(oi => oi.FreeDays)
                    .HasConversion(
                        v => string.Join(",", v.ToString()),
                        v => v.Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(d => Enum.Parse<DayOfWeek>(d)).ToArray()
                    );
            }
            builder.Property(oi => oi.ClientSettings).HasColumnType("jsonb").HasJsonValueConversion();
            builder.OwnsOne(oi => oi.ResponsiblePersonOne);
            builder.OwnsOne(oi => oi.ResponsiblePersonTwo);
        }
    }
}
