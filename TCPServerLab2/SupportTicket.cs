using System;
using System.Collections.Generic;

namespace TCPServerLab2;

public partial class SupportTicket
{
    public int TicketId { get; set; }

    public int UserId { get; set; }

    public string UserEmail { get; set; } = null!;

    public DateTime SubmissionDate { get; set; }

    public int StatusId { get; set; }

    public string Description { get; set; } = null!;

    public virtual SupportStatus Status { get; set; } = null!;

    public virtual Account User { get; set; } = null!;
}
