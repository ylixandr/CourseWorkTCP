using System;
using System.Collections.Generic;

namespace TCPServer;

public partial class Status
{
    public int Id { get; set; }
    public string StatusName { get; set; }
    public string Type { get; set; } // Новое поле: "ApplicationStatus", "SupportStatus"

    public virtual ICollection<Application> Applications { get; set; } = new List<Application>();
    public virtual ICollection<SupportTicket> SupportTickets { get; set; } = new List<SupportTicket>();
}