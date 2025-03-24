using System;
using System.Collections.Generic;

public class SMAIndicator : Indicator
{
    private readonly int _period;
    private readonly Queue<decimal> _window;
    private decimal _rollingSum;

    public string Name => "SMA";

    public SMAIndicator(int period)
    {
        _period = period;
        _window = new Queue<decimal>();
        _rollingSum = 0;
    }

    public decimal? Feed(List<TradingDay> history, int index)
    {
        decimal close = history[index].Close;

        _window.Enqueue(close);
        _rollingSum += close;

        if (_window.Count > _period)
        {
            _rollingSum -= _window.Dequeue();
        }

        return _window.Count == _period ? _rollingSum / _period : null;
    }
}
