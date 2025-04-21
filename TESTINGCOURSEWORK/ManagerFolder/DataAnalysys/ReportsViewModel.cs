using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Client.ManagerFolder.DataAnalysys
{
    public class ReportsViewModel : INotifyPropertyChanged
    {
        private readonly AnalysisService _analysisService;
        private string _selectedReportType;
        private string _statusMessage;
        private string _reportText;

        public ObservableCollection<string> ReportTypes { get; } = new ObservableCollection<string>
        {
            "Полный отчет", "Рентабельность капитала", "Долговая нагрузка", "Коэффициент автономии", "Динамика капитала"
        };

        public string SelectedReportType
        {
            get => _selectedReportType;
            set
            {
                if (_selectedReportType != value)
                {
                    _selectedReportType = value;
                    OnPropertyChanged(nameof(SelectedReportType));
                    UpdateReportText(); // Только обновляем текст
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

        public string ReportText
        {
            get => _reportText;
            set
            {
                _reportText = value;
                OnPropertyChanged(nameof(ReportText));
            }
        }

        public ICommand GenerateReportCommand { get; }

        public ReportsViewModel()
        {
            _analysisService = new AnalysisService();
            GenerateReportCommand = new RelayCommand(GenerateAndSaveReport);
            SelectedReportType = "Полный отчет";
            UpdateReportText(); // Инициализация текста отчета
        }

        private void UpdateReportText()
        {
            try
            {
                var results = _analysisService.CalculateFinancialMetrics();
                if (!results.Any())
                {
                    ReportText = "Нет данных для анализа.";
                    StatusMessage = "Данные для отчета отсутствуют.";
                    return;
                }

                var report = new StringBuilder();
                report.AppendLine($"Отчет по анализу капитала: {SelectedReportType}");
                report.AppendLine($"Дата генерации: {DateTime.Now:dd.MM.yyyy HH:mm}");
                report.AppendLine(new string('=', 50));
                report.AppendLine();

                var latestResult = results.OrderBy(r => r.Period).Last();

                if (_selectedReportType == "Полный отчет" || _selectedReportType == "Рентабельность капитала")
                {
                    double? roe = latestResult.ReturnOnEquity;
                    report.AppendLine("Рентабельность капитала (ROE):");
                    report.AppendLine($"Значение: {roe?.ToString("F2") ?? "нет данных"}%");
                    if (roe.HasValue)
                    {
                        if (roe > 15)
                            report.AppendLine("Оценка: Высокая рентабельность. Компания эффективно использует капитал.");
                        else if (roe > 5)
                            report.AppendLine("Оценка: Средняя рентабельность. Есть потенциал для роста.");
                        else
                            report.AppendLine("Оценка: Низкая рентабельность. Требуются меры для повышения доходности.");
                        report.AppendLine("Рекомендации:");
                        report.AppendLine("- Увеличьте операционную прибыль за счет оптимизации процессов.");
                        report.AppendLine("- Рассмотрите инвестиции в высокодоходные проекты.");
                    }
                    report.AppendLine();
                }

                if (_selectedReportType == "Полный отчет" || _selectedReportType == "Долговая нагрузка")
                {
                    double? debtToEquity = latestResult.DebtToEquityRatio;
                    report.AppendLine("Долговая нагрузка (Debt-to-Equity):");
                    report.AppendLine($"Значение: {debtToEquity?.ToString("F2") ?? "нет данных"}");
                    if (debtToEquity.HasValue)
                    {
                        if (debtToEquity < 0.5)
                            report.AppendLine("Оценка: Низкая долговая нагрузка. Финансовая устойчивость высокая.");
                        else if (debtToEquity < 1.5)
                            report.AppendLine("Оценка: Умеренная долговая нагрузка. Контроль долгов необходим.");
                        else
                            report.AppendLine("Оценка: Высокая долговая нагрузка. Риски финансовой нестабильности.");
                        report.AppendLine("Рекомендации:");
                        report.AppendLine("- Снизьте зависимость от заемного капитала.");
                        report.AppendLine("- Пересмотрите структуру долгов, выбрав более дешевые источники.");
                    }
                    report.AppendLine();
                }

                if (_selectedReportType == "Полный отчет" || _selectedReportType == "Коэффициент автономии")
                {
                    double? autonomy = latestResult.AutonomyRatio;
                    report.AppendLine("Коэффициент автономии:");
                    report.AppendLine($"Значение: {autonomy?.ToString("F2") ?? "нет данных"}");
                    if (autonomy.HasValue)
                    {
                        if (autonomy > 0.5)
                            report.AppendLine("Оценка: Высокая автономия. Компания независима от внешнего финансирования.");
                        else if (autonomy > 0.3)
                            report.AppendLine("Оценка: Средняя автономия. Желательно увеличить собственный капитал.");
                        else
                            report.AppendLine("Оценка: Низкая автономия. Высокая зависимость от кредиторов.");
                        report.AppendLine("Рекомендации:");
                        report.AppendLine("- Увеличьте долю собственного капитала через реинвестирование прибыли.");
                        report.AppendLine("- Сократите использование заемных средств.");
                    }
                    report.AppendLine();
                }

                if (_selectedReportType == "Полный отчет" || _selectedReportType == "Динамика капитала")
                {
                    double? equityChange = latestResult.EquityChange;
                    report.AppendLine("Динамика собственного капитала:");
                    report.AppendLine($"Значение: {equityChange?.ToString("F2") ?? "нет данных"}");
                    if (equityChange.HasValue)
                    {
                        if (equityChange > 0)
                            report.AppendLine("Оценка: Положительная динамика. Капитал растет.");
                        else if (equityChange == 0)
                            report.AppendLine("Оценка: Стабильный капитал. Рост отсутствует.");
                        else
                            report.AppendLine("Оценка: Отрицательная динамика. Капитал сокращается.");
                        report.AppendLine("Рекомендации:");
                        report.AppendLine("- Для роста: привлекайте инвесторов или реинвестируйте прибыль.");
                        report.AppendLine("- Для снижения затрат: оптимизируйте операционные расходы.");
                    }
                    report.AppendLine();
                }

                report.AppendLine("Общие рекомендации:");
                report.AppendLine("- Проведите аудит расходов для выявления неэффективных затрат.");
                report.AppendLine("- Разработайте стратегию роста, ориентированную на новые рынки или продукты.");
                report.AppendLine("- Регулярно мониторьте ключевые финансовые показатели.");

                ReportText = report.ToString();
                StatusMessage = $"Отчет '{SelectedReportType}' готов для сохранения.";
            }
            catch (Exception ex)
            {
                ReportText = "Ошибка при генерации отчета.";
                StatusMessage = $"Ошибка: {ex.Message}";
            }
        }

        private void GenerateAndSaveReport(object parameter)
        {
            try
            {
                if (string.IsNullOrEmpty(ReportText) || ReportText.StartsWith("Ошибка"))
                {
                    StatusMessage = "Нет отчета для сохранения. Сначала сгенерируйте отчет.";
                    return;
                }

                var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Text files (*.txt)|*.txt",
                    FileName = $"Report_{SelectedReportType}_{DateTime.Now:yyyyMMdd}.txt"
                };
                if (saveFileDialog.ShowDialog() == true)
                {
                    File.WriteAllText(saveFileDialog.FileName, ReportText);
                    StatusMessage = $"Отчет сохранен в {saveFileDialog.FileName}";
                }
                else
                {
                    StatusMessage = $"Сохранение отменено.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка при сохранении: {ex.Message}";
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}