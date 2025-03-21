using System;
using System.Collections.Generic;

public class SMACalculator
{
    private readonly int _period;
    private decimal _rollingSum;
    private readonly Queue<decimal> _window;

    public SMACalculator(int period)
    {
        _period = period;
        _rollingSum = 0;
        _window = new Queue<decimal>();
    }

    public decimal? Feed(decimal newClose)
    {
        _window.Enqueue(newClose);
        _rollingSum += newClose;

        if(_window.Count > _period)
        {
            _rollingSum -= _window.Dequeue();
        }

        return _window.Count == _period ? _rollingSum / _period : null;
    }

}