using System.Collections.Generic;

namespace TCPServer.ProductionModule
{
    public class ProductCategory
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? ParentCategoryId { get; set; } // Для иерархии (подкатегории)

        // Навигационные свойства
        public virtual ProductCategory ParentCategory { get; set; }
        public virtual ICollection<ProductCategory> SubCategories { get; set; }
        public virtual ICollection<Product> Products { get; set; }

        public ProductCategory()
        {
            SubCategories = new HashSet<ProductCategory>();
            Products = new HashSet<Product>();
        }
    }
}