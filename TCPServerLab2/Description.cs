using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TCPServer.balanceModule;
using TCPServer.ProductionModule;

namespace TCPServer
{
    public partial class Description
    {
        public int Id { get; set; }
        public string Content { get; set; }

        // Существующие навигационные свойства
      
       
        public virtual ICollection<SupportTicket> SupportTickets { get; set; }
        public virtual ICollection<Asset> Assets { get; set; }
        public virtual ICollection<Liability> Liabilities { get; set; }
        public virtual ICollection<Equity> Equity { get; set; }
        public virtual ICollection<Operation> Operations { get; set; }
        public virtual ICollection<Product> Products { get; set; }


        public Description()
        {
          
            
            SupportTickets = new HashSet<SupportTicket>();
            Assets = new HashSet<Asset>();
            Liabilities = new HashSet<Liability>();
            Equity = new HashSet<Equity>();
            Operations = new HashSet<Operation>();
            Products = new HashSet<Product>();
        }
    }
}
