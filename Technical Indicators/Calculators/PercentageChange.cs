using System;
using System.Collections.Generic;

public class PercentageChangeCalculator
{
    private readonly int _period;

    public PercentageChangeCalculator(int period)
    {
        _period = period;
    }

    public decimal? Calculate(List<TradingDay> stockHistory, int currentIndex)
    {
        if (currentIndex < _period) return null; // Not enough data
        decimal previousClose = stockHistory[currentIndex - _period].Close;
        decimal currentClose = stockHistory[currentIndex].Close;

        if (previousClose == 0) return null; // Divide by zero
        return ((currentClose - previousClose) / previousClose) * 100;
    }
}
