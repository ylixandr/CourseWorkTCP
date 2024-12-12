using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Azure;
using Newtonsoft.Json;
using TCPServer;


namespace TCPServer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Server server = new Server();
            await server.StartAsync();
            #region Оптимальный вариант
            //// Определяем порт для прослушивания
            //int port = 5001;

            //// Создаем TCP-листенер
            //TcpListener server = new TcpListener(IPAddress.Any, port);
            //server.Start();
            //Console.WriteLine("Сервер запущен на порту " + port);

            //while (true)
            //{
            //    // Ожидаем подключения клиента
            //    using TcpClient client = server.AcceptTcpClient();
            //    Console.WriteLine("Подключен клиент");

            //    // Получаем сетевой поток
            //    using NetworkStream stream = client.GetStream();

            //    // Буфер для чтения данных
            //    byte[] buffer = new byte[1024];
            //    int bytesRead = stream.Read(buffer, 0, buffer.Length);
            //    string data = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            //    #region Десериализация JSON и добавление в БД
            //    //// Десериализуем JSON
            //    dynamic message = JsonConvert.DeserializeObject(data);



            //    // Обработка сообщения (привести пример)
            //    Console.WriteLine("Получено сообщение:");

            //    using (TestjsonContext db = new TestjsonContext())
            //    {
            //        db.Accounts.Add(new Account { Login = message.login, Password = "5540040404040", RoleId = 1 });
            //        db.SaveChanges();
            //        Console.WriteLine("Успешно добавлено в БД");




            //    }
            //    // Отправка ответа клиенту
            //    byte[] answer = Encoding.UTF8.GetBytes("Ответ из бд");
            //    stream.Write(answer, 0, answer.Length);

            //    #endregion
            //    // Формирование ответа (привести пример)


            //    //if (data == "Get_Users")
            //    //{
            //    //    byte[] responseBytes = Encoding.UTF8.GetBytes(Get_users());
            //    //    stream.Write(responseBytes, 0, responseBytes.Length);
            //    //}






            //}

            ////static string Get_users()
            ////{
            ////    using (TestjsonContext db = new TestjsonContext())
            ////    {
            ////        // Предположим, что у вас есть свойство Role в модели User
            ////        var user = db.Accounts.SingleOrDefault(a => a.Login == "ylixandr" && a.Password == "554008");

            ////        if (user != null)
            ////        {
            ////            return $"Юзер получен: {user.Login}, Роль: {user.Role}";
            ////        }
            ////        else
            ////        {
            ////            return "Нет доступа";
            ////        }
            ////    }
            ////}
            #endregion
        }
    }
}