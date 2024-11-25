using Azure;
using Azure.Core;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using System.Security.Cryptography.Pkcs;
using System.Text;
using System.Threading.Tasks;
using TCPServerLab2;
using TESTINGCOURSEWORK.Models;

public class Server
{
    private readonly int _port = 12345;
    private TcpListener _server;


    public Server()
    {
        _server = new TcpListener(IPAddress.Any, _port);
    }

    public async Task StartAsync()
    {
        _server.Start();
        Console.WriteLine("Сервер запущен...");

        while (true)
        {
            TcpClient client = await _server.AcceptTcpClientAsync();
            _ = Task.Run(() => HandleClientAsync(client)); // Асинхронная обработка каждого клиента
            Console.WriteLine("Клиент подклбчен");
        }
    }

    private async Task HandleClientAsync(TcpClient client)
    {
        Console.WriteLine("Клиент подключен");

        // Получаем IP-адрес клиента
        string clientIp = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
        using NetworkStream stream = client.GetStream();
        byte[] buffer = new byte[256];

        try
        {
            while (client.Connected)
            {
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead == 0) break; // Клиент закрыл соединение

                string receivedData = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                // Проверка на сообщение "close"
                if (receivedData == "close")
                {
                    Console.WriteLine($"Клиент {clientIp} закрыл соединение");
                    break;
                }
                string[] credentials = receivedData.Split(':');

                if (credentials[0] == "admin")
                {
                    Console.WriteLine("Admin here it is");
                    string username = credentials[1];
                    string password = credentials[2];
                    string code = credentials[3];

                    Console.WriteLine($"Получены данные от клиента {clientIp} - Username: {username}, Password: {password}, Code {code}");

                    string response = await ValidateAdminUserAsync(username, password, code) ? "Success" : "Invalid credentials";
                    byte[] responseData = Encoding.UTF8.GetBytes(response);
                    await stream.WriteAsync(responseData, 0, responseData.Length);

                }
                else if (credentials[0] == "manager")
                {
                    Console.WriteLine("Manager here it is");
                    string username = credentials[1];
                    string password = credentials[2];
                    string code = credentials[3];

                    Console.WriteLine($"Получены данные от клиента {clientIp} - Username: {username}, Password: {password}, Code {code}");

                    string response = await ValidateManagerUserAsync(username, password, code) ? "Success" : "Invalid credentials";
                    byte[] responseData = Encoding.UTF8.GetBytes(response);
                    await stream.WriteAsync(responseData, 0, responseData.Length);

                }
                else if (credentials[0] == "regUser") // Новая команда для регистрации
                {
                    string username = credentials[1];
                    string password = credentials[2];

                    Console.WriteLine($"Попытка регистрации клиента {clientIp} - Username: {username}, Password: {password}");

                    string response = await AddUserAsync(username, password) ? "Success" : "UserExists";
                    byte[] responseData = Encoding.UTF8.GetBytes(response);
                    await stream.WriteAsync(responseData, 0, responseData.Length);

                }
                else if (credentials[0] == "regUserAdmin") // Новая команда для регистрации
                {
                    string username = credentials[1];
                    string password = credentials[2];
                    int roleID = int.Parse(credentials[3]);

                    Console.WriteLine($"Попытка регистрации клиента {clientIp} - Username: {username}, Password: {password}, RoleId: {roleID}");

                    string response = await AddUserAsync(username, password, roleID) ? "Success" : "UserExists";
                    byte[] responseData = Encoding.UTF8.GetBytes(response);
                    await stream.WriteAsync(responseData, 0, responseData.Length);

                }

                else if (credentials[0] == "loginUser")
                {
                    Console.WriteLine("Обрабатываю запрос на вход в систему...");
                    string username = credentials[1];
                    string password = credentials[2];

                    Console.WriteLine($"Получены учетные данные -Ip {clientIp} Username: {username}, Password: {password}");

                    var user = await ValidateUserAsync(username, password);
                    if (user != null)
                    {
                        string response = $"Success:{user.Id}"; // Возвращаем UserId в ответе
                        byte[] responseData = Encoding.UTF8.GetBytes(response);
                        await stream.WriteAsync(responseData, 0, responseData.Length);
                    }
                    else
                    {
                        byte[] responseData = Encoding.UTF8.GetBytes("Invalid credentials");
                        await stream.WriteAsync(responseData, 0, responseData.Length);
                    }
                }
                else if (receivedData == "getUsers")
                {
                    Console.WriteLine("Запрос на получение пользователей");
                    string jsonData = await GetAllUsers(); // Получаем данные о пользователях
                    byte[] responseData = Encoding.UTF8.GetBytes(jsonData); // Преобразуем в байты
                    await stream.WriteAsync(responseData, 0, responseData.Length); // Отправляем клиенту

                }
                else if(receivedData == "getEmployees"){
                    Console.WriteLine("Запрос на получение работников");
                    string jsonData = await GetAllEmployees(); // Получаем данные о пользователях
                    byte[] responseData = Encoding.UTF8.GetBytes(jsonData); // Преобразуем в байты
                    await stream.WriteAsync(responseData, 0, responseData.Length); // Отправляем клиенту
                }
                else if (credentials[0] == "deleteUser" && int.TryParse(credentials[1], out int userId))
                {
                    Console.WriteLine($"Запрос на удаление пользователя ID: {userId}");
                    string response = await DeleteUserByIdAsync(userId) ? "User deleted" : "Failed to delete user";
                    byte[] responseData = Encoding.UTF8.GetBytes(response);
                    await stream.WriteAsync(responseData, 0, responseData.Length);
                }
                else if (credentials[0] == "deleteEmployee" && int.TryParse(credentials[1], out int employeeId))
                {
                    Console.WriteLine($"Запрос на удаление пользователя ID: {employeeId}");
                    string response = await DeleteEmployeeByIdAsync(employeeId) ? "User deleted" : "Failed to delete user";
                    byte[] responseData = Encoding.UTF8.GetBytes(response);
                    await stream.WriteAsync(responseData, 0, responseData.Length);
                }
                else if (receivedData == "export_employee_data")
                {
                    string employeeDataJson = await GetEmployeeDataAsync();
                    byte[] responseData = Encoding.UTF8.GetBytes(employeeDataJson);
                    await stream.WriteAsync(responseData, 0, responseData.Length);
                }
                else if (receivedData == "getTransactions")
                {
                    Console.WriteLine("Запрос на получение транзакций");
                    string jsonData = await GetAllTransactions(); // Получаем данные о пользователях
                    byte[] responseData = Encoding.UTF8.GetBytes(jsonData); // Преобразуем в байты
                    await stream.WriteAsync(responseData, 0, responseData.Length); // Отправляем клиенту
                }
                else if (credentials[0] == "transaction")
                {
                    Console.WriteLine("Обработка транзакции");

                    try
                    {
                        // Извлечение данных из команды
                        string transactionType = credentials[1]; // Тип операции: Пополнение или Списание
                        decimal amount = decimal.Parse(credentials[2]); // Сумма транзакции
                        string description = credentials[3]; // Описание

                        Console.WriteLine($"Данные транзакции: Тип - {transactionType}, Сумма - {amount}, Описание - {description}");

                        // Добавление транзакции в базу данных
                        using (var context = new CrmsystemContext())
                        {
                            var balance = context.Balances.FirstOrDefault();

                            if(transactionType == "Пополнение")
                            {
                                balance.Amount += amount;
                            }
                            else if(balance.Amount > amount)
                            {
                                balance.Amount -= amount;
                            }
                            else
                            {
                               
                                throw new Exception("Недостаточно средств на балансе для выплаты зарплаты.");
                                
                            }
                            var transaction = new Transaction
                            {
                                TransactionDate = DateTime.Now,
                                Amount = amount,
                                TransactionType = transactionType,
                                Description = description
                            };

                            context.Transactions.Add(transaction);
                            context.SaveChanges();
                        }

                        // Отправка успешного ответа клиенту
                        byte[] responseData = Encoding.UTF8.GetBytes("Success");
                        await stream.WriteAsync(responseData, 0, responseData.Length);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка при добавлении транзакции: {ex.Message}");

                        // Отправка ошибки клиенту
                        byte[] responseData = Encoding.UTF8.GetBytes("Error");
                        await stream.WriteAsync(responseData, 0, responseData.Length);
                    }
                }
                else if (credentials[0] == "export_transactions")
                {
                    Console.WriteLine("Отправка данных о транзакциях для экспорта в Excel");

                    try
                    {
                        using (var context = new CrmsystemContext())
                        {
                            var transactions = context.Transactions
                                .OrderByDescending(t => t.TransactionDate)
                                .ToList();

                            string json = JsonConvert.SerializeObject(transactions);
                            byte[] responseData = Encoding.UTF8.GetBytes(json);
                            await stream.WriteAsync(responseData, 0, responseData.Length);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка при экспорте транзакций: {ex.Message}");
                        byte[] errorResponse = Encoding.UTF8.GetBytes("Error");
                        await stream.WriteAsync(errorResponse, 0, errorResponse.Length);
                    }
                }
                else if (credentials[0] == "generate_report")
                {
                    Console.WriteLine("Запрос на генерацию отчета получен.");

                    // Получаем данные из базы
                    using (var db = new CrmsystemContext())
                    {
                        var transactions = db.Transactions.ToList();
                        var balance = db.Balances.FirstOrDefault()?.Amount ?? 0;

                        var report = new
                        {
                            Balance = balance,
                            TotalTransactions = transactions.Count,
                            IncomeSum = transactions.Where(t => t.TransactionType == "Пополнение").Sum(t => t.Amount),
                            ExpenseSum = transactions.Where(t => t.TransactionType == "Списание").Sum(t => t.Amount),
                            MaxTransaction = transactions.OrderByDescending(t => t.Amount).FirstOrDefault(),
                            MinTransaction = transactions.OrderBy(t => t.Amount).FirstOrDefault(),
                            AverageTransaction = transactions.Average(t => t.Amount),
                            MonthlySummary = transactions.GroupBy(t => t.TransactionDate.Month)
                                .Select(g => new
                                {
                                    Month = g.Key,
                                    Total = g.Count(),
                                    Income = g.Where(t => t.TransactionType == "Пополнение").Sum(t => t.Amount),
                                    Expense = g.Where(t => t.TransactionType == "Списание").Sum(t => t.Amount),
                                    MonthEndBalance = balance // Вычисление через динамику
                                }).ToList(),
                            Errors = transactions.GroupBy(t => new { t.Amount, t.TransactionType })
                                .Where(g => g.Count() > 1)
                                .Select(g => g.Key).ToList()
                        };

                        string jsonReport = JsonConvert.SerializeObject(report);
                        byte[] responseData = Encoding.UTF8.GetBytes(jsonReport);
                        await stream.WriteAsync(responseData, 0, responseData.Length);
                    }

                }
                else if (credentials[0] == "deleteTransaction" && int.TryParse(credentials[1], out int transactionId))
                {
                    Console.WriteLine($"Запрос на удаление транзакции ID: {transactionId}");
                    string response = await DeleteTransactionByIdAsync(transactionId) ? "Transaction deleted" : "Failed to delete transaction";
                    byte[] responseData = Encoding.UTF8.GetBytes(response);
                    await stream.WriteAsync(responseData, 0, responseData.Length);
                }
                else if (credentials[0] == "addEmployee")
                {
                    try
                    {
                        string firstName = credentials[1];
                        string lastName = credentials[2];
                        string middleName = credentials[3];
                        decimal salary = decimal.Parse(credentials[4]);
                        DateOnly birthDate = DateOnly.Parse(credentials[5]);
                        string position = credentials[6];
                        DateOnly hireDate = DateOnly.Parse(credentials[7]);

                        Console.WriteLine($"Получен запрос на добавление сотрудника: {firstName} {lastName}");

                        string response = await AddEmployeeAsync(firstName, lastName, middleName, salary, birthDate, position, hireDate)
                            ? "Success"
                            : "Error";
                        byte[] responseData = Encoding.UTF8.GetBytes(response);
                        await stream.WriteAsync(responseData, 0, responseData.Length);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка обработки запроса: {ex.Message}");
                        byte[] responseData = Encoding.UTF8.GetBytes("Error");
                        await stream.WriteAsync(responseData, 0, responseData.Length);
                    }
                }
                else if (credentials[0] == "addApplication")
                {
                    try
                    {
                        // Распаковываем данные из сообщения
                        string login = credentials[1];
                        string contactInfo = credentials[2];
                        string productName = credentials[3];
                        string description = credentials[4];
                        decimal totalPrice = decimal.Parse(credentials[5]);
                        int quantity = int.Parse(credentials[6]); // Новое поле: Количество
                        string unitOfMeasurement = credentials[7]; // Новое поле: Единица измерения

                        Console.WriteLine($"Получен запрос на добавление заявки от {login} на продукт: {productName}");

                        // Вызываем метод для добавления заявки
                        string response = await AddApplicationAsync(login, contactInfo, productName, description, totalPrice, quantity, unitOfMeasurement)
                            ? "Success"
                            : "Error";

                        // Отправляем ответ клиенту
                        byte[] responseData = Encoding.UTF8.GetBytes(response);
                        await stream.WriteAsync(responseData, 0, responseData.Length);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка обработки запроса: {ex.Message}");
                        byte[] responseData = Encoding.UTF8.GetBytes("Error");
                        await stream.WriteAsync(responseData, 0, responseData.Length);
                    }
                }

                else if (credentials[0] == "getApplications")
                {
                    // Извлекаем UserId из запроса
                    if (int.TryParse(credentials[1], out int appUserId))
                    {
                        Console.WriteLine($"Запрос на получение заявок для пользователя с ID: {appUserId}");
                        try
                        {
                            string jsonData = await GetApplicationsByUserId(appUserId); // Получаем данные о заявках пользователя
                            byte[] responseData = Encoding.UTF8.GetBytes(jsonData); // Преобразуем в байты
                            await stream.WriteAsync(responseData, 0, responseData.Length); // Отправляем клиенту
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Ошибка при обработке заявок: {ex.Message}");
                            byte[] errorResponse = Encoding.UTF8.GetBytes("Error");
                            await stream.WriteAsync(errorResponse, 0, errorResponse.Length);
                        }
                    }
                    else
                    {
                        byte[] errorResponse = Encoding.UTF8.GetBytes("InvalidUserId");
                        await stream.WriteAsync(errorResponse, 0, errorResponse.Length);
                    }
                }

                else if (credentials[0] == "getUnprocessedApplications")
                {
                    Console.WriteLine("Запрос на получение необработанных заявок");

                    try
                    {
                        string jsonData = await GetUnprocessedApplications(); // Получаем данные о необработанных заявках
                        byte[] responseData = Encoding.UTF8.GetBytes(jsonData); // Преобразуем в байты
                        await stream.WriteAsync(responseData, 0, responseData.Length); // Отправляем клиенту
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка при обработке необработанных заявок: {ex.Message}");
                        byte[] errorResponse = Encoding.UTF8.GetBytes("ServerError");
                        await stream.WriteAsync(errorResponse, 0, errorResponse.Length);
                    }
                }


                else if (credentials[0] == "deleteApplication" && int.TryParse(credentials[1], out int applicationId))
                {
                    Console.WriteLine($"Запрос на удаление заявки ID: {applicationId}");

                    try
                    {
                        string response;
                        if (await DeleteApplicationByIdAsync(applicationId))
                        {
                            response = "Application deleted";
                        }
                        else
                        {
                            response = "Application not found or could not be deleted";
                        }

                        byte[] responseData = Encoding.UTF8.GetBytes(response);
                        await stream.WriteAsync(responseData, 0, responseData.Length);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка при обработке удаления: {ex.Message}");
                        byte[] errorResponse = Encoding.UTF8.GetBytes("Error deleting application");
                        await stream.WriteAsync(errorResponse, 0, errorResponse.Length);
                    }
                }
                else if (credentials[0] == "getApplicationHistory")
                {
                    if (int.TryParse(credentials[1], out int HistuserId))
                    {
                        Console.WriteLine($"Запрос на историю заявок для пользователя с ID: {HistuserId}");
                        string jsonData = await GetApplicationHistoryByUserId(HistuserId); // Получаем историю заявок
                        byte[] responseData = Encoding.UTF8.GetBytes(jsonData); // Преобразуем в байты
                        await stream.WriteAsync(responseData, 0, responseData.Length); // Отправляем клиенту
                    }
                    else
                    {
                        byte[] errorResponse = Encoding.UTF8.GetBytes("InvalidUserId");
                        await stream.WriteAsync(errorResponse, 0, errorResponse.Length);
                    }
                }
               
                else if (receivedData.StartsWith("approveApplication"))
                {
                    int approveApplicationId = int.Parse(receivedData.Split(':')[1]);
                    await ApproveApplication(approveApplicationId);
                    byte[] responseData = Encoding.UTF8.GetBytes("Success");
                    await stream.WriteAsync(responseData, 0, responseData.Length);
                }
                else if (receivedData.StartsWith("rejectApplication"))
                {
                    int rejectApplicationId = int.Parse(receivedData.Split(':')[1]);
                    await RejectApplication(rejectApplicationId);
                    byte[] responseData = Encoding.UTF8.GetBytes("Success");
                    await stream.WriteAsync(responseData, 0, responseData.Length);
                }
                else if (receivedData == "export_applications")
                {
                    Console.WriteLine("Запрос на экспорт всех заявок.");
                    string jsonData = await GetAllApplicationsForExport();

                    // Если данные есть, отправляем их клиенту
                    byte[] responseData = Encoding.UTF8.GetBytes(jsonData);
                    await stream.WriteAsync(responseData, 0, responseData.Length);
                }
                
                else if (receivedData == "getProductData")
                {
                    Console.WriteLine("Запрос на получение данных о продукции");
                    string jsonData = await GetProductData(); // Получаем данные о продукции
                    byte[] responseData = Encoding.UTF8.GetBytes(jsonData); // Преобразуем в байты
                    await stream.WriteAsync(responseData, 0, responseData.Length); // Отправляем клиенту
                }

              
                
                //else if (credentials[0] == "getApplications")
                //{
                //    try
                //    {
                //        using (var dbContext = new CrmsystenContext())
                //        {
                //            var applications = dbContext.Applications.Include(a => a.Status).ToList();
                //            var jsonResponse = JsonConvert.SerializeObject(applications);

                //            byte[] responseData = Encoding.UTF8.GetBytes(jsonResponse);
                //            await stream.WriteAsync(responseData, 0, responseData.Length);
                //        }
                //    }
                //    catch (Exception ex)
                //    {
                //        byte[] responseData = Encoding.UTF8.GetBytes("Error: " + ex.Message);
                //        await stream.WriteAsync(responseData, 0, responseData.Length);
                //    }
                //}

                else if (credentials[0] == "getApplicationsApproved")
                {
                    Console.WriteLine("Запрос на получение необработанных заявок");

                    try
                    {
                        string jsonData = await GetApprovedApplications(); 
                        byte[] responseData = Encoding.UTF8.GetBytes(jsonData); // Преобразуем в байты
                        await stream.WriteAsync(responseData, 0, responseData.Length); // Отправляем клиенту
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка при обработке  заявок: {ex.Message}");
                        byte[] errorResponse = Encoding.UTF8.GetBytes("ServerError");
                        await stream.WriteAsync(errorResponse, 0, errorResponse.Length);
                    }
                }
                else if (credentials[0] == "addProduct")
                {
                    try
                    {
                        // Распаковываем данные из сообщения
                        string productName = credentials[1];
                        string unitOfMeasurement = credentials[2];
                        int quantity = int.Parse(credentials[3]);
                        decimal unitPrice = decimal.Parse(credentials[4]);


                        Console.WriteLine($"Получен запрос на добавление продукта ");

                        // Вызываем метод для добавления заявки
                        string response = await AddProductAsync(productName, unitOfMeasurement, quantity, unitPrice)
                            ? "Success"
                            : "Error";

                        // Отправляем ответ клиенту
                        byte[] responseData = Encoding.UTF8.GetBytes(response);
                        await stream.WriteAsync(responseData, 0, responseData.Length);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка обработки запроса: {ex.Message}");
                        byte[] responseData = Encoding.UTF8.GetBytes("Error");
                        await stream.WriteAsync(responseData, 0, responseData.Length);
                    }
                }
                else if (receivedData.StartsWith("adjustStock"))
                {
                    Console.WriteLine("Запрос на корректировку склада.");

                    try
                    {
                        string jsonRequest = receivedData.Substring("adjustStock:".Length);
                        var stockRequest = JsonConvert.DeserializeObject<StockAdjustmentRequest>(jsonRequest);

                        using (var dbContext = new CrmsystemContext())
                        {
                            var product = dbContext.Products.FirstOrDefault(p => p.ProductId == stockRequest.ProductId);
                            if (product != null)
                            {
                                if (stockRequest.TransactionType == "Приход")
                                {
                                    product.Quantity -= stockRequest.Quantity;
                                }
                                else if (stockRequest.TransactionType == "Расход")
                                {
                                    product.Quantity += stockRequest.Quantity;
                                }

                                // Update LastUpdated field
                                product.LastUpdated = DateTime.Now;

                                // Save transaction
                                var transaction = new ProductTransaction
                                {
                                    ProductId = stockRequest.ProductId,
                                    Quantity = stockRequest.Quantity,
                                    TransactionType = stockRequest.TransactionType,
                                    Description = stockRequest.Description,
                                    TransactionDate = DateTime.Now
                                };

                                dbContext.ProductTransactions.Add(transaction);
                                dbContext.SaveChanges();
                            }
                        }

                        byte[] responseData = Encoding.UTF8.GetBytes("Success");
                        await stream.WriteAsync(responseData, 0, responseData.Length);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка при обработке запроса корректировки склада: {ex.Message}");
                        byte[] errorResponse = Encoding.UTF8.GetBytes("Error: " + ex.Message);
                        await stream.WriteAsync(errorResponse, 0, errorResponse.Length);
                    }
                }
                else if (receivedData == "export_products")
                {
                    Console.WriteLine("Отправка данных о продуктах для экспорта в Excel");

                    try
                    {
                        using (var context = new CrmsystemContext())
                        {
                            var products = context.Products.ToList();
                            string json = JsonConvert.SerializeObject(products);
                            byte[] responseData = Encoding.UTF8.GetBytes(json);
                            await stream.WriteAsync(responseData, 0, responseData.Length);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка при экспорте продуктов: {ex.Message}");
                        byte[] errorResponse = Encoding.UTF8.GetBytes("Error");
                        await stream.WriteAsync(errorResponse, 0, errorResponse.Length);
                    }
                }

                else if (receivedData == "export_transactions_Prod")
                {
                    Console.WriteLine("Отправка данных о транзакциях для экспорта в Excel");

                    try
                    {
                        using (var context = new CrmsystemContext())
                        {
                            var transactions = context.ProductTransactions
                                .OrderByDescending(t => t.TransactionDate)
                                .ToList();

                            string json = JsonConvert.SerializeObject(transactions);
                            byte[] responseData = Encoding.UTF8.GetBytes(json);
                            await stream.WriteAsync(responseData, 0, responseData.Length);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка при экспорте транзакций: {ex.Message}");
                        byte[] errorResponse = Encoding.UTF8.GetBytes("Error");
                        await stream.WriteAsync(errorResponse, 0, errorResponse.Length);
                    }
                }
                else if (receivedData.StartsWith("calculate_salary"))
                {
                    Console.WriteLine("Запрос на начисление зарплаты.");

                    try
                    {
                        string jsonData = receivedData.Substring("calculate_salary:".Length);
                        var salaryRequest = JsonConvert.DeserializeObject<SalaryRecord>(jsonData);

                        using (var dbContext = new CrmsystemContext())
                        {
                            var employees = dbContext.Employees.Where(e => e.Salary.HasValue).ToList();
                            var balance = dbContext.Balances.FirstOrDefault();

                            if (balance == null)
                            {
                                throw new Exception("Баланс компании не найден.");
                            }

                            decimal totalSalary = 0;
                            List<SalaryRecord> salaryRecords = new();

                            foreach (var employee in employees)
                            {
                                decimal dailySalary = employee.Salary.Value;
                                decimal daysInMonth = DateTime.DaysInMonth(salaryRequest.Date.Year, salaryRequest.Date.Month);
                                decimal employeeSalary = dailySalary * daysInMonth;

                                salaryRecords.Add(new SalaryRecord
                                {
                                    LastName = employee.LastName ?? "Неизвестно",
                                    Salary = employeeSalary
                                });

                                totalSalary += employeeSalary;
                            }

                            // Вычет подоходного налога (13%)
                            decimal taxAmount = totalSalary * 0.13M;
                            decimal netSalary = totalSalary - taxAmount;

                            if (balance.Amount < netSalary)
                            {
                                throw new Exception("Недостаточно средств на балансе для выплаты зарплаты.");
                            }

                            // Обновление баланса
                            balance.Amount -= netSalary;
                            dbContext.SaveChanges();

                            // Отправка JSON с данными
                            string jsonResponse = JsonConvert.SerializeObject(salaryRecords);
                            byte[] responseData = Encoding.UTF8.GetBytes(jsonResponse);
                            await stream.WriteAsync(responseData, 0, responseData.Length);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка при начислении зарплаты: {ex.Message}");
                        byte[] responseData = Encoding.UTF8.GetBytes($"Error: {ex.Message}");
                        await stream.WriteAsync(responseData, 0, responseData.Length);
                    }
                }
                else if (receivedData == "getTransactionSummary")
                {
                    Console.WriteLine("Запрос на получение суммарных транзакций по типам");
                    string jsonData = await GetTransactionSummaryAsync();
                    byte[] responseData = Encoding.UTF8.GetBytes(jsonData);
                    await stream.WriteAsync(responseData, 0, responseData.Length);
                }
                else if (receivedData == "getApplicationStatusSummary")
                {
                    Console.WriteLine("Запрос на получение данных о статусах заявок");
                    string jsonData = await GetApplicationStatusSummaryAsync();
                    byte[] responseData = Encoding.UTF8.GetBytes(jsonData);
                    await stream.WriteAsync(responseData, 0, responseData.Length);
                }
                else if (receivedData == "getEmployeeSalaries")
                {
                    Console.WriteLine("Запрос на получение данных о зарплатах сотрудников");
                    string jsonData = await GetEmployeeSalaryDataAsync();
                    byte[] responseData = Encoding.UTF8.GetBytes(jsonData);
                    await stream.WriteAsync(responseData, 0, responseData.Length);
                }
                else if (credentials[0] == "deleteProduct" && int.TryParse(credentials[1], out int productId))
                {
                    Console.WriteLine($"Запрос на удаление продукта ID: {productId}");
                    string response = await DeleteProductByIdAsync(productId) ? "Product deleted" : "Failed to delete product";
                    byte[] responseData = Encoding.UTF8.GetBytes(response);
                    await stream.WriteAsync(responseData, 0, responseData.Length);
                }
               
                else if (credentials[0] == "getSupport")
                {
                    // Извлекаем UserId из запроса
                    if (int.TryParse(credentials[1], out int appUserId))
                    {
                        Console.WriteLine($"Запрос на получение поддержки для пользователя с ID: {appUserId}");
                        try
                        {
                            string jsonData = await GetSupportsByUserId(appUserId); // Получаем данные о заявках пользователя
                            byte[] responseData = Encoding.UTF8.GetBytes(jsonData); // Преобразуем в байты
                            await stream.WriteAsync(responseData, 0, responseData.Length); // Отправляем клиенту
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Ошибка при обработке заявок: {ex.Message}");
                            byte[] errorResponse = Encoding.UTF8.GetBytes("Error");
                            await stream.WriteAsync(errorResponse, 0, errorResponse.Length);
                        }
                    }
                    else
                    {
                        byte[] errorResponse = Encoding.UTF8.GetBytes("InvalidUserId");
                        await stream.WriteAsync(errorResponse, 0, errorResponse.Length);
                    }
                }
                else if (credentials[0] == "addSupport")
                {

                    try
                    {
                        // Распаковываем данные из сообщения
                        string email = credentials[1];
                        string description = credentials[2];
                        

                        if (int.TryParse(credentials[3], out int supUserId))
                        {
                            Console.WriteLine($"Получен запрос на добавление заявки id {supUserId}");

                            // Вызываем метод для добавления заявки
                            string response = await AddSupportAsync(email, description,supUserId )
                                ? "Success"
                                : "Error";

                            // Отправляем ответ клиенту
                            byte[] responseData = Encoding.UTF8.GetBytes(response);
                            await stream.WriteAsync(responseData, 0, responseData.Length);
                        }
                           
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка обработки запроса: {ex.Message}");
                        byte[] responseData = Encoding.UTF8.GetBytes("Error");
                        await stream.WriteAsync(responseData, 0, responseData.Length);
                    }
                }
                else if (credentials[0] == "updateRole")
                {
                    if (int.TryParse(credentials[1], out int admuserId) && int.TryParse(credentials[2], out int newRoleId))
                    {
                        Console.WriteLine($"Запрос на изменение роли для пользователя с ID: {admuserId} на роль {newRoleId}");
                        try
                        {
                            bool success = await UpdateUserRoleAsync(admuserId, newRoleId);
                            string response = success ? "Success" : "Error";
                            byte[] responseData = Encoding.UTF8.GetBytes(response);
                            await stream.WriteAsync(responseData, 0, responseData.Length);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Ошибка: {ex.Message}");
                            byte[] errorResponse = Encoding.UTF8.GetBytes("Error");
                            await stream.WriteAsync(errorResponse, 0, errorResponse.Length);
                        }
                    }
                    else
                    {
                        byte[] errorResponse = Encoding.UTF8.GetBytes("InvalidData");
                        await stream.WriteAsync(errorResponse, 0, errorResponse.Length);
                    }
                }
                else if (credentials[0] == "updateAdminPanel")
                {
                    string managerCode = credentials[1];
                    string adminCode = credentials[2];
                    try
                    {
                       
                        
                            using (var dbContext = new CrmsystemContext())
                            {
                                var existingPanel = dbContext.AdminPanels.FirstOrDefault(p => p.Id == 1);
                                if (existingPanel != null)
                                {
                                    // Обновление данных
                                    existingPanel.AdminCode = adminCode;
                                    existingPanel.ManagerCode = managerCode;

                                    await dbContext.SaveChangesAsync();

                                    byte[] successResponse = Encoding.UTF8.GetBytes("Success");
                                    await stream.WriteAsync(successResponse, 0, successResponse.Length);
                                }
                                else
                                {
                                    byte[] errorResponse = Encoding.UTF8.GetBytes("AdminPanelNotFound");
                                    await stream.WriteAsync(errorResponse, 0, errorResponse.Length);
                                }
                            }
                        
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка при обновлении AdminPanel: {ex.Message}");
                        byte[] errorResponse = Encoding.UTF8.GetBytes("Error");
                        await stream.WriteAsync(errorResponse, 0, errorResponse.Length);
                    }
                }








            }
        }
        finally
        {
            // Код выполняется только при закрытии соединения
            Console.WriteLine($"Клиент {clientIp} отключен");
            client.Close();
        }
    }
    private async Task<bool> UpdateUserRoleAsync(int userId, int newRoleId)
    {
        using (var dbContext = new CrmsystemContext())
        {
            var user = await dbContext.Accounts.FirstOrDefaultAsync(a => a.Id == userId);

            if (user != null)
            {
                // Обновляем роль пользователя
                user.RoleId = newRoleId;

                // Сохраняем изменения
                await dbContext.SaveChangesAsync();
                return true;
            }
            else
            {
                Console.WriteLine($"Пользователь с ID {userId} не найден.");
                return false;
            }
        }
    }

    private async Task<bool> DeleteProductByIdAsync(int productId)
    {
        try
        {
            using (var dbContext = new CrmsystemContext())
            {
                var product = await dbContext.Products.FindAsync(productId); // Предполагается, что таблица называется `Products`.
                if (product != null)
                {
                    dbContext.Products.Remove(product);
                    await dbContext.SaveChangesAsync();
                    return true;
                }
            }
            return false; // Продукт не найден.
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при удалении продукта: {ex.Message}");
            return false; // Ошибка при удалении.
        }
    }

    private async Task<string> GetEmployeeSalaryDataAsync()
    {
        using (var dbContext = new CrmsystemContext())
        {
            // Получаем данные о сотрудниках и зарплатах
            var employeeSalaries = await dbContext.Employees
                .Where(e => e.Salary.HasValue)
                .Select(e => new
                {
                    e.FirstName,
                    e.LastName,
                    e.Position,
                    e.Salary
                })
                .ToListAsync();

            // Преобразуем в JSON
            return employeeSalaries.Any()
                ? JsonConvert.SerializeObject(employeeSalaries)
                : "NoData";
        }
    }

    private async Task<string> GetApplicationStatusSummaryAsync()
    {
        using (var dbContext = new CrmsystemContext())
        {
            // Группируем по статусам и считаем количество заявок
            var statusSummary = await dbContext.Applications
                .GroupBy(a => a.Status.StatusName) // Используем название статуса
                .Select(g => new
                {
                    StatusName = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            // Возвращаем результат в JSON-формате
            return statusSummary.Any()
                ? JsonConvert.SerializeObject(statusSummary)
                : "NoData";
        }
    }

    private async Task<string> GetTransactionSummaryAsync()
    {
        using (var dbContext = new CrmsystemContext())
        {
            // Группируем транзакции по типу и считаем общую сумму
            var transactionSummary = await dbContext.ProductTransactions
                .GroupBy(t => t.TransactionType)
                .Select(g => new
                {
                    TransactionType = g.Key,
                    TotalAmount = g.Sum(t => Math.Abs(t.Quantity))
                })
                .ToListAsync();

            // Сериализуем данные в JSON
            return transactionSummary.Any()
                ? JsonConvert.SerializeObject(transactionSummary)
                : "NoData";
        }
    }
    private async Task<bool> AddProductAsync(string productName, string unitOfMeasurement, int quantity,  decimal unitPrice)
    {
        try
        {
            using (var context = new CrmsystemContext())
            {
                
                // Создаем новую заявку
                var product = new Product
                {
                    ProductName = productName,
                    UnitOfMeasurement = unitOfMeasurement,
                    Quantity = quantity,
                    Description = "",
                    UnitPrice = unitPrice,
                    LastUpdated = DateTime.Now,
                    
                };

                // Добавляем заявку в базу
                context.Products.Add(product);
                await context.SaveChangesAsync();
            }

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при добавлении заявки: {ex.Message}");
            return false;
        }
    }

    private async Task<string> GetProductData()
    {
        using (var dbContext = new CrmsystemContext())
        {
            // Создаем проекцию с выбором только необходимых полей для продукции
            var products = await dbContext.Products
                .Select(p => new
                {
                    p.ProductId,
                    p.ProductName,
                    p.Description,
                    p.Quantity,
                    p.UnitOfMeasurement,
                    p.UnitPrice
                })
                .ToListAsync();

            // Сериализуем список продукции в JSON
            return products.Count == 0 ? "NoData" : JsonConvert.SerializeObject(products);
        }
    }
        
    private async Task<string> GetAllApplicationsForExport()
    {
        using (var dbContext = new CrmsystemContext())
        {
            var applications = await dbContext.Applications
                .Include(a => a.Account)  // Подключаем Account для логина
                .Include(a => a.Status)   // Подключаем Status для статуса заявки
                .Select(a => new
                {
                    a.Id,
                    Login = a.Account.Login,
                    a.ContactInfo,
                    a.ProductName,
                    a.Description,
                    a.TotalPrice,
                    a.Quantity,               // Новое поле
                    a.UnitOfMeasurement,      // Новое поле
                    Status = a.Status.StatusName,
                    DateSubmitted = a.DateSubmitted.HasValue
                        ? a.DateSubmitted.Value.ToString("dd.MM.yyyy HH:mm:ss")
                        : null
                })
                .ToListAsync();

            return applications.Count == 0 ? "Error" : JsonConvert.SerializeObject(applications);
        }
    }

   


    private async Task ApproveApplication(int applicationId)
    {
        using (var dbContext = new CrmsystemContext())
        {
            var application = await dbContext.Applications.FindAsync(applicationId);
            if (application != null)
            {
                application.StatusId = 2; // ID статуса "Одобрено"
                await dbContext.SaveChangesAsync();
            }
        }
    }

    private async Task RejectApplication(int applicationId)
    {
        using (var dbContext = new CrmsystemContext())
        {
            var application = await dbContext.Applications.FindAsync(applicationId);
            if (application != null)
            {
                application.StatusId = 3; // ID статуса "Отклонено"
                await dbContext.SaveChangesAsync();
            }
        }
    }
    //private async Task<string> GetUnprocessedApplications()
    //{
    //    using (var dbContext = new TestjsonContext())
    //    {
    //        // Фильтруем заявки, которые находятся в статусе "Ожидает"
    //        var applications = await dbContext.Applications
    //            .Include(a => a.Account)  // Для получения логина пользователя
    //            .Include(a => a.Status)   // Для получения названия статуса
    //            .Where(a => a.StatusId == 1) // ID статуса "Ожидает"
    //            .Select(a => new
    //            {
    //                a.Id,
    //                Login = a.Account.Login,  // Логин пользователя
    //                a.ContactInfo,
    //                a.ProductName,
    //                a.Description,
    //                a.TotalPrice,
    //                Status = a.Status.StatusName,   // Название статуса
    //                DateSubmitted = a.DateSubmitted.HasValue
    //                    ? a.DateSubmitted.Value.ToString("dd.MM.yyyy")
    //                    : null // Форматируем дату
    //            })
    //            .ToListAsync();

    //        // Если данных нет, возвращаем "NoData"
    //        return applications.Count == 0 ? "NoData" : JsonConvert.SerializeObject(applications);
    //    }
    //}
    private async Task<string> GetUnprocessedApplications()
    {
        using (var dbContext = new CrmsystemContext())
        {
            var applications = await dbContext.Applications
                .Include(a => a.Account)
                .Include(a => a.Status)
                .Where(a => a.StatusId == 1) // ID статуса "Ожидает"
                .Select(a => new
                {
                    a.Id,
                    Login = a.Account.Login,
                    a.ContactInfo,
                    a.ProductName,
                    a.Description,
                    a.TotalPrice,
                    a.Quantity,               // Новое поле
                    a.UnitOfMeasurement,      // Новое поле
                    Status = a.Status.StatusName,
                    DateSubmitted = a.DateSubmitted.HasValue
                        ? a.DateSubmitted.Value.ToString("dd.MM.yyyy")
                        : null
                })
                .ToListAsync();

            return applications.Count == 0 ? "NoData" : JsonConvert.SerializeObject(applications);
        }
    }
    private async Task<string> GetApprovedApplications()
    {
        using (var dbContext = new CrmsystemContext())
        {
            var applications = await dbContext.Applications
                .Include(a => a.Account)
                .Include(a => a.Status)
                .Where(a => a.StatusId == 2) // ID статуса "Ожидает"
                .Select(a => new
                {
                    a.Id,
                    Login = a.Account.Login,
                    a.ContactInfo,
                    a.ProductName,
                    a.Description,
                    a.TotalPrice,
                    a.Quantity,               // Новое поле
                    a.UnitOfMeasurement,      // Новое поле
                    Status = a.Status.StatusName,
                    DateSubmitted = a.DateSubmitted.HasValue
                        ? a.DateSubmitted.Value.ToString("dd.MM.yyyy")
                        : null
                })
                .ToListAsync();

            return applications.Count == 0 ? "NoData" : JsonConvert.SerializeObject(applications);
        }
    }



    private async Task<string> GetAllApplications()
    {
        using (var dbContext = new CrmsystemContext())
        {
            var applications = await dbContext.Applications
                .Include(a => a.Account)
                .Where(a => a.DateSubmitted >= DateTime.Now.AddDays(-3))
                .Select(a => new
                {
                    a.Id,
                    Login = a.Account.Login,
                    a.ContactInfo,
                    a.ProductName,
                    a.Description,
                    a.TotalPrice,
                    a.Status,
                    a.Quantity, // Новое поле
                    a.UnitOfMeasurement, // Новое поле
                    DateSubmitted = a.DateSubmitted.HasValue
                        ? a.DateSubmitted.Value.ToString("dd.MM.yyyy")
                        : null
                })
                .ToListAsync();

            return applications.Count == 0 ? "NoData" : JsonConvert.SerializeObject(applications);
        }
    }
    private async Task<string> GetSupportsByUserId(int userId)
    {
        using (var dbContext = new CrmsystemContext()) // Используйте ваш контекст
        {


            var supports = await dbContext.SupportTickets
                .Include(a => a.User)  // Подключаем Account для получения логина
                .Include(a => a.Status)   // Подключаем Status для получения названия статуса
                .Where(a => a.User.Id == userId)
                .Select(a => new
                {
                    a.TicketId,
                    a.SubmissionDate,
                    a.Description,
                    StatusName = a.Status.StatusName,
                   
                })
                .ToListAsync();

            // Возвращаем результат
            if (supports.Count == 0)
            {
                return "NoData";
            }

            return JsonConvert.SerializeObject(supports);
        }
    }


    private async Task<string> GetApplicationsByUserId(int userId)
    {
        using (var dbContext = new CrmsystemContext()) // Используйте ваш контекст
        {
            var threeDaysAgo = DateTime.Now.AddDays(-3);

            // Фильтруем заявки только для указанного пользователя
            var applications = await dbContext.Applications
                .Include(a => a.Account)  // Подключаем Account для получения логина
                .Include(a => a.Status)   // Подключаем Status для получения названия статуса
                .Where(a => a.AccountId == userId &&
                            a.DateSubmitted.HasValue &&
                            a.DateSubmitted.Value >= threeDaysAgo) // Убедимся, что DateSubmitted не null
                .Select(a => new
                {
                    a.Id,
                    Login = a.Account.Login,  // Логин пользователя
                    a.ContactInfo,
                    a.ProductName,
                    a.Description,
                    a.TotalPrice,
                    a.Quantity,              // Новое поле: Количество
                    a.UnitOfMeasurement,     // Новое поле: Единица измерения
                    Status = a.Status.StatusName,   // Название статуса заявки
                    DateSubmitted = a.DateSubmitted.HasValue
                        ? a.DateSubmitted.Value.ToString("dd.MM.yyyy")
                        : null // Форматируем дату как строку
                })
                .ToListAsync();

            // Возвращаем результат
            if (applications.Count == 0)
            {
                return "NoData";
            }

            return JsonConvert.SerializeObject(applications);
        }
    }


    private async Task<string> GetApplicationHistoryByUserId(int userId)
    {
        using (var dbContext = new CrmsystemContext()) // Используем контекст базы данных
        {
            // Выбираем заявки текущего пользователя
            var applications = await dbContext.Applications
                .Include(a => a.Account)  // Подключаем Account для получения логина
                .Include(a => a.Status)   // Подключаем Status для получения названия статуса
                .Where(a => a.AccountId == userId) // Фильтруем по UserId
                .Select(a => new
                {
                    a.Id,
                    Login = a.Account.Login,        // Логин пользователя
                    a.ContactInfo,                 // Контактная информация
                    a.ProductName,                 // Название продукта
                    a.Description,                 // Описание продукта
                    a.TotalPrice,                  // Общая цена
                    a.Quantity,                    // Количество
                    a.UnitOfMeasurement,           // Единица измерения
                    Status = a.Status.StatusName,  // Название статуса заявки
                    DateSubmitted = a.DateSubmitted.HasValue
                        ? a.DateSubmitted.Value.ToString("dd.MM.yyyy")
                        : null                      // Форматируем дату как строку
                })
                .ToListAsync();

            // Если заявок нет, возвращаем "NoData", иначе сериализуем их
            return applications.Count == 0 ? "NoData" : JsonConvert.SerializeObject(applications);
        }
    }

        private async Task<bool> DeleteApplicationByIdAsync(int applicationId)
    {
        try
        {
            using (var dbContext = new CrmsystemContext())
            {
                // Попробуем найти заявку
                var application = await dbContext.Applications.FindAsync(applicationId);
                if (application == null)
                {
                    Console.WriteLine($"Заявка с ID {applicationId} не найдена.");
                    return false;
                }

                // Удаляем заявку
                dbContext.Applications.Remove(application);
                await dbContext.SaveChangesAsync();

                Console.WriteLine($"Заявка с ID {applicationId} успешно удалена.");
                return true;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при удалении заявки с ID {applicationId}: {ex.Message}");
            return false;
        }
    }

    private async Task<bool> DeleteEmployeeByIdAsync(int employeeId)
    {
        try
        {
            using (var dbContext = new CrmsystemContext())
            {
                var user = await dbContext.Employees.FindAsync(employeeId);
                if (user != null)
                {
                    dbContext.Employees.Remove(user);
                    await dbContext.SaveChangesAsync();
                    return true;
                }
            }
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при удалении пользователя: {ex.Message}");
            return false;
        }
    }
    private async Task<bool> AddSupportAsync(string email, string description, int userID )
    {
        try
        {
            using (var context = new CrmsystemContext())
            {
                // Получаем пользователя по логину
                var account = await context.Accounts.FirstOrDefaultAsync(a => a.Id == userID);
               
                // Создаем новую заявку
                var support = new SupportTicket
                {

                    User = account,
                    UserId = userID,
                    UserEmail = email,
                    Description = description,
                   
                    StatusId = 1, // Статус по умолчанию
                    SubmissionDate = DateTime.Now
                };

                // Добавляем заявку в базу
                context.SupportTickets.Add(support);
                await context.SaveChangesAsync();
            }

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при добавлении заявки: {ex.Message}");
            return false;
        }
    }

    private async Task<bool> AddApplicationAsync(string login, string contactInfo, string productName, string description, decimal totalPrice, int quantity, string unitOfMeasurement)
    {
        try
        {
            using (var context = new CrmsystemContext())
            {
                // Получаем пользователя по логину
                var account = await context.Accounts.FirstOrDefaultAsync(a => a.Login == login);
                if (account == null)
                    throw new Exception("Пользователь не найден.");

                // Проверяем корректность количества и единицы измерения
                if (quantity <= 0)
                    throw new Exception("Количество должно быть больше нуля.");
                if (string.IsNullOrWhiteSpace(unitOfMeasurement))
                    throw new Exception("Единица измерения не может быть пустой.");

                // Создаем новую заявку
                var application = new Application
                {
                    AccountId = account.Id,
                    ContactInfo = contactInfo,
                    ProductName = productName,
                    Description = description,
                    TotalPrice = totalPrice,
                    Quantity = quantity, // Сохраняем количество
                    UnitOfMeasurement = unitOfMeasurement, // Сохраняем единицу измерения
                    StatusId = 1, // Статус по умолчанию
                    DateSubmitted = DateTime.Now
                };

                // Добавляем заявку в базу
                context.Applications.Add(application);
                await context.SaveChangesAsync();
            }

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при добавлении заявки: {ex.Message}");
            return false;
        }
    }




    private async Task<Account> ValidateUserAsync(string username, string password)
    {
        using (var dbContext = new CrmsystemContext())
        {
            return await dbContext.Accounts
                .SingleOrDefaultAsync(u => u.Login == username && u.Password == password && u.RoleId == 3);
        }
    }
    private async Task<bool> ValidateManagerUserAsync(string username, string password, string code)
    {
        using (var dbContext = new CrmsystemContext())
        {
            var adminPanel = await dbContext.AdminPanels.SingleOrDefaultAsync();

            if (adminPanel != null && adminPanel.ManagerCode.Trim() == code.Trim())
            {

                var user = await dbContext.Accounts
                                           .SingleOrDefaultAsync(u => u.Login == username && u.Password == password && u.RoleId == 2);


                return user != null;
            }
            else
            {
                return false;
            }

        }
    }

    private async Task<bool> AddEmployeeAsync(string firstName, string lastName, string middleName, decimal salary, DateOnly birthDate, string position, DateOnly hireDate)
    {
        try
        {
            using (var context = new CrmsystemContext())
            {
                var newEmployee = new Employee
                {
                    FirstName = firstName,
                    LastName = lastName,
                    MiddleName = middleName,
                    Salary = salary,
                    BirthDate = birthDate,
                    Position = position,
                    HireDate = hireDate
                };

                context.Employees.Add(newEmployee);
                await context.SaveChangesAsync();
                return true;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка добавления сотрудника: {ex.Message}");
            return false;
        }
    }

    private async Task<bool> ValidateAdminUserAsync(string username, string password, string code)
    {
        using (var dbContext = new CrmsystemContext())
        {
            var adminPanel = await dbContext.AdminPanels.SingleOrDefaultAsync();

            if (adminPanel != null && adminPanel.AdminCode.Trim() == code.Trim())
            {

                var user = await dbContext.Accounts
                                           .SingleOrDefaultAsync(u => u.Login == username && u.Password == password && u.RoleId == 1);


                return user != null;
            }
            else
            {
                return false;
            }

        }
    }
   
    private async Task<bool> DeleteTransactionByIdAsync(int transactionId)
    {
        try
        {
            using (var dbContext = new CrmsystemContext())
            {
                var transaction = await dbContext.Transactions.FindAsync(transactionId);
                if (transaction != null)
                {
                    dbContext.Transactions.Remove(transaction);
                    await dbContext.SaveChangesAsync();
                    return true;
                }
            }
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при удалении пользователя: {ex.Message}");
            return false;
        }
    }
    private async Task<bool> DeleteUserByIdAsync(int userId)
    {
        try
        {
            using (var dbContext = new CrmsystemContext())
            {
                var user = await dbContext.Accounts.FindAsync(userId);
                if (user != null)
                {
                    dbContext.Accounts.Remove(user);
                    await dbContext.SaveChangesAsync();
                    return true;
                }
            }
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при удалении пользователя: {ex.Message}");
            return false;
        }
    }
    private async Task<bool> AddUserAsync(string username, string password)
    {
        using (var dbContext = new CrmsystemContext())
        {
            // Проверка: существует ли пользователь с таким именем
            var existingUser = await dbContext.Accounts.SingleOrDefaultAsync(u => u.Login == username);
            if (existingUser != null)
            {
                Console.WriteLine($"Пользователь {username} уже существует");
                return false; // Пользователь уже зарегистрирован
            }

            // Если пользователя с таким именем нет, добавляем нового пользователя
            var newUser = new Account
            {
                Login = username,
                Password = password,
                RoleId = 3
            };

            await dbContext.Accounts.AddAsync(newUser);
            await dbContext.SaveChangesAsync();

            Console.WriteLine($"Новый пользователь добавлен: {username}");
            return true;
        }
    }
    private async Task<bool> AddUserAsync(string username, string password, int roleID) //for admin panel
    {
        using (var dbContext = new CrmsystemContext())
        {
            // Проверка: существует ли пользователь с таким именем
            var existingUser = await dbContext.Accounts.SingleOrDefaultAsync(u => u.Login == username);
            if (existingUser != null)
            {
                Console.WriteLine($"Пользователь {username} уже существует");
                return false; // Пользователь уже зарегистрирован
            }

            // Если пользователя с таким именем нет, добавляем нового пользователя
            var newUser = new Account
            {
                Login = username,
                Password = password,
                RoleId = roleID
            };

            await dbContext.Accounts.AddAsync(newUser);
            await dbContext.SaveChangesAsync();

            Console.WriteLine($"Новый пользователь добавлен: {username}");
            return true;
        }
    }
    private async Task<string> GetAllUsers()
    {
        using (var dbContext = new CrmsystemContext())
        {
            // Создаем проекцию с выбором только необходимых полей
            var users = await dbContext.Accounts
             .Select(u => new
             {
                 u.Id,
                 u.Login,
                 u.Password,
                 u.RoleId
             })
             .ToListAsync();

            // Сериализуем список пользователей в JSON
            return users.Count == 0 ? "NoData" : JsonConvert.SerializeObject(users);
        }


    }
    private async Task<string> GetAllTransactions()
    {
        using (var dbContext = new CrmsystemContext())
        {
            // Создаем проекцию с выбором только необходимых полей
            var transactions = await dbContext.Transactions
             .Select(u => new
             {
                 u.Id,
                 u.TransactionDate,
                 u.Description,
                 u.Amount,
                 u.TransactionType
             })
             .ToListAsync();

            // Сериализуем список пользователей в JSON
            return transactions.Count == 0 ? "NoData" : JsonConvert.SerializeObject(transactions);
        }


    }
    
   


    private async Task<string> GetEmployeeDataAsync()
    {
        using (var dbContext = new CrmsystemContext())
        {
            var employees = await dbContext.Employees.ToListAsync();
            return JsonConvert.SerializeObject(employees);
        }
    }
    private async Task<string> GetAllEmployees()
    {
        using (var dbContext = new CrmsystemContext())
        {
            // Создаем проекцию с выбором только необходимых полей
            var users = await dbContext.Employees
             .Select(u => new
             {
                 u.EmployeeId,
                 u.LastName,
                 u.FirstName,
                 u.Position,
                 u.Salary,
                 u.HireDate
             })
             .ToListAsync();

            // Сериализуем список пользователей в JSON
            return users.Count == 0 ? "NoData" : JsonConvert.SerializeObject(users);
        }

    }
}
