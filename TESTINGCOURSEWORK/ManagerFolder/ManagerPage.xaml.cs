﻿using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
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

using TESTINGCOURSEWORK.ManagerFolder;
using TESTINGCOURSEWORK.Models;

namespace TESTINGCOURSEWORK
{
    /// <summary>
    /// Логика взаимодействия для ManagerPage.xaml
    /// </summary>
    public partial class ManagerPage : Window
    {
        public ObservableCollection<PieSeries> TransactionData { get; set; } = new ObservableCollection<PieSeries>();

        // Сумма входящих и исходящих транзакций для PieChart
        public ObservableCollection<double> IncomingValues { get; set; } = new ObservableCollection<double>();
        public ObservableCollection<double> OutgoingValues { get; set; } = new ObservableCollection<double>();

        public ObservableCollection<ChartPoint> BalanceData { get; set; } = new ObservableCollection<ChartPoint>();
        private ObservableCollection<Models.Transaction> transactions;
        public ObservableCollection<Models.Transaction> Transactions { get { return transactions; } set { transactions = value; } }
        public ObservableCollection<TCPServerLab2.Product> Products { get; set; } = new ObservableCollection<TCPServerLab2.Product>();

        private ObservableCollection<TCPServerLab2.Employee> employees;
        public ObservableCollection<TCPServerLab2.Employee> Employees

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
                    List<TCPServerLab2.Employee>? users = new List<TCPServerLab2.Employee>();
                    users = JsonConvert.DeserializeObject<List<TCPServerLab2.Employee>>(response);
                    Employees = new ObservableCollection<TCPServerLab2.Employee>(users);
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

        private async void Balance_Button_Click(object sender, RoutedEventArgs e)
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
                    List<Models.Transaction>? transactions = new List<Models.Transaction>();
                    transactions = JsonConvert.DeserializeObject<List<Models.Transaction>>(response);
                    Transactions = new ObservableCollection<Models.Transaction>(transactions);
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
                var transactions = JsonConvert.DeserializeObject<List<Models.Transaction>>(transactionsJson);

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

        private async void DeleteTransactionButton_Click(object sender, RoutedEventArgs e)
        {
            if (TransactionDataGrid.SelectedItem is Models.Transaction selectedTransaction)
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
                    (TransactionDataGrid.ItemsSource as ObservableCollection<Models.Transaction>)?.Remove(selectedTransaction);
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
            ManagerApplicationDataGrid.Visibility = Visibility.Hidden;
            topManagerPanel.Visibility = Visibility.Hidden;
            ProductGrid.Visibility = Visibility.Hidden;
            ProductDataGrid.Visibility = Visibility.Hidden;
            Financial_EditorGrid.Visibility = Visibility.Hidden;

        }

