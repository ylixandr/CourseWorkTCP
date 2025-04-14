using System;
using System.Collections.Generic;

namespace TCPServer;

public partial class Application
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public string ContactInfo { get; set; }
    public int? ProductId { get; set; }
    public int? DescriptionId { get; set; } // Новое поле
    public decimal? TotalPrice { get; set; }
    public int? Quantity { get; set; }
    public string UnitOfMeasurement { get; set; }
    public int StatusId { get; set; }
    public DateTime? DateSubmitted { get; set; }

    public virtual Account Account { get; set; }
    public virtual Product Product { get; set; }
    public virtual Status Status { get; set; }
    public virtual Description Description { get; set; } // Новая связь
}