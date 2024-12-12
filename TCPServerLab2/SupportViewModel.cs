using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCPServer
{
    public class SupportViewModel
    {
        public int TicketId { get; set; }


        public DateTime SubmissionDate { get; set; }

        public string StatusName { get; set; }

        public string Description { get; set; } = null!;
    }
}
