using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Client.ManagerFolder.DataAnalysys.Models;
using OfficeOpenXml;

namespace Client.ManagerFolder.DataAnalysys
{
    public class ExcelImportService
    {
        public ExcelImportService()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial; // Для EPPlus
        }

        public IEnumerable<string> GetSheetNames(string filePath)
        {
            if (!File.Exists(filePath))
                throw new Exception("Файл не найден.");

            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var sheets = package.Workbook.Worksheets.Select(ws => ws.Name).ToList();
                if (!
sheets.Any())
                    throw new Exception("В файле нет листов.");
                return sheets;
            }
        }

        public IEnumerable<string> GetColumnHeaders(string filePath, string sheetName)
        {
            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var worksheet = package.Workbook.Worksheets[sheetName];
                if (worksheet == null)
                    throw new Exception($"Лист '{sheetName}' не найден.");

                if (worksheet.Dimension == null)
                    throw new Exception("Лист пуст или не содержит данных.");

                var headers = new List<string>();
                for (int col = 1; col <= worksheet.Dimension.Columns; col++)
                {
                    var header = worksheet.Cells[1, col].Text?.Trim();
                    if (!string.IsNullOrEmpty(header))
                        headers.Add(header);
                }

                if (!headers.Any())
                    throw new Exception("Заголовки столбцов не найдены в первой строке.");

                return headers;
            }
        }

        public List<Dictionary<string, string>> PreviewData(string filePath, string sheetName, List<ColumnMapping> mappings)
        {
            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var worksheet = package.Workbook.Worksheets[sheetName];
                if (worksheet == null)
                    throw new Exception($"Лист '{sheetName}' не найден.");

                if (worksheet.Dimension == null)
                    throw new Exception("Лист пуст.");

                var data = new List<Dictionary<string, string>>();
                var headerMap = mappings.ToDictionary(m => m.SelectedColumn, m => m.ExpectedColumn);
                var columnIndices = mappings.Where(m => !string.IsNullOrEmpty(m.SelectedColumn))
                                           .ToDictionary(m => m.ExpectedColumn, m =>
                                           {
                                               var cell = worksheet.Cells[1, 1, 1, worksheet.Dimension.Columns]
                                                                  .FirstOrDefault(c => c.Text == m.SelectedColumn);
                                               if (cell == null)
                                                   throw new Exception($"Столбец '{m.SelectedColumn}' не найден.");
                                               return cell.Start.Column;
                                           });

                for (int row = 2; row <= Math.Min(worksheet.Dimension.Rows, 101); row++)
                {
                    var rowData = new Dictionary<string, string>();
                    foreach (var mapping in mappings.Where(m => !string.IsNullOrEmpty(m.SelectedColumn)))
                    {
                        var value = worksheet.Cells[row, columnIndices[mapping.ExpectedColumn]].Text?.Trim();
                        rowData[mapping.ExpectedColumn] = value;
                    }
                    if (rowData.Any())
                        data.Add(rowData);
                }
                return data;
            }
        }

        public List<BalanceModel> ImportBalanceData(string filePath, string sheetName, List<ColumnMapping> mappings)
        {
            var rawData = PreviewData(filePath, sheetName, mappings);
            var data = new List<BalanceModel>();
            foreach (var row in rawData)
            {
                var model = new BalanceModel();
                if (row.TryGetValue("Период", out var period))
                    model.Period = period;
                if (row.TryGetValue("Активы", out var assets) && double.TryParse(assets, out var assetsValue))
                    model.Assets = assetsValue;
                if (row.TryGetValue("Собственный капитал", out var equity) && double.TryParse(equity, out var equityValue))
                    model.Equity = equityValue;
                if (row.TryGetValue("Заемный капитал", out var borrowed) && double.TryParse(borrowed, out var borrowedValue))
                    model.BorrowedCapital = borrowedValue;
                if (row.TryGetValue("Обязательства", out var liabilities) && double.TryParse(liabilities, out var liabilitiesValue))
                    model.Liabilities = liabilitiesValue;
                if (row.TryGetValue("Чистая прибыль", out var profit) && double.TryParse(profit, out var profitValue))
                    model.NetProfit = profitValue;

                if (!string.IsNullOrEmpty(model.Period) && model.Assets.HasValue && model.Equity.HasValue)
                    data.Add(model);
                else
                    throw new Exception("Обязательные поля (Период, Активы, Собственный капитал) отсутствуют или некорректны.");
            }
            return data;
        }

        public List<CapitalFlowModel> ImportCapitalFlowData(string filePath, string sheetName, List<ColumnMapping> mappings)
        {
            var rawData = PreviewData(filePath, sheetName, mappings);
            var data = new List<CapitalFlowModel>();
            foreach (var row in rawData)
            {
                var model = new CapitalFlowModel();
                if (row.TryGetValue("Период", out var period))
                    model.Period = period;
                if (row.TryGetValue("Собственный капитал (начало)", out var initial) && double.TryParse(initial, out var initialValue))
                    model.InitialEquity = initialValue;
                if (row.TryGetValue("Прирост капитала", out var increase) && double.TryParse(increase, out var increaseValue))
                    model.CapitalIncrease = increaseValue;
                if (row.TryGetValue("Убытки", out var losses) && double.TryParse(losses, out var lossesValue))
                    model.Losses = lossesValue;
                if (row.TryGetValue("Собственный капитал (конец)", out var final) && double.TryParse(final, out var finalValue))
                    model.FinalEquity = finalValue;

                if (!string.IsNullOrEmpty(model.Period) && model.FinalEquity.HasValue)
                    data.Add(model);
                else
                    throw new Exception("Обязательные поля (Период, Собственный капитал (конец)) отсутствуют или некорректны.");
            }
            return data;
        }

        public List<FinancialMetricsModel> ImportFinancialMetricsData(string filePath, string sheetName, List<ColumnMapping> mappings)
        {
            var rawData = PreviewData(filePath, sheetName, mappings);
            var data = new List<FinancialMetricsModel>();
            foreach (var row in rawData)
            {
                var model = new FinancialMetricsModel();
                if (row.TryGetValue("Период", out var period))
                    model.Period = period;
                if (row.TryGetValue("Рентабельность капитала (%)", out var roe) && double.TryParse(roe, out var roeValue))
                    model.ReturnOnEquity = roeValue;
                if (row.TryGetValue("Коэффициент ликвидности", out var liquidity) && double.TryParse(liquidity, out var liquidityValue))
                    model.LiquidityRatio = liquidityValue;
                if (row.TryGetValue("Доля заемного капитала (%)", out var share) && double.TryParse(share, out var shareValue))
                    model.BorrowedCapitalShare = shareValue;

                if (!string.IsNullOrEmpty(model.Period))
                    data.Add(model);
                else
                    throw new Exception("Обязательное поле (Период) отсутствует или некорректно.");
            }
            return data;
        }

        public void SaveToJson<T>(List<T> data, string fileName)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(data, options);
            File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName), json);
        }
    }
}