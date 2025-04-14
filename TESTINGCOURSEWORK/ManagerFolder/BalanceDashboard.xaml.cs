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

            var request = new
            {
                StartDate = StartDatePicker.SelectedDate.Value.ToString("yyyy-MM-dd"),
                EndDate = EndDatePicker.SelectedDate.Value.ToString("yyyy-MM-dd")
            };

            string jsonData = JsonConvert.SerializeObject(request);
            string command = $"getBalanceSnapshot{jsonData}";
            await LoadBalanceData(command);
            await UpdateChartAsync();
        }

        private async void ComparePeriodsButton_Click(object sender, RoutedEventArgs e)
        {
            if (Period1StartPicker.SelectedDate == null || Period1EndPicker.SelectedDate == null ||
                Period2StartPicker.SelectedDate == null || Period2EndPicker.SelectedDate == null)
            {
                MessageBox.Show("Выберите даты для обоих периодов!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var request = new
            {
                Period1Start = Period1StartPicker.SelectedDate.Value.ToString("yyyy-MM-dd"),
                Period1End = Period1EndPicker.SelectedDate.Value.ToString("yyyy-MM-dd"),
                Period2Start = Period2StartPicker.SelectedDate.Value.ToString("yyyy-MM-dd"),
                Period2End = Period2EndPicker.SelectedDate.Value.ToString("yyyy-MM-dd")
            };

            string jsonData = JsonConvert.SerializeObject(request);
            string command = $"compareBalanceSnapshots{jsonData}";
            string response = await NetworkService.Instance.SendMessageAsync(command);

            if (response.StartsWith("Error"))
            {
                MessageBox.Show($"Ошибка: {response}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var comparisonData = JsonConvert.DeserializeObject<dynamic>(response);

            Period1AssetsTextBlock.Text = ((decimal)comparisonData.Period1.Assets).ToString("F2");
            Period1LiabilitiesTextBlock.Text = ((decimal)comparisonData.Period1.Liabilities).ToString("F2");
            Period1EquityTextBlock.Text = ((decimal)comparisonData.Period1.Equity).ToString("F2");

            Period2AssetsTextBlock.Text = ((decimal)comparisonData.Period2.Assets).ToString("F2");
            Period2LiabilitiesTextBlock.Text = ((decimal)comparisonData.Period2.Liabilities).ToString("F2");
            Period2EquityTextBlock.Text = ((decimal)comparisonData.Period2.Equity).ToString("F2");

            AssetsValues.Clear();
            LiabilitiesValues.Clear();
            EquityValues.Clear();
            Labels.Clear();

            AssetsValues.Add((decimal)comparisonData.Period1.Assets);
            AssetsValues.Add((decimal)comparisonData.Period2.Assets);
            LiabilitiesValues.Add((decimal)comparisonData.Period1.Liabilities);
            LiabilitiesValues.Add((decimal)comparisonData.Period2.Liabilities);
            EquityValues.Add((decimal)comparisonData.Period1.Equity);
            EquityValues.Add((decimal)comparisonData.Period2.Equity);

            Labels.Add("Период 1");
            Labels.Add("Период 2");

            BalanceChart.Update(true);
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

                var balanceData = JsonConvert.DeserializeObject<dynamic>(response);

                TotalAssetsTextBlock.Text = ((decimal)balanceData.Assets.Total).ToString("F2");
                TotalLiabilitiesTextBlock.Text = ((decimal)balanceData.Liabilities.Total).ToString("F2");
                EquityTextBlock.Text = ((decimal)balanceData.Equity).ToString("F2");
                BalanceCheckTextBlock.Text = ((decimal)balanceData.BalanceCheck).ToString("F2");

                AssetsListView.ItemsSource = balanceData.Assets.Categories;
                LiabilitiesListView.ItemsSource = balanceData.Liabilities.Categories;

                await LoadAllAssets();
                await LoadAllLiabilities();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadAllAssets()
        {
            try
            {
                using (var dbContext = new CrmsystemContext())
                {
                    var assetsQuery = dbContext.Assets.AsQueryable();
                    if (StartDatePicker.SelectedDate.HasValue)
                        assetsQuery = assetsQuery.Where(a => a.AcquisitionDate >= StartDatePicker.SelectedDate.Value);
                    if (EndDatePicker.SelectedDate.HasValue)
                        assetsQuery = assetsQuery.Where(a => a.AcquisitionDate <= EndDatePicker.SelectedDate.Value);

                    var assets = await assetsQuery
                        .Include(a => a.Description)
                        .ToListAsync();

                    AllAssets = assets.Select(a => new AssetViewModel
                    {
                        Id = a.Id,
                        Category = a.Category,
                        Name = a.Name,
                        Amount = a.Amount,
                        Currency = a.Currency,
                        AcquisitionDate = a.AcquisitionDate,
                        DepreciationRate = a.DepreciationRate?.ToString("F2"),
                        Description = a.Description?.Content
                    }).ToList();

                    FilteredAssets = AllAssets;
                    ApplyAssetFilters(null, null);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке списка активов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadAllLiabilities()
        {
            try
            {
                using (var dbContext = new CrmsystemContext())
                {
                    var liabilitiesQuery = dbContext.Liabilities.AsQueryable();
                    if (StartDatePicker.SelectedDate.HasValue)
                        liabilitiesQuery = liabilitiesQuery.Where(l => l.DueDate >= StartDatePicker.SelectedDate.Value);
                    if (EndDatePicker.SelectedDate.HasValue)
                        liabilitiesQuery = liabilitiesQuery.Where(l => l.DueDate <= EndDatePicker.SelectedDate.Value);

                    var liabilities = await liabilitiesQuery
                        .Include(l => l.Description)
                        .ToListAsync();

                    AllLiabilities = liabilities.Select(l => new LiabilityViewModel
                    {
                        Id = l.Id,
                        Category = l.Category,
                        Name = l.Name,
                        Amount = l.Amount,
                        DueDate = l.DueDate,
                        Description = l.Description?.Content
                    }).ToList();

                    FilteredLiabilities = AllLiabilities;
                    ApplyLiabilityFilters(null, null);

                    var today = DateTime.Today;
                    var dueSoon = AllLiabilities
                        .Where(l => (l.DueDate - today).TotalDays <= 7 && (l.DueDate - today).TotalDays >= 0)
                        .ToList();

                    if (dueSoon.Any())
                    {
                        string message = "Приближаются сроки погашения обязательств:\n";
                        foreach (var liability in dueSoon)
                        {
                            message += $"- {liability.Name} (Срок: {liability.DueDate:yyyy-MM-dd}, Сумма: {liability.Amount:F2})\n";
                        }
                        MessageBox.Show(message, "Уведомление", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке списка обязательств: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplyAssetFilters(object sender, EventArgs e)
        {
            var categoryFilter = AssetFilterCategoryComboBox.SelectedItem != null
                ? ((ComboBoxItem)AssetFilterCategoryComboBox.SelectedItem).Content.ToString()
                : "Все";
            var searchText = AssetSearchTextBox.Text?.ToLower() ?? "";

            var filtered = AllAssets.AsEnumerable();

            if (categoryFilter != "Все")
            {
                filtered = filtered.Where(a => a.Category == categoryFilter);
            }

            if (!string.IsNullOrEmpty(searchText))
            {
                filtered = filtered.Where(a => (a.Name?.ToLower().Contains(searchText) ?? false) ||
                                               (a.Description?.ToLower().Contains(searchText) ?? false));
            }

            FilteredAssets = filtered.ToList();
            AllAssetsListView.ItemsSource = FilteredAssets;
        }

        private void ApplyLiabilityFilters(object sender, EventArgs e)
        {
            var categoryFilter = LiabilityFilterCategoryComboBox.SelectedItem != null
                ? ((ComboBoxItem)LiabilityFilterCategoryComboBox.SelectedItem).Content.ToString()
                : "Все";
            var searchText = LiabilitySearchTextBox.Text?.ToLower() ?? "";

            var filtered = AllLiabilities.AsEnumerable();

            if (categoryFilter != "Все")
            {
                filtered = filtered.Where(l => l.Category == categoryFilter);
            }

            if (!string.IsNullOrEmpty(searchText))
            {
                filtered = filtered.Where(l => (l.Name?.ToLower().Contains(searchText) ?? false) ||
                                               (l.Description?.ToLower().Contains(searchText) ?? false));
            }

            FilteredLiabilities = filtered.ToList();
            AllLiabilitiesListView.ItemsSource = FilteredLiabilities;
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
                    MessageBox.Show($"Актив успешно добавлен! Id: {result.AssetId}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
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
                    MessageBox.Show("Актив успешно обновлён!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
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
                    MessageBox.Show("Актив успешно удалён!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
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
                    MessageBox.Show("Обязательство успешно обновлено!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
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
                    MessageBox.Show("Обязательство успешно удалено!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
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