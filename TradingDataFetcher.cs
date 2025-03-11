using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;


public class StockData
{
    public DateTime Date { get; set; }
    public decimal Open { get; set; }
    public decimal High { get; set; }
    public decimal Low { get; set; }
    public decimal Close { get; set; }
    public long Volume { get; set; }

    // Constructor to initialize the StockData object.
    public StockData(DateTime date, decimal open, decimal high, decimal low, decimal close, long volume)
    {
        Date = date;
        Open = open;
        High = high;
        Low = low;
        Close = close;
        Volume = volume;
    }

    public override string ToString()
    {
        return $"{Date:yyyy-MM-dd}: Open={Open}, High={High}, Low={Low}, Close={Close}, Volume={Volume}";
    }
}


public static class StockDataFetcher
{
    public static async Task<List<StockData>> FetchAndParseStockDataAsync(string symbol, string apiKey)
    {
        string url = $"https://www.alphavantage.co/query?function=TIME_SERIES_DAILY&symbol={symbol}&apikey={apiKey}&outputsize=full&datatype=csv";

        using HttpClient client = new HttpClient();
        try
        {
            string response = await client.GetStringAsync(url);
            return ParseStockData(response);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching data: {ex.Message}");
            return new List<StockData>(); // Return empty list if API fails
        }
    }

    private static List<StockData> ParseStockData(string csvData)
    {
        List<StockData> stockHistory = new List<StockData>();
        using StringReader reader = new StringReader(csvData);
        string? line = reader.ReadLine(); // Skip header

        while ((line = reader.ReadLine()) != null)
        {
            string[] columns = line.Split(',');
            if (columns.Length < 6) continue;

            stockHistory.Add(new StockData(
                DateTime.Parse(columns[0]),
                decimal.Parse(columns[1]),
                decimal.Parse(columns[2]),
                decimal.Parse(columns[3]),
                decimal.Parse(columns[4]),
                long.Parse(columns[5])
            ));
        }

        stockHistory.Reverse(); // Earliest dates first
        return stockHistory;
    }
}
