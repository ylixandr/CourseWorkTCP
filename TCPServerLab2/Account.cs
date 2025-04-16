using System;
using System.Collections.Generic;

namespace TCPServer;

public partial class Account
{
    public int Id { get; set; }

    public string Login { get; set; } = null!;

    public string Password { get; set; } = null!;

    public int RoleId { get; set; }

   

    public virtual Role Role { get; set; } = null!;

    public virtual ICollection<SupportTicket> SupportTickets { get; set; } = new List<SupportTicket>();
}
