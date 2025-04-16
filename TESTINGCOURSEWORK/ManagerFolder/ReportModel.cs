using TESTINGCOURSEWORK.Models;

namespace TESTINGCOURSEWORK
{
    public class ReportModel
    {
        public decimal Balance { get; set; }
        public int TotalTransactions { get; set; }
        public decimal IncomeSum { get; set; }
        public decimal ExpenseSum { get; set; }
      
        public double AverageTransaction { get; set; }
        public List<MonthlySummary> MonthlySummary { get; set; }
        public List<SuspiciousTransaction> Errors { get; set; }
    }
}