using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

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
        }
    }

    private async Task HandleClientAsync(TcpClient client)
    {
        Console.WriteLine("Клиент подключен");

        using NetworkStream stream = client.GetStream();
        byte[] buffer = new byte[256];

        while (client.Connected)
        {
            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            if (bytesRead == 0) break; // Клиент закрыл соединение

            string loginData = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            string[] credentials = loginData.Split(':');
            string username = credentials[0];
            string password = credentials[1];

            // Здесь идет проверка данных в базе данных
            string response = await ValidateUserAsync(username, password) ? "Success" : "Invalid credentials";
            byte[] responseData = Encoding.UTF8.GetBytes(response);

            await stream.WriteAsync(responseData, 0, responseData.Length);
        }

        Console.WriteLine("Клиент отключен");
        client.Close();
    }

    private async Task<bool> ValidateUserAsync(string username, string password)
    {
        using (var dbContext = new YourDbContext())
        {
            var user = await dbContext.Users
                                      .SingleOrDefaultAsync(u => u.Username == username && u.Password == password);
            return user != null;
        }
    }
}
