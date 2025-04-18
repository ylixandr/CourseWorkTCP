using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.InventoryViewModel
{

    // Модель для продукта
    public class ProductViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Article { get; set; }
        public string Barcode { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal SellingPrice { get; set; }
        public string Currency { get; set; }
        public int? DescriptionId { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }

    // Модель для категории продукции
    public class ProductCategoryViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? ParentCategoryId { get; set; }
        public string ParentCategoryName { get; set; } // Для отображения родительской категории
        public override string ToString() => Name; // Теперь ComboBox будет показывать Name
    }

    // Модель для склада
    public class WarehouseViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    // Модель для инвентаря (остатков на складе)
    public class InventoryViewModel
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } // Для отображения
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; } // Для отображения
        public decimal Quantity { get; set; }
        public decimal ReservedQuantity { get; set; }
    }

    // Модель для инвентарной транзакции
    public class InventoryTransactionViewModel
    {
        public int Id { get; set; }
        public int ProductBatchId { get; set; }
        public int? FromWarehouseId { get; set; }
        public string FromWarehouseName { get; set; } // Для отображения
        public int? ToWarehouseId { get; set; }
        public string ToWarehouseName { get; set; } // Для отображения
        public decimal Quantity { get; set; }
        public string TransactionType { get; set; } // "Receipt", "Shipment", "Transfer"
        public DateTime TransactionDate { get; set; }
    }

    // Модель для сводки по продукции
    public class ProductSummaryViewModel
    {
        public int TotalProducts { get; set; }
        public int TotalCategories { get; set; }
        public int TotalWarehouses { get; set; }
        public decimal TotalQuantity { get; set; }
        public decimal TotalInventoryValue { get; set; } // Сумма Quantity * PurchasePrice
    }
}

