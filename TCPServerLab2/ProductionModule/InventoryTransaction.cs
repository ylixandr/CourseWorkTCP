using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TCPServer.balanceModule;

namespace TCPServer.ProductionModule
{
    public class InventoryTransaction
    {
        public int Id { get; set; }
        public int ProductBatchId { get; set; }
        public int? FromWarehouseId { get; set; }
        public int? ToWarehouseId { get; set; }
        public string TransactionType { get; set; } // "receipt", "shipment", "transfer"
        public decimal Quantity { get; set; }
        public DateTime TransactionDate { get; set; }
        public int AuditLogId { get; set; }

        public virtual ProductBatch ProductBatch { get; set; }
        public virtual Warehouse FromWarehouse { get; set; }
        public virtual Warehouse ToWarehouse { get; set; }
        public virtual AuditLog AuditLog { get; set; }
    }
}
