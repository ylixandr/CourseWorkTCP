using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TCPServer.ProductionModule;

namespace TCPServer.balanceModule
{
    public class AuditLog
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Action { get; set; }
        public string EntityType { get; set; }
        public int EntityId { get; set; }
        public string Details { get; set; }
        public DateTime Timestamp { get; set; }

        // Навигационное свойство
        public virtual ICollection<BalanceSnapshot> BalanceSnapshots { get; set; }
        public virtual ICollection<InventoryTransaction> InventoryTransactions { get; set; }

        public AuditLog()
        {
            BalanceSnapshots = new HashSet<BalanceSnapshot>();
            InventoryTransactions = new HashSet<InventoryTransaction>();
        }
    }
}
