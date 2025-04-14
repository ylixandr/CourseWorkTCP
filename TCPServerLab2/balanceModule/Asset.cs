using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCPServer.balanceModule
{
    public class Asset
    {
        public int Id { get; set; }
        public string Category { get; set; } // "Денежные средства", "Основные средства", "НМА", "ТМЦ", "Дебиторская задолженность"
        public string Name { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } // Например, "RUB", "USD" (для валютных счетов)
        public DateTime AcquisitionDate { get; set; } // Дата приобретения
        public decimal? DepreciationRate { get; set; } // Ставка амортизации (для ОС), в процентах в год

        // Навигационные свойства
        public int? DescriptionId { get; set; }
        public virtual Description Description { get; set; }
    }
}
