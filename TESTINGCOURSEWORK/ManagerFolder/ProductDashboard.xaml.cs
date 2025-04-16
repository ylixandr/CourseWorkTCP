using Client.InventoryViewModel;
using LiveCharts;
using LiveCharts.Wpf;
using Microsoft.Win32;
using Newtonsoft.Json;
using OfficeOpenXml;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using TCPServer.balanceModule;
using TCPServer;
using TCPServer.ProductionModule;

namespace Client.ManagerFolder
{
    public partial class ProductDashboard : Window
    {
        private ObservableCollection<ProductViewModel> _products;
        private ObservableCollection<WarehouseViewModel> _warehouses;
        private ObservableCollection<InventoryViewModel.InventoryViewModel> _inventory;
        private ObservableCollection<ProductCategoryViewModel> _categories;
        private ObservableCollection<InventoryTransactionViewModel> _transactions;
        private ObservableCollection<AuditLogViewModel> _auditLogs;
        private ProductSummaryViewModel _summary;
        private readonly ProductService _productService;

        public ProductDashboard()
        {
            _productService = new ProductService();
            InitializeComponent();
            InitializeData();
            LoadInitialData();
        }

        private void InitializeData()
        {
            _products = new ObservableCollection<ProductViewModel>();
            _warehouses = new ObservableCollection<WarehouseViewModel>();
            _inventory = new ObservableCollection<InventoryViewModel.InventoryViewModel>();
            _categories = new ObservableCollection<ProductCategoryViewModel>();
            _transactions = new ObservableCollection<InventoryTransactionViewModel>();
            _auditLogs = new ObservableCollection<AuditLogViewModel>();
            _summary = new ProductSummaryViewModel();

            ProductsListView.ItemsSource = _products;
            WarehousesListView.ItemsSource = _warehouses;
            InventoryListView.ItemsSource = _inventory;
            AuditLogsListView.ItemsSource = _auditLogs;

            TotalProductsTextBlock.Text = "0";
            TotalCategoriesTextBlock.Text = "0";
            TotalWarehousesTextBlock.Text = "0";
            TotalQuantityTextBlock.Text = "0";
            TotalInventoryValueTextBlock.Text = "0.00";

            InventoryChart.Series = new SeriesCollection
            {
                new LineSeries { Title = "Запас", Values = new ChartValues<decimal>(), Stroke = System.Windows.Media.Brushes.Green, Fill = System.Windows.Media.Brushes.Transparent },
                new LineSeries { Title = "Стоимость", Values = new ChartValues<decimal>(), Stroke = System.Windows.Media.Brushes.Pink, Fill = System.Windows.Media.Brushes.Transparent }
            };
        }

