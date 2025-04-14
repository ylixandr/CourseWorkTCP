using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCPServer.balanceModule
{
    public class Operation
    {
        public int Id { get; set; }
        public string Type { get; set; } // "Поступление", "Выбытие", "Амортизация", "Погашение долга", "Переоценка"
        public string EntityType { get; set; } // "Asset", "Liability", "Equity"
        public int EntityId { get; set; } // ID объекта (Asset, Liability, Equity)
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public int UserId { get; set; } // Кто выполнил операцию

        // Навигационные свойства
        public int? DescriptionId { get; set; }
        public virtual Description Description { get; set; }
        public virtual Account User { get; set; }
    }
}
