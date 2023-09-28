using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OilPricesProfile.Models.DataModels.Config
{
    public class OilDepotConfig : IEntityTypeConfiguration<OilDepot>
    {
        public void Configure(EntityTypeBuilder<OilDepot> builder)
        {
            builder.Property(m => m.Name)
                .HasColumnName("Наименование нефтебазы");
        }
    }
}
