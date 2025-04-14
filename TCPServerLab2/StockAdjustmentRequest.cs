using System;
using System.Collections.Generic;

namespace TCPServer;

public partial class StockAdjustmentRequest
{
    public int RequestId { get; set; }

    public int ProductId { get; set; }

    public decimal Quantity { get; set; }

    public string TransactionType { get; set; } = null!;

    public int? DescriptionId { get; set; } // Новое поле
    public virtual Description Description { get; set; } // Новая связь

    public DateTime RequestDate { get; set; }

    public virtual Product Product { get; set; } = null!;
}
