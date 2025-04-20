using System;
using System.Collections.Generic;
using System.Linq;

namespace Client.ManagerFolder.DataAnalysys
{
    public class AnalysisService
    {
        private readonly DataService _dataService;

        public AnalysisService()
        {
            _dataService = new DataService();
        }

        public List<FinancialAnalysisResult> CalculateFinancialMetrics()
        {
            var balanceData = _dataService.LoadBalanceData();
            var capitalFlowData = _dataService.LoadCapitalFlowData();
            var results = new List<FinancialAnalysisResult>();

            

            foreach (var balance in balanceData.OrderBy(b => b.Period))
            {
                var result = new FinancialAnalysisResult
                {
                    Period = balance.Period
                };

                if (balance.Equity.HasValue && balance.NetProfit.HasValue && balance.Equity.Value != 0)
                    result.ReturnOnEquity = (balance.NetProfit.Value / balance.Equity.Value) * 100;

                if (balance.Equity.HasValue && balance.BorrowedCapital.HasValue && balance.Equity.Value != 0)
                    result.DebtToEquityRatio = balance.BorrowedCapital.Value / balance.Equity.Value;

                if (balance.Equity.HasValue && balance.Assets.HasValue && balance.Assets.Value != 0)
                    result.AutonomyRatio = balance.Equity.Value / balance.Assets.Value;

                results.Add(result);
            }

            var capitalFlow = capitalFlowData.OrderBy(c => c.Period).ToList();
            for (int i = 0; i < capitalFlow.Count; i++)
            {
                var period = capitalFlow[i].Period;
                var existingResult = results.FirstOrDefault(r => r.Period == period) ?? new FinancialAnalysisResult { Period = period };
                if (!results.Contains(existingResult))
                    results.Add(existingResult);

                existingResult.EquityChange = capitalFlow[i].FinalEquity;
                if (i > 0)
                    existingResult.EquityChange = capitalFlow[i].FinalEquity - capitalFlow[i - 1].FinalEquity;
            }

            return results.OrderBy(r => r.Period).ToList();
        }

        public Dictionary<string, double> GetCapitalStructure(string period)
        {
            var balanceData = _dataService.LoadBalanceData();
            var balance = balanceData.FirstOrDefault(b => b.Period == period);
            var result = balance != null
                ? new Dictionary<string, double>
                {
                    { "Собственный капитал", balance.Equity ?? 0 },
                    { "Заемный капитал", balance.BorrowedCapital ?? 0 }
                }
                : new Dictionary<string, double>
                {
                    { "Собственный капитал", 0 },
                    { "Заемный капитал", 0 }
                };

          

            return result;
        }
    }

    public class FinancialAnalysisResult
    {
        public string Period { get; set; }
        public double? ReturnOnEquity { get; set; }
        public double? DebtToEquityRatio { get; set; }
        public double? AutonomyRatio { get; set; }
        public double? EquityChange { get; set; }
    }
}