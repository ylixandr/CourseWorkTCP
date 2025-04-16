using TCPServer.ProductionModule;

namespace TCPServer.ProductionModule
{
    public class Inventory
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int WarehouseId { get; set; }
        public decimal Quantity { get; set; } // Текущий остаток
        public decimal ReservedQuantity { get; set; } // Зарезервированное количество

        // Навигационные свойства
        public virtual Product Product { get; set; }
        public virtual Warehouse Warehouse { get; set; }
    }
}