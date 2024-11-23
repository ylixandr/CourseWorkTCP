using Newtonsoft.Json;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TESTINGCOURSEWORK.Models;

namespace TESTINGCOURSEWORK.ManagerFolder
{
    /// <summary>
    /// Логика взаимодействия для SalaryPage.xaml
    /// </summary>
    public partial class SalaryPage : Window
    {
        public SalaryPage()
        {
            InitializeComponent();
        }

        private async void OnCalculateSalaryClick(object sender, RoutedEventArgs e)
        {
            if (SalaryDatePicker.SelectedDate == null)
            {
                MessageBox.Show("Пожалуйста, выберите дату.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                DateTime selectedDate = SalaryDatePicker.SelectedDate.Value;
                string command = "calculate_salary";

                // Отправка команды на сервер
                var request = new { Date = selectedDate.ToString("yyyy-MM-dd") };
                string jsonRequest = JsonConvert.SerializeObject(request);

                string response = await NetworkService.Instance.SendMessageAsync($"{command}:{jsonRequest}");

                if (response.StartsWith("Error"))
                {
                    MessageBox.Show($"Ошибка при начислении зарплаты: {response}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Получение данных о зарплатах
                var salaryRecords = JsonConvert.DeserializeObject<List<SalaryRecord>>(response);

                // Генерация Excel-файла
                CreateSalaryExcelReport(salaryRecords, selectedDate);

                MessageBox.Show("Зарплата успешно начислена, отчет сохранен на рабочем столе!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CreateSalaryExcelReport(List<SalaryRecord> salaryRecords, DateTime calculationDate)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Отчет по зарплатам");

                // Заголовки
                worksheet.Cells[1, 1].Value = "Фамилия";
                worksheet.Cells[1, 2].Value = "Дата начисления";
                worksheet.Cells[1, 3].Value = "Сумма зарплаты";

                // Заполнение данными
                int row = 2;
                foreach (var record in salaryRecords)
                {
                    worksheet.Cells[row, 1].Value = record.LastName;
                    worksheet.Cells[row, 2].Value = calculationDate.ToString("yyyy-MM-dd");
                    worksheet.Cells[row, 3].Value = record.Salary;
                    row++;
                }

                // Сохранение файла
                string filePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "SalaryReport.xlsx");
                package.SaveAs(new FileInfo(filePath));
            }
        }

    }
}
