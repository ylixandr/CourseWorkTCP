namespace TCPServer.ProductionModule
{
    public class ProductComponent
    {
        public int Id { get; set; }
        public int ParentProductId { get; set; } // Основной товар
        public int ComponentProductId { get; set; } // Комплектующий товар
        public decimal Quantity { get; set; } // Количество комплектующих на единицу товара

        // Навигационные свойства
        public virtual Product ParentProduct { get; set; }
        public virtual Product ComponentProduct { get; set; }
    }
}