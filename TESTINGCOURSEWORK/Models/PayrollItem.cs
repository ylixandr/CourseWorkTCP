using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Models
{
    public class PayrollItem
    {
        public string Type { get; set; }
        public decimal Amount { get; set; }
        public string DaysOrHours { get; set; } // Для начислений
    }
}
