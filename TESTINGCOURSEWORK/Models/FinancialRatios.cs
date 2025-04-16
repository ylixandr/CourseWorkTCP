using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Models
{
    public class FinancialRatios
    {
        public decimal CurrentRatio { get; set; }
        public decimal QuickRatio { get; set; }
        public decimal ROA { get; set; }
        public decimal ROE { get; set; }
        public decimal DebtToEquity { get; set; }
    }
}
