using System;
using System.Collections.Generic;

namespace TCPServer;

public partial class AdminPanel
{
    public int Id { get; set; }

    public string AdminCode { get; set; } = null!;

    public string ManagerCode { get; set; } = null!;
}
