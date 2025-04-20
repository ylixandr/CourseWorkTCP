using Client.ManagerFolder.DataAnalysys.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;

namespace Client.ManagerFolder.DataAnalysys
{
    public class DataViewModel : INotifyPropertyChanged
    {
        private readonly DataService _dataService;
        private ObservableCollection<UnifiedDataModel> _allData;
        private ObservableCollection<UnifiedDataModel> _filteredData;
        private string _selectedFormat;
        private string _periodFilter;
        private string _statusMessage;
        private Dictionary<string, bool> _visibleColumns;
        private bool _isFullScreen;
        private string _fullScreenButtonText = "Развернуть";

        public ObservableCollection<string> DataFormats { get; } = new ObservableCollection<string>
        {
            "Все", "Баланс", "Движение капитала", "Финансовые показатели"
        };

        public ObservableCollection<UnifiedDataModel> FilteredData
        {
            get => _filteredData;
            set
            {
                _filteredData = value;
                OnPropertyChanged(nameof(FilteredData));
            }
        }

        public string SelectedFormat
        {
            get => _selectedFormat;
            set
            {
                _selectedFormat = value;
                OnPropertyChanged(nameof(SelectedFormat));
                UpdateVisibleColumns();
                ApplyFilters();
            }
        }

        public string PeriodFilter
        {
            get => _periodFilter;
            set
            {
                _periodFilter = value;
                OnPropertyChanged(nameof(PeriodFilter));
                ApplyFilters();
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged(nameof(StatusMessage));
            }
        }

        public Dictionary<string, bool> VisibleColumns
        {
            get => _visibleColumns;
            set
            {
                _visibleColumns = value;
                OnPropertyChanged(nameof(VisibleColumns));
            }
        }

        public bool IsFullScreen
        {
            get => _isFullScreen;
            set
            {
                _isFullScreen = value;
                OnPropertyChanged(nameof(IsFullScreen));
                FullScreenButtonText = _isFullScreen ? "Свернуть" : "Развернуть";
            }
        }

        public string FullScreenButtonText
        {
            get => _fullScreenButtonText;
            set
            {
                _fullScreenButtonText = value;
                OnPropertyChanged(nameof(FullScreenButtonText));
            }
        }

        public ICommand ClearFilterCommand { get; }
        public ICommand ToggleFullScreenCommand { get; }

        public DataViewModel()
        {
            _dataService = new DataService();
            _allData = new ObservableCollection<UnifiedDataModel>();
            _filteredData = new ObservableCollection<UnifiedDataModel>();
            ClearFilterCommand = new RelayCommand(ClearFilters);
            ToggleFullScreenCommand = new RelayCommand(ToggleFullScreen);
            VisibleColumns = new Dictionary<string, bool>();
            SelectedFormat = "Все";
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                var balanceData = _dataService.LoadBalanceData();
                var capitalFlowData = _dataService.LoadCapitalFlowData();
                var financialMetricsData = _dataService.LoadFinancialMetricsData();

                _allData.Clear();
                foreach (var item in balanceData)
                    _allData.Add(new UnifiedDataModel(item, "Баланс"));
                foreach (var item in capitalFlowData)
                    _allData.Add(new UnifiedDataModel(item, "Движение капитала"));
                foreach (var item in financialMetricsData)
                    _allData.Add(new UnifiedDataModel(item, "Финансовые показатели"));

                StatusMessage = $"Данные загружены. Баланс: {balanceData.Count}, Движение капитала: {capitalFlowData.Count}, Финансовые показатели: {financialMetricsData.Count}. Всего записей: {_allData.Count}";
                UpdateVisibleColumns();
                ApplyFilters();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка при загрузке данных: {ex.Message}";
            }
        }

        private void UpdateVisibleColumns()
        {
            VisibleColumns = new Dictionary<string, bool>
            {
                { "Assets", _selectedFormat == "Все" || _selectedFormat == "Баланс" },
                { "Equity", _selectedFormat == "Все" || _selectedFormat == "Баланс" },
                { "BorrowedCapital", _selectedFormat == "Все" || _selectedFormat == "Баланс" },
                { "Liabilities", _selectedFormat == "Все" || _selectedFormat == "Баланс" },
                { "NetProfit", _selectedFormat == "Все" || _selectedFormat == "Баланс" },
                { "InitialEquity", _selectedFormat == "Все" || _selectedFormat == "Движение капитала" },
                { "CapitalIncrease", _selectedFormat == "Все" || _selectedFormat == "Движение капитала" },
                { "Losses", _selectedFormat == "Все" || _selectedFormat == "Движение капитала" },
                { "FinalEquity", _selectedFormat == "Все" || _selectedFormat == "Движение капитала" },
                { "ReturnOnEquity", _selectedFormat == "Все" || _selectedFormat == "Финансовые показатели" },
                { "LiquidityRatio", _selectedFormat == "Все" || _selectedFormat == "Финансовые показатели" },
                { "BorrowedCapitalShare", _selectedFormat == "Все" || _selectedFormat == "Финансовые показатели" }
            };
            OnPropertyChanged(nameof(VisibleColumns));
        }

        private void ApplyFilters()
        {
            var filtered = _allData.AsEnumerable();

            if (!string.IsNullOrEmpty(SelectedFormat) && SelectedFormat != "Все")
                filtered = filtered.Where(d => d.Format == SelectedFormat);

            if (!string.IsNullOrEmpty(PeriodFilter))
                filtered = filtered.Where(d => d.Period != null && d.Period.Contains(PeriodFilter, StringComparison.OrdinalIgnoreCase));

            FilteredData = new ObservableCollection<UnifiedDataModel>(filtered);
            StatusMessage = $"Фильтр применен. Отображено записей: {FilteredData.Count}";
        }

        private void ClearFilters(object parameter)
        {
            SelectedFormat = "Все";
            PeriodFilter = string.Empty;
            ApplyFilters();
        }

        private void ToggleFullScreen(object parameter)
        {
            IsFullScreen = !IsFullScreen;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class UnifiedDataModel
    {
        public string Period { get; set; }
        public double? Assets { get; set; }
        public double? Equity { get; set; }
        public double? BorrowedCapital { get; set; }
        public double? Liabilities { get; set; }
        public double? NetProfit { get; set; }
        public double? InitialEquity { get; set; }
        public double? CapitalIncrease { get; set; }
        public double? Losses { get; set; }
        public double? FinalEquity { get; set; }
        public double? ReturnOnEquity { get; set; }
        public double? LiquidityRatio { get; set; }
        public double? BorrowedCapitalShare { get; set; }
        public string Format { get; set; }
        public UnifiedDataModel(FinancialMetricsModel metrics, string format)
        {
            Period = metrics.Period;
            ReturnOnEquity = metrics.ReturnOnEquity;
            LiquidityRatio = metrics.LiquidityRatio;
            BorrowedCapitalShare = metrics.BorrowedCapitalShare;
            Format = format;
        }
        public UnifiedDataModel(BalanceModel balance, string format)
        {
            Period = balance.Period;
            Assets = balance.Assets;
            Equity = balance.Equity;
            BorrowedCapital = balance.BorrowedCapital;
            Liabilities = balance.Liabilities;
            NetProfit = balance.NetProfit;
            Format = format;
        }

        public UnifiedDataModel(CapitalFlowModel capitalFlow, string format)
        {
            Period = capitalFlow.Period;
            InitialEquity = capitalFlow.InitialEquity;
            CapitalIncrease = capitalFlow.CapitalIncrease;
            Losses = capitalFlow.Losses;
            FinalEquity = capitalFlow.FinalEquity;
            Format = format;
        }

      
    }
}