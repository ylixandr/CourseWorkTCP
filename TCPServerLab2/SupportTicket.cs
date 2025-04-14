using System;
using System.Collections.Generic;

namespace TCPServer;

public partial class SupportTicket
{
    public int TicketId { get; set; }

    public int UserId { get; set; }

    public string UserEmail { get; set; } = null!;

    public DateTime SubmissionDate { get; set; }

    public int StatusId { get; set; }


    public virtual Account User { get; set; } = null!;
    public virtual Status Status { get; set; }
    public int? DescriptionId { get; set; } // Новое поле
    public virtual Description Description { get; set; } // Новая связь
}
