using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TESTINGCOURSEWORK.Models
{

    public class ApplicationViewModel
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string ContactInfo { get; set; }
        public string ProductName { get; set; }
        public string Description { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; }
        public string DateSubmitted { get; set; }
        public int Quantity { get; set; } // Новое поле для количества
        public string UnitOfMeasurement { get; set; } // Новое поле для единицы измерения
    }

}