        private async void LoadInitialData()
        {
            try
            {
                await LoadProductSummaryAsync();
                await LoadCategoriesAsync();
                await LoadProductsAsync();
                await LoadWarehousesAsync();
                await LoadInventoryAsync();
                await LoadAuditLogsAsync();
                UpdateChartData("Months");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadProductSummaryAsync()
        {
            var response = await NetworkService.Instance.SendMessageAsync("getProductSnapshot");
            if (response.StartsWith("Error"))
            {
                MessageBox.Show(response, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var summary = JsonConvert.DeserializeObject<ProductSummaryDto>(response);
            _summary.TotalProducts = summary.TotalProducts;
            _summary.TotalCategories = summary.TotalCategories;
            _summary.TotalWarehouses = summary.TotalWarehouses;
            _summary.TotalQuantity = summary.TotalQuantity;
            _summary.TotalInventoryValue = summary.TotalInventoryValue;

            TotalProductsTextBlock.Text = _summary.TotalProducts.ToString();
            TotalCategoriesTextBlock.Text = _summary.TotalCategories.ToString();
            TotalWarehousesTextBlock.Text = _summary.TotalWarehouses.ToString();
            TotalQuantityTextBlock.Text = _summary.TotalQuantity.ToString("F2");
            TotalInventoryValueTextBlock.Text = _summary.TotalInventoryValue.ToString("F2");
        }

        private async Task LoadProductsAsync()
        {
            var response = await NetworkService.Instance.SendMessageAsync("getAllProducts");
            if (response.StartsWith("Error"))
            {
                MessageBox.Show(response, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var products = JsonConvert.DeserializeObject<List<ProductDto>>(response);
            _products.Clear();
            foreach (var product in products)
            {
                var category = _categories.FirstOrDefault(c => c.Id == product.CategoryId);
                _products.Add(new ProductViewModel
                {
                    Id = product.Id,
                    Name = product.Name ?? "Без названия",
                    CategoryId = product.CategoryId,
                    CategoryName = category?.Name ?? "Неизвестно",
                    PurchasePrice = product.PurchasePrice,
                    SellingPrice = product.SellingPrice,
                    Description = product.Description
                });
            }

            TransactionProductComboBox.Items.Clear();
            TransactionProductComboBox.ItemsSource = _products;
        }

        private async Task LoadWarehousesAsync()
        {
            var response = await NetworkService.Instance.SendMessageAsync("getAllWarehouses");
            if (response.StartsWith("Error"))
            {
                MessageBox.Show(response, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var warehouses = JsonConvert.DeserializeObject<List<WarehouseDto>>(response);
            _warehouses.Clear();
            foreach (var warehouse in warehouses)
            {
                _warehouses.Add(new WarehouseViewModel
                {
                    Id = warehouse.Id,
                    Name = warehouse.Name
                });
            }

            FromWarehouseComboBox.Items.Clear();
            ToWarehouseComboBox.Items.Clear();
            FromWarehouseComboBox.ItemsSource = _warehouses;
            ToWarehouseComboBox.ItemsSource = _warehouses;
        }

        private async Task LoadInventoryAsync()
        {
            var response = await NetworkService.Instance.SendMessageAsync("getInventory");
            if (response.StartsWith("Error"))
            {
                MessageBox.Show(response, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var inventory = JsonConvert.DeserializeObject<List<InventoryDto>>(response);
            _inventory.Clear();
            foreach (var item in inventory)
            {
                var product = _products.FirstOrDefault(p => p.Id == item.ProductId);
                var warehouse = _warehouses.FirstOrDefault(w => w.Id == item.WarehouseId);
                _inventory.Add(new InventoryViewModel.InventoryViewModel
                {
                    Id = item.Id,
                    ProductId = item.ProductId,
                    ProductName = product?.Name ?? "Неизвестно",
                    WarehouseId = item.WarehouseId,
                    WarehouseName = warehouse?.Name ?? "Неизвестно",
                    Quantity = item.Quantity,
                    ReservedQuantity = item.ReservedQuantity
                });
            }
        }

        private async Task LoadCategoriesAsync()
        {
            _categories.Clear();
            var categories = await _productService.GetCategoriesAsync();
            _categories.Add(new ProductCategoryViewModel { Id = 0, Name = "Все" });
            foreach (var category in categories)
            {
                _categories.Add(new ProductCategoryViewModel
                {
                    Id = category.Id,
                    Name = category.Name
                });
            }

            ProductFilterCategoryComboBox.Items.Clear();
            ProductCategoryComboBox.Items.Clear();
            ProductFilterCategoryComboBox.ItemsSource = _categories;
            ProductCategoryComboBox.ItemsSource = _categories;
        }

        private async Task LoadAuditLogsAsync()
        {
            _auditLogs.Clear();
            var auditLogs = await _productService.GetAuditLogsAsync();
            foreach (var log in auditLogs)
            {
                _auditLogs.Add(new AuditLogViewModel
                {
                    Id = log.Id,
                    Action = log.Action,
                    UserName = log.UserName,
                    EntityType = log.EntityType,
                    EntityId = log.EntityId,
                    Details = log.Details,
                    Timestamp = log.Timestamp
                });
            }
        }

        private void ProductsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProductsListView.SelectedItem is ProductViewModel selectedProduct)
            {
                ProductNameTextBox.Text = selectedProduct.Name;
                ProductCategoryComboBox.SelectedItem = _categories.FirstOrDefault(c => c.Id == selectedProduct.CategoryId);
                PurchasePriceTextBox.Text = selectedProduct.PurchasePrice.ToString();
                SellingPriceTextBox.Text = selectedProduct.SellingPrice.ToString();
                InitialQuantityTextBox.Text = "";
                ProductDescriptionTextBox.Text = selectedProduct.Description;
            }
        }

        private void WarehousesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private async void AddProductButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(ProductNameTextBox.Text) ||
                    ProductCategoryComboBox.SelectedItem is not ProductCategoryViewModel selectedCategory ||
                    !decimal.TryParse(PurchasePriceTextBox.Text, out var purchasePrice) ||
                    !decimal.TryParse(SellingPriceTextBox.Text, out var sellingPrice))
                {
                    MessageBox.Show("Заполните все обязательные поля корректно.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                decimal? initialQuantity = null;
                if (!string.IsNullOrEmpty(InitialQuantityTextBox.Text) && decimal.TryParse(InitialQuantityTextBox.Text, out var quantity))
                {
                    initialQuantity = quantity;
                }

                var productDto = new ProductDto
                {
                    Name = ProductNameTextBox.Text,
                    CategoryId = selectedCategory.Id,
                    PurchasePrice = purchasePrice,
                    SellingPrice = sellingPrice,
                    Description = ProductDescriptionTextBox.Text,
                    InitialQuantity = initialQuantity
                };

                var jsonData = JsonConvert.SerializeObject(productDto);
                var response = await NetworkService.Instance.SendMessageAsync($"addProduct{jsonData}");
                var result = JsonConvert.DeserializeObject<dynamic>(response);

                if (result.Success == true)
                {
                    MessageBox.Show("Продукт добавлен успешно.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    ClearProductForm();
                    await LoadProductsAsync();
                    await LoadProductSummaryAsync();
                }
                else
                {
                    MessageBox.Show($"Ошибка: {result.Error}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка добавления продукта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private async void UpdateProductButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ProductsListView.SelectedItem == null ||
                    string.IsNullOrEmpty(ProductNameTextBox.Text) ||
                    ProductCategoryComboBox.SelectedItem is not ProductCategoryViewModel selectedCategory ||
                    !decimal.TryParse(PurchasePriceTextBox.Text, out var purchasePrice) ||
                    !decimal.TryParse(SellingPriceTextBox.Text, out var sellingPrice))
                {
                    MessageBox.Show("Выберите продукт и заполните все обязательные поля корректно.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                var selectedProduct = (ProductViewModel)ProductsListView.SelectedItem;
                var productDto = new ProductDto
                {
                    Id = selectedProduct.Id,
                    Name = ProductNameTextBox.Text,
                    CategoryId = selectedCategory.Id,
                    PurchasePrice = purchasePrice,
                    SellingPrice = sellingPrice,
                    Description = ProductDescriptionTextBox.Text
                };

                var jsonData = JsonConvert.SerializeObject(productDto);
                var response = await NetworkService.Instance.SendMessageAsync($"updateProduct{jsonData}");
                var result = JsonConvert.DeserializeObject<dynamic>(response);

                if (result.Success == true)
                {
                    MessageBox.Show("Продукт обновлён успешно.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    ClearProductForm();
                    await LoadProductsAsync();
                }
                else
                {
                    MessageBox.Show($"Ошибка: {result.Error}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обновления продукта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void DeleteProductButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ProductsListView.SelectedItem is not ProductViewModel selectedProduct)
                {
                    MessageBox.Show("Выберите продукт для удаления.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var jsonData = JsonConvert.SerializeObject(new { Id = selectedProduct.Id });
                var response = await NetworkService.Instance.SendMessageAsync($"deleteProduct{jsonData}");
                var result = JsonConvert.DeserializeObject<dynamic>(response);

                if (result.Success == true)
                {
                    MessageBox.Show("Продукт удалён успешно.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    ClearProductForm();
                    await LoadProductsAsync();
                    await LoadProductSummaryAsync();
                }
                else
                {
                    MessageBox.Show($"Ошибка: {result.Error}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления продукта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ExecuteTransactionButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (TransactionTypeComboBox.SelectedItem == null ||
                    TransactionProductComboBox.SelectedItem is not ProductViewModel selectedProduct ||
                    !decimal.TryParse(TransactionQuantityTextBox.Text, out var quantity) || quantity <= 0)
                {
                    MessageBox.Show("Заполните все обязательные поля корректно.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var transactionType = (TransactionTypeComboBox.SelectedItem as ComboBoxItem).Content.ToString().ToLower();
                int? fromWarehouseId = FromWarehouseComboBox.SelectedItem is WarehouseViewModel fromWarehouse ? fromWarehouse.Id : null;
                int? toWarehouseId = ToWarehouseComboBox.SelectedItem is WarehouseViewModel toWarehouse ? toWarehouse.Id : null;

                if (transactionType == "приём" && !toWarehouseId.HasValue)
                {
                    MessageBox.Show("Укажите склад назначения для приёма.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (transactionType == "отгрузка" && !fromWarehouseId.HasValue)
                {
                    MessageBox.Show("Укажите склад отправления для отгрузки.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (transactionType == "перемещение" && (!fromWarehouseId.HasValue || !toWarehouseId.HasValue))
                {
                    MessageBox.Show("Укажите оба склада для перемещения.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var transactionDto = new InventoryTransactionDto
                {
                    ProductBatchId = selectedProduct.Id,
                    FromWarehouseId = fromWarehouseId,
                    ToWarehouseId = toWarehouseId,
                    Quantity = quantity,
                    TransactionType = transactionType,
                    TransactionDate = DateTime.Now
                };

                var jsonData = JsonConvert.SerializeObject(transactionDto);
                var response = await NetworkService.Instance.SendMessageAsync($"addInventoryTransaction{jsonData}");
                var result = JsonConvert.DeserializeObject<dynamic>(response);

                if (result.Success == true)
                {
                    MessageBox.Show("Транзакция выполнена успешно.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    ClearTransactionForm();
                    await LoadInventoryAsync();
                    await LoadProductSummaryAsync();
                }
                else
                {
                    MessageBox.Show($"Ошибка: {result.Error}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка выполнения транзакции: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private async void ApplyProductFilters(object sender, RoutedEventArgs e)
        {
            try
            {
                var category = ProductFilterCategoryComboBox.SelectedItem as ProductCategoryViewModel;
                ProductFilterCategoryComboBox.SelectedItem = category.Name;
                var searchText = ProductSearchTextBox?.Text.ToLower();

                var filteredProducts = _products.Where(p =>
                    (category == null || category.Name == "Все" || p.CategoryId == category.Id) &&
                    (string.IsNullOrEmpty(searchText) || p.Name.ToLower().Contains(searchText))
                ).ToList();

                ProductsListView.ItemsSource = new ObservableCollection<ProductViewModel>(filteredProducts);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка фильтрации: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ShowPeriodButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (StartDatePicker.SelectedDate == null || EndDatePicker.SelectedDate == null)
                {
                    MessageBox.Show("Выберите даты периода.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var interval = (ChartIntervalComboBox.SelectedItem as ComboBoxItem).Content.ToString();
                UpdateChartData(interval);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка отображения периода: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ComparePeriodsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Period1StartPicker.SelectedDate == null || Period1EndPicker.SelectedDate == null ||
                    Period2StartPicker.SelectedDate == null || Period2EndPicker.SelectedDate == null)
                {
                    MessageBox.Show("Выберите даты для обоих периодов.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var periods = new
                {
                    Period1Start = Period1StartPicker.SelectedDate.Value.ToString("yyyy-MM-dd"),
                    Period1End = Period1EndPicker.SelectedDate.Value.ToString("yyyy-MM-dd"),
                    Period2Start = Period2StartPicker.SelectedDate.Value.ToString("yyyy-MM-dd"),
                    Period2End = Period2EndPicker.SelectedDate.Value.ToString("yyyy-MM-dd")
                };

                var jsonData = JsonConvert.SerializeObject(periods);
                var response = await NetworkService.Instance.SendMessageAsync($"compareProductSnapshots{jsonData}");
                var result = JsonConvert.DeserializeObject<dynamic>(response);

                if (response.StartsWith("Error"))
                {
                    MessageBox.Show(response, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                Period1QuantityTextBlock.Text = result.Period1.Quantity.ToString("F2");
                Period1ValueTextBlock.Text = result.Period1.Value.ToString("F2");
                Period2QuantityTextBlock.Text = result.Period2.Quantity.ToString("F2");
                Period2ValueTextBlock.Text = result.Period2.Value.ToString("F2");

                var turnover = CalculateInventoryTurnover();
                var avgUnitCost = _inventory.Any() ? _inventory.Average(i => _products.FirstOrDefault(p => p.Id == i.ProductId)?.PurchasePrice ?? 0) : 0;
                var reservedPercentage = _inventory.Any() ? (_inventory.Sum(i => i.ReservedQuantity) / _inventory.Sum(i => i.Quantity)) * 100 : 0;

                InventoryTurnoverTextBlock.Text = turnover.ToString("F2");
                AverageUnitCostTextBlock.Text = avgUnitCost.ToString("F2");
                ReservedPercentageTextBlock.Text = reservedPercentage.ToString("F2");

                var forecastQuantity = _inventory.Sum(i => i.Quantity) * 1.1m;
                var forecastValue = _inventory.Sum(i => i.Quantity * (_products.FirstOrDefault(p => p.Id == i.ProductId)?.PurchasePrice ?? 0)) * 1.1m;

                ForecastQuantityTextBlock.Text = forecastQuantity.ToString("F2");
                ForecastValueTextBlock.Text = forecastValue.ToString("F2");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сравнения периодов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ChartIntervalComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ChartIntervalComboBox.SelectedItem != null)
            {
                var interval = (ChartIntervalComboBox.SelectedItem as ComboBoxItem).Content.ToString();
                UpdateChartData(interval);
            }
        }

        private async void UpdateChartData(string interval)
        {
            try
            {
                var quantityValues = new ChartValues<decimal>();
                var valueValues = new ChartValues<decimal>();
                var labels = new List<string>();

                var startDate = StartDatePicker.SelectedDate ?? DateTime.Now.AddMonths(-6);
                var endDate = EndDatePicker.SelectedDate ?? DateTime.Now;

                if (interval == "Days")
                {
                    for (var date = startDate; date <= endDate; date = date.AddDays(1))
                    {
                        quantityValues.Add(_inventory.Sum(i => i.Quantity));
                        valueValues.Add(_inventory.Sum(i => i.Quantity * (_products.FirstOrDefault(p => p.Id == i.ProductId)?.PurchasePrice ?? 0)));
                        labels.Add(date.ToString("yyyy-MM-dd"));
                    }
                }
                else if (interval == "Weeks")
                {
                    for (var date = startDate; date <= endDate; date = date.AddDays(7))
                    {
                        quantityValues.Add(_inventory.Sum(i => i.Quantity));
                        valueValues.Add(_inventory.Sum(i => i.Quantity * (_products.FirstOrDefault(p => p.Id == i.ProductId)?.PurchasePrice ?? 0)));
                        labels.Add(date.ToString("yyyy-MM-dd"));
                    }
                }
                else // Months
                {
                    for (var date = startDate; date <= endDate; date = date.AddMonths(1))
                    {
                        quantityValues.Add(_inventory.Sum(i => i.Quantity));
                        valueValues.Add(_inventory.Sum(i => i.Quantity * (_products.FirstOrDefault(p => p.Id == i.ProductId)?.PurchasePrice ?? 0)));
                        labels.Add(date.ToString("yyyy-MM"));
                    }
                }

                InventoryChart.Series[0].Values = quantityValues;
                InventoryChart.Series[1].Values = valueValues;
                InventoryChart.AxisX[0].Labels = labels;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обновления графика: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ExportToExcelButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "Excel files (*.xlsx)|*.xlsx",
                    FileName = $"InventoryExport_{DateTime.Now:yyyyMMdd}.xlsx"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                    using (var package = new ExcelPackage())
                    {
                        var worksheet = package.Workbook.Worksheets.Add("Инвентарь");
                        worksheet.Cells[1, 1].Value = "Id";
                        worksheet.Cells[1, 2].Value = "Продукт";
                        worksheet.Cells[1, 3].Value = "Склад";
                        worksheet.Cells[1, 4].Value = "Количество";
                        worksheet.Cells[1, 5].Value = "Зарезервировано";

                        for (int i = 0; i < _inventory.Count; i++)
                        {
                            worksheet.Cells[i + 2, 1].Value = _inventory[i].Id;
                            worksheet.Cells[i + 2, 2].Value = _inventory[i].ProductName;
                            worksheet.Cells[i + 2, 3].Value = _inventory[i].WarehouseName;
                            worksheet.Cells[i + 2, 4].Value = _inventory[i].Quantity;
                            worksheet.Cells[i + 2, 5].Value = _inventory[i].ReservedQuantity;
                        }

                        worksheet.Cells.AutoFitColumns();
                        File.WriteAllBytes(saveFileDialog.FileName, package.GetAsByteArray());
                        MessageBox.Show("Экспорт в Excel завершён.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка экспорта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearProductForm()
        {
            ProductNameTextBox.Text = "";
            ProductCategoryComboBox.SelectedItem = null;
            PurchasePriceTextBox.Text = "";
            SellingPriceTextBox.Text = "";
            InitialQuantityTextBox.Text = "";
            ProductDescriptionTextBox.Text = "";
            ProductsListView.SelectedItem = null;
        }

        private void ClearTransactionForm()
        {
            TransactionTypeComboBox.SelectedItem = null;
            TransactionProductComboBox.SelectedItem = null;
            FromWarehouseComboBox.SelectedItem = null;
            ToWarehouseComboBox.SelectedItem = null;
            TransactionQuantityTextBox.Text = "";
        }

        private decimal CalculateInventoryTurnover()
        {
            return _transactions.Any() ? _transactions.Count / _inventory.Sum(i => i.Quantity) : 0;
        }

        private async void DeleteWarehouseButton_Click(object sender, RoutedEventArgs e)
        {
            if (WarehousesListView.SelectedItem == null)
            {
                MessageBox.Show("Выберите Склад для удаления!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var selectedWarehouse = (WarehouseViewModel)WarehousesListView.SelectedItem;
                var liabilityData = new { Id = selectedWarehouse.Id };
                string jsonData = JsonConvert.SerializeObject(liabilityData);
                string command = $"deleteWarehouse{jsonData}";
                string response = await NetworkService.Instance.SendMessageAsync(command);

                var result = JsonConvert.DeserializeObject<dynamic>(response);
                if (result.Success == true)
                {
                    using (var dbContext = new CrmsystemContext())
                    {
                        var auditLog = new AuditLog
                        {
                            UserName = "User1",
                            Action = "Удаление",
                            EntityType = "Warehouse",
                            EntityId = selectedWarehouse.Id,
                            Details = $"Удален склад: {selectedWarehouse.Name}",
                        };

                        dbContext.AuditLogs.Add(auditLog);
                        await dbContext.SaveChangesAsync();
                    }

                    MessageBox.Show("Склад успешно удалено!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadWarehousesAsync();
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

    public class AuditLogViewModel
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Action { get; set; }
        public string EntityType { get; set; }
        public int EntityId { get; set; }
        public string Details { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class WarehouseViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }
    }
}