public interface Strategy<TConfig>
{
    string Name { get; }
    TradeDirection Direction { get; }

    List<string> RequiredIndicators { get; }

    bool ShouldEnter(int index, List<TradingDay> history, IndicatorSnapshot indicators, TConfig config);

    bool TryGetEntryPrice(int index, List<TradingDay> history, TConfig config, out decimal entryPrice);

    bool ShouldExit(int entryIndex, int currentIndex, List<TradingDay> history, Dictionary<DateTime, IndicatorSnapshot> indicatorMap, TConfig config, decimal entryPrice);

}
