using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCPServer.ProductionModule
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Article { get; set; } // Артикул
        public string Barcode { get; set; } // Штрих-код
        public int CategoryId { get; set; } // Внешний ключ на категорию
        public decimal PurchasePrice { get; set; } // Закупочная цена
        public decimal SellingPrice { get; set; } // Продажная цена
        public string Currency { get; set; } // Валюта (RUB, USD и т.д.)
        public int? DescriptionId { get; set; } // Внешний ключ на описание

        // Навигационные свойства
        public virtual ProductCategory Category { get; set; }
        public virtual Description Description { get; set; }
        public virtual ICollection<ProductBatch> Batches { get; set; }
        public virtual ICollection<Inventory> Inventories { get; set; }
        public virtual ICollection<ProductComponent> Components { get; set; } // Комплектующие

        public Product()
        {
            Batches = new HashSet<ProductBatch>();
            Inventories = new HashSet<Inventory>();
            Components = new HashSet<ProductComponent>();
        }
    }
}
