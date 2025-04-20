using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace Client.ManagerFolder.DataAnalysys
{

    public class CapitalAnalysViewModel : INotifyPropertyChanged
    {
        private string _currentViewTitle = "Добро пожаловать";
        private object _selectedView;
        private string _statusMessage = "Готово";
        private bool _isImportActive;
        private bool _isDataActive;
        private bool _isAnalysisActive;
        private bool _isReportsActive;

        public string CurrentViewTitle
        {
            get => _currentViewTitle;
            set
            {
                _currentViewTitle = value;
                OnPropertyChanged(nameof(CurrentViewTitle));
            }
        }

        public object SelectedView
        {
            get => _selectedView;
            set
            {
                _selectedView = value;
                OnPropertyChanged(nameof(SelectedView));
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged(nameof(StatusMessage));
            }
        }

        public bool IsImportActive
        {
            get => _isImportActive;
            set
            {
                _isImportActive = value;
                OnPropertyChanged(nameof(IsImportActive));
            }
        }

        public bool IsDataActive
        {
            get => _isDataActive;
            set
            {
                _isDataActive = value;
                OnPropertyChanged(nameof(IsDataActive));
            }
        }

        public bool IsAnalysisActive
        {
            get => _isAnalysisActive;
            set
            {
                _isAnalysisActive = value;
                OnPropertyChanged(nameof(IsAnalysisActive));
            }
        }

        public bool IsReportsActive
        {
            get => _isReportsActive;
            set
            {
                _isReportsActive = value;
                OnPropertyChanged(nameof(IsReportsActive));
            }
        }

        public ICommand NavigateCommand { get; }

        public CapitalAnalysViewModel()
        {
            NavigateCommand = new RelayCommand(Navigate);
        }

        private void Navigate(object parameter)
        {
            IsImportActive = false;
            IsDataActive = false;
            IsAnalysisActive = false;
            IsReportsActive = false;

            string section = parameter as string;
            switch (section)
            {
                case "Import":
                    CurrentViewTitle = "Импорт данных";
                    SelectedView = null;
                    StatusMessage = "Открыт раздел импорта";
                    IsImportActive = true;
                    var importWindow = new ImportWindow { Owner = Application.Current.MainWindow };
                    importWindow.ShowDialog();
                    break;
                case "Data":
                    CurrentViewTitle = "Просмотр данных";
                    var dataView = new DataView();
                    SelectedView = dataView;
                    StatusMessage = $"Открыт раздел данных. SelectedView установлен в DataView (Type: {dataView.GetType().Name}).";
                    IsDataActive = true;
                    break;
                case "Analysis":
                    CurrentViewTitle = "Анализ капитала";
                    var analysisView = new AnalysisView();
                    SelectedView = analysisView;
                    StatusMessage = $"Открыт раздел анализа. SelectedView установлен в AnalysisView (Type: {analysisView.GetType().Name}).";
                    IsAnalysisActive = true;
                    break;
                case "Reports":
                    CurrentViewTitle = "Отчеты";
                    var reportsView = new ReportsView();
                    SelectedView = reportsView;
                    StatusMessage = $"Открыт раздел отчетов. SelectedView установлен в ReportsView (Type: {reportsView.GetType().Name}).";
                    IsReportsActive = true;
                    break;
                default:
                    StatusMessage = $"Неизвестный раздел: {section}";
                    break;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
    // Простая реализация ICommand
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }

        public event EventHandler CanExecuteChanged;

        // Метод для явного вызова CanExecuteChanged
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
