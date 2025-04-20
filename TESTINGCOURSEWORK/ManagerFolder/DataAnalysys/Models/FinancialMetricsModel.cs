namespace Client.ManagerFolder.DataAnalysys.Models
{
    public class FinancialMetricsModel
    {
        public string Period { get; set; }
        public double? ReturnOnEquity { get; set; }
        public double? LiquidityRatio { get; set; }
        public double? BorrowedCapitalShare { get; set; }
    }
}