using System;
using System.Collections.Generic;

namespace TCPServer;

public partial class ProductTransaction
{
    public int TransactionId { get; set; }
    public int ProductId { get; set; }
    public decimal Quantity { get; set; }
    public DateTime TransactionDate { get; set; }
    public int? DescriptionId { get; set; } // Новое поле для связи с Descriptions

    public virtual Product Product { get; set; }
    public virtual Description Description { get; set; } // Навигационное свойство
}