using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;

// Class to hold the stock data for each trading day.
class StockData
{
    public string Date { get; set; }
    public decimal Open { get; set; }
    public decimal High { get; set; }
    public decimal Low { get; set; }
    public decimal Close { get; set; }
    public long Volume { get; set; }

    // Constructor to initialize the StockData object.
    public StockData(string date, decimal open, decimal high, decimal low, decimal close, long volume)
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
        return $"{Date}: Open={Open}, High={High}, Low={Low}, Close={Close}, Volume={Volume}";
    }
}

class Program
{
    static async Task Main(string[] args)
    {
        // Prompt the user for the stock symbol.
        Console.Write("Enter stock symbol: ");
        string symbol = Console.ReadLine()?.Trim().ToUpper();
        string apiKey = "IL34F8GFHFQ1ZK8G";

        // Construct the API URL to fetch daily stock data in CSV format.
        string url = $"https://www.alphavantage.co/query?function=TIME_SERIES_DAILY&symbol={symbol}&apikey={apiKey}&outputsize=full&datatype=csv";

        using HttpClient client = new HttpClient();
        try
        {
            // Fetch the CSV response from the API.
            string response = await client.GetStringAsync(url);

            // List to store the stock data objects.
            List<StockData> stockHistory = new List<StockData>();

            // Use StringReader to read the CSV data line by line.
            using StringReader reader = new StringReader(response);
            string? line = reader.ReadLine(); // Skip header line.

            while ((line = reader.ReadLine()) != null)
            {
                string[] columns = line.Split(',');
                if (columns.Length < 6)
                    continue; // Skip if row doesn't have all fields.

                // Parse each field.
                string date = columns[0];
                decimal open = decimal.Parse(columns[1]);
                decimal high = decimal.Parse(columns[2]);
                decimal low = decimal.Parse(columns[3]);
                decimal close = decimal.Parse(columns[4]);
                long volume = long.Parse(columns[5]);

                // Create a StockData object and add to the list.
                stockHistory.Add(new StockData(date, open, high, low, close, volume));
            }

            // Reverse the list so that the earliest date is first.
            stockHistory.Reverse();

            // --- FILTER CALCULATION ---
            Console.WriteLine("\nFilter Results (Only showing days with enough historical data):");
            // We'll start at index 49 since we need 50 days for volume filter.
            // Note: ATR calculation needs at least 10 days (which is satisfied for i>=49).
            for (int i = 49; i < stockHistory.Count; i++)
            {
                // 1. Minimum Price Filter: Close price >= $1.00.
                bool passesMinPrice = stockHistory[i].Close >= 1.00m;

                // 2. 50-day Average Volume Filter.
                long volumeSum = 0;
                for (int j = i - 49; j <= i; j++)
                {
                    volumeSum += stockHistory[j].Volume;
                }
                decimal avgVolume = volumeSum / 50.0m;
                bool passesVolumeFilter = avgVolume >= 1000000m;

                // 3. 10-day Average True Range (ATR) Filter.
                // ATR requires the true range for each day:
                // TrueRange = max( (High - Low), |High - Previous Close|, |Low - Previous Close| )
                decimal atrSum = 0;
                int count = 0;
                // Loop over the last 10 days.
                for (int k = i - 9; k <= i; k++)
                {
                    // Skip the first day in the series (can't compute TR without previous close)
                    if (k == 0) continue;
                    decimal highLow = stockHistory[k].High - stockHistory[k].Low;
                    decimal highPrevClose = Math.Abs(stockHistory[k].High - stockHistory[k - 1].Close);
                    decimal lowPrevClose = Math.Abs(stockHistory[k].Low - stockHistory[k - 1].Close);
                    decimal trueRange = Math.Max(highLow, Math.Max(highPrevClose, lowPrevClose));
                    atrSum += trueRange;
                    count++;
                }
                decimal atr = atrSum / count;
                // ATR percentage = (ATR / current day's close) * 100.
                decimal atrPercentage = (atr / stockHistory[i].Close) * 100;
                bool passesAtrFilter = atrPercentage >= 5.0m;

                // Combine all filters.
                bool passesFilter = passesMinPrice && passesVolumeFilter && passesAtrFilter;

                // Print out the results: Date, Close Price, 50-day Avg Volume, ATR % and overall filter pass.
                Console.WriteLine($"{stockHistory[i].Date} - Close: {stockHistory[i].Close:F2} - 50-day Avg Vol: {avgVolume:F0} - ATR%: {atrPercentage:F2}% - Filter Pass: {passesFilter}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}
