using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TCPServerLab2;

namespace TESTINGCOURSEWORK.Models
{
    public class Account
    {
        public int Id { get; set; }
        public string? Login { get; set; }
        public string? Password { get; set; }
        public int RoleId { get; set; }


    }
}
