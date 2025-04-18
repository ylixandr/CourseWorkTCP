using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using LiveCharts;
using LiveCharts.Wpf;
using OfficeOpenXml; // Добавляем для EPPlus
using System.IO;
using TCPServer;
using Client.Models;
using TCPServer.balanceModule;

namespace Client.ManagerFolder
{
    public partial class BalanceDashboard : Window
    {
        private List<AssetViewModel> AllAssets { get; set; }
        private List<LiabilityViewModel> AllLiabilities { get; set; }
        private List<AssetViewModel> FilteredAssets { get; set; }
        private List<LiabilityViewModel> FilteredLiabilities { get; set; }

        // Для графика
        public ChartValues<decimal> AssetsValues { get; set; } = new ChartValues<decimal>();
        public ChartValues<decimal> LiabilitiesValues { get; set; } = new ChartValues<decimal>();
        public ChartValues<decimal> EquityValues { get; set; } = new ChartValues<decimal>();
        public List<string> Labels { get; set; } = new List<string>();

        private readonly List<string> _currentAssetCategories = new List<string> { "Денежные средства", "Дебиторская задолженность" };
        private readonly List<string> _currentLiabilityCategories = new List<string> { "Краткосрочные кредиты", "Кредиторская задолженность" };
        public BalanceDashboard()
        {
            // Указываем лицензию EPPlus
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            InitializeComponent();
            InitializeChart();
            FilteredAssets = new List<AssetViewModel>();
            FilteredLiabilities = new List<LiabilityViewModel>();

            Loaded += async (s, e) =>
            {
                await LoadBalanceData();
            };
        }

        private void InitializeChart()
        {
            BalanceChart.DataContext = this;
        }
        private (decimal CurrentRatio, decimal QuickRatio, decimal ROA, decimal ROE, decimal DebtToEquity) CalculateFinancialRatios(BalanceData balanceData)
        {
            decimal totalAssets = balanceData.Assets.Total;
            decimal totalLiabilities = balanceData.Liabilities.Total;
            decimal equity = balanceData.Equity;

            var assetsCategories = balanceData.Assets.Categories ?? new List<CategoryData>();
            var liabilitiesCategories = balanceData.Liabilities.Categories ?? new List<CategoryData>();

            // Текущие активы: только "Cash", так как "AccountsReceivable" в данных нет
            decimal currentAssets = assetsCategories
                .Where(c => c.Category != null && c.Category == "Cash")
                .Sum(c => c.Total);

            // Быстрые активы: то же, что и текущие активы, так как используем только "Cash"
            decimal quickAssets = assetsCategories
                .Where(c => c.Category != null && c.Category == "Cash")
                .Sum(c => c.Total);

            // Текущие обязательства: "ShortTerm" и "AccountsPayable"
            decimal currentLiabilities = liabilitiesCategories
                .Where(c => c.Category != null && (c.Category == "ShortTerm" || c.Category == "AccountsPayable"))
                .Sum(c => c.Total);

            // Чистая прибыль: берём из нераспределённой прибыли в Equity
            decimal netProfit;
            using (var dbContext = new CrmsystemContext())
            {
                // Ищем запись в Equity с описанием "Нераспределённая прибыль"
                var undistributedProfit = dbContext.Equity
                    .Include(e => e.Description)
                    .Where(e => e.Description.Content.Contains("Нераспределённая прибыль"))
                    .Sum(e => e.Amount);

                netProfit = undistributedProfit; // Используем нераспределённую прибыль как чистую прибыль
            }

            decimal currentRatio = currentLiabilities != 0 ? currentAssets / currentLiabilities : 0;
            decimal quickRatio = currentLiabilities != 0 ? quickAssets / currentLiabilities : 0;
            decimal roa = totalAssets != 0 ? netProfit / totalAssets : 0;
            decimal roe = equity != 0 ? netProfit / equity : 0;
            decimal debtToEquity = equity != 0 ? totalLiabilities / equity : 0;

            return (currentRatio, quickRatio, roa, roe, debtToEquity);
        }
        private async void ExportToExcelButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Путь для сохранения файла на рабочем столе
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string filePath = Path.Combine(desktopPath, $"BalanceExport_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");

