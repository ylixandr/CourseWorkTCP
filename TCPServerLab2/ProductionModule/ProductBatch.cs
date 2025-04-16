using System;
using System.Collections.Generic;

namespace TCPServer.ProductionModule
{
    public class ProductBatch
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string BatchNumber { get; set; } // Номер партии
        public DateTime? ExpiryDate { get; set; } // Срок годности
        public string SerialNumber { get; set; } // Серийный номер
        public decimal Quantity { get; set; } // Количество в партии
        public decimal Cost { get; set; } // Себестоимость партии
        public string Currency { get; set; } // Валюта себестоимости

        // Навигационные свойства
        public virtual Product Product { get; set; }
        public virtual ICollection<InventoryTransaction> Transactions { get; set; }

        public ProductBatch()
        {
            Transactions = new HashSet<InventoryTransaction>();
        }
    }
}