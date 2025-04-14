using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using Client.ManagerFolder;
using ClosedXML.Excel;
using Newtonsoft.Json;
using TCPServer;

namespace TESTINGCOURSEWORK.ManagerFolder
{
    public partial class SalaryPage : Window
    {
        private ObservableCollection<PayrollItem> _accruals = new ObservableCollection<PayrollItem>();
        private ObservableCollection<PayrollItem> _deductions = new ObservableCollection<PayrollItem>();
        private Employee _selectedEmployee;
        private Dictionary<string, decimal> _deductionRates;
        private bool _isUpdating = false; // Флаг для предотвращения бесконечного цикла

        public ObservableCollection<string> AccrualTypes { get; private set; }
        public ObservableCollection<string> DeductionTypes { get; private set; }

        public SalaryPage()
        {
            InitializeComponent();
            LoadTypesFromJson();
            LoadDeductionRates();

            AccrualsListView.ItemsSource = _accruals;
            DeductionsListView.ItemsSource = _deductions;

            // Подписываемся на изменения коллекций
            _accruals.CollectionChanged += (s, e) =>
            {
                if (!_isUpdating)
                {
                    Dispatcher.InvokeAsync(UpdateTotalsAndDeductions);
                }
                if (e.OldItems != null)
                {
                    foreach (PayrollItem item in e.OldItems)
                    {
                        item.PropertyChanged -= Item_PropertyChanged;
                    }
                }
                if (e.NewItems != null)
                {
                    foreach (PayrollItem item in e.NewItems)
                    {
                        item.PropertyChanged += Item_PropertyChanged;
                    }
                }
            };

            _deductions.CollectionChanged += (s, e) =>
            {
                if (!_isUpdating)
                {
                    Dispatcher.InvokeAsync(UpdateTotalsAndDeductions);
                }
                if (e.OldItems != null)
                {
                    foreach (PayrollItem item in e.OldItems)
                    {
                        item.PropertyChanged -= Item_PropertyChanged;
                    }
                }
                if (e.NewItems != null)
                {
                    foreach (PayrollItem item in e.NewItems)
                    {
                        item.PropertyChanged += Item_PropertyChanged;
                    }
                }
            };
        }

        public SalaryPage(Employee selectedEmployee) : this()
        {
            _selectedEmployee = selectedEmployee;
            FullNameTextBox.Text = $"{selectedEmployee.LastName} {selectedEmployee.FirstName} {selectedEmployee.MiddleName}";
            PositionTextBox.Text = selectedEmployee.Position;
            EmployeeIdTextBox.Text = selectedEmployee.EmployeeId.ToString();
            BaseSalaryTextBox.Text = selectedEmployee.Salary.ToString();
        }

        private void LoadTypesFromJson()
        {
            // Загрузка типов начислений
            string accrualFilePath = "data\\AccrualTypes.json";
            if (File.Exists(accrualFilePath))
            {
                string json = File.ReadAllText(accrualFilePath);
                AccrualTypes = JsonConvert.DeserializeObject<ObservableCollection<string>>(json) ?? new ObservableCollection<string>();
            }
            else
            {
                AccrualTypes = new ObservableCollection<string>
                {
                    "Оклад",
                    "Премия",
                    "Оплата за сверхурочные",
                    "Компенсация за отпуск",
                    "Больничный"
                };
                File.WriteAllText(accrualFilePath, JsonConvert.SerializeObject(AccrualTypes, Formatting.Indented));
            }

            // Загрузка типов удержаний
            string deductionFilePath = "data\\DeductionTypes.json";
            if (File.Exists(deductionFilePath))
            {
                string json = File.ReadAllText(deductionFilePath);
                DeductionTypes = JsonConvert.DeserializeObject<ObservableCollection<string>>(json) ?? new ObservableCollection<string>();
            }
            else
            {
                DeductionTypes = new ObservableCollection<string>
                {
                    "НДФЛ",
                    "Страховые взносы",
                    "Пенсионные взносы",
                    "Алименты",
                    "Штраф",
                    "Профсоюзные взносы"
                };
                File.WriteAllText(deductionFilePath, JsonConvert.SerializeObject(DeductionTypes, Formatting.Indented));
            }
        }

