using System;
using System.Collections.Generic;
using TESTINGCOURSEWORK;

namespace TCPServerLab2;

public partial class Role
{
    public int RoleId { get; set; }

    public string RoleName { get; set; } = null!;

    public virtual ICollection<Account> Accounts { get; set; } = new List<Account>();
}
