using System;
using System.Collections.Generic;

public class ATRIndicator : Indicator
{
    private readonly int _period;
    private int _count = 0;
    private decimal? _previousAtr = null;
    private decimal _trSum = 0;

    public string Name => "ATR";

    public ATRIndicator(int period)
    {
        _period = period;
    }

    public decimal? Feed(List<TradingDay> history, int index)
    {
        if (index == 0) return null;

        decimal high = history[index].High;
        decimal low = history[index].Low;
        decimal prevClose = history[index - 1].Close;

        decimal highLow = high - low;
        decimal highPrevClose = Math.Abs(high - prevClose);
        decimal lowPrevClose = Math.Abs(low - prevClose);

        decimal trueRange = Math.Max(highLow, Math.Max(highPrevClose, lowPrevClose));

        if (_count < _period)
        {
            _trSum += trueRange;
            _count++;

            if (_count == _period)
            {
                _previousAtr = _trSum / _period;
                return _previousAtr;
            }

            return null;
        }

        _previousAtr = ((_previousAtr.Value * (_period - 1)) + trueRange) / _period;
        return _previousAtr;
    }
}
