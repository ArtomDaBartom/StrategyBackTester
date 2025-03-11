using System;
using System.Collections.Generic;
using System.Linq;

public static class PerformanceMetrics
{
    public static void CalculateAndPrint(List<Trade> trades)
    {
        if (trades.Count == 0)
        {
            Console.WriteLine("No trades executed.");
            return;
        }

        int totalTrades = trades.Count;
        int winningTrades = trades.Count(t => t.IsWinningTrade);
        decimal winRate = totalTrades > 0 ? (decimal)winningTrades / totalTrades * 100 : 0;
        decimal netPercent = trades.Sum(t => t.TradeReturnPercentage);

        decimal avgWin = trades.Where(t => t.TradeReturnPercentage > 0)
            .Select(t => t.TradeReturnPercentage)
            .DefaultIfEmpty(0)
            .Average();
        decimal avgLoss = trades.Where(t => t.TradeReturnPercentage < 0)
            .Select(t => t.TradeReturnPercentage)
            .DefaultIfEmpty(0)
            .Average();

        decimal avgWinLossRatio = Math.Abs(avgLoss) > 0 ? avgWin / Math.Abs(avgLoss) : 0;

        Console.WriteLine("----- Trade Metrics -----");
        Console.WriteLine($"Total Trades: {totalTrades}");
        Console.WriteLine($"Win Rate: {winRate:F2}%");
        Console.WriteLine($"Net % Gain/Loss (non-compounded): {netPercent:F2}%");
        Console.WriteLine($"Average Win vs. Average Loss Ratio: {avgWinLossRatio:F2}");
        Console.WriteLine("-------------------------");
    }
}
