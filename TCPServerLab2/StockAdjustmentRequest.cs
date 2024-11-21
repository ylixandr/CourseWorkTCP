using System;
using System.Collections.Generic;

namespace TCPServerLab2;

public partial class StockAdjustmentRequest
{
    public int RequestId { get; set; }

    public int ProductId { get; set; }

    public decimal Quantity { get; set; }

    public string TransactionType { get; set; } = null!;

    public string Description { get; set; } = null!;

    public DateTime RequestDate { get; set; }

    public virtual Product Product { get; set; } = null!;
}
