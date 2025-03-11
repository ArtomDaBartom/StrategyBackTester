using System;

public class Trade
{
    public DateTime EntryDate { get; set; }

    public decimal EntryPrice { get; set; }

    public DateTime ExitDate { get; set; }

    public decimal ExitPrice { get; set; }

    public decimal TradeReturnPercentage { get; private set; }

    public bool IsWinningTrade => TradeReturnPercentage > 0;

    public decimal ProfitLoss { get; private set; }

    // Constructor: Initializes a new trade with entry date and entry price.
    public Trade(DateTime entryDate, decimal entryPrice)
    {
        EntryDate = entryDate;
        EntryPrice = entryPrice;
    }

    // Close the trade by providing the exit date and exit price.
    public void CloseTrade(DateTime exitDate, decimal exitPrice)
    {
        ExitDate = exitDate;
        ExitPrice = exitPrice;
        // Calculate the return percentage as: ((ExitPrice - EntryPrice) / EntryPrice) * 100.
        TradeReturnPercentage = ((exitPrice - EntryPrice) / EntryPrice) * 100;

        //Calculate the proft or loss.
        ProfitLoss = ExitPrice - EntryPrice;
    }
}