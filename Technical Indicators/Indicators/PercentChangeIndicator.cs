using System;
using System.Collections.Generic;

public class PercentChangeIndicator : Indicator
{
    private readonly int _period;
    private readonly Queue<decimal> _window;

    public string Name => "PercentChange";

    public PercentChangeIndicator(int period)
    {
        _period = period;
        _window = new Queue<decimal>();
    }

    public decimal? Feed(List<TradingDay> history, int index)
    {
        decimal currentClose = history[index].Close;
        _window.Enqueue(currentClose);

        if (_window.Count <= _period)
            return null;

        decimal previousClose = _window.Dequeue();
        if (previousClose == 0) return null;

        return ((currentClose - previousClose) / previousClose) * 100;
    }
}
