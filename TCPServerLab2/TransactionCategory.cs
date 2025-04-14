using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCPServer
{
    public partial class TransactionCategory
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}
