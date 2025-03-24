using System;
using System.Collections.Generic;

public class AvgVolumeIndicator : Indicator
{
    private readonly int _period;
    private readonly Queue<decimal> _window;
    private decimal _rollingSum;

    public string Name => "AvgVolume";

    public AvgVolumeIndicator(int period)
    {
        _period = period;
        _window = new Queue<decimal>();
        _rollingSum = 0;
    }

    public decimal? Feed(List<TradingDay> history, int index)
    {
        decimal volume = history[index].Volume;

        _window.Enqueue(volume);
        _rollingSum += volume;

        if (_window.Count > _period)
        {
            _rollingSum -= _window.Dequeue();
        }

        return _window.Count == _period ? _rollingSum / _period : null;
    }
}
