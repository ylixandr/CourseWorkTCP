using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TCPServerLab2;
using TESTINGCOURSEWORK.Models;

namespace TESTINGCOURSEWORK.SupplierFolder
{
    /// <summary>
    /// Логика взаимодействия для SupplierMainPage.xaml
    /// </summary>
    public partial class SupplierMainPage : Window
    {
        private ObservableCollection<ApplicationViewModel> applications;
        public ObservableCollection<ApplicationViewModel> Applications { get { return applications; } set { applications = value; } }
        public SupplierMainPage()
        {
            InitializeComponent();
            HideAllGrid();
        }

        private void Create_Button_Click(object sender, RoutedEventArgs e)
        {
            CreateNewApplication createNewApplication = new CreateNewApplication();
            createNewApplication.Show();
            this.Hide();
        }

        private async void Delete_Button_Click(object sender, RoutedEventArgs e)
        {
            if (ApplicationDataGrid.SelectedItem is ApplicationViewModel selectedApplication)
            {
                var result = MessageBox.Show(
                    $"Вы уверены, что хотите удалить заявку ID: {selectedApplication.Id}?",
                    "Подтверждение",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        // Отправка запроса на сервер для удаления заявки
                        string request = $"deleteApplication:{selectedApplication.Id}";
                        string response = await NetworkService.Instance.SendMessageAsync(request);

                        if (response == "Application deleted")
                        {
                            MessageBox.Show("Заявка успешно удалена.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                            // Обновление данных в DataGrid для текущего пользователя
                            await LoadApplicationsAsync();
                        }
                        else
                        {
                            MessageBox.Show("Не удалось удалить заявку. Попробуйте снова.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении заявки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите заявку для удаления.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }


        private async void History_Button_Click(object sender, RoutedEventArgs e)
        {
            HideAllGrid();

            try
            {
                // Запрос истории заявок по текущему AccountId
                string request = $"getApplicationHistory:{CurrentUser.UserId}";
                string response = await NetworkService.Instance.SendMessageAsync(request);

                if (response == "NoData")
                {
                    MessageBox.Show("Нет данных для отображения.");
                    return;
                }

                topEditingPanel.Visibility = Visibility.Visible;
                HistoryDataGrid.Visibility = Visibility.Visible;

                // Десериализация истории заявок, включая новые поля
                var applications = JsonConvert.DeserializeObject<List<ApplicationViewModel>>(response);

                // Назначение источника данных для HistoryDataGrid
                HistoryDataGrid.ItemsSource = applications;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }


        private async void form_Button_Click(object sender, RoutedEventArgs e)
        {
            HideAllGrid();
            topEditingPanel.Visibility = Visibility.Visible;

            try
            {
                // Запрос заявок по текущему AccountId
                string request = $"getApplications:{CurrentUser.UserId}";
                string response = await NetworkService.Instance.SendMessageAsync(request);

                if (response == "NoData")
                {
                    MessageBox.Show("Нет данных для отображения.");
                    return;
                }

                ApplicationDataGrid.Visibility = Visibility.Visible;

                // Обновление структуры заявок с учетом новых полей
                var applications = JsonConvert.DeserializeObject<List<ApplicationViewModel>>(response);

                // Назначение данных DataGrid
                ApplicationDataGrid.ItemsSource = applications;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private async Task LoadApplicationsAsync()
        {
            try
            {
                // Отправка запроса на сервер для получения заявок текущего пользователя
                string request = $"getApplications:{CurrentUser.UserId}";
                string response = await NetworkService.Instance.SendMessageAsync(request);

                if (response == "NoData")
                {
                    ApplicationDataGrid.ItemsSource = null;
                    MessageBox.Show("Нет заявок для отображения.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Десериализация данных
                var applications = JsonConvert.DeserializeObject<List<ApplicationViewModel>>(response);

                // Привязка данных к DataGrid
                ApplicationDataGrid.ItemsSource = applications;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки заявок: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void HideAllGrid()
        {

            topSupportPanel.Visibility = Visibility.Hidden;
            SupportTicketsDataGrid.Visibility = Visibility.Hidden;
            topEditingPanel.Visibility = Visibility.Hidden;
            ApplicationDataGrid.Visibility = Visibility.Hidden;
            HistoryDataGrid.Visibility = Visibility.Hidden;


        }

        private async void support_Button_Click(object sender, RoutedEventArgs e)
        {
            HideAllGrid();
            topSupportPanel.Visibility = Visibility.Visible;
            
            try
            {
                // Запрос заявок по текущему AccountId
                string request = $"getSupport:{CurrentUser.UserId}";
                string response = await NetworkService.Instance.SendMessageAsync(request);

                if (response == "NoData")
                {
                    MessageBox.Show("Нет данных для отображения.");
                    return;
                }

                SupportTicketsDataGrid.Visibility = Visibility.Visible;

                // Обновление структуры заявок с учетом новых полей
                var supports = JsonConvert.DeserializeObject<List<SupportViewModel>>(response);

                // Назначение данных DataGrid
                SupportTicketsDataGrid.ItemsSource = supports;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }

        }

        private void CreateTicket_Click(object sender, RoutedEventArgs e)
        {
            var createTicketWindow = new CreateTicketWindow(); // отдельное окно для создания
            createTicketWindow.ShowDialog();
           
        }

    }
}
