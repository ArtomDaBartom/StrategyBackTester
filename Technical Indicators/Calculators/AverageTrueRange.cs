using System;
using System.Collections.Generic;

public class ATRCalculator
{
    private readonly int _period;
    private decimal _rollingSum;
    private readonly Queue<decimal> _window;

    public ATRCalculator(int period)
    {
        _period = period;
        _rollingSum = 0;
        _window = new Queue<decimal>();
    }

    public decimal? Feed(decimal high, decimal low, decimal newClose)
    {
        decimal highLow = high - low;
        decimal highPrevClose = newClose != 0 ? Math.Abs(high - newClose) : highLow;
        decimal lowPrevClose = newClose != 0 ? Math.Abs(low - newClose) : highLow;
        decimal trueRange = Math.Max(highLow, Math.Max(highPrevClose, lowPrevClose));

        _window.Enqueue(trueRange);
        _rollingSum += trueRange;

        if (_window.Count > _period)
        {
            _rollingSum -= _window.Dequeue();
        }

        return _window.Count == _period ? _rollingSum / _period : null;
    }

}