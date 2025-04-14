using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCPServer.balanceModule
{
    public class AuditLog
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int OperationId { get; set; }
        public string Action { get; set; } // "Создание", "Обновление", "Удаление"
        public DateTime Timestamp { get; set; }

        // Навигационные свойства
        public virtual Account User { get; set; }
        public virtual Operation Operation { get; set; }
    }
}
