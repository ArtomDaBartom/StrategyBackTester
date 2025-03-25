using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;




public class IndicatorSnapshot
{
    public DateTime Date { get; set; }
    public Dictionary<string, decimal?> Values { get; } = new();

    public IndicatorSnapshot(DateTime date)
    {
        Date = date;
    }

    public decimal? Get(string key) =>
        Values.ContainsKey(key) ? Values[key] : null;
}




public interface Indicator
{
    string Name { get; }
    decimal? Feed(List<TradingDay> history, int index);
}




public class IndicatorPrecalculator
{
    private readonly List<Indicator> _indicators;

    public IndicatorPrecalculator(List<Indicator> indicators)
    {
        _indicators = indicators;
    }

    public Dictionary<DateTime, IndicatorSnapshot> Precalculate(List<TradingDay> stockHistory)
    {
        var result = new Dictionary<DateTime, IndicatorSnapshot>();

        for (int i = 0; i < stockHistory.Count; i++)
        {
            var snapshot = new IndicatorSnapshot(stockHistory[i].Date);

            foreach (var indicator in _indicators)
            {
                var value = indicator.Feed(stockHistory, i);
                snapshot.Values[indicator.Name] = value;
            }

            result[snapshot.Date] = snapshot;
        }

        return result;
    }
}




public static class IndicatorBuilder
{
    public static List<Indicator> Build(MRSLongConfig config, List<string> required)
    {
        var indicators = new List<Indicator>();

        if (required.Contains("SMA"))
            indicators.Add(new SMAIndicator(config.SmaPeriod));

        if (required.Contains("ATR"))
            indicators.Add(new ATRIndicator(config.AtrPeriod));

        if (required.Contains("AvgVolume"))
            indicators.Add(new AvgVolumeIndicator(config.AvgVolumePeriod));

        if (required.Contains("PercentChange"))
            indicators.Add(new PercentChangeIndicator(config.DropPercentageDays));


        return indicators;
    }
}



