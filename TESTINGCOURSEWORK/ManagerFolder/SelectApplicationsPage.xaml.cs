using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using TCPServerLab2;
using TESTINGCOURSEWORK.Models;

namespace TESTINGCOURSEWORK.ManagerFolder
{
    public partial class SelectApplicationsPage : Window
    {
        public List<TCPServerLab2.Application> SelectedApplications { get; private set; }

        public SelectApplicationsPage()
        {
            InitializeComponent();
            this.DataContext = this;
            LoadApplications();




        }
        private async void LoadApplications()
        {

            try
            {
                // Запрос заявок по текущему AccountId
                string request = $"getApplicationsApproved";
                string response = await NetworkService.Instance.SendMessageAsync(request);

                if (response == "NoData")
                {
                    MessageBox.Show("Нет данных для отображения.");
                    return;
                }

                ApplicationsDataGrid.Visibility = Visibility.Visible;

                // Обновление структуры заявок с учетом новых полей
                var applications = JsonConvert.DeserializeObject<List<ApplicationViewModel>>(response);

                // Назначение данных DataGrid
                ApplicationsDataGrid.ItemsSource = applications;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }
        private async void RequestApplicationsData()
        {
            // Отправляем запрос на сервер для получения заявок с статусом "Ожидает"
            string command = "getUnprocessedApplications"; // Команда для получения заявок
            string response = await NetworkService.Instance.SendMessageAsync(command);

            // Обрабатываем ответ от сервера
            if (response != "NoData")
            {
                var applications = JsonConvert.DeserializeObject<List<ApplicationViewModel>>(response);
                ApplicationsDataGrid.ItemsSource = applications;
            }
            else
            {
                MessageBox.Show("Нет данных", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (ApplicationsDataGrid.SelectedItem is ApplicationViewModel application)
            {
                try
                {
                    // Проверка заполнения полей
                    string productName = application.ProductName ;
                    string unitOfMeasurement = application.UnitOfMeasurement;
                    int? quantity = 0;
                    decimal? unitPrice = 0;
                    if (application.Quantity == null || application.TotalPrice == null)
                    {
                        MessageBox.Show("Введите корректные значения для количества и цены!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    else
                    {
                       quantity = application.Quantity;
                       unitPrice = application.TotalPrice/quantity;
                    }
                        

                    // Формируем сообщение для сервера
                    string message = $"addProduct:{productName}:{unitOfMeasurement}:{quantity}:{unitPrice}";

                    // Отправка данных на сервер
                    var response = await NetworkService.Instance.SendMessageAsync(message);
                    if (response == "Success")
                    {
                        MessageBox.Show("Продукт успешно добавлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        (ApplicationsDataGrid.ItemsSource as ObservableCollection<ApplicationViewModel>)?.Remove(application);
                        
                      
                    }
                    else
                    {
                        MessageBox.Show("Ошибка при добавлении продукта.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите заявку");
            }
           
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            ManagerPage managerPage = new ManagerPage();
            managerPage.Show();
            this.Close();
        }
    }


}
