using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WoWSChatTranslator.Commands;
using WoWSChatTranslator.Model;
using WoWSChatTranslator.Models;

namespace WoWSChatTranslator.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public UserSettings Settings { get; private set; } = UserSettings.Load();
        public ApiKeyManager ApiKeyManager { get; private set; } = new ApiKeyManager();
        public Translator Translator { get; } = new Translator();
        private HttpServer? _server;

        private string _status = StatusCode.NotInitialized;
        public string Status
        {
            get => _status;
            set
            {
                _status = value;
                Log($"Status: {value}");
                OnPropertyChanged(nameof(Status)); 
                OnPropertyChanged(nameof(IsControlButtonEnabled));
            }
        }

        public string ApiKey
        {
            get => ApiKeyManager.ApiKey;
            set { ApiKeyManager.ApiKey = value; OnPropertyChanged(nameof(ApiKey)); }
        }

        public string TargetLangCode
        {
            get => Settings.TargetLangCode;
            set { Settings.TargetLangCode = value; Log($"Selected language: {value}"); OnPropertyChanged(nameof(TargetLangCode)); }
        }

        private string _logText = string.Empty;
        public string LogText
        {
            get => _logText;
            set { _logText = value; OnPropertyChanged(nameof(LogText)); }
        }

        public bool IsServerRunning
        {
            get => _server != null && _server.IsRunning;
            set
            {
                // Toggle server state based on the value
                if (value && _server == null)
                {
                    StartCommand.Execute(null);
                }
                else if (!value && _server != null)
                {
                    StopCommand.Execute(null);
                }
                OnPropertyChanged(nameof(IsServerRunning));
                OnPropertyChanged(nameof(ControlButtonName));
            }
        }

        public string ControlButtonName
        {
            get => IsServerRunning ? "Stop Translating" : "Start Transrating";
            set { /* This property is read-only, no setter needed */ }
        }

        private bool _isControlButtonEnabled = true;
        public bool IsControlButtonEnabled
        {
            get => _isControlButtonEnabled;
            private set
            {
                if (_isControlButtonEnabled != value)
                {
                    _isControlButtonEnabled = value;
                    OnPropertyChanged(nameof(IsControlButtonEnabled));
                }
            }
        }

        public ICommand ControlCommand { get; } = new RelayCommand(() =>
        {
            // This command is used to toggle the server state
            if (App.Current.MainWindow.DataContext is MainViewModel viewModel)
            {
                viewModel.IsServerRunning = !viewModel.IsServerRunning;
            }
        });

        public ObservableCollection<string> TargetLanguageCodes { get; } = new ObservableCollection<string>(Translator.SupportedLanguageCodes);

        public ICommand StartCommand { get; }
        public ICommand StopCommand { get; }

        public MainViewModel()
        {
            StartCommand = new RelayCommand(async () => await StartAsync());
            StopCommand = new RelayCommand(Stop);
        }

        private async Task StartAsync()
        {
            IsControlButtonEnabled = false;
            Status = StatusCode.Auhtorizing;
            switch (await Translator.InitializeAsync(ApiKey))
            {
                case DeepLClientState.Initialized:
                    Status = StatusCode.Initialized;
                    _server = new HttpServer(Translator, Settings);
                    await Task.Run(() => _server.StartAsync());
                    break;
                case DeepLClientState.ConnectionError:
                    Status = StatusCode.NetworkError;
                    break;
                case DeepLClientState.AuthorizationError:
                    Status = StatusCode.AuthorizationError;
                    break;
                case DeepLClientState.LimitReached:
                    Status = StatusCode.LimitReached;
                    break;
                case DeepLClientState.NotInitialized:
                    Status = StatusCode.NotInitialized;
                    break;
                default:
                    Status = StatusCode.Unkwnon;
                    break;
            }
            IsControlButtonEnabled = true;

        }

        private void Stop()
        {
            _server?.Stop();
            _server = null;
            Status = "Server stopped.";
        }

        public void SaveSettings()
        {
            Settings.Save();
            ApiKeyManager.SaveApiKey();
        }

        public void Log(string message)
        {
            LogText += $"{DateTime.Now:HH:mm:ss} - {message}\n";
            OnPropertyChanged(nameof(LogText));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class StatusCode
    {
        public static string NotInitialized => "Not Initialized";
        public static string Initialized => "Initialized";
        public static string Auhtorizing => "Authorizing";
        public static string AuthorizationError => "Authorization Error";
        public static string NetworkError => "Connection Error";
        public static string LimitReached => "API Limit Reached";
        public static string Unkwnon => "Unknown Error";
    }
}
