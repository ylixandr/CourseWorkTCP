using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCPServer.balanceModule
{
    public class Liability
    {
        public int Id { get; set; }
        public string Category { get; set; } // "Кредиторская задолженность", "Заемный капитал"
        public string Name { get; set; }
        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; } // Срок погашения

        // Навигационные свойства
        public int? DescriptionId { get; set; }
        public virtual Description Description { get; set; }
    }
}
