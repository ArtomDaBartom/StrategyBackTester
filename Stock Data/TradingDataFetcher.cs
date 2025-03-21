using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;

public static class StockDataFetcher
{
    private static readonly HttpClient _httpClient = new HttpClient();

    public static async Task<List<TradingDay>> FetchAndParseStockDataAsync(string symbol, string apiKey)
    {
        string url = $"https://www.alphavantage.co/query?function=TIME_SERIES_DAILY&symbol={symbol}&apikey={apiKey}&outputsize=full&datatype=csv";

        try
        {
            string response = await _httpClient.GetStringAsync(url);
            return ParseStockData(response);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching data: {ex.Message}");
            return new List<TradingDay>(); // fallback: empty list
        }
    }

    private static List<TradingDay> ParseStockData(string csvData)
    {
        var stockHistory = new List<TradingDay>();
        using StringReader reader = new StringReader(csvData);
        string? line = reader.ReadLine(); // Skip header

        while ((line = reader.ReadLine()) != null)
        {
            string[] columns = line.Split(',');
            if (columns.Length < 6) continue;

            stockHistory.Add(new TradingDay(
                DateTime.Parse(columns[0]),
                decimal.Parse(columns[1]),
                decimal.Parse(columns[2]),
                decimal.Parse(columns[3]),
                decimal.Parse(columns[4]),
                long.Parse(columns[5])
            ));
        }

        stockHistory.Reverse(); // earliest to latest
        return stockHistory;
    }
}

