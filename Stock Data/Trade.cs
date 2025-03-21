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

    public Trade(DateTime entryDate, decimal entryPrice)
    {
        EntryDate = entryDate;
        EntryPrice = entryPrice;
    }

    public void CloseTrade(DateTime exitDate, decimal exitPrice)
    {
        ExitDate = exitDate;
        ExitPrice = exitPrice;
        TradeReturnPercentage = ((exitPrice - EntryPrice) / EntryPrice) * 100;
        ProfitLoss = ExitPrice - EntryPrice;
    }
}