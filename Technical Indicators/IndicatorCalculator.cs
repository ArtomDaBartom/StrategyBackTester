using System;
using System.Collections.Generic;

public class IndicatorPrecalculator
{
    private readonly int _smaPeriod;
    private readonly int _atrPeriod;
    private readonly int _volumePeriod;
    private readonly int _dropDays;

    private readonly SMACalculator _sma;
    private readonly ATRCalculator _atr;
    private readonly VolumeCalculator _volume;
    private readonly PercentageChangeCalculator _drop;

    public IndicatorPrecalculator(int smaPeriod, int atrPeriod, int volumePeriod, int dropDays)
    {
        _smaPeriod = smaPeriod;
        _atrPeriod = atrPeriod;
        _volumePeriod = volumePeriod;
        _dropDays = dropDays;

        _sma = new SMACalculator(smaPeriod);
        _atr = new ATRCalculator(atrPeriod);
        _volume = new VolumeCalculator(volumePeriod);
        _drop = new PercentageChangeCalculator(dropDays);
    }

    public Dictionary<DateTime, IndicatorSnapshot> Precalculate(List<TradingDay> stockHistory)
    {
        var result = new Dictionary<DateTime, IndicatorSnapshot>();

        for (int i = 0; i < stockHistory.Count; i++)
        {
            var day = stockHistory[i];
            var snapshot = new IndicatorSnapshot(day.Date)
            {
                Sma = _sma.Feed(day.Close),
                Atr = _atr.Feed(day.High, day.Low, i > 0 ? stockHistory[i - 1].Close : day.Close),
                AvgVolume = _volume.Feed(day.Volume),
                DropPercentX = _drop.Calculate(stockHistory, i)
            };

            result[day.Date] = snapshot;
        }

        return result;
    }
}
