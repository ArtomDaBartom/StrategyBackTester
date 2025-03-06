using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;
using System.Linq;

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


//Class for trades
class Trade
{
    // The date when the trade was entered.
    public string EntryDate { get; set; }

    // The price at which the trade was entered.
    public decimal EntryPrice { get; set; }

    // The date when the trade was closed.
    public string ExitDate { get; set; }

    // The price at which the trade was closed.
    public decimal ExitPrice { get; set; }

    // The trade's return expressed as a percentage, calculated upon trade close.
    public decimal TradeReturnPercentage { get; private set; }

    // A convenience property that tells us if this trade was profitable.
    public bool IsWinningTrade => TradeReturnPercentage > 0;

    // The profit or loss of the trade assuming only one trade is bought.
    public decimal ProfitLoss { get; private set; }

    // Constructor: Initializes a new trade with entry date and entry price.
    public Trade(string entryDate, decimal entryPrice)
    {
        EntryDate = entryDate;
        EntryPrice = entryPrice;
    }

    // Close the trade by providing the exit date and exit price.
    public void CloseTrade(string exitDate, decimal exitPrice)
    {
        ExitDate = exitDate;
        ExitPrice = exitPrice;
        // Calculate the return percentage as: ((ExitPrice - EntryPrice) / EntryPrice) * 100.
        TradeReturnPercentage = ((exitPrice - EntryPrice) / EntryPrice) * 100;

        //Calculate the proft or loss.
        ProfitLoss = ExitPrice - EntryPrice;
    }
}


class Program
{
    static async Task Main(string[] args)
    {
        // Prompt the user for the stock symbol.
        Console.Write("Enter stock symbol: ");
        List<Trade> trades = null;
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

            // List to store trades.
            trades = new List<Trade>();

            // --- FILTER CALCULATION ---
            Console.WriteLine("\nFilter Results (Only showing days with enough historical data):");
            // We'll start at index 150 since we need 151 days for Simple Moving Average.
            // Note: ATR calculation needs at least 10 days, Volume needs at least 50 days (which is satisfied for i>= 150).
            for (int i = 150; i < stockHistory.Count; i++){

                // --- FILTER CALCULATION ---

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
                int countATR = 0;
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
                    countATR++;
                }
                decimal atr = atrSum / countATR;
                // ATR percentage = (ATR / current day's close) * 100.
                decimal atrPercentage = (atr / stockHistory[i].Close) * 100;
                bool passesAtrFilter = atrPercentage >= 5.0m;

                // Combine all filters.
                bool passesFilter = passesMinPrice && passesVolumeFilter && passesAtrFilter;

                // --- ENTRY CONDITION CALCULATIONS ---

                // Entry Condition 1: Closed above the 150 day moving average.
                decimal sum150 = 0;

                // Sum up the closing prices for the last 150 days (today included)
                for(int l =  i - 149; l < i; l++)
                {
                    sum150 += stockHistory[l].Close;
                }

                //Calculate the 150-day simple moving average.
                decimal sma150 = sum150 / 150.0m;

                //Check if the current days is above the 150 day-day moving average.
                bool SmaEntryCondition = stockHistory[i].Close > sma150;

                //Entry Condition 2: Dropped at least 12.5% in the last 3 days.
                if (i < 3) continue;

                decimal close3DaysAgo = stockHistory[i - 3].Close;

                //Calculate the percentage drop over the last 3 days.
                decimal dropPercentage = ((close3DaysAgo - stockHistory[i].Close) / close3DaysAgo) * 100;

                //Calculate if the drop is at least 12.5%
                bool dropEntryCondition = dropPercentage >= 12.5m;

                bool validEntry = passesFilter && SmaEntryCondition && dropEntryCondition;



                // Print out the results: Date, Close Price, 50-day Avg Volume, ATR % and overall filter pass.
                Console.WriteLine($"{stockHistory[i].Date} - Close: {stockHistory[i].Close:F2} - Valid Trade Entry? {validEntry}");

                // If there is a valid entry point, attempt initiate a trade. 
                if (validEntry)
                {
                    //Calculate the limit order price and determine whether we hit it or not.
                    decimal limitOrderPrice = stockHistory[i - 1].Close * 0.93m;
                    bool limitOrderHit = stockHistory[i].Low <= limitOrderPrice;

                    //If we hit the limit order, vamos.
                    if (limitOrderHit)
                    {
                        decimal executionPrice = limitOrderPrice;

                        //Open a new trade.
                        Trade trade = new Trade(stockHistory[i].Date, executionPrice);
                        

                        int dayOfExit = -1;

                        //Loop through subsequent days to determine when the trade will be closed.
                        for(int x = i + 1; x < stockHistory.Count; x++)
                        {
                            //Check for exit conditions on each day.

                            //Exit Condition 1 -- Stop Loss: 2.5x ATR below execution price.

                            //Calculate ATR for day x
                            decimal atrSumClose = 0;
                            int countATRClose = 0;
                            for (int k = x - 9; k <= x; k++)
                            {
                                decimal highLow = stockHistory[k].High - stockHistory[k].Low;
                                decimal highPrevClose = Math.Abs(stockHistory[k].High - stockHistory[k - 1].Close);
                                decimal lowPrevClose = Math.Abs(stockHistory[k].Low - stockHistory[k - 1].Close);
                                decimal trueRange = Math.Max(highLow, Math.Max(highPrevClose, lowPrevClose));
                                atrSumClose += trueRange;
                                countATRClose++;
                            }
                            decimal atrClose = atrSumClose / countATRClose;

                            //Calculate our stop loss level. 
                            decimal stopLoss = executionPrice - 2.5m * atrClose;

                            if (stockHistory[x].Close <= stopLoss)
                            {
                                dayOfExit = x;
                                break; // Exit the trade on day x (stopped out)
                            }

                            //Exit Condition 2 -- Profit Target: 4% or more profit.
                            if (stockHistory[x].Close >= executionPrice * 1.04m && stockHistory.Count > x + 1)
                            {
                                dayOfExit = x + 1;
                                break;
                            }

                            //Exit Condition 3 -- Time Out: 3 days without trigger conditions 1 & 2.
                            if( x - i + 1 == 3 && stockHistory.Count > x + 1)
                            {
                                dayOfExit = x + 1;
                                break;
                            }
                        } //exit the simulatinon loop

                        //If we found a valid exit condition, exit the trade.
                        if(dayOfExit != -1)
                        {
                            trade.CloseTrade(stockHistory[dayOfExit].Date, stockHistory[dayOfExit].Close);
                            trades.Add(trade);
                        }

                    }
                } //end simulation loop, continue checking the next day
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }

