using System.Collections.Generic;

namespace TCPServer.ProductionModule
{
    public class Warehouse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }

        public virtual ICollection<Inventory> Inventories { get; set; }
        public virtual ICollection<InventoryTransaction> FromTransactions { get; set; }
        public virtual ICollection<InventoryTransaction> ToTransactions { get; set; }

        public Warehouse()
        {
            Inventories = new HashSet<Inventory>();
            FromTransactions = new HashSet<InventoryTransaction>();
            ToTransactions = new HashSet<InventoryTransaction>();
        }
    }
}