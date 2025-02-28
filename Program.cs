using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;

class Program
{
    static async Task Main(string[] args)
    {
        // Prompt the user to enter a stock symbol
        Console.Write("Enter stock symbol: ");
        string symbol = Console.ReadLine()?.Trim().ToUpper(); // Read input, remove spaces, convert to uppercase

        // Define API Key
        string apiKey = "IL34F8GFHFQ1ZK8G";

        // Construct API request URL (requests historical daily stock prices in CSV format)
        string url = $"https://www.alphavantage.co/query?function=TIME_SERIES_DAILY&symbol={symbol}&apikey={apiKey}&outputsize=full&datatype=csv";

        using HttpClient client = new HttpClient(); // Create an HTTP client
        try
        {
            // Send the request and get the response as a string
            string response = await client.GetStringAsync(url);

            List<string> prices = new List<string>(); // Store the extracted prices

            using StringReader reader = new StringReader(response); // Read response line-by-line
            string? line = reader.ReadLine(); // Read and ignore the header line

            while ((line = reader.ReadLine()) != null) // Process each line
            {
                string[] columns = line.Split(','); // Split the CSV row into columns
                if (columns.Length < 5) continue;  // Skip invalid rows

                string date = columns[0]; // First column = date
                decimal closePrice = decimal.Parse(columns[4]); // Fifth column = closing price (convert to decimal)

                prices.Add($"{date} - {closePrice:F2}"); // Store the formatted result
            }

            prices.Reverse(); // Reverse the list to show earliest dates first

            foreach (var price in prices) // Print the final output
            {
                Console.WriteLine(price);
            }
        }
        catch (Exception ex) // Handle errors
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}
