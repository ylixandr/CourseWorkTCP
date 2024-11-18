using Azure.Core;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TCPServerLab2;

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
                    Console.WriteLine("loged user here it is");
                    string username = credentials[1];
                    string password = credentials[2];


                    Console.WriteLine($"Получены данные от клиента {clientIp} - Username: {username}, Password: {password}, ");

                    string response = await ValidateUserAsync(username, password) ? "Success" : "Invalid credentials";
                    byte[] responseData = Encoding.UTF8.GetBytes(response);
                    await stream.WriteAsync(responseData, 0, responseData.Length);

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
                        using (var context = new TestjsonContext())
                        {
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
                        using (var context = new TestjsonContext())
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
                    using (var db = new TestjsonContext())
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


            }
        }
        finally
        {
            // Код выполняется только при закрытии соединения
            Console.WriteLine($"Клиент {clientIp} отключен");
            client.Close();
        }
    }

    private async Task<bool> ValidateUserAsync(string username, string password)
    {
        using (var dbContext = new TestjsonContext())
        {

            var user = await dbContext.Accounts
                                       .SingleOrDefaultAsync(u => u.Login == username && u.Password == password && u.RoleId == 3);
            return user != null;

        }
    }
    private async Task<bool> ValidateManagerUserAsync(string username, string password, string code)
    {
        using (var dbContext = new TestjsonContext())
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


    private async Task<bool> ValidateAdminUserAsync(string username, string password, string code)
    {
        using (var dbContext = new TestjsonContext())
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
    private async Task<bool> DeleteEmployeeByIdAsync(int employeeId)
    {
        try
        {
            using (var dbContext = new TestjsonContext())
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
    private async Task<bool> DeleteUserByIdAsync(int userId)
    {
        try
        {
            using (var dbContext = new TestjsonContext())
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
        using (var dbContext = new TestjsonContext())
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
        using (var dbContext = new TestjsonContext())
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
        using (var dbContext = new TestjsonContext())
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
        using (var dbContext = new TestjsonContext())
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
        using (var dbContext = new TestjsonContext())
        {
            var employees = await dbContext.Employees.ToListAsync();
            return JsonConvert.SerializeObject(employees);
        }
    }
    private async Task<string> GetAllEmployees()
    {
        using (var dbContext = new TestjsonContext())
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