                // Создаём новый Excel-файл
                using (var package = new ExcelPackage(new FileInfo(filePath)))
                {
                    // Лист 1: Сводка
                    var summarySheet = package.Workbook.Worksheets.Add("Сводка");
                    summarySheet.Cells[1, 1].Value = "Сводка баланса";
                    summarySheet.Cells[1, 1, 1, 2].Merge = true;
                    summarySheet.Cells[1, 1].Style.Font.Bold = true;
                    summarySheet.Cells[1, 1].Style.Font.Size = 14;

                    summarySheet.Cells[2, 1].Value = "Активы";
                    summarySheet.Cells[2, 2].Value = decimal.Parse(TotalAssetsTextBlock.Text);
                    summarySheet.Cells[3, 1].Value = "Пассивы";
                    summarySheet.Cells[3, 2].Value = decimal.Parse(TotalLiabilitiesTextBlock.Text);
                    summarySheet.Cells[4, 1].Value = "Собственный капитал";
                    summarySheet.Cells[4, 2].Value = decimal.Parse(EquityTextBlock.Text);
                    summarySheet.Cells[5, 1].Value = "Проверка баланса";
                    summarySheet.Cells[5, 2].Value = decimal.Parse(BalanceCheckTextBlock.Text);

                    // Форматирование
                    summarySheet.Cells[2, 1, 5, 1].Style.Font.Bold = true;
                    summarySheet.Cells[2, 2, 5, 2].Style.Numberformat.Format = "#,##0.00";
                    summarySheet.Cells[2, 1, 5, 2].AutoFitColumns();

                    // Лист 2: Активы
                    var assetsSheet = package.Workbook.Worksheets.Add("Активы");
                    assetsSheet.Cells[1, 1].Value = "Список активов";
                    assetsSheet.Cells[1, 1, 1, 7].Merge = true;
                    assetsSheet.Cells[1, 1].Style.Font.Bold = true;
                    assetsSheet.Cells[1, 1].Style.Font.Size = 14;

                    // Заголовки
                    assetsSheet.Cells[2, 1].Value = "Id";
                    assetsSheet.Cells[2, 2].Value = "Категория";
                    assetsSheet.Cells[2, 3].Value = "Название";
                    assetsSheet.Cells[2, 4].Value = "Сумма";
                    assetsSheet.Cells[2, 5].Value = "Валюта";
                    assetsSheet.Cells[2, 6].Value = "Дата";
                    assetsSheet.Cells[2, 7].Value = "Описание";
                    assetsSheet.Cells[2, 1, 2, 7].Style.Font.Bold = true;

                    // Данные
                    for (int i = 0; i < AllAssets.Count; i++)
                    {
                        var asset = AllAssets[i];
                        assetsSheet.Cells[i + 3, 1].Value = asset.Id;
                        assetsSheet.Cells[i + 3, 2].Value = asset.Category;
                        assetsSheet.Cells[i + 3, 3].Value = asset.Name;
                        assetsSheet.Cells[i + 3, 4].Value = asset.Amount;
                        assetsSheet.Cells[i + 3, 4].Style.Numberformat.Format = "#,##0.00";
                        assetsSheet.Cells[i + 3, 5].Value = asset.Currency;
                        assetsSheet.Cells[i + 3, 6].Value = asset.AcquisitionDate.ToString("yyyy-MM-dd");
                        assetsSheet.Cells[i + 3, 7].Value = asset.Description;
                    }
                    assetsSheet.Cells[2, 1, AllAssets.Count + 2, 7].AutoFitColumns();

                    // Лист 3: Обязательства
                    var liabilitiesSheet = package.Workbook.Worksheets.Add("Обязательства");
                    liabilitiesSheet.Cells[1, 1].Value = "Список обязательств";
                    liabilitiesSheet.Cells[1, 1, 1, 6].Merge = true;
                    liabilitiesSheet.Cells[1, 1].Style.Font.Bold = true;
                    liabilitiesSheet.Cells[1, 1].Style.Font.Size = 14;

                    // Заголовки
                    liabilitiesSheet.Cells[2, 1].Value = "Id";
                    liabilitiesSheet.Cells[2, 2].Value = "Категория";
                    liabilitiesSheet.Cells[2, 3].Value = "Название";
                    liabilitiesSheet.Cells[2, 4].Value = "Сумма";
                    liabilitiesSheet.Cells[2, 5].Value = "Дата";
                    liabilitiesSheet.Cells[2, 6].Value = "Описание";
                    liabilitiesSheet.Cells[2, 1, 2, 6].Style.Font.Bold = true;

                    // Данные
                    for (int i = 0; i < AllLiabilities.Count; i++)
                    {
                        var liability = AllLiabilities[i];
                        liabilitiesSheet.Cells[i + 3, 1].Value = liability.Id;
                        liabilitiesSheet.Cells[i + 3, 2].Value = liability.Category;
                        liabilitiesSheet.Cells[i + 3, 3].Value = liability.Name;
                        liabilitiesSheet.Cells[i + 3, 4].Value = liability.Amount;
                        liabilitiesSheet.Cells[i + 3, 4].Style.Numberformat.Format = "#,##0.00";
                        liabilitiesSheet.Cells[i + 3, 5].Value = liability.DueDate.ToString("yyyy-MM-dd");
                        liabilitiesSheet.Cells[i + 3, 6].Value = liability.Description;
                    }
                    liabilitiesSheet.Cells[2, 1, AllLiabilities.Count + 2, 6].AutoFitColumns();

                    // Сохраняем файл
                    await package.SaveAsync();
                }

