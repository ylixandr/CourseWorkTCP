using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using OfficeOpenXml;

namespace Client.ManagerFolder.DataAnalysys
{
    public class AnalysisViewModel : INotifyPropertyChanged
    {
        private readonly AnalysisService _analysisService;
        private ObservableCollection<FinancialAnalysisResult> _analysisResults;
        private string _selectedAnalysisType;
        private string _statusMessage;
        private SeriesCollection _equitySeries;
        private SeriesCollection _roeSeries;
        private SeriesCollection _capitalStructureSeries;
        private string[] _labels;
        private Func<double, string> _formatter;
        private Dictionary<string, bool> _visibleColumns;

        public ObservableCollection<string> AnalysisTypes { get; } = new ObservableCollection<string>
        {
            "Все", "Рентабельность капитала", "Долговая нагрузка", "Коэффициент автономии", "Динамика капитала"
        };

        public ObservableCollection<FinancialAnalysisResult> AnalysisResults
        {
            get => _analysisResults;
            set
            {
                _analysisResults = value;
                OnPropertyChanged(nameof(AnalysisResults));
            }
        }

        public string SelectedAnalysisType
        {
            get => _selectedAnalysisType;
            set
            {
                if (_selectedAnalysisType != value)
                {
                    _selectedAnalysisType = value;
                    OnPropertyChanged(nameof(SelectedAnalysisType));
                    UpdateVisibleColumns();
                    StatusMessage = $"Выбран тип анализа: {value}";
                }
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

        public SeriesCollection EquitySeries
        {
            get => _equitySeries;
            set
            {
                _equitySeries = value;
                OnPropertyChanged(nameof(EquitySeries));
            }
        }

        public SeriesCollection RoeSeries
        {
            get => _roeSeries;
            set
            {
                _roeSeries = value;
                OnPropertyChanged(nameof(RoeSeries));
            }
        }

        public SeriesCollection CapitalStructureSeries
        {
            get => _capitalStructureSeries;
            set
            {
                _capitalStructureSeries = value;
                OnPropertyChanged(nameof(CapitalStructureSeries));
            }
        }

        public string[] Labels
        {
            get => _labels;
            set
            {
                _labels = value;
                OnPropertyChanged(nameof(Labels));
            }
        }

        public Func<double, string> Formatter
        {
            get => _formatter;
            set
            {
                _formatter = value;
                OnPropertyChanged(nameof(Formatter));
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

        public ICommand ExportAnalysisCommand { get; }

        public AnalysisViewModel()
        {
            _analysisService = new AnalysisService();
            AnalysisResults = new ObservableCollection<FinancialAnalysisResult>();
            ExportAnalysisCommand = new RelayCommand(ExportAnalysis);
            SelectedAnalysisType = "Все";
            Formatter = value => value.ToString("N2");
            LoadAnalysisData();
        }

        private void LoadAnalysisData()
        {
            try
            {
                var results = _analysisService.CalculateFinancialMetrics();
                AnalysisResults.Clear();
                foreach (var result in results)
                    AnalysisResults.Add(result);

                Labels = results.Select(r => r.Period).ToArray();

                EquitySeries = new SeriesCollection
                {
                    new LineSeries
                    {
                        Title = "Собственный капитал",
                        Values = new ChartValues<double>(results.Select(r => r.EquityChange ?? 0))
                    }
                };

                RoeSeries = new SeriesCollection
                {
                    new ColumnSeries
                    {
                        Title = "Рентабельность капитала (%)",
                        Values = new ChartValues<double>(results.Select(r => r.ReturnOnEquity ?? 0))
                    }
                };

                var lastPeriod = results.OrderBy(r => r.Period).LastOrDefault()?.Period;
                if (lastPeriod != null)
                {
                    var capitalStructure = _analysisService.GetCapitalStructure(lastPeriod);
                    var hasNonZeroValues = capitalStructure.Values.Any(v => v != 0);
                    if (hasNonZeroValues)
                    {
                        CapitalStructureSeries = new SeriesCollection
                        {
                            new PieSeries { Title = "Собственный капитал", Values = new ChartValues<double> { capitalStructure["Собственный капитал"] }, DataLabels = true },
                            new PieSeries { Title = "Заемный капитал", Values = new ChartValues<double> { capitalStructure["Заемный капитал"] }, DataLabels = true }
                        };
                        StatusMessage = $"Круговая диаграмма загружена для периода: {lastPeriod}";
                    }
                    else
                    {
                        CapitalStructureSeries = new SeriesCollection();
                        StatusMessage = $"Данные о структуре капитала для периода {lastPeriod} нулевые.";
                    }
                }
                else
                {
                    CapitalStructureSeries = new SeriesCollection();
                    StatusMessage = "Нет периодов для анализа.";
                }

                StatusMessage = $"Анализ выполнен. Обработано периодов: {results.Count}";
                UpdateVisibleColumns();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка при выполнении анализа: {ex.Message}";
                CapitalStructureSeries = new SeriesCollection();
            }
        }

        private void UpdateVisibleColumns()
        {
            VisibleColumns = new Dictionary<string, bool>
            {
                { "ReturnOnEquity", _selectedAnalysisType == "Все" || _selectedAnalysisType == "Рентабельность капитала" },
                { "DebtToEquityRatio", _selectedAnalysisType == "Все" || _selectedAnalysisType == "Долговая нагрузка" },
                { "AutonomyRatio", _selectedAnalysisType == "Все" || _selectedAnalysisType == "Коэффициент автономии" },
                { "EquityChange", _selectedAnalysisType == "Все" || _selectedAnalysisType == "Динамика капитала" }
            };
            OnPropertyChanged(nameof(VisibleColumns));
            StatusMessage = $"Обновлены столбцы для типа анализа: {_selectedAnalysisType}";
        }

        private void ExportAnalysis(object parameter)
        {
            try
            {
                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Анализ капитала");
                    worksheet.Cells[1, 1].Value = "Период";
                    worksheet.Cells[1, 2].Value = "Рентабельность капитала (%)";
                    worksheet.Cells[1, 3].Value = "Долговая нагрузка";
                    worksheet.Cells[1, 4].Value = "Коэффициент автономии";
                    worksheet.Cells[1, 5].Value = "Изменение капитала";

                    for (int i = 0; i < AnalysisResults.Count; i++)
                    {
                        var result = AnalysisResults[i];
                        worksheet.Cells[i + 2, 1].Value = result.Period;
                        worksheet.Cells[i + 2, 2].Value = result.ReturnOnEquity?.ToString("F2");
                        worksheet.Cells[i + 2, 3].Value = result.DebtToEquityRatio?.ToString("F2");
                        worksheet.Cells[i + 2, 4].Value = result.AutonomyRatio?.ToString("F2");
                        worksheet.Cells[i + 2, 5].Value = result.EquityChange?.ToString("F2");
                    }

                    var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                    {
                        Filter = "Excel files (*.xlsx)|*.xlsx",
                        FileName = "AnalysisReport.xlsx"
                    };
                    if (saveFileDialog.ShowDialog() == true)
                    {
                        package.SaveAs(new System.IO.FileInfo(saveFileDialog.FileName));
                        StatusMessage = $"Анализ экспортирован в {saveFileDialog.FileName}";
                    }
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка при экспорте: {ex.Message}";
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}