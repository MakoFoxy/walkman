using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Player.Domain;

namespace Player.DataAccess.EntityConfigurations
{
    public class TrackTypeConfiguration : IEntityTypeConfiguration<TrackType>
    {
        public void Configure(EntityTypeBuilder<TrackType> builder)
        {
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Code).IsRequired();
            builder.Property(e => e.Name).IsRequired();
            builder.HasData(new List<TrackType>
            {
                new TrackType
                {
                    Id = Guid.Parse("225EA1D7-EDE3-4733-BA4A-8ED9459F94C6"),
                    Code = TrackType.Advert,
                    Name = "Реклама"
                },
                new TrackType
                {
                    Id = Guid.Parse("B8B2D657-394E-45A3-9A4D-25FFBA47DCAF"),
                    Code = TrackType.Music,
                    Name = "Музыка"
                },
                new TrackType
                {
                    Id = Guid.Parse("CDD0F07D-E4AF-43D3-9A28-3B8674649FE4"),
                    Code = TrackType.Silent,
                    Name = "Тишина"
                }
            });
        }
    }
}