        private void supplier_Button_Click(object sender, RoutedEventArgs e)
        {
            HideAllGrid();
            topManagerPanel.Visibility = Visibility.Visible;
            try
            {


                // Делаем видимыми верхнюю панель и DataGrid

                ManagerApplicationDataGrid.Visibility = Visibility.Visible;

                // Загружаем данные о необработанных заявках
                LoadUnprocessedApplications();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }


        private async void Approve_Button_Click(object sender, RoutedEventArgs e)
        {
            if (ManagerApplicationDataGrid.SelectedItem is not ApplicationViewModel selectedApplication)
            {
                MessageBox.Show("Пожалуйста, выберите заявку для обработки.");
                return;
            }

            try
            {
                string response = await NetworkService.Instance.SendMessageAsync($"approveApplication:{selectedApplication.Id}");
                if (response == "Success")
                {
                    MessageBox.Show("Заявка одобрена.");
                    LoadUnprocessedApplications(); // Обновляем данные в DataGrid
                }
                else
                {
                    MessageBox.Show("Ошибка при одобрении заявки.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }


        private async void Reject_Button_Click(object sender, RoutedEventArgs e)
        {
            if (ManagerApplicationDataGrid.SelectedItem is not ApplicationViewModel selectedApplication)
            {
                MessageBox.Show("Пожалуйста, выберите заявку для обработки.");
                return;
            }

            try
            {
                string response = await NetworkService.Instance.SendMessageAsync($"rejectApplication:{selectedApplication.Id}");
                if (response == "Success")
                {
                    MessageBox.Show("Заявка отклонена.");
                    LoadUnprocessedApplications(); // Обновляем данные в DataGrid
                }
                else
                {
                    MessageBox.Show("Ошибка при отклонении заявки.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }



        private async void LoadUnprocessedApplications()
        {
            try
            {
                string response = await NetworkService.Instance.SendMessageAsync("getUnprocessedApplications");
                if (response == "NoData")
                {
                    ManagerApplicationDataGrid.ItemsSource = null;
                    MessageBox.Show("Нет необработанных заявок.");
                }
                else
                {
                    var applications = JsonConvert.DeserializeObject<List<ApplicationViewModel>>(response);

                    // Если нужно отфильтровать только заявки с заполненным количеством и ед. измерения:
                    applications = applications.Where(app => app.Quantity > 0 && !string.IsNullOrEmpty(app.UnitOfMeasurement)).ToList();

                    ManagerApplicationDataGrid.ItemsSource = applications;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }


        private async void ExportReportApp_Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string command = "export_applications";
                string applicationsJson = await NetworkService.Instance.SendMessageAsync(command);

                if (applicationsJson == "Error")
                {
                    MessageBox.Show("Ошибка при получении данных о заявках.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var applications = JsonConvert.DeserializeObject<List<ApplicationViewModel>>(applicationsJson);

                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                using (var package = new ExcelPackage())
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Applications");

                    worksheet.Cells[1, 1].Value = "ID";
                    worksheet.Cells[1, 2].Value = "Логин";
                    worksheet.Cells[1, 3].Value = "Контактная информация";
                    worksheet.Cells[1, 4].Value = "Название продукта";
                    worksheet.Cells[1, 5].Value = "Описание";
                    worksheet.Cells[1, 6].Value = "Сумма";
                    worksheet.Cells[1, 7].Value = "Количество";
                    worksheet.Cells[1, 8].Value = "Ед. измерения";
                    worksheet.Cells[1, 9].Value = "Статус";
                    worksheet.Cells[1, 10].Value = "Дата подачи";

                    int row = 2;
                    foreach (var application in applications)
                    {
                        worksheet.Cells[row, 1].Value = application.Id;
                        worksheet.Cells[row, 2].Value = application.Login;
                        worksheet.Cells[row, 3].Value = application.ContactInfo;
                        worksheet.Cells[row, 4].Value = application.ProductName;
                        worksheet.Cells[row, 5].Value = application.Description;
                        worksheet.Cells[row, 6].Value = application.TotalPrice;
                        worksheet.Cells[row, 7].Value = application.Quantity; // Новое поле
                        worksheet.Cells[row, 8].Value = application.UnitOfMeasurement; // Новое поле
                        worksheet.Cells[row, 9].Value = application.Status;
                        worksheet.Cells[row, 10].Value = application.DateSubmitted;
                        row++;
                    }

                    string filePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "ApplicationsReport.xlsx");
                    package.SaveAs(new FileInfo(filePath));
                    MessageBox.Show($"Файл сохранен на рабочем столе как {filePath}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

        }

        private void AddProductButton_Click(object sender, RoutedEventArgs e)
        {
            AddProductWindow addProductWindow = new AddProductWindow();
            addProductWindow.Show();
            this.Hide();
        }

        private async void AddFromApplicationsButton_Click(object sender, RoutedEventArgs e)
        {
            SelectApplicationsPage selectApplicationsPage = new SelectApplicationsPage();
            selectApplicationsPage.ShowDialog();
            this.Hide();
        }

        private async void AdjustStockButton_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                // Отправляем запрос на сервер для получения данных о продукции
                string response = await NetworkService.Instance.SendMessageAsync("getProductData");

                // Если данных нет, показываем сообщение
                if (response == "NoData")
                {
                    ProductDataGrid.ItemsSource = null;
                    MessageBox.Show("Нет данных о продукции.");
                }
                else
                {
                    // Десериализация данных в список продуктов
                    var products = JsonConvert.DeserializeObject<ObservableCollection<TCPServerLab2.Product>>(response);

                    // Привязка данных к DataGrid
                    ObservableCollection<TCPServerLab2.Product> products1 = products;
                    AdjustStockWindow adjustStockWindow = new AdjustStockWindow(products1);
                    adjustStockWindow.Show();
                    this.Hide();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных о продукции: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void Button_Click_Accounting(object sender, RoutedEventArgs e)
        {
            HideAllGrid();
            // Показать верхнюю панель
            ProductGrid.Visibility = Visibility.Visible;

            // Показать DataGrid для учета продукции
            ProductDataGrid.Visibility = Visibility.Visible;

            // Загружаем данные для учета продукции (например, из базы данных или другого источника)
            LoadProductData();
        }
        private async void LoadProductData()
        {
            try
            {
                // Отправляем запрос на сервер для получения данных о продукции
                string response = await NetworkService.Instance.SendMessageAsync("getProductData");

                // Если данных нет, показываем сообщение
                if (response == "NoData")
                {
                    ProductDataGrid.ItemsSource = null;
                    MessageBox.Show("Нет данных о продукции.");
                }
                else
                {
                    // Десериализация данных в список продуктов
                    var products = JsonConvert.DeserializeObject<List<TCPServerLab2.Product>>(response);

                    // Привязка данных к DataGrid
                    ProductDataGrid.ItemsSource = products;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных о продукции: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ExportToExcelButtonProd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Запрос на экспорт продуктов
                string productsCommand = "export_products";
                string productsJson = await NetworkService.Instance.SendMessageAsync(productsCommand);

                if (productsJson == "Error")
                {
                    MessageBox.Show("Ошибка при получении данных о продуктах.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Десериализация данных о продуктах из JSON
                var products = JsonConvert.DeserializeObject<List<TCPServerLab2.Product>>(productsJson);

                // Запрос на экспорт транзакций
                string transactionsCommand = "export_transactionsProd";
                string transactionsJson = await NetworkService.Instance.SendMessageAsync(transactionsCommand);

                if (transactionsJson == "Error")
                {
                    MessageBox.Show("Ошибка при получении данных о транзакциях.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Десериализация данных о транзакциях из JSON
                var transactions = JsonConvert.DeserializeObject<List<TCPServerLab2.ProductTransaction>>(transactionsJson);

                // Создание и сохранение Excel-файла
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                using (var package = new ExcelPackage())
                {
                    // Добавление первой таблицы - Продукты
                    ExcelWorksheet productsWorksheet = package.Workbook.Worksheets.Add("Products");

                    // Заголовки для таблицы продуктов
                    productsWorksheet.Cells[1, 1].Value = "ProductId";
                    productsWorksheet.Cells[1, 2].Value = "ProductName";
                    productsWorksheet.Cells[1, 3].Value = "Description";
                    productsWorksheet.Cells[1, 4].Value = "Quantity";
                    productsWorksheet.Cells[1, 5].Value = "UnitOfMeasurement";
                    productsWorksheet.Cells[1, 6].Value = "UnitPrice";
                    productsWorksheet.Cells[1, 7].Value = "LastUpdated";

                    //Заполнение данными о продуктах
                    int row = 2;
                    foreach (var product in products)
                    {
                        productsWorksheet.Cells[row, 1].Value = product.ProductId;
                        productsWorksheet.Cells[row, 2].Value = product.ProductName;
                        productsWorksheet.Cells[row, 3].Value = product.Description;
                        productsWorksheet.Cells[row, 4].Value = product.Quantity;
                        productsWorksheet.Cells[row, 5].Value = product.UnitOfMeasurement;
                        productsWorksheet.Cells[row, 6].Value = product.UnitPrice;
                        productsWorksheet.Cells[row, 7].Value = product.LastUpdated?.ToString("yyyy-MM-dd HH:mm:ss");
                        row++;
                    }

                    // Добавление второй таблицы - Транзакции
                    ExcelWorksheet transactionsWorksheet = package.Workbook.Worksheets.Add("Transactions");

                    // Заголовки для таблицы транзакций
                    transactionsWorksheet.Cells[1, 1].Value = "TransactionId";
                    transactionsWorksheet.Cells[1, 2].Value = "ProductId";
                    transactionsWorksheet.Cells[1, 3].Value = "Quantity";
                    transactionsWorksheet.Cells[1, 4].Value = "TransactionType";
                    transactionsWorksheet.Cells[1, 5].Value = "Description";
                    transactionsWorksheet.Cells[1, 6].Value = "TransactionDate";

                    // Заполнение данными о транзакциях
                    row = 2;
                    foreach (var transaction in transactions)
                    {
                        transactionsWorksheet.Cells[row, 1].Value = transaction.TransactionId;
                        transactionsWorksheet.Cells[row, 2].Value = transaction.ProductId;
                        transactionsWorksheet.Cells[row, 3].Value = transaction.Quantity;
                        transactionsWorksheet.Cells[row, 4].Value = transaction.TransactionType;
                        transactionsWorksheet.Cells[row, 5].Value = transaction.Description;
                        transactionsWorksheet.Cells[row, 6].Value = transaction.TransactionDate?.ToString("yyyy-MM-dd HH:mm:ss");
                        row++;
                    }

                    // Сохранение Excel файла
                    string filePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Products_Transactions.xlsx");
                    package.SaveAs(new FileInfo(filePath));

                    MessageBox.Show($"Файл сохранен на рабочем столе как {filePath}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Finance_Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Salary_Button_Click(object sender, RoutedEventArgs e)
        {
            SalaryPage salaryPage = new SalaryPage();
            salaryPage.ShowDialog();
        }


    }
  
}
