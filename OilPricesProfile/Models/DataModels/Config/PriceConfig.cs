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

            builder.Property(m => m.MinPricePerTonInclVat)
                .HasColumnName("Мин. цена,\r\nруб./т вкл. НДС");

            builder.Property(m => m.MaxPricePerTonInclVat)
                .HasColumnName("Макс. цена,\r\nруб./т вкл. НДС");

            builder.Property(m => m.WeightedAveragePricePerTonInclVat)
                .HasColumnName("Средневзвешенная цена,\r\nруб./т вкл. НДС");

            builder.Property(m => m.WeightedAverageIndexPerTonInclVat)
                .HasColumnName("Средневзвешенный индекс,\r\nруб./т вкл. НДС");

            // Other property configurations can be added here...
        }
    }
}
