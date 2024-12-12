using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCPServer
{
    public class MonthlySummary
    {
        public int Month { get; set; }
        public int Total { get; set; }
        public decimal Income { get; set; }
        public decimal Expense { get; set; }
        public decimal MonthEndBalance { get; set; }
    }
}
