using System;
using System.Collections.Generic;

public static class MRSelloffLongSimulator
{
    public static List<Trade> RunMRSelloffLong(List<StockData> stockHistory)
    {
        List<Trade> trades = new List<Trade>();

        // Variables to track rolling sums for optimized calculations
        decimal prevSMA150Sum = 0;
        decimal prevATRSum_Entry = 0;
        decimal prevATRSum_Exit = 0;
        decimal prevVolumeSum = 0;

        // Iterate over each day in stock history
        for (int i = 0; i < stockHistory.Count; i++)
        {
            // --- FILTER CALCULATIONS ---

            // Condition 1: Minimum price filter ($1.00 or higher)
            bool passesMinPrice = stockHistory[i].Close >= MRS_Long_Config.min_price;

            // Condition 2: 50-day average volume filter (Must be at least 1M shares)
            decimal avgVolume = TechnicalIndicators.CalculateAvgVolume(stockHistory, i, MRS_Long_Config.avg_volume_period, ref prevVolumeSum);
            bool passesVolumeFilter = avgVolume >= MRS_Long_Config.min_avg_volume;

            // Condition 3: 10-day Average True Range (ATR) filter (Volatility threshold)
            decimal atrEntry = TechnicalIndicators.CalculateATR(stockHistory, i, MRS_Long_Config.atr_period, ref prevATRSum_Entry);
            decimal atrPercentage = (atrEntry / stockHistory[i].Close) * 100;
            bool passesAtrFilter = atrPercentage >= MRS_Long_Config.atr_target;

            // Combine all filter conditions
            bool passesFilter = passesMinPrice && passesVolumeFilter && passesAtrFilter;

            // --- ENTRY CONDITIONS ---

            // Entry Condition 1: Closed above the 150-day SMA
            decimal sma150 = TechnicalIndicators.CalculateSMA(stockHistory, i, MRS_Long_Config.sma_period, ref prevSMA150Sum);
            bool SmaEntryCondition = stockHistory[i].Close > sma150;

            // Entry Condition 2: Stock has dropped at least 12.5% in the last 3 days
            decimal? dropPercentage = TechnicalIndicators.CalculatePercentageChange(stockHistory, i, MRS_Long_Config.drop_percentage_days);
            bool dropEntryCondition = dropPercentage >= MRS_Long_Config.drop_percentage_preReq;

            // Validate final entry condition
            bool validEntry = passesFilter && SmaEntryCondition && dropEntryCondition;

            // --- TRADE INITIATION ---
            if (validEntry)
            {
                // Limit order: Attempt to buy 7% below the previous day's close
                if (i > 0)
                {
                    decimal limitOrderPrice = stockHistory[i - 1].Close * MRS_Long_Config.limit_order_target;
                    bool limitOrderHit = stockHistory[i].Low <= limitOrderPrice;

                    if (limitOrderHit)
                    {
                        decimal executionPrice = limitOrderPrice;
                        Trade trade = new Trade(stockHistory[i].Date, executionPrice);

                        int dayOfExit = -1;

                        // --- TRADE EXIT CONDITIONS ---
                        for (int j = i + 1; j < stockHistory.Count; j++)
                        {
                            // Exit Condition 1: Stop Loss - 2.5x ATR below execution price
                            decimal atrExit = TechnicalIndicators.CalculateATR(stockHistory, j, MRS_Long_Config.atr_period, ref prevATRSum_Exit);
                            decimal stopLoss = executionPrice - MRS_Long_Config.atr_stopLoss_multiplier * atrExit;
                            if (stockHistory[j].Close <= stopLoss)
                            {
                                dayOfExit = j;
                                break; // Exit immediately if stop loss is hit
                            }

                            // Exit Condition 2: Profit Target - 4% gain, exit next day at close
                            if (stockHistory[j].Close >= executionPrice * MRS_Long_Config.profit_target && stockHistory.Count > j + 1)
                            {
                                dayOfExit = j + 1;
                                break;
                            }

                            // Exit Condition 3: Timeout - If neither SL nor TP is hit within 3 days, exit next day
                            if (j - i + 1 == MRS_Long_Config.timeout_duration && stockHistory.Count > j + 1)
                            {
                                dayOfExit = j + 1;
                                break;
                            }
                        }

                        // Close the trade if an exit condition was met
                        if (dayOfExit != -1)
                        {
                            trade.CloseTrade(stockHistory[dayOfExit].Date, stockHistory[dayOfExit].Close);
                            trades.Add(trade);
                        }
                    }
                }
            }
        }

        return trades;
    }
}

