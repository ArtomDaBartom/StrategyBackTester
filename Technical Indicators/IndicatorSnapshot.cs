using System;

public class IndicatorSnapshot
{
    public DateTime Date { get; set; }
    public decimal? Sma { get; set; }
    public decimal? Atr { get; set; }
    public decimal? AvgVolume { get; set; }
    public decimal? DropPercentX { get; set; }

    public IndicatorSnapshot(DateTime date)
    {
        Date = date;
    }
}
