using TCPServerLab2;

namespace TESTINGCOURSEWORK
{
    public class ReportModel
    {
        public decimal Balance { get; set; }
        public int TotalTransactions { get; set; }
        public decimal IncomeSum { get; set; }
        public decimal ExpenseSum { get; set; }
        public Transaction MaxTransaction { get; set; }
        public Transaction MinTransaction { get; set; }
        public double AverageTransaction { get; set; }
        public List<MonthlySummary> MonthlySummary { get; set; }
        public List<SuspiciousTransaction> Errors { get; set; }
    }
}