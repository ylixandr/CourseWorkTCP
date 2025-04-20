using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using OfficeOpenXml;

namespace Client.ManagerFolder.DataAnalysys
{
    public class ReportsViewModel : INotifyPropertyChanged
    {
        private readonly AnalysisService _analysisService;
        private ObservableCollection<FinancialAnalysisResult> _reportResults;
        private string _selectedReportType;
        private string _statusMessage;

        public ObservableCollection<string> ReportTypes { get; } = new ObservableCollection<string>
        {
            "Полный отчет", "Рентабельность капитала", "Долговая нагрузка", "Коэффициент автономии", "Динамика капитала"
        };

        public ObservableCollection<FinancialAnalysisResult> ReportResults
        {
            get => _reportResults;
            set
            {
                _reportResults = value;
                OnPropertyChanged(nameof(ReportResults));
            }
        }

        public string SelectedReportType
        {
            get => _selectedReportType;
            set
            {
                if (_selectedReportType != value)
                {
                    _selectedReportType = value;
                    OnPropertyChanged(nameof(SelectedReportType));
                    GenerateReport();
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

        public ICommand GenerateReportCommand { get; }

        public ReportsViewModel()
        {
            _analysisService = new AnalysisService();
            ReportResults = new ObservableCollection<FinancialAnalysisResult>();
            GenerateReportCommand = new RelayCommand(GenerateReport);
            SelectedReportType = "Полный отчет";
            GenerateReport();
        }

        private void GenerateReport(object parameter = null)
        {
            try
            {
                var results = _analysisService.CalculateFinancialMetrics();
                ReportResults.Clear();
                foreach (var result in results)
                {
                    ReportResults.Add(result);
                }

                StatusMessage = $"Отчет '{SelectedReportType}' сгенерирован. Периодов: {results.Count}";

                // Экспорт в Excel
                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Отчет по капиталу");
                    worksheet.Cells[1, 1].Value = "Период";
                    int col = 2;

                    if (_selectedReportType == "Полный отчет" || _selectedReportType == "Рентабельность капитала")
                        worksheet.Cells[1, col++].Value = "Рентабельность капитала (%)";
                    if (_selectedReportType == "Полный отчет" || _selectedReportType == "Долговая нагрузка")
                        worksheet.Cells[1, col++].Value = "Долговая нагрузка";
                    if (_selectedReportType == "Полный отчет" || _selectedReportType == "Коэффициент автономии")
                        worksheet.Cells[1, col++].Value = "Коэффициент автономии";
                    if (_selectedReportType == "Полный отчет" || _selectedReportType == "Динамика капитала")
                        worksheet.Cells[1, col++].Value = "Изменение капитала";

                    for (int i = 0; i < results.Count; i++)
                    {
                        var result = results[i];
                        worksheet.Cells[i + 2, 1].Value = result.Period;
                        col = 2;

                        if (_selectedReportType == "Полный отчет" || _selectedReportType == "Рентабельность капитала")
                            worksheet.Cells[i + 2, col++].Value = result.ReturnOnEquity?.ToString("F2");
                        if (_selectedReportType == "Полный отчет" || _selectedReportType == "Долговая нагрузка")
                            worksheet.Cells[i + 2, col++].Value = result.DebtToEquityRatio?.ToString("F2");
                        if (_selectedReportType == "Полный отчет" || _selectedReportType == "Коэффициент автономии")
                            worksheet.Cells[i + 2, col++].Value = result.AutonomyRatio?.ToString("F2");
                        if (_selectedReportType == "Полный отчет" || _selectedReportType == "Динамика капитала")
                            worksheet.Cells[i + 2, col++].Value = result.EquityChange?.ToString("F2");
                    }

                    var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                    {
                        Filter = "Excel files (*.xlsx)|*.xlsx",
                        FileName = $"Report_{SelectedReportType}_{DateTime.Now:yyyyMMdd}.xlsx"
                    };
                    if (saveFileDialog.ShowDialog() == true)
                    {
                        package.SaveAs(new System.IO.FileInfo(saveFileDialog.FileName));
                        StatusMessage = $"Отчет экспортирован в {saveFileDialog.FileName}";
                    }
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка при генерации отчета: {ex.Message}";
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}