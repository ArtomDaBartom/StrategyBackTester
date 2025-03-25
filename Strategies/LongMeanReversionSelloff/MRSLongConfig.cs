public class MRSLongConfig
{
    public int SmaPeriod { get; set; }
    public int AtrPeriod { get; set; }
    public int AvgVolumePeriod { get; set; }
    public int DropPercentageDays { get; set; }

    public decimal MinPrice { get; set; }
    public decimal MinAvgVolume { get; set; }
    public decimal AtrTargetPercentage { get; set; }
    public decimal DropPercentagePreReq { get; set; }

    public decimal LimitOrderDiscount { get; set; }
    public decimal AtrStopLossMultiplier { get; set; }
    public decimal ProfitTarget { get; set; }
    public int TimeoutDuration { get; set; }


}
