using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OilPricesProfile.Models.DataModels.Config
{
    public class PetroleumProductConfig : IEntityTypeConfiguration<PetroleumProduct>
    {
        public void Configure(EntityTypeBuilder<PetroleumProduct> builder)
        {
            builder.Property(m => m.Name)
                .HasColumnName("Наименование нефтепродукта");
        }
    }
}
