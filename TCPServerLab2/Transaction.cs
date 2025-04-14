using System;
using System.Collections.Generic;

namespace TCPServer;

public partial class Transaction
{
    public int Id { get; set; }
    public int CategoryId { get; set; } // Новое поле
    public int? RelatedEntityId { get; set; } // Новое поле
    public string RelatedEntityType { get; set; } // Новое поле
    public decimal Amount { get; set; }
    public DateTime TransactionDate { get; set; }
    public int? DescriptionId { get; set; } // Новое поле

    public virtual TransactionCategory Category { get; set; }
    public virtual Description Description { get; set; }
}
