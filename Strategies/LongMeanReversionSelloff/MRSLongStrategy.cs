using System;
using System.Collections.Generic;

public class MRSLongStrategy : Strategy
{
    public string Name => "MRS Long";

    public List<string> RequiredIndicators => new List<string>
    {
        "SMA",
        "ATR",
        "AvgVolume",
        "PercentChange"
    };

    public TradeDirection Direction => TradeDirection.Long;

    public bool ShouldEnter(int index, List<TradingDay> history, IndicatorSnapshot indicators, object config)
    {

        if (config is not MRSLongConfig cfg)
            throw new ArgumentException("Invalid config type passed to MRSLongStrategy.");

        var day = history[index];

        var sma = indicators.Get("SMA");
        var atr = indicators.Get("ATR");
        var volume = indicators.Get("AvgVolume");
        var drop = indicators.Get("PercentChange");

        bool passesMinPrice = day.Close >= cfg.MinPrice;
        bool passesVolume = volume.HasValue && volume >= cfg.MinAvgVolume;
        bool passesATR = atr.HasValue && ((atr.Value / day.Close) * 100 >= cfg.AtrTargetPercentage);
        bool passesFilter = passesMinPrice && passesVolume && passesATR;

        bool aboveSMA = sma.HasValue && day.Close > sma.Value;
        bool dropCondition = drop.HasValue && drop.Value <= cfg.DropPercentagePreReq;

        return passesFilter && aboveSMA && dropCondition;
    }


    public bool TryGetEntryPrice(int index, List<TradingDay> history, object config, out decimal entryPrice)
    {
        entryPrice = 0;

        if (config is not MRSLongConfig cfg)
            throw new ArgumentException("Invalid config type passed to MRSLongStrategy.");

        if (index == 0) return false; // No previous day

        var day = history[index];
        var prevDay = history[index - 1];

        decimal limitPrice = prevDay.Close * cfg.LimitOrderDiscount;

        if (day.Low <= limitPrice)
        {
            entryPrice = limitPrice;
            return true;
        }

        return false;
    }


    public bool ShouldExit(int entryIndex, int currentIndex, List<TradingDay> history, Dictionary<DateTime, IndicatorSnapshot> indicatorMap, object config, decimal entryPrice)
    {
        if (config is not MRSLongConfig cfg)
            throw new ArgumentException("Invalid config type passed to MRSLongStrategy.");

        var day = history[currentIndex];
        var indicators = indicatorMap.ContainsKey(day.Date) ? indicatorMap[day.Date] : null;

        if (indicators == null)
            return false;

        var atr = indicators.Get("ATR") ?? 0;
        decimal stopLoss = entryPrice - cfg.AtrStopLossMultiplier * atr;

        bool hitStopLoss = day.Close <= stopLoss;
        bool hitProfitTarget = day.Close >= entryPrice * cfg.ProfitTarget;
        bool timeoutExit = (currentIndex - entryIndex + 1) == cfg.TimeoutDuration;

        return hitStopLoss || hitProfitTarget || timeoutExit;
    }

}
