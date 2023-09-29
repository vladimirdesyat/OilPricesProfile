using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OilPricesProfile.Models.DataModels.Config
{
    public class PriceConfig : IEntityTypeConfiguration<Price>
    {
        public void Configure(EntityTypeBuilder<Price> builder)
        {
            builder.Property(m => m.Date)
               .HasColumnName("Дата")
               .HasMaxLength(255); // Optional: You can specify the maximum length for the column

            builder.Property(m => m.MinPricePerLiterInclVat)
                .HasColumnName("Мин. цена,\r\nруб./л вкл. НДС");

            builder.Property(m => m.MaxPricePerLiterInclVat)
                .HasColumnName("Макс. цена,\r\nруб./л вкл. НДС");

            builder.Property(m => m.WeightedAveragePricePerLiterInclVat)
                .HasColumnName("Средневзвешенная цена,\r\nруб./л вкл. НДС");

            builder.Property(m => m.WeightedAverageIndexPerLiterInclVat)
                .HasColumnName("Средневзвешенный индекс,\r\nруб./л вкл. НДС");

            // Other property configurations can be added here...
        }
    }
}
