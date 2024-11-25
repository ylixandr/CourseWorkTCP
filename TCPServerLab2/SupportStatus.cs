﻿using System;
using System.Collections.Generic;

namespace TCPServerLab2;

public partial class SupportStatus
{
    public int StatusId { get; set; }

    public string StatusName { get; set; } = null!;

    public virtual ICollection<SupportTicket> SupportTickets { get; set; } = new List<SupportTicket>();
}