        //Calculate final metrics if we haev any trades
        if(trades != null)
        {

            //Calculate total trades.
            int totalTrades = trades.Count;

            //Calculate win rate.
            int winningTrades = trades.Count(t => t.IsWinningTrade);
            decimal winRate = totalTrades > 0 ? (decimal)winningTrades / totalTrades * 100 : 0;

            //Calculate Net % Gain/Loss
            decimal netPercent = trades.Sum(t => t.TradeReturnPercentage);

            //Calculate Average Win vs. Loss Ratio
            decimal avgWin = trades.Where(t => t.TradeReturnPercentage > 0)
                .Select(t => t.TradeReturnPercentage)
                .DefaultIfEmpty(0)
                .Average();
            decimal avgLoss = trades.Where(t => t.TradeReturnPercentage < 0)
                .Select(t => t.TradeReturnPercentage)
                .DefaultIfEmpty(0)
                .Average();

            decimal avgWinLossRatio = Math.Abs(avgLoss) > 0 ? avgWin / Math.Abs(avgLoss) : 0;

            Console.WriteLine("----- Trade Metrics -----");
            Console.WriteLine($"Total Trades: {totalTrades}");
            Console.WriteLine($"Win Rate: {winRate:F2}%");
            Console.WriteLine($"Net % Gain/Loss (non-compounded): {netPercent:F2}%");
            Console.WriteLine($"Average Win vs. Average Loss Ratio: {avgWinLossRatio:F2}");
            Console.WriteLine("-------------------------");


        }
    }
}
