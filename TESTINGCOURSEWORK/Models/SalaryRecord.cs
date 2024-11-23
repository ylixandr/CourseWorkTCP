using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TESTINGCOURSEWORK.Models
{
    public class SalaryRecord
    {
        public string LastName { get; set; }
        public decimal Salary { get; set; }
        public DateOnly Date { get; set; } // Если нужна дата
    }

}
