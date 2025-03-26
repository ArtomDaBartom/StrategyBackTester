using System;
using System.Collections.Generic;

public class Simulator<TConfig>
{
    private readonly Strategy<TConfig> _strategy;
    private readonly TConfig _config;

    public Simulator(Strategy<TConfig> strategy, TConfig config)
    {
        _strategy = strategy;
        _config = config;
    }

    public List<Trade> Run(List<TradingDay> history, Dictionary<DateTime, IndicatorSnapshot> indicatorMap)
    {
        var trades = new List<Trade>();

        for (int i = 0; i < history.Count; i++)
        {
            var day = history[i];
            if (!indicatorMap.ContainsKey(day.Date)) continue;

            var indicators = indicatorMap[day.Date];

            // Check if the strategy wants to enter a trade today
            if (!_strategy.ShouldEnter(i, history, indicators, _config))
                continue;

            if (!_strategy.TryGetEntryPrice(i, history, _config, out decimal entryPrice))
                continue;

            var trade = new Trade(day.Date, entryPrice, _strategy.Direction);

            int exitDayIndex = -1;

            // Look forward to determine when to exit
            for (int j = i + 1; j < history.Count; j++)
            {

                if (_strategy.ShouldExit(i, j, history, indicatorMap, _config, entryPrice))
                {
                    exitDayIndex = j;
                    break;
                }
            }

            // Exit the trade if an exit was found. 
            if (exitDayIndex != -1)
            {
                var exitDay = history[exitDayIndex];
                trade.CloseTrade(exitDay.Date, exitDay.Close);
                trades.Add(trade);
            }
        }
        return trades;
    }
}