        private void LoadDeductionRates()
        {
            string ratesFilePath = "data\\DeductionRates.json";
            if (File.Exists(ratesFilePath))
            {
                string json = File.ReadAllText(ratesFilePath);
                _deductionRates = JsonConvert.DeserializeObject<Dictionary<string, decimal>>(json) ?? new Dictionary<string, decimal>();
            }
            else
            {
                _deductionRates = new Dictionary<string, decimal>
                {
                    { "НДФЛ", 0.13m },           // 13%
                    { "Пенсионные взносы", 0.22m }, // 22%
                    { "Страховые взносы", 0.051m }  // 5.1%
                };
                File.WriteAllText(ratesFilePath, JsonConvert.SerializeObject(_deductionRates, Formatting.Indented));
            }
        }

        private void Item_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PayrollItem.Amount) && !_isUpdating)
            {
                Dispatcher.InvokeAsync(UpdateTotalsAndDeductions);
            }
        }

        private void UpdateTotalsAndDeductions()
        {
            if (_isUpdating) return; // Предотвращаем рекурсию

            _isUpdating = true;
            try
            {
                decimal totalAccrued = _accruals.Sum(item => item.Amount);
                UpdateMandatoryDeductions(totalAccrued);

                decimal totalDeducted = _deductions.Sum(item => item.Amount);
                decimal toBePaid = totalAccrued - totalDeducted;

                TotalAccruedTextBox.Text = totalAccrued.ToString("F2");
                TotalDeductedTextBox.Text = totalDeducted.ToString("F2");
                ToBePaidTextBox.Text = toBePaid.ToString("F2");
            }
            finally
            {
                _isUpdating = false;
            }
        }

        private void UpdateMandatoryDeductions(decimal totalAccrued)
        {
            // Обновляем только те удержания, которые должны быть автоматическими
            foreach (var rate in _deductionRates)
            {
                var deduction = _deductions.FirstOrDefault(d => d.Type == rate.Key);
                decimal newAmount = totalAccrued * rate.Value;

                if (deduction != null)
                {
                    // Обновляем сумму только если она изменилась, чтобы избежать лишних событий
                    if (deduction.Amount != newAmount)
                    {
                        deduction.Amount = newAmount;
                    }
                }
                else
                {
                    _deductions.Add(new PayrollItem { Type = rate.Key, Amount = newAmount });
                }
            }
        }

        private void AddAccrual_Click(object sender, RoutedEventArgs e)
        {
            _accruals.Add(new PayrollItem { Type = AccrualTypes[0], Amount = 0, DaysOrHours = "0" });
        }

        private void AddDeduction_Click(object sender, RoutedEventArgs e)
        {
            if (DeductionTypes.Any(t => !_deductionRates.ContainsKey(t)))
            {
                string firstNonMandatoryType = DeductionTypes.First(t => !_deductionRates.ContainsKey(t));
                _deductions.Add(new PayrollItem { Type = firstNonMandatoryType, Amount = 0 });
            }
        }

        private async void FillData_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string request = $"getPayrollData:{_selectedEmployee.EmployeeId}:{PeriodTextBox.Text}:{YearTextBox.Text}";
                string response = await NetworkService.Instance.SendMessageAsync(request);

                if (response == "NoData")
                {
                    FullNameTextBox.Text = $"{_selectedEmployee.LastName} {_selectedEmployee.FirstName} {_selectedEmployee.MiddleName}";
                    PositionTextBox.Text = _selectedEmployee.Position;
                    EmployeeIdTextBox.Text = _selectedEmployee.EmployeeId.ToString();
                    BaseSalaryTextBox.Text = _selectedEmployee.Salary.ToString();

                    _accruals.Clear();
                    _deductions.Clear();
                    UpdateTotalsAndDeductions();

                    MessageBox.Show("Данные для этого сотрудника за указанный период отсутствуют. Заполнены только основные поля.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    var payrollData = JsonConvert.DeserializeObject<PayrollData>(response);

                    FullNameTextBox.Text = payrollData.FullName;
                    PositionTextBox.Text = payrollData.Position;
                    EmployeeIdTextBox.Text = payrollData.EmployeeId.ToString();
                    BaseSalaryTextBox.Text = payrollData.BaseSalary.ToString();
                    PeriodTextBox.Text = payrollData.Period;
                    YearTextBox.Text = payrollData.Year;

                    _accruals.Clear();
                    foreach (var accrual in payrollData.Accruals)
                    {
                        _accruals.Add(accrual);
                    }

                    _deductions.Clear();
                    foreach (var deduction in payrollData.Deductions)
                    {
                        _deductions.Add(deduction);
                    }

                    UpdateTotalsAndDeductions();
                    MessageBox.Show("Данные успешно загружены из JSON.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ExportToExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var payrollData = new PayrollData
                {
                    EmployeeId = int.Parse(EmployeeIdTextBox.Text),
                    FullName = FullNameTextBox.Text,
                    Position = PositionTextBox.Text,
                    BaseSalary = decimal.Parse(BaseSalaryTextBox.Text),
                    Period = PeriodTextBox.Text,
                    Year = YearTextBox.Text,
                    Accruals = _accruals.ToList(),
                    Deductions = _deductions.ToList(),
                    TotalAccrued = decimal.Parse(TotalAccruedTextBox.Text),
                    TotalDeducted = decimal.Parse(TotalDeductedTextBox.Text),
                    ToBePaid = decimal.Parse(ToBePaidTextBox.Text)
                };

                string jsonData = System.Text.Json.JsonSerializer.Serialize(payrollData);
                string message = $"savePayrollJson:{jsonData}";
                var response = await NetworkService.Instance.SendMessageAsync(message);

                if (response == "Success")
                {
                    ExportToExcel();
                }
                else
                {
                    MessageBox.Show("Ошибка при сохранении данных на сервере.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportToExcel()
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Расчетный лист");

                worksheet.Cell(1, 1).Value = "УТВЕРЖДЕНО";
                worksheet.Cell(2, 1).Value = "РАСЧЕТНЫЙ ЛИСТ";
                worksheet.Cell(3, 1).Value = $"за {PeriodTextBox.Text} 20{YearTextBox.Text} г.";
                worksheet.Cell(5, 1).Value = "Ф.И.О.";
                worksheet.Cell(5, 2).Value = FullNameTextBox.Text;
                worksheet.Cell(6, 1).Value = "Должность";
                worksheet.Cell(6, 2).Value = PositionTextBox.Text;
                worksheet.Cell(7, 1).Value = "Табельный номер";
                worksheet.Cell(7, 2).Value = EmployeeIdTextBox.Text;
                worksheet.Cell(8, 1).Value = "Оклад/тариф";
                worksheet.Cell(8, 2).Value = BaseSalaryTextBox.Text;

                worksheet.Cell(10, 1).Value = "НАЧИСЛЕНИЯ";
                worksheet.Cell(11, 1).Value = "Вид начисления";
                worksheet.Cell(11, 2).Value = "Сумма";
                worksheet.Cell(11, 3).Value = "Дней/часов";
                int row = 12;
                foreach (var accrual in _accruals)
                {
                    worksheet.Cell(row, 1).Value = accrual.Type;
                    worksheet.Cell(row, 2).Value = accrual.Amount;
                    worksheet.Cell(row, 3).Value = accrual.DaysOrHours;
                    row++;
                }

                worksheet.Cell(10, 4).Value = "УДЕРЖАНИЯ";
                worksheet.Cell(11, 4).Value = "Вид удержания";
                worksheet.Cell(11, 5).Value = "Сумма";
                row = 12;
                foreach (var deduction in _deductions)
                {
                    worksheet.Cell(row, 4).Value = deduction.Type;
                    worksheet.Cell(row, 5).Value = deduction.Amount;
                    row++;
                }

                worksheet.Cell(row + 1, 1).Value = "ИТОГО НАЧИСЛЕНО:";
                worksheet.Cell(row + 1, 2).Value = TotalAccruedTextBox.Text;
                worksheet.Cell(row + 2, 1).Value = "ИТОГО УДЕРЖАНО:";
                worksheet.Cell(row + 2, 2).Value = TotalDeductedTextBox.Text;
                worksheet.Cell(row + 3, 1).Value = "К ВЫДАЧЕ:";
                worksheet.Cell(row + 3, 2).Value = ToBePaidTextBox.Text;

                workbook.SaveAs($"salaryReports\\PayrollSlip_{EmployeeIdTextBox.Text}.xlsx");
            }

            MessageBox.Show("Расчетный лист успешно экспортирован в Excel!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public void AddAccrualType(string newType)
        {
            if (!AccrualTypes.Contains(newType))
            {
                AccrualTypes.Add(newType);
                File.WriteAllText("data\\AccrualTypes.json", JsonConvert.SerializeObject(AccrualTypes, Formatting.Indented));
            }
        }

        public void RemoveAccrualType(string type)
        {
            if (AccrualTypes.Contains(type))
            {
                AccrualTypes.Remove(type);
                File.WriteAllText("data\\AccrualTypes.json", JsonConvert.SerializeObject(AccrualTypes, Formatting.Indented));
            }
        }

        public void AddDeductionType(string newType)
        {
            if (!DeductionTypes.Contains(newType))
            {
                DeductionTypes.Add(newType);
                File.WriteAllText("data\\DeductionTypes.json", JsonConvert.SerializeObject(DeductionTypes, Formatting.Indented));
            }
        }

        public void RemoveDeductionType(string type)
        {
            if (DeductionTypes.Contains(type))
            {
                DeductionTypes.Remove(type);
                File.WriteAllText("data\\DeductionTypes.json", JsonConvert.SerializeObject(DeductionTypes, Formatting.Indented));
            }
        }

        private void ManageAccrualTypes_Click(object sender, RoutedEventArgs e)
        {
            var manageWindow = new ManageTypesWindow(AccrualTypes, AddAccrualType, RemoveAccrualType);
            manageWindow.ShowDialog();
        }

        private void ManageDeductionTypes_Click(object sender, RoutedEventArgs e)
        {
            var manageWindow = new ManageTypesWindow(DeductionTypes, AddDeductionType, RemoveDeductionType);
            manageWindow.ShowDialog();
        }

        private void ManageDeductionRates_Click(object sender, RoutedEventArgs e)
        {
            var manageRatesWindow = new ManageDeductionRatesWindow(_deductionRates);
            manageRatesWindow.ShowDialog();
            LoadDeductionRates();
            UpdateMandatoryDeductions(_accruals.Sum(item => item.Amount));
        }
    }

    public class PayrollItem : System.ComponentModel.INotifyPropertyChanged
    {
        private string _type;
        private decimal _amount;
        private string _daysOrHours;

        public string Type
        {
            get => _type;
            set
            {
                _type = value;
                OnPropertyChanged(nameof(Type));
            }
        }

        public decimal Amount
        {
            get => _amount;
            set
            {
                _amount = value;
                OnPropertyChanged(nameof(Amount));
            }
        }

        public string DaysOrHours
        {
            get => _daysOrHours;
            set
            {
                _daysOrHours = value;
                OnPropertyChanged(nameof(DaysOrHours));
            }
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }

    public class PayrollData
    {
        public int EmployeeId { get; set; }
        public string FullName { get; set; }
        public string Position { get; set; }
        public decimal BaseSalary { get; set; }
        public string Period { get; set; }
        public string Year { get; set; }
        public List<PayrollItem> Accruals { get; set; }
        public List<PayrollItem> Deductions { get; set; }
        public decimal TotalAccrued { get; set; }
        public decimal TotalDeducted { get; set; }
        public decimal ToBePaid { get; set; }
    }
}