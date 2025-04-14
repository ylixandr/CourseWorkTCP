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
using System.Text.Json;
using System.Threading.Tasks;
using TCPServer;
using TCPServer;
using TCPServer.balanceModule;
using TESTINGCOURSEWORK.Models;

public class Server
{
    private readonly int _port = 12345;
    private TcpListener _server;
    private static readonly object _lock = new object();
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
        byte[] buffer = new byte[8192];

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
                else if (receivedData == "getEmployees")
                {
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
                else if (credentials[0] == "salaryEmployee" && int.TryParse(credentials[1], out int salemployeeId))
                {
                    Console.WriteLine($"Запрос на начисление зарплаты пользователя ID: {salemployeeId}");
                    string response = await SalaryEmployeeByIdAsync(salemployeeId) ? "User detected" : "Failed to detect user";
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
                        string transactionType = credentials[1];
                        decimal amount = decimal.Parse(credentials[2]);
                        string description = credentials[3];

                        Console.WriteLine($"Данные транзакции: Тип - {transactionType}, Сумма - {amount}, Описание - {description}");

                        using (var context = new CrmsystemContext())
                        {
                            var latestBalance = context.BalanceHistories
                                .OrderByDescending(b => b.PeriodEnd)
                                .FirstOrDefault();

                            decimal currentBalance = latestBalance?.Balance ?? 0;

                            string categoryName = transactionType == "Пополнение" ? "Продажи" : "Зарплата";
                            var category = await context.TransactionCategories
                                .FirstOrDefaultAsync(c => c.Name == categoryName);
                            if (category == null)
                            {
                                category = new TransactionCategory { Name = categoryName };
                                context.TransactionCategories.Add(category);
                                await context.SaveChangesAsync();
                            }

                            if (transactionType == "Пополнение")
                            {
                                currentBalance += amount;
                            }
                            else if (currentBalance >= amount)
                            {
                                currentBalance -= amount;
                            }
                            else
                            {
                                throw new Exception("Недостаточно средств на балансе для выплаты зарплаты.");
                            }

                            var descriptionEntity = new Description
                            {
                                Content = description
                            };
                            context.Descriptions.Add(descriptionEntity);
                            await context.SaveChangesAsync();

                            var transaction = new Transaction
                            {
                                TransactionDate = DateTime.Now,
                                Amount = amount,
                                CategoryId = category.Id,
                                DescriptionId = descriptionEntity.Id,
                                RelatedEntityType = transactionType == "Пополнение" ? "Manual" : "Employee", // Уточняем тип сущности
                                RelatedEntityId = null // Укажи ID, если транзакция связана с конкретной сущностью
                            };

                            context.Transactions.Add(transaction);

                            var now = DateTime.Now;
                            var newBalanceEntry = new BalanceHistory
                            {
                                PeriodStart = new DateTime(now.Year, now.Month, 1), // Начало месяца
                                PeriodEnd = new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month)), // Конец месяца
                                TotalIncome = transactionType == "Пополнение" ? amount : 0,
                                TotalExpenses = transactionType != "Пополнение" ? amount : 0,
                                Balance = currentBalance
                            };
                            context.BalanceHistories.Add(newBalanceEntry);

                            await context.SaveChangesAsync();
                        }

                        byte[] responseData = Encoding.UTF8.GetBytes("Success");
                        await stream.WriteAsync(responseData, 0, responseData.Length);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка при добавлении транзакции: {ex.Message}");
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
                        var transactions = await db.Transactions
                            .Include(t => t.Category)
                            .ToListAsync();

                        // Получаем текущий баланс из последней записи BalanceHistory
                        var latestBalance = await db.BalanceHistories
                            .OrderByDescending(b => b.PeriodEnd)
                            .FirstOrDefaultAsync();
                        decimal currentBalance = latestBalance?.Balance ?? 0;

                        // Находим категории для доходов и расходов
                        var incomeCategory = await db.TransactionCategories.FirstOrDefaultAsync(c => c.Name == "Продажи");
                        var expenseCategory = await db.TransactionCategories.FirstOrDefaultAsync(c => c.Name == "Зарплата");

                        int? incomeCategoryId = incomeCategory?.Id;
                        int? expenseCategoryId = expenseCategory?.Id;

                        var report = new
                        {
                            Balance = currentBalance,
                            TotalTransactions = transactions.Count,
                            IncomeSum = incomeCategoryId.HasValue
                                ? transactions.Where(t => t.CategoryId == incomeCategoryId.Value).Sum(t => t.Amount)
                                : 0,
                            ExpenseSum = expenseCategoryId.HasValue
                                ? transactions.Where(t => t.CategoryId == expenseCategoryId.Value).Sum(t => t.Amount)
                                : 0,
                            MaxTransaction = transactions.OrderByDescending(t => t.Amount).FirstOrDefault(),
                            MinTransaction = transactions.OrderBy(t => t.Amount).FirstOrDefault(),
                            AverageTransaction = transactions.Any() ? transactions.Average(t => t.Amount) : 0,
                            MonthlySummary = transactions.GroupBy(t => t.TransactionDate.Month)
                                .Select(g => new
                                {
                                    Month = g.Key,
                                    Total = g.Count(),
                                    Income = incomeCategoryId.HasValue
                                        ? g.Where(t => t.CategoryId == incomeCategoryId.Value).Sum(t => t.Amount)
                                        : 0,
                                    Expense = expenseCategoryId.HasValue
                                        ? g.Where(t => t.CategoryId == expenseCategoryId.Value).Sum(t => t.Amount)
                                        : 0,
                                    MonthEndBalance = currentBalance
                                }).ToList(),
                            Errors = transactions.GroupBy(t => new { t.Amount, t.CategoryId }) // Используем CategoryId
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
                        decimal totalPrice = decimal.Parse(credentials[7]);
                        int quantity = int.Parse(credentials[5]); // Новое поле: Количество
                        string unitOfMeasurement = credentials[6]; // Новое поле: Единица измерения

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
                                    product.Quantity += stockRequest.Quantity;
                                }
                                else if (stockRequest.TransactionType == "Расход")
                                {
                                    if (product.Quantity < Math.Abs(stockRequest.Quantity))
                                    {
                                        throw new Exception("Недостаток");
                                    }
                                    else
                                    {
                                        product.Quantity += stockRequest.Quantity;
                                    }
                                }

