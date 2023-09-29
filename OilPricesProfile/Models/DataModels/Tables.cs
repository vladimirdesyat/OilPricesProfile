using System.ComponentModel.DataAnnotations.Schema;

public class OilDepot
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public string? Name { get; set; }
}


public class PetroleumProduct
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public string? Name { get; set; }
}

public class Price
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public int OilDepotId { get; set; } // Foreign key to OilDepot
    public int PetroleumProductId { get; set; } // Foreign key to PetroleumProduct
    public DateTime Date { get; set; }
    public double? MinPricePerLiterInclVat { get; set; }
    public double? MaxPricePerLiterInclVat { get; set; }
    public double? WeightedAveragePricePerLiterInclVat { get; set; }
    public double? WeightedAverageIndexPerLiterInclVat { get; set; }
    
    public PetroleumProduct PetroleumProduct { get; set; } // Parent reference
    
    public OilDepot OilDepot { get; set; } // Parent reference

}