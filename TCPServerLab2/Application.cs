using System;
using System.Collections.Generic;

namespace TCPServer;

public partial class Application
{
    public int Id { get; set; }

    public int AccountId { get; set; }

    public string? ContactInfo { get; set; }

    public string? ProductName { get; set; }

    public string? Description { get; set; }

    public decimal? TotalPrice { get; set; }

    public int? Quantity { get; set; }

    public string? UnitOfMeasurement { get; set; }

    public int StatusId { get; set; }

    public DateTime? DateSubmitted { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual Status Status { get; set; } = null!;
}
