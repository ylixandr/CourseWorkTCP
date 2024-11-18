using System.Configuration;
using System.Data;
using System.Windows;

namespace TESTINGCOURSEWORK
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected  override async void OnExit(ExitEventArgs e)
        {
            try
            {
                await NetworkService.Instance.SendCloseMessageAndCloseConnection();
            }
            finally
            {
                base.OnExit(e);
            }
        }
    }

}
