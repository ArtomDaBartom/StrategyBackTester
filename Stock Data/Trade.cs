using System;

public enum TradeDirection
{
    Long,
    Short
}


public class Trade
{
    public DateTime EntryDate { get; set; }

    public decimal EntryPrice { get; set; }

    public DateTime ExitDate { get; set; }

    public decimal ExitPrice { get; set; }

    public TradeDirection Direction { get; set; }

    public decimal TradeReturnPercentage { get; private set; }

    public bool IsWinningTrade => TradeReturnPercentage > 0;

    public decimal ProfitLoss { get; private set; }

    public Trade(DateTime entryDate, decimal entryPrice, TradeDirection direction)
    {
        EntryDate = entryDate;
        EntryPrice = entryPrice;
        Direction = direction;
    }

    public void CloseTrade(DateTime exitDate, decimal exitPrice)
    {
        ExitDate = exitDate;
        ExitPrice = exitPrice;

        if (Direction == TradeDirection.Long)
        {
            ProfitLoss = ExitPrice - EntryPrice;
        }
        else // Short
        {
            ProfitLoss = EntryPrice - ExitPrice;
        }

        TradeReturnPercentage = (ProfitLoss / EntryPrice) * 100;
    }


}



