using System;

namespace TCPServer.balanceModule
{
    public class ExchangeRate
    {
        public int Id { get; set; }
        public string FromCurrency { get; set; } // Исходная валюта (например, USD)
        public string ToCurrency { get; set; } // Целевая валюта (например, RUB)
        public decimal Rate { get; set; } // Курс
        public DateTime Date { get; set; } // Дата курса
    }
}