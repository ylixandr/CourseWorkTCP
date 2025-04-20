namespace Client.ManagerFolder.DataAnalysys.Models
{
    public class BalanceModel
    {
        public string Period { get; set; }
        public double? Assets { get; set; }
        public double? Equity { get; set; }
        public double? BorrowedCapital { get; set; }
        public double? Liabilities { get; set; }
        public double? NetProfit { get; set; }
    }
}