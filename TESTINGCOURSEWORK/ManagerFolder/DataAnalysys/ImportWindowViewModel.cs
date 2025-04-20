using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;

namespace Client.ManagerFolder.DataAnalysys
{
    public class ImportWindowViewModel : INotifyPropertyChanged
    {
        private string _selectedFileName;
        private string _selectedSheet;
        private ObservableCollection<string> _sheetNames; // Изменено на ObservableCollection
        private bool _isBalanceFormat;
        private bool _isCapitalFlowFormat;
        private bool _isFinancialMetricsFormat;
        private ObservableCollection<ColumnMapping> _columnMappings;
        private ObservableCollection<string> _availableColumns; // Изменено на ObservableCollection
        private string _errorMessage;

        private readonly ExcelImportService _excelService;

        public string SelectedFileName
        {
            get => _selectedFileName;
            set
            {
                _selectedFileName = value;
                OnPropertyChanged(nameof(SelectedFileName));
            }
        }

        public string SelectedSheet
        {
            get => _selectedSheet;
            set
            {
                _selectedSheet = value;
                OnPropertyChanged(nameof(SelectedSheet));
                LoadAvailableColumns();
            }
        }

        public ObservableCollection<string> SheetNames
        {
            get => _sheetNames;
            set
            {
                _sheetNames = value;
                OnPropertyChanged(nameof(SheetNames));
            }
        }

        public bool IsBalanceFormat
        {
            get => _isBalanceFormat;
            set
            {
                _isBalanceFormat = value;
                OnPropertyChanged(nameof(IsBalanceFormat));
                UpdateColumnMappings();
            }
        }

        public bool IsCapitalFlowFormat
        {
            get => _isCapitalFlowFormat;
            set
            {
                _isCapitalFlowFormat = value;
                OnPropertyChanged(nameof(IsCapitalFlowFormat));
                UpdateColumnMappings();
            }
        }

        public bool IsFinancialMetricsFormat
        {
            get => _isFinancialMetricsFormat;
            set
            {
                _isFinancialMetricsFormat = value;
                OnPropertyChanged(nameof(IsFinancialMetricsFormat));
                UpdateColumnMappings();
            }
        }

        public ObservableCollection<ColumnMapping> ColumnMappings
        {
            get => _columnMappings;
            set
            {
                _columnMappings = value;
                OnPropertyChanged(nameof(ColumnMappings));
            }
        }

        public ObservableCollection<string> AvailableColumns
        {
            get => _availableColumns;
            set
            {
                _availableColumns = value;
                OnPropertyChanged(nameof(AvailableColumns));
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged(nameof(ErrorMessage));
            }
        }

        public ICommand SelectFileCommand { get; }
        public ICommand PreviewCommand { get; }
        public ICommand ImportCommand { get; }
        public ICommand CancelCommand { get; }

        public ImportWindowViewModel()
        {
            _excelService = new ExcelImportService();
            SelectFileCommand = new RelayCommand(SelectFile);
            PreviewCommand = new RelayCommand(Preview, CanPreview);
            ImportCommand = new RelayCommand(Import, CanImport);
            CancelCommand = new RelayCommand(Cancel);
            ColumnMappings = new ObservableCollection<ColumnMapping>();
            AvailableColumns = new ObservableCollection<string>();
            SheetNames = new ObservableCollection<string>();
        }

