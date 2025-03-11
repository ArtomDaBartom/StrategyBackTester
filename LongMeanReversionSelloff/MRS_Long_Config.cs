public static class MRS_Long_Config
{
    // General Strategy Settings
    public const int atr_period = 10;  // ATR calculation period
    public const int sma_period = 150; // Simple Moving Average period
    public const int avg_volume_period = 50; // 50-day average volume filter

    // Entry Conditions & Filters
    public const decimal min_price = 1.00m; // Minimum stock price requirement
    public const decimal atr_target = 5.0m; //Minimum ATR requirement
    public const decimal min_avg_volume = 1_000_000m; // Minimum 50-day average volume
    public const decimal drop_percentage_preReq = 12.5m; // 12.5% drop requirement
    public const int drop_percentage_days = 3;

    // Execution Settings
    public const decimal limit_order_target = 0.93m; // Limit order 7% below previous close

    // Exit Conditions
    public const decimal atr_stopLoss_multiplier = 2.5m; // ATR stop loss multiplier
    public const decimal profit_target = 1.04m; // 4% profit target before exiting next day
    public const int timeout_duration = 3; // Maximum hold period before forced exit
}
