using System;
using System.Collections.Generic;

public class MRSLongStrategy : Strategy<MRSLongConfig>
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

    public bool ShouldEnter(int index, List<TradingDay> history, IndicatorSnapshot indicators, MRSLongConfig config)
    {

        var day = history[index];

        var sma = indicators.Get("SMA");
        var atr = indicators.Get("ATR");
        var volume = indicators.Get("AvgVolume");
        var drop = indicators.Get("PercentChange");

        bool passesMinPrice = day.Close >= config.MinPrice;
        bool passesVolume = volume.HasValue && volume >= config.MinAvgVolume;
        bool passesATR = atr.HasValue && ((atr.Value / day.Close) * 100 >= config.AtrTargetPercentage);
        bool passesFilter = passesMinPrice && passesVolume && passesATR;

        bool aboveSMA = sma.HasValue && day.Close > sma.Value;
        bool dropCondition = drop.HasValue && drop.Value <= config.DropPercentagePreReq;

        return passesFilter && aboveSMA && dropCondition;
    }


    public bool TryGetEntryPrice(int index, List<TradingDay> history, MRSLongConfig config, out decimal entryPrice)
    {
        entryPrice = 0;

        if (index == 0) return false; // No previous day

        var day = history[index];
        var prevDay = history[index - 1];

        decimal limitPrice = prevDay.Close * config.LimitOrderDiscount;

        if (day.Low <= limitPrice)
        {
            entryPrice = limitPrice;
            return true;
        }

        return false;
    }


    public bool ShouldExit(int entryIndex, int currentIndex, List<TradingDay> history, Dictionary<DateTime, IndicatorSnapshot> indicatorMap, MRSLongConfig config, decimal entryPrice)
    {

        var day = history[currentIndex];
        var indicators = indicatorMap.ContainsKey(day.Date) ? indicatorMap[day.Date] : null;

        if (indicators == null)
            return false;

        var atr = indicators.Get("ATR") ?? 0;
        decimal stopLoss = entryPrice - config.AtrStopLossMultiplier * atr;

        bool hitStopLoss = day.Close <= stopLoss;
        bool hitProfitTarget = day.Close >= entryPrice * config.ProfitTarget;
        bool timeoutExit = (currentIndex - entryIndex + 1) == config.TimeoutDuration;

        return hitStopLoss || hitProfitTarget || timeoutExit;
    }

}
