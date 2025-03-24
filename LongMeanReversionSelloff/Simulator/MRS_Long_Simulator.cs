using System;
using System.Collections.Generic;

public class MRSLongSimulator
{
    private readonly MRSLongConfig _config;

    public MRSLongSimulator(MRSLongConfig config)
    {
        _config = config;
    }

    public List<Trade> Run( List<TradingDay> stockHistory,Dictionary<DateTime, IndicatorSnapshot> indicatorMap)
    {
        var trades = new List<Trade>();


        // Iterate over the stock history
        for (int i = 0; i < stockHistory.Count; i++)
        {

            var day = stockHistory[i];

            // If there is no data for a day, skip it.
            if (!indicatorMap.ContainsKey(day.Date)) continue;

            var indicators = indicatorMap[day.Date];

            var sma = indicators.Get("SMA");
            var atr = indicators.Get("ATR");
            var volume = indicators.Get("AvgVolume");
            var percentChange = indicators.Get("PercentChange");


            // --- FILTER CONDITIONS ---

            bool passesMinPrice = day.Close >= _config.MinPrice;

            bool passesVolume = volume.HasValue && volume >= _config.MinAvgVolume;

            bool passesATR = atr.HasValue && ((atr / day.Close) * 100 >= _config.AtrTargetPercentage);

            bool passesFilter = passesMinPrice && passesVolume && passesATR;

            // --- ENTRY CONDITIONS ---

            bool aboveSMA = sma.HasValue &&
                            day.Close > sma;

            bool dropCondition = percentChange.HasValue &&
                                 percentChange <= _config.DropPercentagePreReq;

            bool validEntry = passesFilter && aboveSMA && dropCondition;

            // If we have a valid entry point, attempt to intitate a trade.
            if (validEntry && i > 0)
            {

                // Place a limit order.
                decimal limitPrice = stockHistory[i - 1].Close * _config.LimitOrderDiscount;

                //If the limit order was hit, open the trade.
                if (day.Low <= limitPrice)
                {
                    decimal executionPrice = limitPrice;
                    var trade = new Trade(day.Date, executionPrice);
                    int exitDay = -1;

                    // Look forward to figure out the close of the trade.
                    for (int j = i + 1; j < stockHistory.Count; j++)
                    {
                        var futureDay = stockHistory[j];
                        var futureIndicators = indicatorMap.ContainsKey(futureDay.Date) ? indicatorMap[futureDay.Date] : null;

                        decimal futureATR = futureIndicators?.Get("ATR") ?? 0;
                        decimal stopLoss = executionPrice - _config.AtrStopLossMultiplier * futureATR;
                        
                        // Exit Condition 1: Stop Loss is hit.
                        if (futureDay.Close <= stopLoss)
                        {
                            exitDay = j;
                            break;
                        }

                        // Exit Condition 2: Profit target is hit.
                        if (futureDay.Close >= executionPrice * _config.ProfitTarget && stockHistory.Count > j + 1)
                        {
                            exitDay = j + 1;
                            break;
                        }

                        // Exit Condition 3: Trade timed out.
                        if ((j - i + 1) == _config.TimeoutDuration && stockHistory.Count > j + 1)
                        {
                            exitDay = j + 1;
                            break;
                        }
                    }

                    // Close the trade.
                    if (exitDay != -1)
                    {
                        trade.CloseTrade(stockHistory[exitDay].Date, stockHistory[exitDay].Close);
                        trades.Add(trade);
                    }
                }
            }
        }
        return trades;
    }
}