                                product.LastUpdated = DateTime.Now;

                                var descriptionEntity = new Description
                                {
                                    Content = stockRequest.DescriptionId != null ? (await dbContext.Descriptions.FindAsync(stockRequest.DescriptionId))?.Content : null
                                };
                                if (!string.IsNullOrEmpty(descriptionEntity.Content))
                                {
                                    dbContext.Descriptions.Add(descriptionEntity);
                                    await dbContext.SaveChangesAsync();
                                }

                                var transaction = new ProductTransaction
                                {
                                    ProductId = stockRequest.ProductId,
                                    Quantity = stockRequest.Quantity,
                                    DescriptionId = descriptionEntity.Id != 0 ? descriptionEntity.Id : (int?)null,
                                    TransactionDate = DateTime.Now
                                };

                                dbContext.ProductTransactions.Add(transaction);
                                await dbContext.SaveChangesAsync();
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

                else if (receivedData == "export_transactionsProd")
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
                            var employees = await dbContext.Employees
                                .Where(e => e.Salary.HasValue)
                                .ToListAsync();

                            // Получаем текущий баланс из последней записи BalanceHistory
                            var latestBalance = await dbContext.BalanceHistories
                                .OrderByDescending(b => b.PeriodEnd)
                                .FirstOrDefaultAsync();
                            decimal currentBalance = latestBalance?.Balance ?? 0;

                            decimal totalSalary = 0;
                            List<SalaryRecord> salaryRecords = new();

                            // Находим категорию "Зарплата"
                            var salaryCategory = await dbContext.TransactionCategories
                                .FirstOrDefaultAsync(c => c.Name == "Зарплата");
                            if (salaryCategory == null)
                            {
                                salaryCategory = new TransactionCategory { Name = "Зарплата" };
                                dbContext.TransactionCategories.Add(salaryCategory);
                                await dbContext.SaveChangesAsync();
                            }

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

                                // Создаем транзакцию для каждой зарплаты
                                var descriptionEntity = new Description
                                {
                                    Content = $"Зарплата сотруднику {employee.LastName} за {salaryRequest.Date:MMMM yyyy}"
                                };
                                dbContext.Descriptions.Add(descriptionEntity);
                                await dbContext.SaveChangesAsync();

