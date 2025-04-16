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
using TCPServer.ProductionModule;
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

             
              
                else if (receivedData == "getEmployeeSalaries")
                {
                    Console.WriteLine("Запрос на получение данных о зарплатах сотрудников");
                    string jsonData = await GetEmployeeSalaryDataAsync();
                    byte[] responseData = Encoding.UTF8.GetBytes(jsonData);
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
                else if (receivedData.StartsWith("getAllAssets")) // Новая команда
                {
                    Console.WriteLine("Получение всех активов.");
                    string response = await GetAllAssetsAsync();
                    byte[] responseData = Encoding.UTF8.GetBytes(response);
                    await stream.WriteAsync(responseData, 0, responseData.Length);
                }
                else if (receivedData.StartsWith("getAllLiabilities")) // Новая команда
                {
                    Console.WriteLine("Получение всех обязательств.");
                    string response = await GetAllLiabilitiesAsync();
                    byte[] responseData = Encoding.UTF8.GetBytes(response);
                    await stream.WriteAsync(responseData, 0, responseData.Length);
                }
                // Внутри HandleClientAsync, после существующих else if
                else if (receivedData.StartsWith("getProductSnapshot"))
                {
                    Console.WriteLine("Получение сводки по продукции.");
                    string response = await ProductService.GetProductSnapshotAsync();
                    byte[] responseData = Encoding.UTF8.GetBytes(response);
                    await stream.WriteAsync(responseData, 0, responseData.Length);
                }
                else if (receivedData.StartsWith("getAllProducts"))
                {
                    Console.WriteLine("Получение всех продуктов.");
                    string response = await ProductService.GetAllProductsAsync();
                    byte[] responseData = Encoding.UTF8.GetBytes(response);
                    await stream.WriteAsync(responseData, 0, responseData.Length);
                }
                else if (receivedData.StartsWith("addProduct"))
                {
                    Console.WriteLine("Добавление продукта.");
                    string jsonData = receivedData.Substring("addProduct".Length);
                    string response = await ProductService.AddProductAsync(jsonData);
                    byte[] responseData = Encoding.UTF8.GetBytes(response);
                    await stream.WriteAsync(responseData, 0, responseData.Length);
                }
                else if (receivedData.StartsWith("updateProduct"))
                {
                    Console.WriteLine("Обновление продукта.");
                    string jsonData = receivedData.Substring("updateProduct".Length);
                    string response = await ProductService.UpdateProductAsync(jsonData);
                    byte[] responseData = Encoding.UTF8.GetBytes(response);
                    await stream.WriteAsync(responseData, 0, responseData.Length);
                }
                else if (receivedData.StartsWith("deleteProduct"))
                {
                    Console.WriteLine("Удаление продукта.");
                    string jsonData = receivedData.Substring("deleteProduct".Length);
                    string response = await ProductService.DeleteProductAsync(jsonData);
                    byte[] responseData = Encoding.UTF8.GetBytes(response);
                    await stream.WriteAsync(responseData, 0, responseData.Length);
                }
                else if (receivedData.StartsWith("getAllWarehouses"))
                {
                    Console.WriteLine("Получение всех складов.");
                    string response = await ProductService.GetAllWarehousesAsync();
                    byte[] responseData = Encoding.UTF8.GetBytes(response);
                    await stream.WriteAsync(responseData, 0, responseData.Length);
                }
                else if (receivedData.StartsWith("addWarehouse"))
                {
                    Console.WriteLine("Добавление склада.");
                    string jsonData = receivedData.Substring("addWarehouse".Length);
                    string response = await ProductService.AddWarehouseAsync(jsonData);
                    byte[] responseData = Encoding.UTF8.GetBytes(response);
                    await stream.WriteAsync(responseData, 0, responseData.Length);
                }
                else if (receivedData.StartsWith("updateWarehouse"))
                {
                    Console.WriteLine("Обновление склада.");
                    string jsonData = receivedData.Substring("updateWarehouse".Length);
                    string response = await ProductService.UpdateWarehouseAsync(jsonData);
                    byte[] responseData = Encoding.UTF8.GetBytes(response);
                    await stream.WriteAsync(responseData, 0, responseData.Length);
                }
                else if (receivedData.StartsWith("deleteWarehouse"))
                {
                    Console.WriteLine("Удаление склада.");
                    string jsonData = receivedData.Substring("deleteWarehouse".Length);
                    string response = await ProductService.DeleteWarehouseAsync(jsonData);
                    byte[] responseData = Encoding.UTF8.GetBytes(response);
                    await stream.WriteAsync(responseData, 0, responseData.Length);
                }
                else if (receivedData.StartsWith("getInventory"))
                {
                    Console.WriteLine("Получение инвентаря.");
                    string response = await ProductService.GetInventoryAsync();
                    byte[] responseData = Encoding.UTF8.GetBytes(response);
                    await stream.WriteAsync(responseData, 0, responseData.Length);
                }
                else if (receivedData.StartsWith("addInventoryTransaction"))
                {
                    Console.WriteLine("Выполнение инвентарной транзакции.");
                    string jsonData = receivedData.Substring("addInventoryTransaction".Length);
                    string response = await ProductService.AddInventoryTransactionAsync(jsonData);
                    byte[] responseData = Encoding.UTF8.GetBytes(response);
                    await stream.WriteAsync(responseData, 0, responseData.Length);
                }
                else if (receivedData.StartsWith("compareProductSnapshots"))
                {
                    Console.WriteLine("Сравнение снимков продукции.");
                    string jsonData = receivedData.Substring("compareProductSnapshots".Length);
                    string response = await ProductService.CompareProductSnapshotsAsync(jsonData);
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


    private async Task<string> CompareBalanceSnapshotsAsync(string jsonData)
    {
        try
        {
            var periods = JsonConvert.DeserializeObject<dynamic>(jsonData);
            if (periods == null)
            {
                return "Error: Неверный формат данных периодов";
            }

            DateTime period1Start = DateTime.Parse((string)periods.Period1Start);
            DateTime period1End = DateTime.Parse((string)periods.Period1End);
            DateTime period2Start = DateTime.Parse((string)periods.Period2Start);
            DateTime period2End = DateTime.Parse((string)periods.Period2End);

            using (var dbContext = new CrmsystemContext())
            {
                // Получаем снимки баланса для первого периода
                var snapshots1 = await dbContext.BalanceSnapshots
                    .Where(s => s.Timestamp >= period1Start && s.Timestamp <= period1End)
                    .ToListAsync();

                // Получаем снимки баланса для второго периода
                var snapshots2 = await dbContext.BalanceSnapshots
                    .Where(s => s.Timestamp >= period2Start && s.Timestamp <= period2End)
                    .ToListAsync();

                // Вычисляем средние значения для каждого периода
                var period1Summary = new
                {
                    Assets = snapshots1.Any() ? snapshots1.Average(s => s.TotalAssets) : 0,
                    Liabilities = snapshots1.Any() ? snapshots1.Average(s => s.TotalLiabilities) : 0,
                    Equity = snapshots1.Any() ? snapshots1.Average(s => s.Equity) : 0
                };

                var period2Summary = new
                {
                    Assets = snapshots2.Any() ? snapshots2.Average(s => s.TotalAssets) : 0,
                    Liabilities = snapshots2.Any() ? snapshots2.Average(s => s.TotalLiabilities) : 0,
                    Equity = snapshots2.Any() ? snapshots2.Average(s => s.Equity) : 0
                };

                var comparisonResult = new
                {
                    Period1 = period1Summary,
                    Period2 = period2Summary
                };

                return JsonConvert.SerializeObject(comparisonResult);
            }
        }
        catch (Exception ex)
        {
            return $"Error: Не удалось сравнить снимки баланса: {ex.Message}";
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
    private async Task<string> GetAllAssetsAsync()
    {
        try
        {
            using (var dbContext = new CrmsystemContext())
            {
                var assets = await dbContext.Assets
                    .Include(a => a.Description) // Подключаем связанную таблицу Descriptions
                    .Select(a => new AssetViewModel
                    {
                        Id = a.Id,
                        Category = a.Category,
                        Name = a.Name,
                        Amount = a.Amount,
                        Currency = a.Currency,
                        AcquisitionDate = a.AcquisitionDate,
                        DepreciationRate = a.DepreciationRate,
                        Description = a.Description != null ? a.Description.Content : null
                    })
                    .ToListAsync();

                return JsonConvert.SerializeObject(assets);
            }
        }
        catch (Exception ex)
        {
            return $"Error: Не удалось загрузить активы: {ex.Message}";
        }
    }
    private async Task<string> GetAllLiabilitiesAsync()
    {
        try
        {
            using (var dbContext = new CrmsystemContext())
            {
                var liabilities = await dbContext.Liabilities
                    .Include(l => l.Description) // Подключаем связанную таблицу Descriptions
                    .Select(l => new LiabilityViewModel
                    {
                        Id = l.Id,
                        Category = l.Category,
                        Name = l.Name,
                        Amount = l.Amount,
                        DueDate = l.DueDate,
                        Description = l.Description != null ? l.Description.Content : null
                    })
                    .ToListAsync();

                return JsonConvert.SerializeObject(liabilities);
            }
        }
        catch (Exception ex)
        {
            return $"Error: Не удалось загрузить обязательства: {ex.Message}";
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
            var assetData = JsonConvert.DeserializeObject<AssetViewModel>(jsonData);
            if (assetData == null)
            {
                return "Error: Неверный формат данных актива";
            }

            using (var dbContext = new CrmsystemContext())
            {
                var asset = await dbContext.Assets
                    .Include(a => a.Description)
                    .FirstOrDefaultAsync(a => a.Id == assetData.Id);

                if (asset == null)
                {
                    return "Error: Актив не найден";
                }

                // Обновляем описание
                if (asset.Description == null)
                {
                    asset.Description = new Description { Content = assetData.Description };
                    dbContext.Descriptions.Add(asset.Description);
                }
                else
                {
                    asset.Description.Content = assetData.Description;
                }

                // Обновляем поля актива
                asset.Category = assetData.Category;
                asset.Name = assetData.Name;
                asset.Amount = assetData.Amount;
                asset.Currency = assetData.Currency;
                asset.AcquisitionDate = assetData.AcquisitionDate;
                asset.DepreciationRate = assetData.DepreciationRate;

                await dbContext.SaveChangesAsync();

                return JsonConvert.SerializeObject(new { Success = true });
            }
        }
        catch (Exception ex)
        {
            return $"Error: Не удалось обновить актив: {ex.Message}";
        }
    }
    private async Task<string> AddLiabilityAsync(string jsonData)
    {
        try
        {
            var liabilityData = JsonConvert.DeserializeObject<LiabilityViewModel>(jsonData);
            if (liabilityData == null)
            {
                return "Error: Неверный формат данных обязательства";
            }

            using (var dbContext = new CrmsystemContext())
            {
                // Создаём запись в таблице Descriptions
                var description = new Description
                {
                    Content = liabilityData.Description
                };
                dbContext.Descriptions.Add(description);
                await dbContext.SaveChangesAsync();

                // Создаём новое обязательство
                var liability = new Liability
                {
                    Category = liabilityData.Category,
                    Name = liabilityData.Name,
                    Amount = liabilityData.Amount,
                    DueDate = liabilityData.DueDate,
                    DescriptionId = description.Id
                };
                dbContext.Liabilities.Add(liability);
                await dbContext.SaveChangesAsync();

                return JsonConvert.SerializeObject(new { Success = true, LiabilityId = liability.Id });
            }
        }
        catch (Exception ex)
        {
            return $"Error: Не удалось добавить обязательство: {ex.Message}";
        }
    }
    private async Task<string> DeleteAssetAsync(string jsonData)
    {
        try
        {
            var data = JsonConvert.DeserializeObject<dynamic>(jsonData);
            int assetId = data.Id;

            using (var dbContext = new CrmsystemContext())
            {
                var asset = await dbContext.Assets
                    .Include(a => a.Description)
                    .FirstOrDefaultAsync(a => a.Id == assetId);

                if (asset == null)
                {
                    return "Error: Актив не найден";
                }

                // Удаляем описание, если оно есть
                if (asset.Description != null)
                {
                    dbContext.Descriptions.Remove(asset.Description);
                }

                // Удаляем актив
                dbContext.Assets.Remove(asset);
                await dbContext.SaveChangesAsync();

                return JsonConvert.SerializeObject(new { Success = true });
            }
        }
        catch (Exception ex)
        {
            return $"Error: Не удалось удалить актив: {ex.Message}";
        }
    }
    private async Task<string> GetBalanceSnapshotAsync(string jsonData = null) // jsonData не используется, но оставим для совместимости
    {
        try
        {
            using (var dbContext = new CrmsystemContext())
            {
                // Подсчитываем активы
                var assetCategories = await dbContext.Assets
                    .GroupBy(a => a.Category)
                    .Select(g => new
                    {
                        Category = g.Key,
                        Total = g.Sum(a => a.Amount)
                    })
                    .ToListAsync();

                decimal totalAssets = assetCategories.Sum(c => c.Total);

                // Подсчитываем обязательства
                var liabilityCategories = await dbContext.Liabilities
                    .GroupBy(l => l.Category)
                    .Select(g => new
                    {
                        Category = g.Key,
                        Total = g.Sum(l => l.Amount)
                    })
                    .ToListAsync();

                decimal totalLiabilities = liabilityCategories.Sum(c => c.Total);

                // Вычисляем капитал (Equity = Assets - Liabilities)
                decimal equity = totalAssets - totalLiabilities;

                // Проверка баланса (Assets = Liabilities + Equity)
                decimal balanceCheck = totalAssets - (totalLiabilities + equity); // Должно быть равно 0

                var balanceData = new
                {
                    Assets = new
                    {
                        Total = totalAssets,
                        Categories = assetCategories
                    },
                    Liabilities = new
                    {
                        Total = totalLiabilities,
                        Categories = liabilityCategories
                    },
                    Equity = equity,
                    BalanceCheck = balanceCheck
                };

                return JsonConvert.SerializeObject(balanceData);
            }
        }
        catch (Exception ex)
        {
            return $"Error: Не удалось получить снимок баланса: {ex.Message}";
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

  
    private async Task<string> UpdateLiabilityAsync(string jsonData)
    {
        try
        {
            var liabilityData = JsonConvert.DeserializeObject<LiabilityViewModel>(jsonData);
            if (liabilityData == null)
            {
                return "Error: Неверный формат данных обязательства";
            }

            using (var dbContext = new CrmsystemContext())
            {
                var liability = await dbContext.Liabilities
                    .Include(l => l.Description)
                    .FirstOrDefaultAsync(l => l.Id == liabilityData.Id);

                if (liability == null)
                {
                    return "Error: Обязательство не найдено";
                }

                // Обновляем описание
                if (liability.Description == null)
                {
                    liability.Description = new Description { Content = liabilityData.Description };
                    dbContext.Descriptions.Add(liability.Description);
                }
                else
                {
                    liability.Description.Content = liabilityData.Description;
                }

                // Обновляем поля обязательства
                liability.Category = liabilityData.Category;
                liability.Name = liabilityData.Name;
                liability.Amount = liabilityData.Amount;
                liability.DueDate = liabilityData.DueDate;

                await dbContext.SaveChangesAsync();

                return JsonConvert.SerializeObject(new { Success = true });
            }
        }
        catch (Exception ex)
        {
            return $"Error: Не удалось обновить обязательство: {ex.Message}";
        }
    }

    private async Task<string> DeleteLiabilityAsync(string jsonData)
    {
        try
        {
            var data = JsonConvert.DeserializeObject<dynamic>(jsonData);
            int liabilityId = data.Id;

            using (var dbContext = new CrmsystemContext())
            {
                var liability = await dbContext.Liabilities
                    .Include(l => l.Description)
                    .FirstOrDefaultAsync(l => l.Id == liabilityId);

                if (liability == null)
                {
                    return "Error: Обязательство не найдено";
                }

                // Удаляем описание, если оно есть
                if (liability.Description != null)
                {
                    dbContext.Descriptions.Remove(liability.Description);
                }

                // Удаляем обязательство
                dbContext.Liabilities.Remove(liability);
                await dbContext.SaveChangesAsync();

                return JsonConvert.SerializeObject(new { Success = true });
            }
        }
        catch (Exception ex)
        {
            return $"Error: Не удалось удалить обязательство: {ex.Message}";
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
public class AssetViewModel
{
    public int Id { get; set; }
    public string Category { get; set; }
    public string Name { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; }
    public DateTime AcquisitionDate { get; set; }
    public decimal? DepreciationRate { get; set; }
    public string Description { get; set; } // Текст описания из таблицы Descriptions
}

public class LiabilityViewModel
{
    public int Id { get; set; }
    public string Category { get; set; }
    public string Name { get; set; }
    public decimal Amount { get; set; }
    public DateTime DueDate { get; set; }
    public string Description { get; set; } // Текст описания из таблицы Descriptions
}