        private void SelectFile(object parameter)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Excel Files|*.xlsx;*.xls"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                SelectedFileName = openFileDialog.FileName;
                try
                {
                    var sheets = _excelService.GetSheetNames(SelectedFileName).ToList();
                    SheetNames.Clear();
                    foreach (var sheet in sheets)
                        SheetNames.Add(sheet);
                    SelectedSheet = SheetNames.FirstOrDefault();
                    ErrorMessage = $"Файл загружен. Найдено листов: {SheetNames.Count}";
                }
                catch (Exception ex)
                {
                    ErrorMessage = $"Ошибка при загрузке файла: {ex.Message}";
                    SheetNames.Clear();
                    SelectedSheet = null;
                    AvailableColumns.Clear();
                }
            }
        }

        private void LoadAvailableColumns()
        {
            AvailableColumns.Clear();
            if (!string.IsNullOrEmpty(SelectedFileName) && !string.IsNullOrEmpty(SelectedSheet))
            {
                try
                {
                    var columns = _excelService.GetColumnHeaders(SelectedFileName, SelectedSheet).ToList();
                    foreach (var column in columns)
                        AvailableColumns.Add(column);
                    ErrorMessage = $"Загружено столбцов: {AvailableColumns.Count}";
                    UpdateColumnMappings();
                }
                catch (Exception ex)
                {
                    ErrorMessage = $"Ошибка при загрузке столбцов: {ex.Message}";
                    AvailableColumns.Clear();
                }
            }
            else
            {
                ErrorMessage = "Выберите файл и лист.";
            }
        }

        private void UpdateColumnMappings()
        {
            ColumnMappings.Clear();
            if (IsBalanceFormat)
            {
                ColumnMappings.Add(new ColumnMapping("Период", true));
                ColumnMappings.Add(new ColumnMapping("Активы", true));
                ColumnMappings.Add(new ColumnMapping("Собственный капитал", true));
                ColumnMappings.Add(new ColumnMapping("Заемный капитал", false));
                ColumnMappings.Add(new ColumnMapping("Обязательства", false));
                ColumnMappings.Add(new ColumnMapping("Чистая прибыль", false));
            }
            else if (IsCapitalFlowFormat)
            {
                ColumnMappings.Add(new ColumnMapping("Период", true));
                ColumnMappings.Add(new ColumnMapping("Собственный капитал (начало)", false));
                ColumnMappings.Add(new ColumnMapping("Прирост капитала", false));
                ColumnMappings.Add(new ColumnMapping("Убытки", false));
                ColumnMappings.Add(new ColumnMapping("Собственный капитал (конец)", true));
            }
            else if (IsFinancialMetricsFormat)
            {
                ColumnMappings.Add(new ColumnMapping("Период", true));
                ColumnMappings.Add(new ColumnMapping("Рентабельность капитала (%)", false));
                ColumnMappings.Add(new ColumnMapping("Коэффициент ликвидности", false));
                ColumnMappings.Add(new ColumnMapping("Доля заемного капитала (%)", false));
            }

            // Автоматическая попытка сопоставления
            foreach (var mapping in ColumnMappings)
            {
                var matchingColumn = AvailableColumns.FirstOrDefault(c => c.Equals(mapping.ExpectedColumn, StringComparison.OrdinalIgnoreCase));
                if (matchingColumn != null)
                    mapping.SelectedColumn = matchingColumn;
            }
        }

        private bool CanPreview(object parameter) => !string.IsNullOrEmpty(SelectedFileName) && !string.IsNullOrEmpty(SelectedSheet) && ColumnMappings.Any();

        private void Preview(object parameter)
        {
            try
            {
                var data = _excelService.PreviewData(SelectedFileName, SelectedSheet, ColumnMappings.ToList());
                ErrorMessage = $"Предпросмотр: Найдено {data.Count} строк данных.";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка при предпросмотре: {ex.Message}";
            }
        }

        private bool CanImport(object parameter) => CanPreview(parameter) && ColumnMappings.All(cm => !cm.IsRequired || !string.IsNullOrEmpty(cm.SelectedColumn));

        private void Import(object parameter)
        {
            try
            {
                if (IsBalanceFormat)
                {
                    var data = _excelService.ImportBalanceData(SelectedFileName, SelectedSheet, ColumnMappings.ToList());
                    _excelService.SaveToJson(data, "balance_data.json");
                    ErrorMessage = "Данные успешно импортированы и сохранены в balance_data.json";
                }
                else if (IsCapitalFlowFormat)
                {
                    var data = _excelService.ImportCapitalFlowData(SelectedFileName, SelectedSheet, ColumnMappings.ToList());
                    _excelService.SaveToJson(data, "capital_flow_data.json");
                    ErrorMessage = "Данные успешно импортированы и сохранены в capital_flow_data.json";
                }
                else if (IsFinancialMetricsFormat)
                {
                    var data = _excelService.ImportFinancialMetricsData(SelectedFileName, SelectedSheet, ColumnMappings.ToList());
                    _excelService.SaveToJson(data, "financial_metrics_data.json");
                    ErrorMessage = "Данные успешно импортированы и сохранены в financial_metrics_data.json";
                }
                var window = Application.Current.Windows.OfType<ImportWindow>().FirstOrDefault();
                window?.Close();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка при импорте: {ex.Message}";
            }
        }

        private void Cancel(object parameter)
        {
            var window = Application.Current.Windows.OfType<ImportWindow>().FirstOrDefault();
            window?.Close();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ColumnMapping : INotifyPropertyChanged
    {
        private string _selectedColumn;

        public string ExpectedColumn { get; }
        public bool IsRequired { get; }
        public string SelectedColumn
        {
            get => _selectedColumn;
            set
            {
                _selectedColumn = value;
                OnPropertyChanged(nameof(SelectedColumn));
            }
        }

        public ColumnMapping(string expectedColumn, bool isRequired)
        {
            ExpectedColumn = expectedColumn;
            IsRequired = isRequired;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}