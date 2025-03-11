using System;
using System.Collections.Generic;
using System.Linq;

public static class TechnicalIndicators
{
    // SMA Calculation Using a Rolling Sum
    public static decimal CalculateSMA(List<StockData> stockHistory, int currentIndex, int period, ref decimal prevSum)
    {
        if (currentIndex < period) return 0; // Not enough data to calculate SMA

        if (prevSum == 0) // First-time calculation (no rolling sum yet)
        {
            prevSum = stockHistory.Skip(currentIndex - period).Take(period).Sum(s => s.Close);
        }
        else
        {
            prevSum = prevSum - stockHistory[currentIndex - period].Close + stockHistory[currentIndex].Close;
        }

        return prevSum / period;
    }

    public static decimal CalculateATR(List<StockData> stockHistory, int currentIndex, int period, ref decimal prevSum)
    {
        if (currentIndex < period) return 0; //Not enough daya to calculate ATR

        // Step 1: Calculate the True Range (TR) for the current day
        decimal highLow = stockHistory[currentIndex].High - stockHistory[currentIndex].Low;
        decimal highPrevClose = Math.Abs(stockHistory[currentIndex].High - stockHistory[currentIndex - 1].Close);
        decimal lowPrevClose = Math.Abs(stockHistory[currentIndex].Low - stockHistory[currentIndex - 1].Close);

        // The True Range is the maximum of these three values.
        decimal trueRange = Math.Max(highLow, Math.Max(highPrevClose, lowPrevClose));

        // Step 2: First-time ATR calculation (if no rolling sum exists yet)
        if (prevSum == 0)
        {
            prevSum = stockHistory
                .Skip(currentIndex - period) // Skip earlier data to start at the correct index
                .Take(period) // Take only 'period' number of days
                .Select((s, index) => // Select each stock day and its index
                    Math.Max(s.High - s.Low, // Option 1: High - Low of that day
                        Math.Max(Math.Abs(s.High - stockHistory[currentIndex - period + index].Close), // Option 2: High - Previous Close
                                 Math.Abs(s.Low - stockHistory[currentIndex - period + index].Close)))) // Option 3: Low - Previous Close
                .Sum(); // Sum all calculated True Ranges to get initial ATR sum
        }
        else
        {
            // Step 3: Use rolling sum for efficiency (subtract estimated oldest TR, add new TR)
            decimal oldATR = prevSum / period;
            prevSum = prevSum - oldATR + trueRange;
        }

        // Step 4: Return the new ATR value (average of the rolling sum)
        return prevSum / period;
    }

    public static decimal CalculateAvgVolume(List<StockData> stockHistory, int currentIndex, int period, ref decimal prevSum)
    {
        if (currentIndex < period) return 0; //Not enough data to calculate Average Volume

        if (prevSum == 0) // First-time calculation (no rolling sum yet)
        {
            prevSum = stockHistory.Skip(currentIndex - period).Take(period).Sum(s => s.Volume);
        }
        else
        {
            prevSum = prevSum - stockHistory[currentIndex - period].Volume + stockHistory[currentIndex].Volume;
        }

        return prevSum / period;

    }

    public static decimal? CalculatePercentageChange(List<StockData> stockHistory, int currentIndex, int period)
    {
        if (currentIndex < period) return null; //Not enough data to calculate.

        decimal close3DaysAgo = stockHistory[currentIndex - 3].Close;

        //Calculate the percentage drop over the last 3 days.
        decimal dropPercentage = ((close3DaysAgo - stockHistory[currentIndex].Close) / close3DaysAgo) * 100;

        return dropPercentage;
    }
}