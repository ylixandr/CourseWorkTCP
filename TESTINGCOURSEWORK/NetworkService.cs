using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

public class NetworkService
{
    private readonly TcpClient _client;
    private readonly NetworkStream _stream;
    private static readonly Lazy<NetworkService> _instance = new Lazy<NetworkService>(() => new NetworkService());

    public static NetworkService Instance => _instance.Value;

    private NetworkService()
    {
        _client = new TcpClient("127.0.0.1", 12345); // Подключение к серверу
        _stream = _client.GetStream();
    }
    public async Task<string> SendMessageAsync(string message, string jsonData)
    {
        // Формируем полное сообщение, например, комбинируя команду и данные
        string fullMessage = $"{message}:{jsonData}";

        byte[] data = Encoding.UTF8.GetBytes(fullMessage);
        await _stream.WriteAsync(data, 0, data.Length);

        byte[] buffer = new byte[4096];
        int bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length);

        return Encoding.UTF8.GetString(buffer, 0, bytesRead);
    }

    public async Task<string> SendMessageAsync(string message)
    {
        byte[] data = Encoding.UTF8.GetBytes(message);
        await _stream.WriteAsync(data, 0, data.Length);

        byte[] buffer = new byte[1024];
        int bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length);

        return Encoding.UTF8.GetString(buffer, 0, bytesRead);
    }

    public async Task SendCloseMessageAndCloseConnection()
    {
        // Отправка сообщения "close" перед завершением соединения
        string closeMessage = "close";
        byte[] data = Encoding.UTF8.GetBytes(closeMessage);
        await _stream.WriteAsync(data, 0, data.Length);

        // Закрытие потоков
        _stream.Close();
        _client.Close();
    }
    //public async Task<string> ReceiveAllUsersAsync()
    //{
    //    StringBuilder dataBuilder = new StringBuilder();
    //    byte[] buffer = new byte[256];

    //    while (true)
    //    {
    //        int bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length);
    //        string chunk = Encoding.UTF8.GetString(buffer, 0, bytesRead);

    //        if (chunk.Contains("<END>"))
    //        {
    //            // Удаляем маркер конца данных и завершаем прием
    //            dataBuilder.Append(chunk.Replace("<END>", ""));
    //            break;
    //        }
    //        else
    //        {
    //            dataBuilder.Append(chunk);
    //        }
    //    }

    //    return dataBuilder.ToString();
    //}


}
