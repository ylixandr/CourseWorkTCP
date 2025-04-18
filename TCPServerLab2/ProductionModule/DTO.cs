using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCPServer.ProductionModule
{
    // DTO для продукта
    public class ProductDto
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public string Article { get; set; }
        public string Barcode { get; set; }
        public int CategoryId { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal SellingPrice { get; set; }
        public string Currency { get; set; }
        public int? DescriptionId { get; set; }
    }

    // DTO для категории продукции
    public class ProductCategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? ParentCategoryId { get; set; }
    }

    // DTO для склада
    public class WarehouseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    // DTO для инвентаря
    public class InventoryDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int WarehouseId { get; set; }
        public decimal Quantity { get; set; }
        public decimal ReservedQuantity { get; set; }
    }

    // DTO для инвентарной транзакции
    public class InventoryTransactionDto
    {
        public int Id { get; set; }
        public int ProductBatchId { get; set; }
        public int? FromWarehouseId { get; set; }
        public int? ToWarehouseId { get; set; }
        public decimal Quantity { get; set; }
        public string TransactionType { get; set; } // "Receipt", "Shipment", "Transfer"
        public DateTime TransactionDate { get; set; }
    }

    // DTO для сводки по продукции
    public class ProductSummaryDto
    {
        public int TotalProducts { get; set; }
        public int TotalCategories { get; set; }
        public int TotalWarehouses { get; set; }
        public decimal TotalQuantity { get; set; }
        public decimal TotalInventoryValue { get; set; }
        public List<CategorySummaryDto> Categories { get; set; } = new List<CategorySummaryDto>();
    }

    // DTO для сводки по категориям
    public class CategorySummaryDto
    {
        public string CategoryName { get; set; }
        public decimal TotalQuantity { get; set; }
        public decimal TotalValue;
    }
}