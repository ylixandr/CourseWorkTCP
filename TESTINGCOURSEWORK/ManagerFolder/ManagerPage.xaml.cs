using Newtonsoft.Json;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TCPServerLab2;
using TESTINGCOURSEWORK.ManagerFolder;

namespace TESTINGCOURSEWORK
{
    /// <summary>
    /// Логика взаимодействия для ManagerPage.xaml
    /// </summary>
    public partial class ManagerPage : Window
    {
        private ObservableCollection<TCPServerLab2.Transaction> transactions;
        public ObservableCollection<TCPServerLab2.Transaction> Transactions { get { return transactions; } set { transactions = value; } }

        private ObservableCollection<Employee> employees;
        public ObservableCollection<Employee> Employees

        {
            get { return employees; }
            set { employees = value; }
        }

        public ManagerPage()
        {
            InitializeComponent();
            HideAllGrid();

        }



        private async void Employee_Button_Click(object sender, RoutedEventArgs e)
        {
            HideAllGrid();
            topEditingPanel.Visibility = Visibility.Visible;
            EmployeeDataGrid.Visibility = Visibility.Visible;
            try
            {
                string response = await NetworkService.Instance.SendMessageAsync("getEmployees");

                if (response == "NoData")
                {
                    MessageBox.Show("Нет данных для отображения.");
                    return;
                }
                else
                {
                    List<Employee>? users = new List<Employee>();
                    users = JsonConvert.DeserializeObject<List<Employee>>(response);
                    Employees = new ObservableCollection<Employee>(users);
                    EmployeeDataGrid.ItemsSource = Employees;

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private void Add_Button_Click(object sender, RoutedEventArgs e)
        {
            AddEmployeePage addEmployeePage = new AddEmployeePage();
            addEmployeePage.Show();
            this.Hide();
        }

        private async void Delete_Button_Click(object sender, RoutedEventArgs e)
        {
            if (EmployeeDataGrid.SelectedItem is Employee selectedEmployee)
            {
                // Формирование сообщения для сервера
                string loginData = $"deleteEmployee:{selectedEmployee.EmployeeId}";

                // Отправка сообщения на сервер
                string response = await NetworkService.Instance.SendMessageAsync(loginData);

                // Проверка ответа сервера
                if (response == "User deleted")
                {
                    MessageBox.Show("Работник успешно уволен.");
                    // Обновление отображения
                    (EmployeeDataGrid.ItemsSource as ObservableCollection<Employee>)?.Remove(selectedEmployee);
                }
                else
                {
                    MessageBox.Show("Не удалось уволить работника.");
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите работника");
            }
        }

        private async void Exxport_Button_Click(object sender, RoutedEventArgs e)
        {

            string command = "export_employee_data";
            string employeeDataJson = await NetworkService.Instance.SendMessageAsync(command);

            // Десериализация данных из JSON
            var employees = JsonConvert.DeserializeObject<List<Employee>>(employeeDataJson);

            // Создание и сохранение Excel-файла
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Employees");

                // Добавление заголовков
                worksheet.Cells[1, 1].Value = "ID";
                worksheet.Cells[1, 2].Value = "Имя";
                worksheet.Cells[1, 3].Value = "Фамилия";
                worksheet.Cells[1, 4].Value = "Отчество";
                worksheet.Cells[1, 5].Value = "Зарплата";
                worksheet.Cells[1, 6].Value = "Дата рождения";
                worksheet.Cells[1, 7].Value = "Позиция";
                worksheet.Cells[1, 8].Value = "Дата найма";

                // Заполнение данных сотрудников
                int row = 2;
                foreach (var employee in employees)
                {
                    worksheet.Cells[row, 1].Value = employee.EmployeeId;
                    worksheet.Cells[row, 2].Value = employee.FirstName;
                    worksheet.Cells[row, 3].Value = employee.LastName;
                    worksheet.Cells[row, 4].Value = employee.MiddleName;
                    worksheet.Cells[row, 5].Value = employee.Salary;
                    worksheet.Cells[row, 6].Value = employee.BirthDate?.ToString("yyyy-MM-dd"); // Обрабатываем nullable дату
                    worksheet.Cells[row, 7].Value = employee.Position;
                    worksheet.Cells[row, 8].Value = employee.HireDate?.ToString("yyyy-MM-dd"); // Обрабатываем nullable дату
                    row++;
                }

                string filePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Employees.xlsx");
                package.SaveAs(new FileInfo(filePath));
                MessageBox.Show($"Файл сохранен на рабочем столе как {filePath}");
            }

            
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private async  void Balance_Button_Click(object sender, RoutedEventArgs e)
        {
            HideAllGrid();
            EditorGrid.Visibility = Visibility.Visible;
            TransactionDataGrid.Visibility = Visibility.Visible;
            try
            {
                string response = await NetworkService.Instance.SendMessageAsync("getTransactions");

                if (response == "NoData")
                {
                    MessageBox.Show("Нет данных для отображения.");
                    return;
                }
                else
                {
                    List<TCPServerLab2.Transaction>? transactions = new List<TCPServerLab2.Transaction>();
                    transactions = JsonConvert.DeserializeObject<List<TCPServerLab2.Transaction>>(response);
                    Transactions = new ObservableCollection<TCPServerLab2.Transaction>(transactions);
                    TransactionDataGrid.ItemsSource = Transactions;

                }

            }
            catch (Exception ex)
            {

                MessageBox.Show($"Ошибка: {ex.Message}");
            }

        }

        private async void ExportToExcelButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string command = "export_transactions";
                string transactionsJson = await NetworkService.Instance.SendMessageAsync(command);

                if (transactionsJson == "Error")
                {
                    MessageBox.Show("Ошибка при получении данных о транзакциях.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Десериализация данных из JSON
                var transactions = JsonConvert.DeserializeObject<List<TCPServerLab2.Transaction>>(transactionsJson);

                // Создание и сохранение Excel-файла
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                using (var package = new ExcelPackage())
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Transactions");

                    // Добавление заголовков
                    worksheet.Cells[1, 1].Value = "ID";
                    worksheet.Cells[1, 2].Value = "Дата транзакции";
                    worksheet.Cells[1, 3].Value = "Сумма";
                    worksheet.Cells[1, 4].Value = "Тип транзакции";
                    worksheet.Cells[1, 5].Value = "Описание";

                    // Заполнение данных транзакций
                    int row = 2;
                    foreach (var transaction in transactions)
                    {
                        worksheet.Cells[row, 1].Value = transaction.Id;
                        worksheet.Cells[row, 2].Value = transaction.TransactionDate.ToString("yyyy-MM-dd HH:mm:ss");
                        worksheet.Cells[row, 3].Value = transaction.Amount;
                        worksheet.Cells[row, 4].Value = transaction.TransactionType;
                        worksheet.Cells[row, 5].Value = transaction.Description;
                        row++;
                    }

                    string filePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Transactions.xlsx");
                    package.SaveAs(new FileInfo(filePath));
                    MessageBox.Show($"Файл сохранен на рабочем столе как {filePath}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private async  void DeleteTransactionButton_Click(object sender, RoutedEventArgs e)
        {
            if (TransactionDataGrid.SelectedItem is TCPServerLab2.Transaction selectedTransaction)
            {
                // Формирование сообщения для сервера
                string loginData = $"deleteTransaction:{selectedTransaction.Id}";

                // Отправка сообщения на сервер
                string response = await NetworkService.Instance.SendMessageAsync(loginData);

                // Проверка ответа сервера
                if (response == "Transaction deleted")
                {
                    MessageBox.Show(" Транзакция успешно удалена");
                    // Обновление отображения
                    (TransactionDataGrid.ItemsSource as ObservableCollection<TCPServerLab2.Transaction>)?.Remove(selectedTransaction);
                }
                else
                {
                    MessageBox.Show("Не удалось удалить транзакцию.");
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите транзакцию");
            }
        }

        private async void GenerateReportButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string command = "generate_report";
                string reportJson = await NetworkService.Instance.SendMessageAsync(command);

                // Десериализация данных
                var report = JsonConvert.DeserializeObject<ReportModel>(reportJson);

                // Формирование текста отчета
                var reportText = new StringBuilder();
                reportText.AppendLine("=== Финансовый отчет ===");
                reportText.AppendLine($"Баланс компании (начальный): {report.Balance:C}");
                reportText.AppendLine($"Баланс после всех операций: {report.Balance + report.IncomeSum - report.ExpenseSum:C}");
                reportText.AppendLine($"Общее количество транзакций: {report.TotalTransactions}");
                reportText.AppendLine($"Общая сумма поступлений: {report.IncomeSum:C}");
                reportText.AppendLine($"Общая сумма списаний: {report.ExpenseSum:C}");
                reportText.AppendLine($"Средняя сумма транзакции: {report.AverageTransaction:C}");
                reportText.AppendLine($"Самая крупная транзакция: {report.MaxTransaction?.Amount:C} ({report.MaxTransaction?.Description})");
                reportText.AppendLine($"Самая мелкая транзакция: {report.MinTransaction?.Amount:C} ({report.MinTransaction?.Description})");
                reportText.AppendLine("\n=== Транзакции по месяцам ===");
                foreach (var month in report.MonthlySummary)
                {
                    reportText.AppendLine($"Месяц: {month.Month}");
                    reportText.AppendLine($"  Количество транзакций: {month.Total}");
                    reportText.AppendLine($"  Поступления: {month.Income:C}");
                    reportText.AppendLine($"  Списания: {month.Expense:C}");
                }
                reportText.AppendLine("\n=== Ошибки или подозрительные транзакции ===");
                foreach (var error in report.Errors)
                {
                    reportText.AppendLine($"  Тип: {error.TransactionType}, Сумма: {error.Amount:C}");
                }

                // Показ отчета в окне
                MessageBox.Show(reportText.ToString(), "Отчетность", MessageBoxButton.OK, MessageBoxImage.Information);

                // Сохранение отчета в текстовый файл
                SaveReportToFile(reportText.ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка генерации отчета: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveReportToFile(string reportContent)
        {
            try
            {
                // Путь для сохранения файла
                string filePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "FinancialReport.txt");

                // Запись в файл
                File.WriteAllText(filePath, reportContent);

                MessageBox.Show($"Отчет сохранен на рабочем столе как {filePath}", "Сохранение отчета", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения отчета: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void AddTransactionButton_Click(object sender, RoutedEventArgs e)
        {
            AddTransactionPage addTransactionPage = new AddTransactionPage();
            addTransactionPage.Show();
            this.Hide();
            
        }

        private void HideAllGrid()
        {
            EditorGrid.Visibility = Visibility.Hidden;

            topEditingPanel.Visibility = Visibility.Hidden;
            EmployeeDataGrid.Visibility = Visibility.Hidden;
            TransactionDataGrid.Visibility = Visibility.Hidden;
            //topEditingPanelForTransactions.Visibility = Visibility.Hidden;
        }
    }
}
