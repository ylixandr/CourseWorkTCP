namespace Client.ManagerFolder.DataAnalysys.Models
{
    public class CapitalFlowModel
    {
        public string Period { get; set; }
        public double? InitialEquity { get; set; }
        public double? CapitalIncrease { get; set; }
        public double? Losses { get; set; }
        public double? FinalEquity { get; set; }
    }
}