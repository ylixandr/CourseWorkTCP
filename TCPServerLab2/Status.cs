using System;
using System.Collections.Generic;

namespace TCPServerLab2;

public partial class Status
{
    public int Id { get; set; }

    public string StatusName { get; set; } = null!;

    public virtual ICollection<Application> Applications { get; set; } = new List<Application>();
}
