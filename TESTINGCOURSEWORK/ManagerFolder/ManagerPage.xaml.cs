using Client.ManagerFolder;
using Client.ManagerFolder.DataAnalysys;
using LiveCharts;
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
      
        public ObservableCollection<TCPServer.Product> Products { get; set; } = new ObservableCollection<TCPServer.Product>();

        private ObservableCollection<TCPServer.Employee> employees;
        public ObservableCollection<TCPServer.Employee> Employees

        {
            get { return employees; }
            set { employees = value; }
        }

        public ManagerPage()
        {
            InitializeComponent();
            HideAllGrid();

        }


        private void BalanceDashboardButton_Click(object sender, RoutedEventArgs e)
        {
            BalanceDashboard balanceDashboard = new BalanceDashboard();
            balanceDashboard.Show();
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
                    List<TCPServer.Employee>? users = new List<TCPServer.Employee>();
                    users = JsonConvert.DeserializeObject<List<TCPServer.Employee>>(response);
                    Employees = new ObservableCollection<TCPServer.Employee>(users);
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
            this.Close();
        }
      



        private async void Delete_Button_Click(object sender, RoutedEventArgs e)
        {
            if (EmployeeDataGrid.SelectedItem is TCPServer.Employee selectedEmployee)
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
                    (EmployeeDataGrid.ItemsSource as ObservableCollection<TCPServer.Employee>)?.Remove(selectedEmployee);
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

    

        private async void ExportToExcelButton_Click(object sender, RoutedEventArgs e)
        {
           
        }

        private async void DeleteTransactionButton_Click(object sender, RoutedEventArgs e)
        {
         
        }

        private async void GenerateReportButton_Click(object sender, RoutedEventArgs e)
        {
            
        }



  

        private void HideAllGrid()
        {
            

            topEditingPanel.Visibility = Visibility.Hidden;
            EmployeeDataGrid.Visibility = Visibility.Hidden;
            
           
           

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

      
       

        private void Button_Click_Accounting(object sender, RoutedEventArgs e)
        {
            HideAllGrid();
           ProductDashboard productDashboard = new ProductDashboard();
            productDashboard.Show();

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
                var products = JsonConvert.DeserializeObject<List<TCPServer.Product>>(productsJson);

                // Запрос на экспорт транзакций
                string transactionsCommand = "export_transactionsProd";
                string transactionsJson = await NetworkService.Instance.SendMessageAsync(transactionsCommand);

                if (transactionsJson == "Error")
                {
                    MessageBox.Show("Ошибка при получении данных о транзакциях.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Десериализация данных о транзакциях из JSON
                var transactions = JsonConvert.DeserializeObject<List<TCPServer.ProductTransaction>>(transactionsJson);

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
            HideAllGrid();
            CapitalAnalys capitalAnalys = new CapitalAnalys();
            capitalAnalys.Show();



        }

        private async void Salary_Button_Click(object sender, RoutedEventArgs e)
        {
            if (EmployeeDataGrid.SelectedItem is TCPServer.Employee selectedEmployee)
            {
                // Формирование сообщения для сервера
                string loginData = $"salaryEmployee:{selectedEmployee.EmployeeId}";

                // Отправка сообщения на сервер
                string response = await NetworkService.Instance.SendMessageAsync(loginData);

                // Проверка ответа сервера
                if (response == "User detected")
                {
                    SalaryPage salaryPage = new SalaryPage(selectedEmployee);
                    salaryPage.ShowDialog();
                }
                else
                {
                    MessageBox.Show("Ошибка сервера. Пожалуйста подождите");
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите работника");
            }
           
        }

        private async void TransactionChartButton_Click(object sender, RoutedEventArgs e)
        {
            //SalaryBarChart.Visibility = Visibility.Hidden;
            //StatusPieChart.Visibility = Visibility.Hidden;
            //try
            //{
            //    // Отправляем запрос на сервер
            //    string response = await NetworkService.Instance.SendMessageAsync("getTransactionSummary");

            //    if (response == "NoData")
            //    {
            //        MessageBox.Show("Нет данных для отображения.");
            //        return;
            //    }
            //    TransactionPieChart.Visibility = Visibility.Visible;
            //    // Десериализация данных
            //    var transactionData = JsonConvert.DeserializeObject<List<TCPServer.TransactionSummary>>(response);

            //    // Очищаем старые серии
            //    TransactionPieChart.Series.Clear();

            //    // Создаём серии для каждого типа транзакций
            //    foreach (var data in transactionData)
            //    {
            //        TransactionPieChart.Series.Add(new PieSeries
            //        {
            //            Title = data.TransactionType,
            //            Values = new ChartValues<decimal> { data.TotalAmount },
            //            DataLabels = true,
            //            LabelPoint = chartPoint => $"{chartPoint.Y} ({chartPoint.Participation:P})"
            //        });
            //    }
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show($"Ошибка: {ex.Message}");
            //}
        }

      

        private void Exit_Button_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void supplier_Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }

}
