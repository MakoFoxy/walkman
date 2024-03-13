using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Player.Domain;

namespace Player.DataAccess.EntityConfigurations
{
    public class ImageConfiguration : IEntityTypeConfiguration<Image>
    {
        public void Configure(EntityTypeBuilder<Image> builder)
        {
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Path).IsRequired();
            builder.Property(i => i.Size)
                .HasConversion(d => d.ToString(),
                    d => (Size)Enum.Parse(typeof(Size), d));
        }
    }
}