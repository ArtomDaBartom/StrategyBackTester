using System;
using System.Collections.Generic;
using System.Linq;

public class PerformanceMetrics
{
    private List<Trade> _executedTrades;
    public int TotalTrades { get; private set; }
    public decimal WinRate { get; private set; }
    public decimal NetPercent { get; private set; }
    public decimal AvgWinLossRatio { get; private set; }

    public PerformanceMetrics(List<Trade> trades)
    {
        _executedTrades = trades;
    }

    public void Calculate()
    {
        TotalTrades = _executedTrades.Count;

        if (TotalTrades == 0)
        {
            Console.WriteLine("No trades executed.");
            return;
        }

        int winningTrades = _executedTrades.Count(t => t.IsWinningTrade);
        WinRate = TotalTrades > 0 ? (decimal)winningTrades / TotalTrades * 100 : 0;
        NetPercent = _executedTrades.Sum(t => t.TradeReturnPercentage);

        decimal avgWin = _executedTrades.Where(t => t.TradeReturnPercentage > 0)
            .Select(t => t.TradeReturnPercentage)
            .DefaultIfEmpty(0)
            .Average();
        decimal avgLoss = _executedTrades.Where(t => t.TradeReturnPercentage < 0)
            .Select(t => t.TradeReturnPercentage)
            .DefaultIfEmpty(0)
            .Average();

        AvgWinLossRatio = Math.Abs(avgLoss) > 0 ? avgWin / Math.Abs(avgLoss) : 0;
    }
    public void Print()
    {
        if (TotalTrades == 0)
        {
            return;
        }
        else
        {
            Console.WriteLine("----- Trade Metrics -----");
            Console.WriteLine($"Total Trades: {TotalTrades}");
            Console.WriteLine($"Win Rate: {WinRate:F2}%");
            Console.WriteLine($"Net % Gain/Loss (non-compounded): {NetPercent:F2}%");
            Console.WriteLine($"Average Win vs. Average Loss Ratio: {AvgWinLossRatio:F2}");
            Console.WriteLine("-------------------------");
        }
    }
}
