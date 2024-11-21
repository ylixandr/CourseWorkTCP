using System;
using System.Collections.Generic;

namespace TCPServerLab2;

public partial class ProductTransaction
{
    public int TransactionId { get; set; }

    public int ProductId { get; set; }

    public string TransactionType { get; set; } = null!;

    public decimal Quantity { get; set; }

    public DateTime? TransactionDate { get; set; }

    public string? Description { get; set; }

    public virtual Product Product { get; set; } = null!;
}
