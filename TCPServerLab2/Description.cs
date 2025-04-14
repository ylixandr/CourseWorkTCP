using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TCPServer.balanceModule;

namespace TCPServer
{
    public partial class Description
    {
        public int Id { get; set; }
        public string Content { get; set; }

        // Существующие навигационные свойства
        public virtual ICollection<ProductTransaction> ProductTransactions { get; set; }
        public virtual ICollection<Transaction> Transactions { get; set; }
        public virtual ICollection<StockAdjustmentRequest> StockAdjustmentRequests { get; set; }
        public virtual ICollection<Application> Applications { get; set; }
        public virtual ICollection<SupportTicket> SupportTickets { get; set; }

        // Новые навигационные свойства
        public virtual ICollection<Asset> Assets { get; set; }
        public virtual ICollection<Liability> Liabilities { get; set; }
        public virtual ICollection<Equity> Equity { get; set; }
        public virtual ICollection<Operation> Operations { get; set; }

        public Description()
        {
            ProductTransactions = new HashSet<ProductTransaction>();
            Transactions = new HashSet<Transaction>();
            StockAdjustmentRequests = new HashSet<StockAdjustmentRequest>();
            Applications = new HashSet<Application>();
            SupportTickets = new HashSet<SupportTicket>();
            Assets = new HashSet<Asset>();
            Liabilities = new HashSet<Liability>();
            Equity = new HashSet<Equity>();
            Operations = new HashSet<Operation>();
        }
    }
}
