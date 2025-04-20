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
        private ObservableCollection<string> _sheetNames;
        private bool _isBalanceFormat;
        private bool _isCapitalFlowFormat;
        private bool _isFinancialMetricsFormat;
        private ObservableCollection<ColumnMapping> _columnMappings;
        private ObservableCollection<string> _availableColumns;
        private string _errorMessage;

        private readonly ExcelImportService _excelService;
        private readonly RelayCommand _previewCommand;
        private readonly RelayCommand _importCommand;

        public string SelectedFileName
        {
            get => _selectedFileName;
            set
            {
                _selectedFileName = value;
                OnPropertyChanged(nameof(SelectedFileName));
                _previewCommand.RaiseCanExecuteChanged();
                _importCommand.RaiseCanExecuteChanged();
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
                _previewCommand.RaiseCanExecuteChanged();
                _importCommand.RaiseCanExecuteChanged();
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
                _previewCommand.RaiseCanExecuteChanged();
                _importCommand.RaiseCanExecuteChanged();
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
                _previewCommand.RaiseCanExecuteChanged();
                _importCommand.RaiseCanExecuteChanged();
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
                _previewCommand.RaiseCanExecuteChanged();
                _importCommand.RaiseCanExecuteChanged();
            }
        }

        public ObservableCollection<ColumnMapping> ColumnMappings
        {
            get => _columnMappings;
            set
            {
                _columnMappings = value;
                OnPropertyChanged(nameof(ColumnMappings));
                _previewCommand.RaiseCanExecuteChanged();
                _importCommand.RaiseCanExecuteChanged();
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
        public ICommand PreviewCommand => _previewCommand;
        public ICommand ImportCommand => _importCommand;
        public ICommand CancelCommand { get; }

        public ImportWindowViewModel()
        {
            _excelService = new ExcelImportService();
            SelectFileCommand = new RelayCommand(SelectFile);
            _previewCommand = new RelayCommand(Preview, CanPreview);
            _importCommand = new RelayCommand(Import, CanImport);
            CancelCommand = new RelayCommand(Cancel);
            ColumnMappings = new ObservableCollection<ColumnMapping>();
            AvailableColumns = new ObservableCollection<string>();
            SheetNames = new ObservableCollection<string>();
            ErrorMessage = "Выберите файл Excel для импорта.";
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
                    ErrorMessage = $"Файл загружен: {Path.GetFileName(SelectedFileName)}. Найдено листов: {SheetNames.Count}. Выберите лист.";
                }
                catch (Exception ex)
                {
                    ErrorMessage = $"Ошибка при загрузке файла: {ex.Message}";
                    SheetNames.Clear();
                    SelectedSheet = null;
                    AvailableColumns.Clear();
                    _previewCommand.RaiseCanExecuteChanged();
                    _importCommand.RaiseCanExecuteChanged();
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
                    if (!columns.Any())
                    {
                        ErrorMessage = $"Ошибка: В листе '{SelectedSheet}' не найдены заголовки столбцов в первой строке.";
                        return;
                    }
                    foreach (var column in columns)
                        AvailableColumns.Add(column);
                    ErrorMessage = $"Лист '{SelectedSheet}' загружен. Найдено столбцов: {AvailableColumns.Count}. Выберите формат данных.";
                    UpdateColumnMappings();
                }
                catch (Exception ex)
                {
                    ErrorMessage = $"Ошибка при загрузке столбцов из листа '{SelectedSheet}': {ex.Message}";
                    AvailableColumns.Clear();
                }
            }
            else
            {
                ErrorMessage = "Выберите файл и лист для загрузки столбцов.";
            }
            _previewCommand.RaiseCanExecuteChanged();
            _importCommand.RaiseCanExecuteChanged();
        }

        private void UpdateColumnMappings()
        {
            ColumnMappings.Clear();
            List<ColumnMapping> mappings;
            if (IsBalanceFormat)
            {
                mappings = new List<ColumnMapping>
                {
                    new ColumnMapping("Период", true),
                    new ColumnMapping("Активы", true),
                    new ColumnMapping("Собственный капитал", true),
                    new ColumnMapping("Заемный капитал", false),
                    new ColumnMapping("Обязательства", false),
                    new ColumnMapping("Чистая прибыль", false)
                };
            }
            else if (IsCapitalFlowFormat)
            {
                mappings = new List<ColumnMapping>
                {
                    new ColumnMapping("Период", true),
                    new ColumnMapping("Собственный капитал (начало)", false),
                    new ColumnMapping("Прирост капитала", false),
                    new ColumnMapping("Убытки", false),
                    new ColumnMapping("Собственный капитал (конец)", true)
                };
            }
            else if (IsFinancialMetricsFormat)
            {
                mappings = new List<ColumnMapping>
                {
                    new ColumnMapping("Период", true),
                    new ColumnMapping("Рентабельность капитала (%)", false),
                    new ColumnMapping("Коэффициент ликвидности", false),
                    new ColumnMapping("Доля заемного капитала (%)", false)
                };
            }
            else
            {
                mappings = new List<ColumnMapping>();
                ErrorMessage = "Выберите формат данных для отображения ожидаемых столбцов.";
                return;
            }

            foreach (var mapping in mappings)
            {
                ColumnMappings.Add(mapping);
                var matchingColumn = AvailableColumns.FirstOrDefault(c => c.Equals(mapping.ExpectedColumn, StringComparison.OrdinalIgnoreCase));
                if (matchingColumn != null)
                    mapping.SelectedColumn = matchingColumn;
            }

            ErrorMessage = AvailableColumns.Any()
                ? $"Сопоставьте столбцы. Доступные столбцы: {string.Join(", ", AvailableColumns)}."
                : "Нет доступных столбцов для сопоставления. Проверьте первую строку листа.";
            _previewCommand.RaiseCanExecuteChanged();
            _importCommand.RaiseCanExecuteChanged();
        }

        private bool CanPreview(object parameter)
        {
            return !string.IsNullOrEmpty(SelectedFileName) && !string.IsNullOrEmpty(SelectedSheet) && ColumnMappings.Any();
        }

        private void Preview(object parameter)
        {
            try
            {
                var data = _excelService.PreviewData(SelectedFileName, SelectedSheet, ColumnMappings.ToList());
                ErrorMessage = $"Предпросмотр: Найдено {data.Count} строк данных.";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка при предпросмотре данных: {ex.Message}";
            }
        }

        private bool CanImport(object parameter)
        {
            return CanPreview(parameter) && ColumnMappings.All(cm => !cm.IsRequired || !string.IsNullOrEmpty(cm.SelectedColumn));
        }

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
                ErrorMessage = $"Ошибка при импорте данных: {ex.Message}";
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
