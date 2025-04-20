using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Client.ManagerFolder.DataAnalysys
{
    public class DataService
    {
        private readonly string _balanceDataPath = "balance_data.json";
        private readonly string _capitalFlowDataPath = "capital_flow_data.json";
        private readonly string _financialMetricsDataPath = "financial_metrics_data.json";

        public List<BalanceModel> LoadBalanceData()
        {
            try
            {
                if (File.Exists(_balanceDataPath))
                {
                    string json = File.ReadAllText(_balanceDataPath);
                    var data = JsonSerializer.Deserialize<List<BalanceModel>>(json);
                    Console.WriteLine($"Loaded balance data: {json}");
                    return data ?? new List<BalanceModel>();
                }
                return new List<BalanceModel>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading balance data: {ex.Message}");
                return new List<BalanceModel>();
            }
        }

        public List<CapitalFlowModel> LoadCapitalFlowData()
        {
            try
            {
                if (File.Exists(_capitalFlowDataPath))
                {
                    string json = File.ReadAllText(_capitalFlowDataPath);
                    var data = JsonSerializer.Deserialize<List<CapitalFlowModel>>(json);
                   
                    return data ?? new List<CapitalFlowModel>();
                }
                return new List<CapitalFlowModel>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading capital flow data: {ex.Message}");
                return new List<CapitalFlowModel>();
            }
        }

        public List<FinancialMetricsModel> LoadFinancialMetricsData()
        {
            try
            {
                if (File.Exists(_financialMetricsDataPath))
                {
                    string json = File.ReadAllText(_financialMetricsDataPath);
                    var data = JsonSerializer.Deserialize<List<FinancialMetricsModel>>(json);
                    Console.WriteLine($"Loaded financial metrics data: {json}");
                    return data ?? new List<FinancialMetricsModel>();
                }
                return new List<FinancialMetricsModel>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading financial metrics data: {ex.Message}");
                return new List<FinancialMetricsModel>();
            }
        }
    }

    public class BalanceModel
    {
        public string Period { get; set; }
        public double? Assets { get; set; }
        public double? Equity { get; set; }
        public double? BorrowedCapital { get; set; }
        public double? Liabilities { get; set; }
        public double? NetProfit { get; set; }
    }

    public class CapitalFlowModel
    {
        public string Period { get; set; }
        public double InitialEquity { get; set; }
        public double CapitalIncrease { get; set; }
        public double Losses { get; set; }
        public double FinalEquity { get; set; }
    }

    public class FinancialMetricsModel
    {
        public string Period { get; set; }
        public double ReturnOnEquity { get; set; }
        public double LiquidityRatio { get; set; }
        public double BorrowedCapitalShare { get; set; }
    }
}