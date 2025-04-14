using System;
using System.Collections.Generic;

namespace TCPServer;

public partial class Product
{
    public int ProductId { get; set; }
    public string ProductName { get; set; }
    public string Description { get; set; }
    public decimal Quantity { get; set; }
    public string UnitOfMeasurement { get; set; }
    public decimal? UnitPrice { get; set; }
    public DateTime? LastUpdated { get; set; }

    public virtual ICollection<ProductTransaction> ProductTransactions { get; set; } = new List<ProductTransaction>();
    public virtual ICollection<StockAdjustmentRequest> StockAdjustmentRequests { get; set; } = new List<StockAdjustmentRequest>();
    public virtual ICollection<Application> Applications { get; set; } = new List<Application>(); // Новая связь
}