                                var transaction = new Transaction
                                {
                                    TransactionDate = DateTime.Now,
                                    Amount = employeeSalary,
                                    CategoryId = salaryCategory.Id,
                                    DescriptionId = descriptionEntity.Id,
                                    RelatedEntityType = "Employee",
                                    RelatedEntityId = employee.EmployeeId
                                };
                                dbContext.Transactions.Add(transaction);
                            }

                            // Вычет подоходного налога (13%)
                            decimal taxAmount = totalSalary * 0.13M;
                            decimal netSalary = totalSalary - taxAmount;

                            if (currentBalance < netSalary)
                            {
                                throw new Exception("Недостаточно средств на балансе для выплаты зарплаты.");
                            }

                            // Обновляем баланс
                            currentBalance -= netSalary;

                            // Создаем новую запись в BalanceHistory
                            var newBalanceEntry = new BalanceHistory
                            {
                                PeriodStart = new DateTime(salaryRequest.Date.Year, salaryRequest.Date.Month, 1),
                                PeriodEnd = new DateTime(salaryRequest.Date.Year, salaryRequest.Date.Month, DateTime.DaysInMonth(salaryRequest.Date.Year, salaryRequest.Date.Month)),
                                TotalIncome = 0,
                                TotalExpenses = netSalary,
                                Balance = currentBalance
                            };
                            dbContext.BalanceHistories.Add(newBalanceEntry);

                            await dbContext.SaveChangesAsync();

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
                            string response = await AddSupportAsync(email, description, supUserId)
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
               
                else if (receivedData.StartsWith("savePayrollJson:"))
                {
                    string[] parts = receivedData.Split(new[] { ':' }, 2); // Разделяем на команду и данные
                    await HandleSavePayrollJson(parts, stream);
                }
                else if (receivedData.StartsWith("getPayrollData:"))
                {
                    string[] parts = receivedData.Split(':');
                    await HandleGetPayrollData(parts, stream);
                }
                else if (receivedData.StartsWith("salaryEmployee:"))
                {
                    string[] parts = receivedData.Split(':');
                    await HandleSalaryEmployee(parts, stream);
                }
                else if (receivedData.StartsWith("getBalanceData"))
                {
                    Console.WriteLine("Запрос данных о балансе.");

                    try
                    {
                        string balanceData = await GetBalanceDataAsync();
                        byte[] responseData = Encoding.UTF8.GetBytes(balanceData);
                        await stream.WriteAsync(responseData, 0, responseData.Length);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка при обработке запроса баланса: {ex.Message}");
                        byte[] errorResponse = Encoding.UTF8.GetBytes("Error: " + ex.Message);
                        await stream.WriteAsync(errorResponse, 0, errorResponse.Length);
                    }
                }
                else if (receivedData.StartsWith("getBalanceSnapshot"))
                {
                    Console.WriteLine("Запрос текущего баланса.");
                    string balanceData = await GetBalanceSnapshotAsync();
                    byte[] responseData = Encoding.UTF8.GetBytes(balanceData);
                    await stream.WriteAsync(responseData, 0, responseData.Length);
                }
                else if (receivedData.StartsWith("addAsset"))
                {
                    Console.WriteLine("Добавление актива.");
                    string jsonData = receivedData.Substring("addAsset".Length);
                    string response = await AddAssetAsync(jsonData);
                    byte[] responseData = Encoding.UTF8.GetBytes(response);
                    await stream.WriteAsync(responseData, 0, responseData.Length);
                }
                else if (receivedData.StartsWith("updateAsset"))
                {
                    Console.WriteLine("Обновление актива.");
                    string jsonData = receivedData.Substring("updateAsset".Length);
                    string response = await UpdateAssetAsync(jsonData);
                    byte[] responseData = Encoding.UTF8.GetBytes(response);
                    await stream.WriteAsync(responseData, 0, responseData.Length);
                }
                else if (receivedData.StartsWith("deleteAsset"))
                {
                    Console.WriteLine("Удаление актива.");
                    string jsonData = receivedData.Substring("deleteAsset".Length);
                    string response = await DeleteAssetAsync(jsonData);
                    byte[] responseData = Encoding.UTF8.GetBytes(response);
                    await stream.WriteAsync(responseData, 0, responseData.Length);
                }
                else if (receivedData.StartsWith("addLiability"))
                {
                    Console.WriteLine("Добавление обязательства.");
                    string jsonData = receivedData.Substring("addLiability".Length);
                    string response = await AddLiabilityAsync(jsonData);
                    byte[] responseData = Encoding.UTF8.GetBytes(response);
                    await stream.WriteAsync(responseData, 0, responseData.Length);
                }
                else if (receivedData.StartsWith("updateLiability"))
                {
                    Console.WriteLine("Обновление обязательства.");
                    string jsonData = receivedData.Substring("updateLiability".Length);
                    string response = await UpdateLiabilityAsync(jsonData);
                    byte[] responseData = Encoding.UTF8.GetBytes(response);
                    await stream.WriteAsync(responseData, 0, responseData.Length);
                }
                else if (receivedData.StartsWith("deleteLiability"))
                {
                    Console.WriteLine("Удаление обязательства.");
                    string jsonData = receivedData.Substring("deleteLiability".Length);
                    string response = await DeleteLiabilityAsync(jsonData);
                    byte[] responseData = Encoding.UTF8.GetBytes(response);
                    await stream.WriteAsync(responseData, 0, responseData.Length);
                }
                else if (receivedData.StartsWith("getBalanceSnapshot"))
                {
                    Console.WriteLine("Получение моментального снимка баланса.");
                    string jsonData = receivedData.Substring("getBalanceSnapshot".Length);
                    string response = await GetBalanceSnapshotAsync(jsonData);
                    byte[] responseData = Encoding.UTF8.GetBytes(response);
                    await stream.WriteAsync(responseData, 0, responseData.Length);
                }
                else if (receivedData.StartsWith("compareBalanceSnapshots"))
                {
                    Console.WriteLine("Сравнение снимков баланса.");
                    string jsonData = receivedData.Substring("compareBalanceSnapshots".Length);
                    string response = await CompareBalanceSnapshotsAsync(jsonData);
                    byte[] responseData = Encoding.UTF8.GetBytes(response);
                    await stream.WriteAsync(responseData, 0, responseData.Length);
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
    private async Task<string> GetBalanceSnapshotAsync(string jsonData)
    {
        try
        {
            // Парсим параметры (даты начала и конца периода)
            var request = JsonConvert.DeserializeObject<dynamic>(jsonData);
            DateTime? startDate = request.StartDate != null ? DateTime.Parse((string)request.StartDate) : (DateTime?)null;
            DateTime? endDate = request.EndDate != null ? DateTime.Parse((string)request.EndDate) : (DateTime?)null;

            using (var dbContext = new CrmsystemContext())
            {
                // Фильтруем активы по дате приобретения
                var assetsQuery = dbContext.Assets.AsQueryable();
                if (startDate.HasValue)
                    assetsQuery = assetsQuery.Where(a => a.AcquisitionDate >= startDate.Value);
                if (endDate.HasValue)
                    assetsQuery = assetsQuery.Where(a => a.AcquisitionDate <= endDate.Value);

                var assets = await assetsQuery.ToListAsync();

                // Фильтруем обязательства по дате погашения
                var liabilitiesQuery = dbContext.Liabilities.AsQueryable();
                if (startDate.HasValue)
                    liabilitiesQuery = liabilitiesQuery.Where(l => l.DueDate >= startDate.Value);
                if (endDate.HasValue)
                    liabilitiesQuery = liabilitiesQuery.Where(l => l.DueDate <= endDate.Value);

                var liabilities = await liabilitiesQuery.ToListAsync();

                // Группируем активы по категориям
                var assetCategories = assets
                    .GroupBy(a => a.Category)
                    .Select(g => new
                    {
                        Category = g.Key,
                        TotalAmount = g.Sum(a => a.Amount)
                    })
                    .ToList();

                decimal totalAssets = assetCategories.Sum(c => c.TotalAmount);

                // Группируем обязательства по категориям
                var liabilityCategories = liabilities
                    .GroupBy(l => l.Category)
                    .Select(g => new
                    {
                        Category = g.Key,
                        TotalAmount = g.Sum(l => l.Amount)
                    })
                    .ToList();

                decimal totalLiabilities = liabilityCategories.Sum(c => c.TotalAmount);

                // Собственный капитал = Активы - Пассивы
                decimal equity = totalAssets - totalLiabilities;

                // Проверка баланса
                decimal balanceCheck = totalAssets - totalLiabilities - equity;

                return JsonConvert.SerializeObject(new
                {
                    Assets = new { Total = totalAssets, Categories = assetCategories },
                    Liabilities = new { Total = totalLiabilities, Categories = liabilityCategories },
                    Equity = equity,
                    BalanceCheck = balanceCheck
                });
            }
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }

    private async Task<string> CompareBalanceSnapshotsAsync(string jsonData)
    {
        try
        {
            var request = JsonConvert.DeserializeObject<dynamic>(jsonData);
            DateTime? period1Start = request.Period1Start != null ? DateTime.Parse((string)request.Period1Start) : (DateTime?)null;
            DateTime? period1End = request.Period1End != null ? DateTime.Parse((string)request.Period1End) : (DateTime?)null;
            DateTime? period2Start = request.Period2Start != null ? DateTime.Parse((string)request.Period2Start) : (DateTime?)null;
            DateTime? period2End = request.Period2End != null ? DateTime.Parse((string)request.Period2End) : (DateTime?)null;

            using (var dbContext = new CrmsystemContext())
            {
                // Первый период
                var assetsQuery1 = dbContext.Assets.AsQueryable();
                if (period1Start.HasValue)
                    assetsQuery1 = assetsQuery1.Where(a => a.AcquisitionDate >= period1Start.Value);
                if (period1End.HasValue)
                    assetsQuery1 = assetsQuery1.Where(a => a.AcquisitionDate <= period1End.Value);
                var assets1 = await assetsQuery1.ToListAsync();

                var liabilitiesQuery1 = dbContext.Liabilities.AsQueryable();
                if (period1Start.HasValue)
                    liabilitiesQuery1 = liabilitiesQuery1.Where(l => l.DueDate >= period1Start.Value);
                if (period1End.HasValue)
                    liabilitiesQuery1 = liabilitiesQuery1.Where(l => l.DueDate <= period1End.Value);
                var liabilities1 = await liabilitiesQuery1.ToListAsync();

                decimal totalAssets1 = assets1.Sum(a => a.Amount);
                decimal totalLiabilities1 = liabilities1.Sum(l => l.Amount);
                decimal equity1 = totalAssets1 - totalLiabilities1;

                // Второй период
                var assetsQuery2 = dbContext.Assets.AsQueryable();
                if (period2Start.HasValue)
                    assetsQuery2 = assetsQuery2.Where(a => a.AcquisitionDate >= period2Start.Value);
                if (period2End.HasValue)
                    assetsQuery2 = assetsQuery2.Where(a => a.AcquisitionDate <= period2End.Value);
                var assets2 = await assetsQuery2.ToListAsync();

                var liabilitiesQuery2 = dbContext.Liabilities.AsQueryable();
                if (period2Start.HasValue)
                    liabilitiesQuery2 = liabilitiesQuery2.Where(l => l.DueDate >= period2Start.Value);
                if (period2End.HasValue)
                    liabilitiesQuery2 = liabilitiesQuery2.Where(l => l.DueDate <= period2End.Value);
                var liabilities2 = await liabilitiesQuery2.ToListAsync();

                decimal totalAssets2 = assets2.Sum(a => a.Amount);
                decimal totalLiabilities2 = liabilities2.Sum(l => l.Amount);
                decimal equity2 = totalAssets2 - totalLiabilities2;

                return JsonConvert.SerializeObject(new
                {
                    Period1 = new { Assets = totalAssets1, Liabilities = totalLiabilities1, Equity = equity1 },
                    Period2 = new { Assets = totalAssets2, Liabilities = totalLiabilities2, Equity = equity2 }
                });
            }
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }
    private async Task HandleSalaryEmployee(string[] parts, NetworkStream stream)
    {
        try
        {
            int employeeId = int.Parse(parts[1]);
            using (var context = new CrmsystemContext())
            {
                var employee = await context.Employees.FirstOrDefaultAsync(e => e.EmployeeId == employeeId);
                if (employee != null)
                {
                    byte[] responseData = Encoding.UTF8.GetBytes("User detected");
                    await stream.WriteAsync(responseData, 0, responseData.Length);
                }
                else
                {
                    byte[] responseData = Encoding.UTF8.GetBytes("User not found");
                    await stream.WriteAsync(responseData, 0, responseData.Length);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при проверке сотрудника: {ex.Message}");
            byte[] responseData = Encoding.UTF8.GetBytes("Error");
            await stream.WriteAsync(responseData, 0, responseData.Length);
        }
    }
    private async Task HandleGetPayrollData(string[] parts, NetworkStream stream)
    {
        try
        {
            int employeeId = int.Parse(parts[1]);
            string period = parts[2];
            string year = parts[3];

            string filePath = Path.Combine("PayrollSlips", "PayrollSlips.json");
            if (!File.Exists(filePath))
            {
                byte[] responseDatab = Encoding.UTF8.GetBytes("NoData");
                await stream.WriteAsync(responseDatab, 0, responseDatab.Length);
                return;
            }

            string existingJson = File.ReadAllText(filePath);
            var payrollList = System.Text.Json.JsonSerializer.Deserialize<List<PayrollData>>(existingJson) ?? new List<PayrollData>();

            var payrollData = payrollList.FirstOrDefault(p =>
                p.EmployeeId == employeeId &&
                p.Period == period &&
                p.Year == year);

            if (payrollData == null)
            {
                byte[] responseDatab = Encoding.UTF8.GetBytes("NoData");
                await stream.WriteAsync(responseDatab, 0, responseDatab.Length);
                return;
            }

            string jsonData = System.Text.Json.JsonSerializer.Serialize(payrollData);
            byte[] responseData = Encoding.UTF8.GetBytes(jsonData);
            await stream.WriteAsync(responseData, 0, responseData.Length);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при получении данных JSON: {ex.Message}");
            byte[] responseData = Encoding.UTF8.GetBytes("Error");
            await stream.WriteAsync(responseData, 0, responseData.Length);
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
    private async Task<string> AddAssetAsync(string jsonData)
    {
        try
        {
            var assetData = JsonConvert.DeserializeObject<dynamic>(jsonData);
            using (var dbContext = new CrmsystemContext())
            {
                // Создаем описание, если есть
                int? descriptionId = null;
                if (!string.IsNullOrEmpty((string)assetData.Description))
                {
                    var description = new Description { Content = assetData.Description };
                    dbContext.Descriptions.Add(description);
                    await dbContext.SaveChangesAsync();
                    descriptionId = description.Id;
                }

                // Создаем актив
                var asset = new Asset
                {
                    Category = assetData.Category,
                    Name = assetData.Name,
                    Amount = (decimal)assetData.Amount,
                    Currency = assetData.Currency ?? "RUB",
                    AcquisitionDate = DateTime.Parse((string)assetData.AcquisitionDate),
                    DepreciationRate = assetData.DepreciationRate != null ? (decimal?)assetData.DepreciationRate : null,
                    DescriptionId = descriptionId
                };

                dbContext.Assets.Add(asset);
                await dbContext.SaveChangesAsync();

                return JsonConvert.SerializeObject(new { Success = true, AssetId = asset.Id });
            }
        }
        catch (Exception ex)
        {
            return JsonConvert.SerializeObject(new { Success = false, Error = ex.Message });
        }
    }

    private async Task<string> UpdateAssetAsync(string jsonData)
    {
        try
        {
            var assetData = JsonConvert.DeserializeObject<dynamic>(jsonData);
            using (var dbContext = new CrmsystemContext())
            {
                var asset = await dbContext.Assets.FindAsync((int)assetData.Id);
                if (asset == null)
                    return JsonConvert.SerializeObject(new { Success = false, Error = "Asset not found" });

                // Обновляем описание, если есть
                if (!string.IsNullOrEmpty((string)assetData.Description))
                {
                    if (asset.DescriptionId != null)
                    {
                        var description = await dbContext.Descriptions.FindAsync(asset.DescriptionId);
                        description.Content = assetData.Description;
                    }
                    else
                    {
                        var description = new Description { Content = assetData.Description };
                        dbContext.Descriptions.Add(description);
                        await dbContext.SaveChangesAsync();
                        asset.DescriptionId = description.Id;
                    }
                }

                // Обновляем актив
                asset.Category = assetData.Category;
                asset.Name = assetData.Name;
                asset.Amount = (decimal)assetData.Amount;
                asset.Currency = assetData.Currency ?? "RUB";
                asset.AcquisitionDate = DateTime.Parse((string)assetData.AcquisitionDate);
                asset.DepreciationRate = assetData.DepreciationRate != null ? (decimal?)assetData.DepreciationRate : null;

                await dbContext.SaveChangesAsync();
                return JsonConvert.SerializeObject(new { Success = true });
            }
        }
        catch (Exception ex)
        {
            return JsonConvert.SerializeObject(new { Success = false, Error = ex.Message });
        }
    }
    private async Task<string> AddLiabilityAsync(string jsonData)
    {
        try
        {
            var liabilityData = JsonConvert.DeserializeObject<dynamic>(jsonData);
            using (var dbContext = new CrmsystemContext())
            {
                int? descriptionId = null;
                if (!string.IsNullOrEmpty((string)liabilityData.Description))
                {
                    var description = new Description { Content = liabilityData.Description };
                    dbContext.Descriptions.Add(description);
                    await dbContext.SaveChangesAsync();
                    descriptionId = description.Id;
                }

                var liability = new Liability
                {
                    Category = liabilityData.Category,
                    Name = liabilityData.Name,
                    Amount = (decimal)liabilityData.Amount,
                    DueDate = DateTime.Parse((string)liabilityData.DueDate),
                    DescriptionId = descriptionId
                };

                dbContext.Liabilities.Add(liability);
                await dbContext.SaveChangesAsync();

                return JsonConvert.SerializeObject(new { Success = true, LiabilityId = liability.Id });
            }
        }
        catch (Exception ex)
        {
            return JsonConvert.SerializeObject(new { Success = false, Error = ex.Message });
        }
    }
    private async Task<string> DeleteAssetAsync(string jsonData)
    {
        try
        {
            var assetData = JsonConvert.DeserializeObject<dynamic>(jsonData);
            using (var dbContext = new CrmsystemContext())
            {
                var asset = await dbContext.Assets.FindAsync((int)assetData.Id);
                if (asset == null)
                    return JsonConvert.SerializeObject(new { Success = false, Error = "Asset not found" });

                dbContext.Assets.Remove(asset);
                await dbContext.SaveChangesAsync();
                return JsonConvert.SerializeObject(new { Success = true });
            }
        }
        catch (Exception ex)
        {
            return JsonConvert.SerializeObject(new { Success = false, Error = ex.Message });
        }
    }
    private async Task<string> GetBalanceSnapshotAsync()
    {
        try
        {
            using (var dbContext = new CrmsystemContext())
            {
                // Получаем активы
                var assets = await dbContext.Assets
                    .GroupBy(a => a.Category)
                    .Select(g => new
                    {
                        Category = g.Key,
                        TotalAmount = g.Sum(a => a.Amount)
                    })
                    .ToListAsync();

                // Получаем пассивы
                var liabilities = await dbContext.Liabilities
                    .GroupBy(l => l.Category)
                    .Select(g => new
                    {
                        Category = g.Key,
                        TotalAmount = g.Sum(l => l.Amount)
                    })
                    .ToListAsync();

                // Получаем собственный капитал
                var equity = await dbContext.Equity
                    .SumAsync(e => e.Amount);

                // Формируем результат
                var balanceSnapshot = new
                {
                    Assets = new
                    {
                        Total = assets.Sum(a => a.TotalAmount),
                        Categories = assets
                    },
                    Liabilities = new
                    {
                        Total = liabilities.Sum(l => l.TotalAmount),
                        Categories = liabilities
                    },
                    Equity = equity,
                    BalanceCheck = assets.Sum(a => a.TotalAmount) - (liabilities.Sum(l => l.TotalAmount) + equity) // Активы = Пассивы + Капитал
                };

                return JsonConvert.SerializeObject(balanceSnapshot);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при получении баланса: {ex.Message}");
            return $"Error: {ex.Message}";
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
            var transactionSummary = await dbContext.ProductTransactions
                .Include(t => t.Product)
                .GroupBy(t => t.Product.ProductName)
                .Select(g => new
                {
                    ProductName = g.Key,
                    TotalAmount = g.Sum(t => Math.Abs(t.Quantity))
                })
                .ToListAsync();

            return transactionSummary.Any()
                ? JsonConvert.SerializeObject(transactionSummary)
                : "NoData";
        }
    }
    private async Task<bool> AddProductAsync(string productName, string unitOfMeasurement, int quantity, decimal unitPrice)
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
                .Include(a => a.Account)
                .Include(a => a.Status)
                .Include(a => a.Product) // Подключаем Product
                .Include(a => a.Description) // Подключаем Description
                .Select(a => new
                {
                    a.Id,
                    Login = a.Account.Login,
                    a.ContactInfo,
                    ProductName = a.Product != null ? a.Product.ProductName : null, // Используем Product.ProductName
                    Description = a.Description != null ? a.Description.Content : null, // Используем Description.Content
                    a.TotalPrice,
                    a.Quantity,
                    a.UnitOfMeasurement,
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
  
    private async Task<string> GetUnprocessedApplications()
    {
        using (var dbContext = new CrmsystemContext())
        {
            // Находим статус "Ожидает" с типом "ApplicationStatus"
            var pendingStatus = await dbContext.Statuses
                .FirstOrDefaultAsync(s => s.StatusName == "Pending" && s.Type == "ApplicationStatus");
            if (pendingStatus == null)
            {
                return "NoData"; // Если статус не найден, возвращаем пустой результат
            }

            var applications = await dbContext.Applications
                .Include(a => a.Account)
                .Include(a => a.Status)
                .Include(a => a.Product)
                .Include(a => a.Description)
                .Where(a => a.StatusId == pendingStatus.Id)
                .Select(a => new
                {
                    a.Id,
                    Login = a.Account.Login,
                    a.ContactInfo,
                    ProductName = a.Product != null ? a.Product.ProductName : null,
                    Description = a.Description != null ? a.Description.Content : null,
                    a.TotalPrice,
                    a.Quantity,
                    a.UnitOfMeasurement,
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
            // Найдем ID статуса "Одобрено" (предположим, что это "Approved" с типом "ApplicationStatus")
            var approvedStatus = await dbContext.Statuses
                .FirstOrDefaultAsync(s => s.StatusName == "Approved" && s.Type == "ApplicationStatus");
            if (approvedStatus == null)
            {
                return "NoData"; // Если статус не найден, возвращаем пустой результат
            }

            var applications = await dbContext.Applications
                .Include(a => a.Account)
                .Include(a => a.Status)
                .Include(a => a.Product) // Подключаем Product
                .Include(a => a.Description) // Подключаем Description
                .Where(a => a.StatusId == approvedStatus.Id) // Используем найденный ID статуса
                .Select(a => new
                {
                    a.Id,
                    Login = a.Account.Login,
                    a.ContactInfo,
                    ProductName = a.Product != null ? a.Product.ProductName : null, // Используем Product.ProductName
                    Description = a.Description != null ? a.Description.Content : null, // Используем Description.Content
                    a.TotalPrice,
                    a.Quantity,
                    a.UnitOfMeasurement,
                    Status = a.Status.StatusName,
                    DateSubmitted = a.DateSubmitted.HasValue
                        ? a.DateSubmitted.Value.ToString("dd.MM.yyyy")
                        : null
                })
                .ToListAsync();

            return applications.Count == 0 ? "NoData" : JsonConvert.SerializeObject(applications);
        }
    }

    private async Task<string> UpdateLiabilityAsync(string jsonData)
    {
        try
        {
            var liabilityData = JsonConvert.DeserializeObject<dynamic>(jsonData);
            using (var dbContext = new CrmsystemContext())
            {
                var liability = await dbContext.Liabilities.FindAsync((int)liabilityData.Id);
                if (liability == null)
                    return JsonConvert.SerializeObject(new { Success = false, Error = "Liability not found" });

                // Обновляем описание, если есть
                if (!string.IsNullOrEmpty((string)liabilityData.Description))
                {
                    if (liability.DescriptionId != null)
                    {
                        var description = await dbContext.Descriptions.FindAsync(liability.DescriptionId);
                        description.Content = liabilityData.Description;
                    }
                    else
                    {
                        var description = new Description { Content = liabilityData.Description };
                        dbContext.Descriptions.Add(description);
                        await dbContext.SaveChangesAsync();
                        liability.DescriptionId = description.Id;
                    }
                }

                // Обновляем обязательство
                liability.Category = liabilityData.Category;
                liability.Name = liabilityData.Name;
                liability.Amount = (decimal)liabilityData.Amount;
                liability.DueDate = DateTime.Parse((string)liabilityData.DueDate);

                await dbContext.SaveChangesAsync();
                return JsonConvert.SerializeObject(new { Success = true });
            }
        }
        catch (Exception ex)
        {
            return JsonConvert.SerializeObject(new { Success = false, Error = ex.Message });
        }
    }

    private async Task<string> DeleteLiabilityAsync(string jsonData)
    {
        try
        {
            var liabilityData = JsonConvert.DeserializeObject<dynamic>(jsonData);
            using (var dbContext = new CrmsystemContext())
            {
                var liability = await dbContext.Liabilities.FindAsync((int)liabilityData.Id);
                if (liability == null)
                    return JsonConvert.SerializeObject(new { Success = false, Error = "Liability not found" });

                dbContext.Liabilities.Remove(liability);
                await dbContext.SaveChangesAsync();
                return JsonConvert.SerializeObject(new { Success = true });
            }
        }
        catch (Exception ex)
        {
            return JsonConvert.SerializeObject(new { Success = false, Error = ex.Message });
        }
    }

    private async Task<string> GetAllApplications()
    {
        using (var dbContext = new CrmsystemContext())
        {
            var applications = await dbContext.Applications
                .Include(a => a.Account)
                .Include(a => a.Status) // Подключаем Status
                .Include(a => a.Product) // Подключаем Product
                .Include(a => a.Description) // Подключаем Description
                .Where(a => a.DateSubmitted >= DateTime.Now.AddDays(-3))
                .Select(a => new
                {
                    a.Id,
                    Login = a.Account.Login,
                    a.ContactInfo,
                    ProductName = a.Product != null ? a.Product.ProductName : null,
                    Description = a.Description != null ? a.Description.Content : null,
                    a.TotalPrice,
                    Status = a.Status != null ? a.Status.StatusName : null, // Используем Status.StatusName
                    a.Quantity,
                    a.UnitOfMeasurement,
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
        using (var dbContext = new CrmsystemContext())
        {
            var supports = await dbContext.SupportTickets
                .Include(a => a.User)
                .Include(a => a.Status)
                .Include(a => a.Description) // Подключаем Description
                .Where(a => a.User.Id == userId)
                .Select(a => new
                {
                    a.TicketId,
                    a.SubmissionDate,
                    Description = a.Description != null ? a.Description.Content : null, // Используем Description.Content
                    StatusName = a.Status.StatusName,
                })
                .ToListAsync();

            return supports.Count == 0 ? "NoData" : JsonConvert.SerializeObject(supports);
        }
    }

    private async Task<string> GetApplicationsByUserId(int userId)
    {
        using (var dbContext = new CrmsystemContext())
        {
            var threeDaysAgo = DateTime.Now.AddDays(-3);

            var applications = await dbContext.Applications
                .Include(a => a.Account)
                .Include(a => a.Status)
                .Include(a => a.Product)
                .Include(a => a.Description)
                .Where(a => a.AccountId == userId &&
                            a.DateSubmitted.HasValue &&
                            a.DateSubmitted.Value >= threeDaysAgo)
                .Select(a => new
                {
                    a.Id,
                    Login = a.Account.Login,
                    a.ContactInfo,
                    ProductName = a.Product != null ? a.Product.ProductName : null,
                    Description = a.Description != null ? a.Description.Content : null,
                    a.TotalPrice,
                    a.Quantity,
                    a.UnitOfMeasurement,
                    Status = a.Status.StatusName,
                    DateSubmitted = a.DateSubmitted.HasValue
                        ? a.DateSubmitted.Value.ToString("dd.MM.yyyy")
                        : null
                })
                .ToListAsync();

            return applications.Count == 0 ? "NoData" : JsonConvert.SerializeObject(applications);
        }
    }


    private async Task<string> GetApplicationHistoryByUserId(int userId)
    {
        using (var dbContext = new CrmsystemContext())
        {
            var applications = await dbContext.Applications
                .Include(a => a.Account)
                .Include(a => a.Status)
                .Include(a => a.Product)
                .Include(a => a.Description)
                .Where(a => a.AccountId == userId)
                .Select(a => new
                {
                    a.Id,
                    Login = a.Account.Login,
                    a.ContactInfo,
                    ProductName = a.Product != null ? a.Product.ProductName : null,
                    Description = a.Description != null ? a.Description.Content : null,
                    a.TotalPrice,
                    a.Quantity,
                    a.UnitOfMeasurement,
                    Status = a.Status.StatusName,
                    DateSubmitted = a.DateSubmitted.HasValue
                        ? a.DateSubmitted.Value.ToString("dd.MM.yyyy")
                        : null
                })
                .ToListAsync();

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
    private async Task<bool> SalaryEmployeeByIdAsync(int employeeId)
    {
        try
        {
            using (var dbContext = new CrmsystemContext())
            {
                var user = await dbContext.Employees.FindAsync(employeeId);
                if (user != null)
                {

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
    private async Task<bool> AddSupportAsync(string email, string description, int userID)
    {
        try
        {
            using (var context = new CrmsystemContext())
            {
                var account = await context.Accounts.FirstOrDefaultAsync(a => a.Id == userID);
                if (account == null)
                    throw new Exception("Пользователь не найден.");

                // Находим статус "Открыт" (или другой статус по умолчанию) для SupportStatus
                var defaultStatus = await context.Statuses
                    .FirstOrDefaultAsync(s => s.StatusName == "Open" && s.Type == "SupportStatus");
                if (defaultStatus == null)
                    throw new Exception("Статус по умолчанию для тикетов техподдержки не найден.");

                // Создаем запись в Descriptions
                var descriptionEntity = new Description
                {
                    Content = description
                };
                context.Descriptions.Add(descriptionEntity);
                await context.SaveChangesAsync();

                var support = new SupportTicket
                {
                    User = account,
                    UserId = userID,
                    UserEmail = email,
                    DescriptionId = descriptionEntity.Id,
                    StatusId = defaultStatus.Id, // Используем найденный статус
                    SubmissionDate = DateTime.Now
                };

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
                var account = await context.Accounts.FirstOrDefaultAsync(a => a.Login == login);
                if (account == null)
                    throw new Exception("Пользователь не найден.");

                if (quantity <= 0)
                    throw new Exception("Количество должно быть больше нуля.");
                if (string.IsNullOrWhiteSpace(unitOfMeasurement))
                    throw new Exception("Единица измерения не может быть пустой.");

                // Находим продукт по имени
                var product = await context.Products
                    .FirstOrDefaultAsync(p => p.ProductName == productName);
                if (product == null)
                    throw new Exception($"Продукт с названием {productName} не найден.");

                // Создаем запись в Descriptions
                var descriptionEntity = new Description
                {
                    Content = description
                };
                context.Descriptions.Add(descriptionEntity);
                await context.SaveChangesAsync();

                // Находим статус "Ожидает" (или другой по умолчанию) с типом "ApplicationStatus"
                var defaultStatus = await context.Statuses
                    .FirstOrDefaultAsync(s => s.StatusName == "Pending" && s.Type == "ApplicationStatus");
                if (defaultStatus == null)
                    throw new Exception("Статус по умолчанию для заявок не найден.");

                var application = new Application
                {
                    AccountId = account.Id,
                    ContactInfo = contactInfo,
                    ProductId = product.ProductId, // Используем ProductId
                    DescriptionId = descriptionEntity.Id, // Используем DescriptionId
                    TotalPrice = totalPrice,
                    Quantity = quantity,
                    UnitOfMeasurement = unitOfMeasurement,
                    StatusId = defaultStatus.Id, // Используем найденный статус
                    DateSubmitted = DateTime.Now
                };

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
    private async Task<string> GetBalanceDataAsync()
    {
        try
        {
            using (var dbContext = new CrmsystemContext())
            {
                // Получаем транзакции с категориями
                var transactions = await dbContext.Transactions
                    .Include(t => t.Category)
                    .ToListAsync();

                // Рассчитываем доходы и расходы на основе категорий
                var summary = new
                {
                    TotalIncome = transactions
                        .Where(t => t.Category.Name == "Продажи" && t.Amount > 0)
                        .Sum(t => t.Amount),
                    TotalExpenses = transactions
                        .Where(t => t.Category.Name == "Зарплата" || t.Category.Name == "Закупки")
                        .Sum(t => Math.Abs(t.Amount)),
                    CurrentBalance = transactions.Sum(t => t.Amount)
                };

                // Сериализуем результат в JSON
                return JsonConvert.SerializeObject(summary);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при получении данных баланса: {ex.Message}");
            return "Error: " + ex.Message;
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
          

                var user = await dbContext.Accounts
                                           .SingleOrDefaultAsync(u => u.Login == username && u.Password == password && (u.RoleId == 2 || u.RoleId == 1));


                return user != null;
           
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
           

                var user = await dbContext.Accounts
                                           .SingleOrDefaultAsync(u => u.Login == username && u.Password == password && u.RoleId == 1);


                return user != null;
         

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
            var transactions = await dbContext.Transactions
                .Include(t => t.Description)
                .Include(t => t.Category)
                .Select(t => new
                {
                    t.Id,
                    t.TransactionDate,
                    Description = t.Description != null ? t.Description.Content : null,
                    t.Amount,
                    TransactionType = t.Category != null ? t.Category.Name : null
                })
                .ToListAsync();

            return transactions.Count == 0 ? "NoData" : JsonConvert.SerializeObject(transactions);
        }
    }

    private async Task HandleSavePayrollJson(string[] parts, NetworkStream stream)
    {
        try
        {
            string jsonData = parts[1];
            var newPayrollData = System.Text.Json.JsonSerializer.Deserialize<PayrollData>(jsonData);

            string filePath = Path.Combine("PayrollSlips", "PayrollSlips.json");
            Directory.CreateDirectory("PayrollSlips");

            lock (_lock) // Блокируем доступ к файлу для других потоков
            {
                List<PayrollData> payrollList;
                if (File.Exists(filePath))
                {
                    string existingJson = File.ReadAllText(filePath);
                    payrollList = System.Text.Json.JsonSerializer.Deserialize<List<PayrollData>>(existingJson) ?? new List<PayrollData>();
                }
                else
                {
                    payrollList = new List<PayrollData>();
                }

                var existingPayroll = payrollList.FirstOrDefault(p =>
                    p.EmployeeId == newPayrollData.EmployeeId &&
                    p.Period == newPayrollData.Period &&
                    p.Year == newPayrollData.Year);

                if (existingPayroll != null)
                {
                    int index = payrollList.IndexOf(existingPayroll);
                    payrollList[index] = newPayrollData;
                }
                else
                {
                    payrollList.Add(newPayrollData);
                }

                string updatedJson = System.Text.Json.JsonSerializer.Serialize(payrollList, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(filePath, updatedJson);
            }

            byte[] responseData = Encoding.UTF8.GetBytes("Success");
            await stream.WriteAsync(responseData, 0, responseData.Length);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при сохранении JSON: {ex.Message}");
            byte[] responseData = Encoding.UTF8.GetBytes("Error");
            await stream.WriteAsync(responseData, 0, responseData.Length);
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
public class PayrollData
{
    public int EmployeeId { get; set; }
    public string FullName { get; set; }
    public string Position { get; set; }
    public decimal BaseSalary { get; set; }
    public string Period { get; set; }
    public string Year { get; set; }
    public List<PayrollItem> Accruals { get; set; }
    public List<PayrollItem> Deductions { get; set; }
    public decimal TotalAccrued { get; set; }
    public decimal TotalDeducted { get; set; }
    public decimal ToBePaid { get; set; }
}

public class PayrollItem
{
    public string Type { get; set; }
    public decimal Amount { get; set; }
    public string DaysOrHours { get; set; }
}