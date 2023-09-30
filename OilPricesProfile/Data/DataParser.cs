using System.Data;
using HtmlAgilityPack;
using OilPricesProfile.Data.Context;

namespace OilPricesProfile.Data
{
    public class DataParser
    {
        private readonly AppDbContext _dbContext;
        private readonly HttpClient _httpClient;
        private readonly ILogger<DataParser> _logger;
        private string oilDepotMatch;
        private string petroleumMatch;
        public DataParser(AppDbContext dbContext, ILogger<DataParser> logger)
        {
            _dbContext = dbContext;
            _httpClient = new HttpClient();
            _logger = logger;
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

                        var rows = ParseTableRows(table);

                        AddColumnsFromH1(web, dataTable, depotsAndPetroleum);

                        SortAndMatch(dataTable);

                        dataTable = CreateDataTable();

                        PopulateDataTable(dataTable, rows);

                        AddAndSetColumnOrdinals(dataTable);

                        var priceList = ConvertDataTableToPriceList(dataTable);

                        ClearDataTable(dataTable);

                        InsertPricesIntoDatabase(priceList);
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., log the error)
                _logger.LogError(ex, "An error occurred while parsing and storing web page data.");
            }
        }

        private void AddColumnsFromH1(HtmlDocument web, DataTable dataTable, List<string> depotsAndPetroleum)
        {
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
                        dataTable.Columns.Add(dictValue);
                    }
                }
            }
        }

        private DataTable CreateDataTable()
        {
            var dataTable = new DataTable();
            dataTable.Columns.Add("Date", typeof(DateTime));
            dataTable.Columns.Add("MinPricePerTonInclVat", typeof(double));
            dataTable.Columns.Add("MaxPricePerTonInclVat", typeof(double));
            dataTable.Columns.Add("WeightedAveragePricePerTonInclVat", typeof(double));
            dataTable.Columns.Add("WeightedAverageIndexPerTonInclVat", typeof(double));
            return dataTable;
        }

        private List<string[]> ParseTableRows(HtmlNode table)
        {
            return table.Descendants("tr")
                .Skip(1) // Skip the header row
                .Select(tr => tr.Elements("td")
                    .Where((num, index) => index != 1 && index != 2)
                    .Select(td => td.InnerText.Trim())
                    .ToArray())
                    .ToList();
        }

        private void PopulateDataTable(DataTable dataTable, List<string[]> rows)
        {
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
        }

        private void AddAndSetColumnOrdinals(DataTable dataTable)
        {
            // Create the DataColumn for OilDepotId
            var oilDepotIdColumn = new DataColumn("OilDepotId", typeof(int));

            // Create the DataColumn for PetroleumProductId
            var petroleumProductIdColumn = new DataColumn("PetroleumProductId", typeof(int));

            // Add the OilDepotId and PetroleumProductId columns to the DataTable
            dataTable.Columns.Add(oilDepotIdColumn);
            dataTable.Columns.Add(petroleumProductIdColumn);

            // Set the ordinal positions of OilDepotId and PetroleumProductId columns
            dataTable.Columns["OilDepotId"].SetOrdinal(0);
            dataTable.Columns["PetroleumProductId"].SetOrdinal(1);

            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                dataTable.Rows[i]["OilDepotId"] = CreateOrUpdateOilDepot(oilDepotMatch);
                dataTable.Rows[i]["PetroleumProductId"] = CreateOrUpdatePetroleumProduct(petroleumMatch);
            }
        }

        private List<Price> ConvertDataTableToPriceList(DataTable dataTable)
        {
            var priceList = new List<Price>();
            foreach (DataRow row in dataTable.Rows)
            {
                Price price = new Price
                {
                    OilDepotId = Convert.ToInt32(row["OilDepotId"]),
                    PetroleumProductId = Convert.ToInt32(row["PetroleumProductId"]),
                    Date = Convert.ToDateTime(row["Date"]),
                    MinPricePerLiterInclVat = row["MinPricePerTonInclVat"] == DBNull.Value ? 0.0 : Convert.ToDouble(row["MinPricePerTonInclVat"]) / 1000.0,
                    MaxPricePerLiterInclVat = row["MaxPricePerTonInclVat"] == DBNull.Value ? 0.0 : Convert.ToDouble(row["MaxPricePerTonInclVat"]) / 1000.0,
                    WeightedAveragePricePerLiterInclVat = row["WeightedAveragePricePerTonInclVat"] == DBNull.Value ? 0.0 : Convert.ToDouble(row["WeightedAveragePricePerTonInclVat"]) / 1000.0,
                    WeightedAverageIndexPerLiterInclVat = row["WeightedAverageIndexPerTonInclVat"] == DBNull.Value ? 0.0 : Convert.ToDouble(row["WeightedAverageIndexPerTonInclVat"]) / 1000.0,

                };
                priceList.Add(price);
            }
            return priceList;
        }

        private void ClearDataTable(DataTable dataTable)
        {
            dataTable.AsEnumerable().ToList().ForEach(m => m.Delete());
        }

        private void InsertPricesIntoDatabase(List<Price> priceList)
        {
            foreach (var price in priceList)
            {
                _dbContext.Prices.Add(price);
            }
            _dbContext.SaveChanges();
        }

        private void SortAndMatch(DataTable dataTable)
        {
            var OilandPetroleum = dataTable.Columns[0].ColumnName;

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