using System.Data;
using System.Threading.Tasks.Dataflow;
using HtmlAgilityPack;
using OilPricesProfile.Data.Context;

namespace OilPricesProfile.Data
{
    public class DataParser
    {
        private readonly AppDbContext _dbContext;
        private readonly HttpClient _httpClient;
        private string oilDepotMatch;
        private string petroleumMatch;
        public DataParser(AppDbContext dbContext)
        {
            _dbContext = dbContext;
            _httpClient = new HttpClient();
        }

        public async Task ParseAndStoreWebPageDataAsync(string url)
        {
            var depotsAndPetroleum = new List<string>()
            {
                "конденсат газовый стабильный (КС Портовая)",
                "пропан-бутан автомобильный (Астраханская ГНС)",
                "конденсат газовый стабильный (1 гр.) КС \"Казачья\"",
                "бензин неэтилированный АИ-92-К5 (ХАБ «ХМАО»)",
                "топливо дизельное ДТ-А-К5 (ХАБ «ХМАО»)",
                "бензин неэтилированный АИ-95-К5 (ХАБ «ХМАО»)",
                "пропан-бутан автомобильный (Установка налива СУГ с.Черноречье)",
                "конденсат газовый (ХАБ \"ЮФО-СКФО\")",
                "пропан-бутан автомобильный (ГНС Александров, Тверь)",
                "пропан-бутан автомобильный (ГНС Хабль, Темрюк)",
            };

            try
            {
                var html = await _httpClient.GetStringAsync(url);
                var web = new HtmlDocument();
                web.LoadHtml(html);

                // Specify the class name of the div you want to parse
                var targetDiv = web.DocumentNode.SelectSingleNode("//div[@class='table-responsive-xl']");

                if (targetDiv != null)
                {
                    var table = targetDiv.SelectSingleNode("//table");

                    if (table != null)
                    {
                        var dataTable = new DataTable();

                        var rows = table.Descendants("tr")
                                .Skip(1) // Skip the header row
                                .Select(tr => tr.Elements("td")
                                .Where((num, index) => index != 1 && index != 2)
                                .Select(td => td.InnerText.Trim())
                                .ToArray())
                                .ToList();

                        foreach(var c in rows)
                        {
                            Console.WriteLine(string.Join("", c));
                        }


                        // Extract the h1 element with class "less"
                        var h1Element = web.DocumentNode.SelectSingleNode("//h1[@class='less']");
                        if (h1Element != null)
                        {
                            // Check if the h1 element's inner text is in the dictionary
                            var h1InnerText = h1Element.InnerText.Trim();

                            foreach (var dictValue in depotsAndPetroleum)
                            {
                                if (h1InnerText.Contains(dictValue))
                                {
                                    // 'dictValue' is present in 'longerText'
                                    // Console.WriteLine($"Found: {dictValue}");
                                    dataTable.Columns.Add(dictValue);
                                }
                            }
                        }

                        Console.WriteLine($"DataTable column count1: {dataTable.Columns.Count}");

                        SortAndMatch(dataTable);

                        Console.WriteLine($"DataTable column count2: {dataTable.Columns.Count}");

                        dataTable.Columns.Add("Date", typeof(DateTime));
                        dataTable.Columns.Add("MinPricePerTonInclVat", typeof(double));
                        dataTable.Columns.Add("MaxPricePerTonInclVat", typeof(double));
                        dataTable.Columns.Add("WeightedAveragePricePerTonInclVat", typeof(double));
                        dataTable.Columns.Add("WeightedAverageIndexPerTonInclVat", typeof(double));

                        Console.WriteLine(dataTable.Columns.Count);

                        foreach (var row in rows)
                        {
                            var newRow = dataTable.NewRow();

                            for (int i = 0; i < row.Length; i++)
                            {
                                if (string.IsNullOrWhiteSpace(row[i]))
                                {
                                    // If the column is of a reference type (e.g., string), set it to null
                                    if (dataTable.Columns[i].DataType == typeof(string))
                                    {
                                        newRow[i] = null;
                                    }
                                    else
                                    {
                                        // If the column is of a value type (e.g., int, double), set it to the default value
                                        newRow[i] = Activator.CreateInstance(dataTable.Columns[i].DataType);
                                    }
                                }
                                else
                                {
                                    newRow[i] = row[i];
                                }
                            }                           

                            dataTable.Rows.Add(newRow);
                        }

                        var OilDepotId = new DataColumn("OilDepotId", typeof(int));

                        var PetroleumProductId = new DataColumn("PetroleumProductId", typeof(int));

                        dataTable.Columns.Add("OilDepotId", typeof(int));
                        dataTable.Columns.Add("PetroleumProductId", typeof(int));

                        dataTable.Columns["OilDepotId"].SetOrdinal(0);
                        dataTable.Columns["PetroleumProductId"].SetOrdinal(1);

                        for (int i = 0; i < dataTable.Rows.Count; i++)
                        {
                            dataTable.Rows[i]["OilDepotId"] = CreateOrUpdateOilDepot(oilDepotMatch);
                            dataTable.Rows[i]["PetroleumProductId"] = CreateOrUpdatePetroleumProduct(petroleumMatch);
                        }

                        var priceList = new List<Price>();

                        foreach (DataRow row in dataTable.Rows)
                        {
                            Price price = new Price
                            {
                                OilDepotId = Convert.ToInt32(row["OilDepotId"]),
                                PetroleumProductId = Convert.ToInt32(row["PetroleumProductId"]),
                                Date = Convert.ToDateTime(row["Date"]),
                                MinPricePerTonInclVat = Convert.ToDouble(row["MinPricePerTonInclVat"]),
                                MaxPricePerTonInclVat = Convert.ToDouble(row["MaxPricePerTonInclVat"]),
                                WeightedAveragePricePerTonInclVat = Convert.ToDouble(row["WeightedAveragePricePerTonInclVat"]),
                                WeightedAverageIndexPerTonInclVat = Convert.ToDouble(row["WeightedAverageIndexPerTonInclVat"]),                                
                            };

                            priceList.Add(price);
                        }

                        dataTable.AsEnumerable().ToList().ForEach(m => m.Delete());

                        foreach (var price in priceList)
                        {
                            _dbContext.Prices.Add(price);
                        }

                        _dbContext.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., log the error)
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        public void SortAndMatch(DataTable dataTable)
        {
            var OilandPetroleum = dataTable.Columns[0].ColumnName;

            Console.WriteLine(OilandPetroleum);

            var oilDepot = new List<string>()
            {
                "КС Портовая",
                "Астраханская ГНС",
                "КС \"Казачья\"",
                "ХАБ «ХМАО»",
                "Установка налива СУГ с.Черноречье",
                "ХАБ \"ЮФО-СКФО\"",
                "ГНС Александров, Тверь",
                "ГНС Хабль, Темрюк"
            };

            var petroleumProduct = new List<string>()
            {                
                "пропан-бутан автомобильный",
                "конденсат газовый стабильный (1 гр.)",
                "конденсат газовый стабильный",
                "бензин неэтилированный АИ-92-К5",
                "топливо дизельное ДТ-А-К5",
                "бензин неэтилированный АИ-95-К5",
                "конденсат газовый",
            };

            var depotMatch = oilDepot.FirstOrDefault(s => OilandPetroleum.Contains(s));
            if (!string.IsNullOrEmpty(depotMatch))
            {
                oilDepotMatch = depotMatch;
            }

            var productMatch = petroleumProduct.FirstOrDefault(s => OilandPetroleum.Contains(s));
            if (!string.IsNullOrEmpty(productMatch))
            {
                petroleumMatch = productMatch;
            }

            dataTable.Columns.RemoveAt(0);
        }

        private int CreateOrUpdateOilDepot(string oilDepotName)
        {
            var oilDepot = _dbContext.OilDepots.SingleOrDefault(o => o.Name == oilDepotName);

            if (oilDepot == null)
            {
                // Create a new OilDepot if it doesn't exist
                oilDepot = new OilDepot
                {
                    Name = oilDepotName
                };

                _dbContext.OilDepots.Add(oilDepot);
                _dbContext.SaveChanges();
            }

            return oilDepot.Id;
        }

        private int CreateOrUpdatePetroleumProduct(string petroleumProductName)
        {
            var petroleumProduct = _dbContext.PetroleumProducts.SingleOrDefault(p => p.Name == petroleumProductName);

            if (petroleumProduct == null)
            {
                // Create a new PetroleumProduct if it doesn't exist
                petroleumProduct = new PetroleumProduct
                {
                    Name = petroleumProductName
                };

                _dbContext.PetroleumProducts.Add(petroleumProduct);
                _dbContext.SaveChanges();
            }

            return petroleumProduct.Id;
        }
    }
}