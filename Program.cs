using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;
using System.Linq;

class Program
{
    static async Task Main(string[] args)
    {
        // Get user input
        Console.Write("Enter stock symbol: ");
        string symbol = Console.ReadLine()?.Trim().ToUpper();
        string apiKey = "IL34F8GFHFQ1ZK8G";

        // Fetch stock data
        List<StockData> stockHistory = await StockDataFetcher.FetchAndParseStockDataAsync(symbol, apiKey);

        // If no data was fetched, exit early
        if (stockHistory.Count == 0)
        {
            Console.WriteLine("No data retrieved. Exiting...");
            return;
        }

        // Run trade simulation
        List<Trade> trades = MRSelloffLongSimulator.RunMRSelloffLong(stockHistory);

        // Print performance metrics
        PerformanceMetrics.CalculateAndPrint(trades);


    }
}

