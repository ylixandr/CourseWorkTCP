using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCPServer.balanceModule
{
    public class Equity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }

        // Навигационные свойства
        public int? DescriptionId { get; set; }
        public virtual Description Description { get; set; }
    }
}
