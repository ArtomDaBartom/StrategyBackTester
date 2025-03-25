public interface Strategy
{
    string Name { get; }
    TradeDirection Direction { get; }

    List<string> RequiredIndicators { get; }

    bool ShouldEnter(int index, List<TradingDay> history, IndicatorSnapshot indicators, object config);

    bool TryGetEntryPrice(int index, List<TradingDay> history, object config, out decimal entryPrice);

    bool ShouldExit(int entryIndex, int currentIndex, List<TradingDay> history, Dictionary<DateTime, IndicatorSnapshot> indicatorMap, object config, decimal entryPrice);

}
