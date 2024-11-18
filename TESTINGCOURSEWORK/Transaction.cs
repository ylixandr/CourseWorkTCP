using System;
using System.Collections.Generic;

namespace TCPServerLab2;

public partial class Transaction
{
    public int Id { get; set; }

    public DateTime TransactionDate { get; set; }

    public decimal Amount { get; set; }

    public string TransactionType { get; set; } = null!;

    public string? Description { get; set; }
}
