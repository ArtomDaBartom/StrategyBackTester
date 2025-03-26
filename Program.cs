using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        // Load config
        var configJson = File.ReadAllText("Strategies/LongMeanReversionSelloff/MRSLongConfig.json");
        var config = JsonSerializer.Deserialize<MRSLongConfig>(configJson);
        // Choose strategy
        var strategy = new MRSLongStrategy();

        // Fetch stock data
        Console.Write("Enter stock symbol: ");
        string symbol = Console.ReadLine()?.Trim().ToUpper();
        string apiKey = "RK2UVVTZ2625NQY4";
        var stockHistory = await StockDataFetcher.FetchAndParseStockDataAsync(symbol, apiKey);

        if (stockHistory.Count == 0)
        {
            Console.WriteLine("No data retrieved. Exiting...");
            return;
        }

        // Build only required indicators
        var indicators = IndicatorBuilder.Build(config, strategy.RequiredIndicators);

        // Precompute indicators
        var precalculator = new IndicatorPrecalculator(indicators);
        var indicatorMap = precalculator.Precalculate(stockHistory);

        // Run simulation
        var simulator = new Simulator<MRSLongConfig>(strategy, config);
        var trades = simulator.Run(stockHistory, indicatorMap);

        // Display results
        var metrics = new PerformanceMetrics(trades);
        metrics.Print();
    }
}