                MessageBox.Show($"Данные успешно экспортированы в {filePath}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Остальной код остаётся без изменений
        private async void ShowPeriodButton_Click(object sender, RoutedEventArgs e)
        {
            if (StartDatePicker.SelectedDate == null || EndDatePicker.SelectedDate == null)
            {
                MessageBox.Show("Выберите даты начала и конца периода!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DateTime startDate = StartDatePicker.SelectedDate.Value;
            DateTime endDate = EndDatePicker.SelectedDate.Value;
            string interval = ((ComboBoxItem)ChartIntervalComboBox.SelectedItem).Content.ToString();

            try
            {
                using (var dbContext = new CrmsystemContext())
                {
                    var snapshots = dbContext.BalanceSnapshots
                        .Where(s => s.Timestamp >= startDate && s.Timestamp <= endDate)
                        .OrderBy(s => s.Timestamp)
                        .ToList();

                    AssetsValues.Clear();
                    LiabilitiesValues.Clear();
                    EquityValues.Clear();
                    Labels.Clear();

                    if (snapshots.Any())
                    {
                        if (interval == "Месяцы")
                        {
                            var groupedData = snapshots
                                .GroupBy(s => new { s.Timestamp.Year, s.Timestamp.Month })
                                .Select(g => new
                                {
                                    Date = new DateTime(g.Key.Year, g.Key.Month, 1),
                                    Assets = g.Average(s => s.TotalAssets),
                                    Liabilities = g.Average(s => s.TotalLiabilities),
                                    Equity = g.Average(s => s.Equity)
                                })
                                .OrderBy(g => g.Date);

                            foreach (var data in groupedData)
                            {
                                AssetsValues.Add(data.Assets);
                                LiabilitiesValues.Add(data.Liabilities);
                                EquityValues.Add(data.Equity);
                                Labels.Add(data.Date.ToString("MMM yyyy"));
                            }
                        }
                        else if (interval == "Кварталы")
                        {
                            var groupedData = snapshots
                                .GroupBy(s => new { s.Timestamp.Year, Quarter = (s.Timestamp.Month - 1) / 3 + 1 })
                                .Select(g => new
                                {
                                    Year = g.Key.Year, // Сохраняем Year
                                    Quarter = g.Key.Quarter, // Сохраняем Quarter
                                    Date = new DateTime(g.Key.Year, (g.Key.Quarter - 1) * 3 + 1, 1),
                                    Assets = g.Average(s => s.TotalAssets),
                                    Liabilities = g.Average(s => s.TotalLiabilities),
                                    Equity = g.Average(s => s.Equity)
                                })
                                .OrderBy(g => g.Date);

                            foreach (var data in groupedData)
                            {
                                AssetsValues.Add(data.Assets);
                                LiabilitiesValues.Add(data.Liabilities);
                                EquityValues.Add(data.Equity);
                                Labels.Add($"Q{data.Quarter} {data.Year}"); // Используем data.Quarter и data.Year
                            }
                        }
                        else if (interval == "Годы")
                        {
                            var groupedData = snapshots
                                .GroupBy(s => new { s.Timestamp.Year })
                                .Select(g => new
                                {
                                    Year = g.Key.Year, // Сохраняем Year
                                    Date = new DateTime(g.Key.Year, 1, 1),
                                    Assets = g.Average(s => s.TotalAssets),
                                    Liabilities = g.Average(s => s.TotalLiabilities),
                                    Equity = g.Average(s => s.Equity)
                                })
                                .OrderBy(g => g.Date);

                            foreach (var data in groupedData)
                            {
                                AssetsValues.Add(data.Assets);
                                LiabilitiesValues.Add(data.Liabilities);
                                EquityValues.Add(data.Equity);
                                Labels.Add(data.Year.ToString()); // Используем data.Year
                            }
                        }

                        // Обновляем прогноз
                        var (forecastAssets, forecastLiabilities, forecastEquity) = ForecastBalance(startDate, endDate, interval);
                        ForecastAssetsTextBlock.Text = forecastAssets.ToString("F2");
                        ForecastLiabilitiesTextBlock.Text = forecastLiabilities.ToString("F2");
                        ForecastEquityTextBlock.Text = forecastEquity.ToString("F2");
                    }
                    else
                    {
                        MessageBox.Show("Нет данных за выбранный период.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ComparePeriodsButton_Click(object sender, RoutedEventArgs e)
        {
            if (Period1StartPicker.SelectedDate == null || Period1EndPicker.SelectedDate == null ||
                Period2StartPicker.SelectedDate == null || Period2EndPicker.SelectedDate == null)
            {
                MessageBox.Show("Выберите даты для обоих периодов!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DateTime period1Start = Period1StartPicker.SelectedDate.Value;
            DateTime period1End = Period1EndPicker.SelectedDate.Value;
            DateTime period2Start = Period2StartPicker.SelectedDate.Value;
            DateTime period2End = Period2EndPicker.SelectedDate.Value;

            try
            {
                using (var dbContext = new CrmsystemContext())
                {
                    // Период 1
                    var period1Snapshots = dbContext.BalanceSnapshots
                        .Where(s => s.Timestamp >= period1Start && s.Timestamp <= period1End)
                        .ToList();

                    if (period1Snapshots.Any())
                    {
                        Period1AssetsTextBlock.Text = period1Snapshots.Average(s => s.TotalAssets).ToString("F2");
                        Period1LiabilitiesTextBlock.Text = period1Snapshots.Average(s => s.TotalLiabilities).ToString("F2");
                        Period1EquityTextBlock.Text = period1Snapshots.Average(s => s.Equity).ToString("F2");
                    }
                    else
                    {
                        Period1AssetsTextBlock.Text = "0";
                        Period1LiabilitiesTextBlock.Text = "0";
                        Period1EquityTextBlock.Text = "0";
                    }

                    // Период 2
                    var period2Snapshots = dbContext.BalanceSnapshots
                        .Where(s => s.Timestamp >= period2Start && s.Timestamp <= period2End)
                        .ToList();

                    if (period2Snapshots.Any())
                    {
                        Period2AssetsTextBlock.Text = period2Snapshots.Average(s => s.TotalAssets).ToString("F2");
                        Period2LiabilitiesTextBlock.Text = period2Snapshots.Average(s => s.TotalLiabilities).ToString("F2");
                        Period2EquityTextBlock.Text = period2Snapshots.Average(s => s.Equity).ToString("F2");
                    }
                    else
                    {
                        Period2AssetsTextBlock.Text = "0";
                        Period2LiabilitiesTextBlock.Text = "0";
                        Period2EquityTextBlock.Text = "0";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сравнении периодов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task UpdateChartAsync()
        {
            using (var dbContext = new CrmsystemContext())
            {
                var endDate = DateTime.Now;
                var startDate = endDate.AddMonths(-5);

                AssetsValues ??= new ChartValues<decimal>();
                LiabilitiesValues ??= new ChartValues<decimal>();
                EquityValues ??= new ChartValues<decimal>();
                Labels ??= new List<string>();

                AssetsValues.Clear();
                LiabilitiesValues.Clear();
                EquityValues.Clear();
                Labels.Clear();

                string interval = ChartIntervalComboBox.SelectedItem != null
                    ? ((ComboBoxItem)ChartIntervalComboBox.SelectedItem).Content.ToString()
                    : "Месяцы";

                if (interval == "Месяцы")
                {
                    for (var date = startDate; date <= endDate; date = date.AddMonths(1))
                    {
                        var assets = await dbContext.Assets
                            .Where(a => a.AcquisitionDate >= startDate && a.AcquisitionDate <= date)
                            .ToListAsync();
                        var liabilities = await dbContext.Liabilities
                            .Where(l => l.DueDate >= startDate && l.DueDate <= date)
                            .ToListAsync();

                        decimal totalAssets = assets.Sum(a => a.Amount);
                        decimal totalLiabilities = liabilities.Sum(l => l.Amount);
                        decimal equity = totalAssets - totalLiabilities;

                        AssetsValues.Add(totalAssets);
                        LiabilitiesValues.Add(totalLiabilities);
                        EquityValues.Add(equity);
                        Labels.Add(date.ToString("MMM yyyy"));
                    }
                }
                else if (interval == "Кварталы")
                {
                    for (var date = startDate; date <= endDate; date = date.AddMonths(3))
                    {
                        var assets = await dbContext.Assets
                            .Where(a => a.AcquisitionDate >= startDate && a.AcquisitionDate <= date)
                            .ToListAsync();
                        var liabilities = await dbContext.Liabilities
                            .Where(l => l.DueDate >= startDate && l.DueDate <= date)
                            .ToListAsync();

                        decimal totalAssets = assets.Sum(a => a.Amount);
                        decimal totalLiabilities = liabilities.Sum(l => l.Amount);
                        decimal equity = totalAssets - totalLiabilities;

                        AssetsValues.Add(totalAssets);
                        LiabilitiesValues.Add(totalLiabilities);
                        EquityValues.Add(equity);
                        Labels.Add($"Q{(date.Month - 1) / 3 + 1} {date.Year}");
                    }
                }
                else if (interval == "Годы")
                {
                    for (var date = startDate; date <= endDate; date = date.AddYears(1))
                    {
                        var assets = await dbContext.Assets
                            .Where(a => a.AcquisitionDate >= startDate && a.AcquisitionDate <= date)
                            .ToListAsync();
                        var liabilities = await dbContext.Liabilities
                            .Where(l => l.DueDate >= startDate && l.DueDate <= date)
                            .ToListAsync();

                        decimal totalAssets = assets.Sum(a => a.Amount);
                        decimal totalLiabilities = liabilities.Sum(l => l.Amount);
                        decimal equity = totalAssets - totalLiabilities;

                        AssetsValues.Add(totalAssets);
                        LiabilitiesValues.Add(totalLiabilities);
                        EquityValues.Add(equity);
                        Labels.Add(date.ToString("yyyy"));
                    }
                }

                BalanceChart.Update(true);
            }
        }

        private async void ChartIntervalComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await UpdateChartAsync();
        }

        private async Task LoadBalanceData(string command = "getBalanceSnapshot")
        {
            try
            {
                string response = await NetworkService.Instance.SendMessageAsync(command);

                if (response.StartsWith("Error"))
                {
                    MessageBox.Show($"Ошибка: {response}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Десериализуем в тип BalanceData вместо dynamic
                var balanceData = JsonConvert.DeserializeObject<BalanceData>(response);

                if (balanceData == null)
                {
                    MessageBox.Show("Не удалось загрузить данные баланса.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Проверки на null уже не нужны, так как свойства класса имеют значения по умолчанию
                TotalAssetsTextBlock.Text = balanceData.Assets.Total.ToString("F2");
                TotalLiabilitiesTextBlock.Text = balanceData.Liabilities.Total.ToString("F2");
                EquityTextBlock.Text = balanceData.Equity.ToString("F2");
                BalanceCheckTextBlock.Text = balanceData.BalanceCheck.ToString("F2");

                AssetsListView.ItemsSource = balanceData.Assets.Categories ?? new List<CategoryData>();
                LiabilitiesListView.ItemsSource = balanceData.Liabilities.Categories ?? new List<CategoryData>();

                var ratios = CalculateFinancialRatios(balanceData);
                CurrentRatioTextBlock.Text = ratios.CurrentRatio.ToString("F2");
                QuickRatioTextBlock.Text = ratios.QuickRatio.ToString("F2");
                ROATextBlock.Text = ratios.ROA.ToString("F2");
                ROETextBlock.Text = ratios.ROE.ToString("F2");
                DebtToEquityTextBlock.Text = ratios.DebtToEquity.ToString("F2");

                await LoadAllAssets();
                await LoadAllLiabilities();
                await LoadAuditLogs();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async Task LoadAuditLogs()
        {
            try
            {
                using (var dbContext = new CrmsystemContext())
                {
                    var logs = await dbContext.AuditLogs
                        .OrderByDescending(l => l.Timestamp)
                        .ToListAsync();
                    if (!logs.Any())
                    {
                        MessageBox.Show("Логи аудита отсутствуют в базе данных.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    AuditLogsListView.ItemsSource = logs;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке логов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async Task LoadAllAssets()
        {
            try
            {
                string response = await NetworkService.Instance.SendMessageAsync("getAllAssets");

                if (response.StartsWith("Error"))
                {
                    MessageBox.Show($"Ошибка: {response}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var assets = JsonConvert.DeserializeObject<List<AssetViewModel>>(response) ?? new List<AssetViewModel>();

                if (!assets.Any())
                {
                    MessageBox.Show("Список активов пуст.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                AllAssets = assets.Select(a => new AssetViewModel
                {
                    Id = a.Id,
                    Category = a.Category ?? "Не указано",
                    Name = a.Name ?? "Без названия",
                    Amount = a.Amount,
                    Currency = a.Currency ?? "USD",
                    AcquisitionDate = a.AcquisitionDate,
                    DepreciationRate = a.DepreciationRate,
                    Description = a.Description ?? ""
                }).ToList();

                ApplyAssetFilters(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке активов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadAllLiabilities()
        {
            try
            {
                string response = await NetworkService.Instance.SendMessageAsync("getAllLiabilities");

                if (response.StartsWith("Error"))
                {
                    MessageBox.Show($"Ошибка: {response}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var liabilities = JsonConvert.DeserializeObject<List<LiabilityViewModel>>(response) ?? new List<LiabilityViewModel>();

                if (!liabilities.Any())
                {
                    MessageBox.Show("Список обязательств пуст.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                AllLiabilities = liabilities.Select(l => new LiabilityViewModel
                {
                    Id = l.Id,
                    Category = l.Category ?? "Не указано",
                    Name = l.Name ?? "Без названия",
                    Amount = l.Amount,
                    DueDate = l.DueDate,
                    Description = l.Description ?? ""
                }).ToList();

                ApplyLiabilityFilters(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке обязательств: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private (decimal forecastAssets, decimal forecastLiabilities, decimal forecastEquity) ForecastBalance(DateTime startDate, DateTime endDate, string interval, int periods = 3)
        {
            using (var dbContext = new CrmsystemContext())
            {
                // Фильтруем снимки по заданному периоду
                var snapshots = dbContext.BalanceSnapshots
                    .Where(s => s.Timestamp >= startDate && s.Timestamp <= endDate)
                    .OrderBy(s => s.Timestamp)
                    .ToList();

                if (snapshots.Count < 2)
                {
                    return (decimal.Parse(TotalAssetsTextBlock.Text),
                            decimal.Parse(TotalLiabilitiesTextBlock.Text),
                            decimal.Parse(EquityTextBlock.Text));
                }

                // Группируем снимки по интервалу
                var groupedSnapshots = new List<BalanceSnapshot>();
                if (interval == "Месяцы")
                {
                    groupedSnapshots = snapshots
                        .GroupBy(s => new { s.Timestamp.Year, s.Timestamp.Month })
                        .Select(g => new BalanceSnapshot
                        {
                            TotalAssets = g.Average(s => s.TotalAssets),
                            TotalLiabilities = g.Average(s => s.TotalLiabilities),
                            Equity = g.Average(s => s.Equity),
                            Timestamp = new DateTime(g.Key.Year, g.Key.Month, 1)
                        })
                        .OrderBy(s => s.Timestamp)
                        .ToList();
                }
                else if (interval == "Кварталы")
                {
                    groupedSnapshots = snapshots
                        .GroupBy(s => new { s.Timestamp.Year, Quarter = (s.Timestamp.Month - 1) / 3 + 1 })
                        .Select(g => new BalanceSnapshot
                        {
                            TotalAssets = g.Average(s => s.TotalAssets),
                            TotalLiabilities = g.Average(s => s.TotalLiabilities),
                            Equity = g.Average(s => s.Equity),
                            Timestamp = new DateTime(g.Key.Year, (g.Key.Quarter - 1) * 3 + 1, 1)
                        })
                        .OrderBy(s => s.Timestamp)
                        .ToList();
                }
                else if (interval == "Годы")
                {
                    groupedSnapshots = snapshots
                        .GroupBy(s => new { s.Timestamp.Year })
                        .Select(g => new BalanceSnapshot
                        {
                            TotalAssets = g.Average(s => s.TotalAssets),
                            TotalLiabilities = g.Average(s => s.TotalLiabilities),
                            Equity = g.Average(s => s.Equity),
                            Timestamp = new DateTime(g.Key.Year, 1, 1)
                        })
                        .OrderBy(s => s.Timestamp)
                        .ToList();
                }

                if (groupedSnapshots.Count < 2)
                {
                    return (snapshots.Last().TotalAssets,
                            snapshots.Last().TotalLiabilities,
                            snapshots.Last().Equity);
                }

                // Рассчитываем средние изменения
                decimal assetsChange = 0, liabilitiesChange = 0, equityChange = 0;
                for (int i = 1; i < groupedSnapshots.Count; i++)
                {
                    assetsChange += (groupedSnapshots[i].TotalAssets - groupedSnapshots[i - 1].TotalAssets) / (groupedSnapshots.Count - 1);
                    liabilitiesChange += (groupedSnapshots[i].TotalLiabilities - groupedSnapshots[i - 1].TotalLiabilities) / (groupedSnapshots.Count - 1);
                    equityChange += (groupedSnapshots[i].Equity - groupedSnapshots[i - 1].Equity) / (groupedSnapshots.Count - 1);
                }

                decimal currentAssets = groupedSnapshots.Last().TotalAssets;
                decimal currentLiabilities = groupedSnapshots.Last().TotalLiabilities;
                decimal currentEquity = groupedSnapshots.Last().Equity;

                decimal forecastAssets = currentAssets + assetsChange * periods;
                decimal forecastLiabilities = currentLiabilities + liabilitiesChange * periods;
                decimal forecastEquity = currentEquity + equityChange * periods;

                return (forecastAssets, forecastLiabilities, forecastEquity);
            }
        }
        private void ApplyAssetFilters(object sender, EventArgs e)
        {
            try
            {
                var categoryFilter = AssetFilterCategoryComboBox.SelectedItem != null
                    ? ((ComboBoxItem)AssetFilterCategoryComboBox.SelectedItem).Content.ToString()
                    : "Все";
                var searchText = AssetSearchTextBox.Text?.ToLower() ?? "";

                // Проверяем, что AllAssets инициализирован
                if (AllAssets == null)
                {
                    FilteredAssets = new List<AssetViewModel>();
                    AllAssetsListView.ItemsSource = FilteredAssets;
                    return;
                }

                var filtered = AllAssets.AsEnumerable();

                if (categoryFilter != "Все")
                {
                    filtered = filtered.Where(a => a.Category != null && a.Category == categoryFilter);
                }

                if (!string.IsNullOrEmpty(searchText))
                {
                    filtered = filtered.Where(a =>
                        (a.Name != null && a.Name.ToLower().Contains(searchText)) ||
                        (a.Description != null && a.Description.ToLower().Contains(searchText)));
                }

                FilteredAssets = filtered.ToList();
                AllAssetsListView.ItemsSource = FilteredAssets;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при фильтрации активов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void ApplyLiabilityFilters(object sender, EventArgs e)
        {
            try
            {
                var categoryFilter = LiabilityFilterCategoryComboBox.SelectedItem != null
                    ? ((ComboBoxItem)LiabilityFilterCategoryComboBox.SelectedItem).Content.ToString()
                    : "Все";
                var searchText = LiabilitySearchTextBox.Text?.ToLower() ?? "";

                // Проверяем, что AllLiabilities инициализирован
                if (AllLiabilities == null)
                {
                    FilteredLiabilities = new List<LiabilityViewModel>();
                    AllLiabilitiesListView.ItemsSource = FilteredLiabilities;
                    return;
                }

                var filtered = AllLiabilities.AsEnumerable();

                if (categoryFilter != "Все")
                {
                    filtered = filtered.Where(l => l.Category != null && l.Category == categoryFilter);
                }

                if (!string.IsNullOrEmpty(searchText))
                {
                    filtered = filtered.Where(l =>
                        (l.Name != null && l.Name.ToLower().Contains(searchText)) ||
                        (l.Description != null && l.Description.ToLower().Contains(searchText)));
                }

                FilteredLiabilities = filtered.ToList();
                AllLiabilitiesListView.ItemsSource = FilteredLiabilities;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при фильтрации обязательств: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void AddAssetButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (AssetCategoryComboBox.SelectedItem == null ||
                    string.IsNullOrEmpty(AssetNameTextBox.Text) ||
                    string.IsNullOrEmpty(AssetAmountTextBox.Text) ||
                    AssetAcquisitionDatePicker.SelectedDate == null)
                {
                    MessageBox.Show("Заполните все обязательные поля!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                decimal amount;
                if (!decimal.TryParse(AssetAmountTextBox.Text, out amount))
                {
                    MessageBox.Show("Сумма должна быть числом!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                decimal? depreciationRate = null;
                if (!string.IsNullOrEmpty(AssetDepreciationRateTextBox.Text))
                {
                    if (!decimal.TryParse(AssetDepreciationRateTextBox.Text, out decimal rate))
                    {
                        MessageBox.Show("Ставка амортизации должна быть числом!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    depreciationRate = rate;
                }

                var assetData = new
                {
                    Category = ((ComboBoxItem)AssetCategoryComboBox.SelectedItem).Content.ToString(),
                    Name = AssetNameTextBox.Text,
                    Amount = amount,
                    Currency = AssetCurrencyTextBox.Text,
                    AcquisitionDate = AssetAcquisitionDatePicker.SelectedDate.Value.ToString("yyyy-MM-dd"),
                    DepreciationRate = depreciationRate,
                    Description = AssetDescriptionTextBox.Text
                };

                string jsonData = JsonConvert.SerializeObject(assetData);
                string command = $"addAsset{jsonData}";
                string response = await NetworkService.Instance.SendMessageAsync(command);

                var result = JsonConvert.DeserializeObject<dynamic>(response);
                if (result.Success == true)
                {
                    // Логирование и сохранение снимка баланса
                    using (var dbContext = new CrmsystemContext())
                    {
                        var auditLog = new AuditLog
                        {
                            UserName = "Manager",
                            Action = "Создание",
                            EntityType = "Asset",
                            EntityId = (int)result.AssetId,
                            Details = $"Добавлен актив: {assetData.Name}, Сумма: {assetData.Amount}",
                        };

                        dbContext.AuditLogs.Add(auditLog);
                        await dbContext.SaveChangesAsync(); // Сохраняем AuditLog, чтобы получить Id

                        await LoadBalanceData(); // Обновляем данные баланса

                        // Проверяем корректность данных перед парсингом
                        decimal totalAssets, totalLiabilities, equity;
                        if (!decimal.TryParse(TotalAssetsTextBlock.Text, out totalAssets))
                        {
                            totalAssets = 0;
                            MessageBox.Show("Не удалось определить общие активы. Установлено значение по умолчанию: 0.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                        if (!decimal.TryParse(TotalLiabilitiesTextBlock.Text, out totalLiabilities))
                        {
                            totalLiabilities = 0;
                            MessageBox.Show("Не удалось определить общие обязательства. Установлено значение по умолчанию: 0.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                        if (!decimal.TryParse(EquityTextBlock.Text, out equity))
                        {
                            equity = 0;
                            MessageBox.Show("Не удалось определить капитал. Установлено значение по умолчанию: 0.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }

                        var snapshot = new BalanceSnapshot
                        {
                            TotalAssets = totalAssets,
                            TotalLiabilities = totalLiabilities,
                            Equity = equity,
                            AuditLogId = auditLog.Id, // Связываем с AuditLog
                            Timestamp = DateTime.Now
                        };

                        dbContext.BalanceSnapshots.Add(snapshot);
                        await dbContext.SaveChangesAsync();
                    }

                    MessageBox.Show($"Актив успешно добавлен! Id: {result.AssetId}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    await UpdateChartAsync();
                }
                else
                {
                    MessageBox.Show($"Ошибка: {result.Error}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении актива: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void AllAssetsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AllAssetsListView.SelectedItem != null)
            {
                var selectedAsset = (AssetViewModel)AllAssetsListView.SelectedItem;
                AssetCategoryComboBox.SelectedItem = AssetCategoryComboBox.Items.Cast<ComboBoxItem>()
                    .FirstOrDefault(item => item.Content.ToString() == selectedAsset.Category);
                AssetNameTextBox.Text = selectedAsset.Name;
                AssetAmountTextBox.Text = selectedAsset.Amount.ToString();
                AssetCurrencyTextBox.Text = selectedAsset.Currency;
                AssetAcquisitionDatePicker.SelectedDate = selectedAsset.AcquisitionDate;
                AssetDepreciationRateTextBox.Text = selectedAsset.DepreciationRate;
                AssetDescriptionTextBox.Text = selectedAsset.Description;
            }
        }

        private async void UpdateAssetButton_Click(object sender, RoutedEventArgs e)
        {
            if (AllAssetsListView.SelectedItem == null)
            {
                MessageBox.Show("Выберите актив для редактирования!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                decimal amount;
                if (!decimal.TryParse(AssetAmountTextBox.Text, out amount))
                {
                    MessageBox.Show("Сумма должна быть числом!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                decimal? depreciationRate = null;
                if (!string.IsNullOrEmpty(AssetDepreciationRateTextBox.Text))
                {
                    if (!decimal.TryParse(AssetDepreciationRateTextBox.Text, out decimal rate))
                    {
                        MessageBox.Show("Ставка амортизации должна быть числом!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    depreciationRate = rate;
                }

                var selectedAsset = (AssetViewModel)AllAssetsListView.SelectedItem;
                var assetData = new
                {
                    Id = selectedAsset.Id,
                    Category = ((ComboBoxItem)AssetCategoryComboBox.SelectedItem).Content.ToString(),
                    Name = AssetNameTextBox.Text,
                    Amount = amount,
                    Currency = AssetCurrencyTextBox.Text,
                    AcquisitionDate = AssetAcquisitionDatePicker.SelectedDate.Value.ToString("yyyy-MM-dd"),
                    DepreciationRate = depreciationRate,
                    Description = AssetDescriptionTextBox.Text
                };

                string jsonData = JsonConvert.SerializeObject(assetData);
                string command = $"updateAsset{jsonData}";
                string response = await NetworkService.Instance.SendMessageAsync(command);

                var result = JsonConvert.DeserializeObject<dynamic>(response);
                if (result.Success == true)
                {
                    // Логирование и сохранение снимка баланса
                    using (var dbContext = new CrmsystemContext())
                    {
                        var auditLog = new AuditLog
                        {
                            UserName = "User1",
                            Action = "Обновление",
                            EntityType = "Asset",
                            EntityId = selectedAsset.Id,
                            Details = $"Обновлён актив: {assetData.Name}, Новая сумма: {assetData.Amount}",
                        };

                        dbContext.AuditLogs.Add(auditLog);
                        await dbContext.SaveChangesAsync(); // Сохраняем AuditLog, чтобы получить Id

                        await LoadBalanceData(); // Обновляем данные баланса

                        var snapshot = new BalanceSnapshot
                        {
                            TotalAssets = decimal.Parse(TotalAssetsTextBlock.Text),
                            TotalLiabilities = decimal.Parse(TotalLiabilitiesTextBlock.Text),
                            Equity = decimal.Parse(EquityTextBlock.Text),
                            AuditLogId = auditLog.Id // Связываем с AuditLog
                        };

                        dbContext.BalanceSnapshots.Add(snapshot);
                        await dbContext.SaveChangesAsync();
                    }

                    MessageBox.Show("Актив успешно обновлён!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    await UpdateChartAsync();
                }
                else
                {
                    MessageBox.Show($"Ошибка: {result.Error}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении актива: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void DeleteAssetButton_Click(object sender, RoutedEventArgs e)
        {
            if (AllAssetsListView.SelectedItem == null)
            {
                MessageBox.Show("Выберите актив для удаления!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var selectedAsset = (AssetViewModel)AllAssetsListView.SelectedItem;
                var assetData = new { Id = selectedAsset.Id };
                string jsonData = JsonConvert.SerializeObject(assetData);
                string command = $"deleteAsset{jsonData}";
                string response = await NetworkService.Instance.SendMessageAsync(command);

                var result = JsonConvert.DeserializeObject<dynamic>(response);
                if (result.Success == true)
                {
                    // Логирование и сохранение снимка баланса
                    using (var dbContext = new CrmsystemContext())
                    {
                        var auditLog = new AuditLog
                        {
                            UserName = "User1",
                            Action = "Удаление",
                            EntityType = "Asset",
                            EntityId = selectedAsset.Id,
                            Details = $"Удалён актив: {selectedAsset.Name}",
                        };

                        dbContext.AuditLogs.Add(auditLog);
                        await dbContext.SaveChangesAsync(); // Сохраняем AuditLog, чтобы получить Id

                        await LoadBalanceData(); // Обновляем данные баланса

                        var snapshot = new BalanceSnapshot
                        {
                            TotalAssets = decimal.Parse(TotalAssetsTextBlock.Text),
                            TotalLiabilities = decimal.Parse(TotalLiabilitiesTextBlock.Text),
                            Equity = decimal.Parse(EquityTextBlock.Text),
                            AuditLogId = auditLog.Id, // Связываем с AuditLog
                            Timestamp = DateTime.Now
                        };

                        dbContext.BalanceSnapshots.Add(snapshot);
                        await dbContext.SaveChangesAsync();
                    }

                    MessageBox.Show("Актив успешно удалён!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    await UpdateChartAsync();
                }
                else
                {
                    MessageBox.Show($"Ошибка: {result.Error}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении актива: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async void AddLiabilityButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (LiabilityCategoryComboBox.SelectedItem == null ||
                    string.IsNullOrEmpty(LiabilityNameTextBox.Text) ||
                    string.IsNullOrEmpty(LiabilityAmountTextBox.Text) ||
                    LiabilityDueDatePicker.SelectedDate == null)
                {
                    MessageBox.Show("Заполните все обязательные поля!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                decimal amount;
                if (!decimal.TryParse(LiabilityAmountTextBox.Text, out amount))
                {
                    MessageBox.Show("Сумма должна быть числом!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var liabilityData = new
                {
                    Category = ((ComboBoxItem)LiabilityCategoryComboBox.SelectedItem).Content.ToString(),
                    Name = LiabilityNameTextBox.Text,
                    Amount = amount,
                    DueDate = LiabilityDueDatePicker.SelectedDate.Value.ToString("yyyy-MM-dd"),
                    Description = LiabilityDescriptionTextBox.Text
                };

                string jsonData = JsonConvert.SerializeObject(liabilityData);
                string command = $"addLiability{jsonData}";
                string response = await NetworkService.Instance.SendMessageAsync(command);

                var result = JsonConvert.DeserializeObject<dynamic>(response);
                if (result.Success == true)
                {
                    MessageBox.Show("Обязательство успешно добавлено!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadBalanceData();
                    await UpdateChartAsync();
                }
                else
                {
                    MessageBox.Show($"Ошибка: {result.Error}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении обязательства: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AllLiabilitiesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AllLiabilitiesListView.SelectedItem != null)
            {
                var selectedLiability = (LiabilityViewModel)AllLiabilitiesListView.SelectedItem;
                LiabilityCategoryComboBox.SelectedItem = LiabilityCategoryComboBox.Items.Cast<ComboBoxItem>()
                    .FirstOrDefault(item => item.Content.ToString() == selectedLiability.Category);
                LiabilityNameTextBox.Text = selectedLiability.Name;
                LiabilityAmountTextBox.Text = selectedLiability.Amount.ToString();
                LiabilityDueDatePicker.SelectedDate = selectedLiability.DueDate;
                LiabilityDescriptionTextBox.Text = selectedLiability.Description;
            }
        }

        private async void UpdateLiabilityButton_Click(object sender, RoutedEventArgs e)
        {
            if (AllLiabilitiesListView.SelectedItem == null)
            {
                MessageBox.Show("Выберите обязательство для редактирования!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                decimal amount;
                if (!decimal.TryParse(LiabilityAmountTextBox.Text, out amount))
                {
                    MessageBox.Show("Сумма должна быть числом!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var selectedLiability = (LiabilityViewModel)AllLiabilitiesListView.SelectedItem;
                var liabilityData = new
                {
                    Id = selectedLiability.Id,
                    Category = ((ComboBoxItem)LiabilityCategoryComboBox.SelectedItem).Content.ToString(),
                    Name = LiabilityNameTextBox.Text,
                    Amount = amount,
                    DueDate = LiabilityDueDatePicker.SelectedDate.Value.ToString("yyyy-MM-dd"),
                    Description = LiabilityDescriptionTextBox.Text
                };

                string jsonData = JsonConvert.SerializeObject(liabilityData);
                string command = $"updateLiability{jsonData}";
                string response = await NetworkService.Instance.SendMessageAsync(command);

                var result = JsonConvert.DeserializeObject<dynamic>(response);
                if (result.Success == true)
                {
                    // Логирование и сохранение снимка баланса
                    using (var dbContext = new CrmsystemContext())
                    {
                        var auditLog = new AuditLog
                        {
                            UserName = "User1",
                            Action = "Обновление",
                            EntityType = "Liability",
                            EntityId = selectedLiability.Id,
                            Details = $"Обновлено обязательство: {liabilityData.Name}, Новая сумма: {liabilityData.Amount}",
                        };

                        dbContext.AuditLogs.Add(auditLog);
                        await dbContext.SaveChangesAsync(); // Сохраняем AuditLog, чтобы получить Id

                        await LoadBalanceData(); // Обновляем данные баланса

                        var snapshot = new BalanceSnapshot
                        {
                            TotalAssets = decimal.Parse(TotalAssetsTextBlock.Text),
                            TotalLiabilities = decimal.Parse(TotalLiabilitiesTextBlock.Text),
                            Equity = decimal.Parse(EquityTextBlock.Text),
                            AuditLogId = auditLog.Id, // Связываем с AuditLog
                            Timestamp = DateTime.Now
                        };

                        dbContext.BalanceSnapshots.Add(snapshot);
                        await dbContext.SaveChangesAsync();
                    }

                    MessageBox.Show("Обязательство успешно обновлено!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    await UpdateChartAsync();
                }
                else
                {
                    MessageBox.Show($"Ошибка: {result.Error}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении обязательства: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void DeleteLiabilityButton_Click(object sender, RoutedEventArgs e)
        {
            if (AllLiabilitiesListView.SelectedItem == null)
            {
                MessageBox.Show("Выберите обязательство для удаления!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var selectedLiability = (LiabilityViewModel)AllLiabilitiesListView.SelectedItem;
                var liabilityData = new { Id = selectedLiability.Id };
                string jsonData = JsonConvert.SerializeObject(liabilityData);
                string command = $"deleteLiability{jsonData}";
                string response = await NetworkService.Instance.SendMessageAsync(command);

                var result = JsonConvert.DeserializeObject<dynamic>(response);
                if (result.Success == true)
                {
                    // Логирование и сохранение снимка баланса
                    using (var dbContext = new CrmsystemContext())
                    {
                        var auditLog = new AuditLog
                        {
                            UserName = "User1",
                            Action = "Удаление",
                            EntityType = "Liability",
                            EntityId = selectedLiability.Id,
                            Details = $"Удалено обязательство: {selectedLiability.Name}",
                        };

                        dbContext.AuditLogs.Add(auditLog);
                        await dbContext.SaveChangesAsync(); // Сохраняем AuditLog, чтобы получить Id

                        await LoadBalanceData(); // Обновляем данные баланса

                        var snapshot = new BalanceSnapshot
                        {
                            TotalAssets = decimal.Parse(TotalAssetsTextBlock.Text),
                            TotalLiabilities = decimal.Parse(TotalLiabilitiesTextBlock.Text),
                            Equity = decimal.Parse(EquityTextBlock.Text),
                            AuditLogId = auditLog.Id // Связываем с AuditLog
                        };

                        dbContext.BalanceSnapshots.Add(snapshot);
                        await dbContext.SaveChangesAsync();
                    }

                    MessageBox.Show("Обязательство успешно удалено!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    await UpdateChartAsync();
                }
                else
                {
                    MessageBox.Show($"Ошибка: {result.Error}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении обязательства: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    public class AssetViewModel
    {
        public int Id { get; set; }
        public string Category { get; set; }
        public string Name { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public DateTime AcquisitionDate { get; set; }
        public string DepreciationRate { get; set; }
        public string Description { get; set; }
    }

    public class LiabilityViewModel
    {
        public int Id { get; set; }
        public string Category { get; set; }
        public string Name { get; set; }
        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; }
        public string Description { get; set; }
    }
}
public class BalanceData
{
    public BalanceAssets Assets { get; set; }
    public BalanceLiabilities Liabilities { get; set; }
    public decimal Equity { get; set; }
    public decimal BalanceCheck { get; set; }
}

public class BalanceAssets
{
    public decimal Total { get; set; }
    public List<CategoryData> Categories { get; set; } = new List<CategoryData>(); // Инициализация по умолчанию
}

public class BalanceLiabilities
{
    public decimal Total { get; set; }
    public List<CategoryData> Categories { get; set; } = new List<CategoryData>(); // Инициализация по умолчанию
}
public class CategoryData
{
    public string Category { get; set; }
    public decimal Total { get; set; }
}