using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        // Load Config from JSON
        var configJson = File.ReadAllText("LongMeanReversionSelloff/Config/MRSLongConfig.json");
        var config = JsonSerializer.Deserialize<MRSLongConfig>(configJson);

        // Fetch Stock Data
        Console.Write("Enter stock symbol: ");
        string symbol = Console.ReadLine()?.Trim().ToUpper();
        string apiKey = "RK2UVVTZ2625NQY4"; 
        var stockHistory = await StockDataFetcher.FetchAndParseStockDataAsync(symbol, apiKey);

        if (stockHistory.Count == 0)
        {
            Console.WriteLine("No data retrieved. Exiting...");
            return;
        }

        // Build indicator list using factory
        var indicators = IndicatorBuilder.Build(config);

        // Precalculate indicator values
        var precalculator = new IndicatorPrecalculator(indicators);
        var indicatorMap = precalculator.Precalculate(stockHistory);

        // Run simulation
        var simulator = new MRSLongSimulator(config);
        var trades = simulator.Run(stockHistory, indicatorMap);

        // Display performance
        var metrics = new PerformanceMetrics(trades);
        metrics.Print();

    }
}


