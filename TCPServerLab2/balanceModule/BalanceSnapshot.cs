namespace TCPServer.balanceModule
{
    public class BalanceSnapshot
    {
        public int Id { get; set; }
        public decimal TotalAssets { get; set; }
        public decimal TotalLiabilities { get; set; }
        public decimal Equity { get; set; }
        public DateTime Timestamp { get; set; }
        public int AuditLogId { get; set; } // Внешний ключ на AuditLog

        // Навигационное свойство
        public virtual AuditLog AuditLog { get; set; }
    }
}