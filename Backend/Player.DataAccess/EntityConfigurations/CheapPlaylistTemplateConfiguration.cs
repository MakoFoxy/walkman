using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Player.Domain;

namespace Player.DataAccess.EntityConfigurations
{
    public class CheapPlaylistTemplateConfiguration : IEntityTypeConfiguration<CheapPlaylistTemplate>
    {
        public void Configure(EntityTypeBuilder<CheapPlaylistTemplate> builder)
        {
            builder.HasKey(e => e.Id);
        }
    }
}