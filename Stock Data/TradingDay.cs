using System;

public class TradingDay
{
    public DateTime Date { get; }
    public decimal Open { get; }
    public decimal High { get; }
    public decimal Low { get; }
    public decimal Close { get; }
    public long Volume { get; }

    public TradingDay(DateTime date, decimal open, decimal high, decimal low, decimal close, long volume)
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