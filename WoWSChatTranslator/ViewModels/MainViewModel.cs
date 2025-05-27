using System;
using System.CodeDom;
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

        private bool _isServerRunning = false;
        public bool IsServerRunning
        {
            get => _isServerRunning;
            set
            {
                if (_isServerRunning != value)
                {
                    _isServerRunning = value;
                    OnPropertyChanged(nameof(IsServerRunning));
                    OnPropertyChanged(nameof(ControlButtonName));
                }
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
                    OnPropertyChanged(nameof(ControlButtonName));
                }
            }
        }

        public ObservableCollection<LanguageOption> AvailableLanguageCodes { get; } = new ObservableCollection<LanguageOption>(Translator.AvailableLanguageCodes);

        public ICommand StartCommand { get; }
        public ICommand StopCommand { get; }

        public ICommand SaveApiKeyCommand { get; }
        public ICommand ControlCommand { get; }

        public MainViewModel()
        {
            StartCommand = new RelayCommand(async () => await StartAsync());
            StopCommand = new RelayCommand(Stop);
            SaveApiKeyCommand = new RelayCommand(SaveApiKey);
            ControlCommand = new RelayCommand(ExecuteControlCommand);
        }

        private void ExecuteControlCommand()
        {
            if (IsServerRunning)
            {
                StopCommand.Execute(null);
            }
            else
            {
                StartCommand.Execute(null);
            }
        }

        private async Task StartAsync()
        {
            IsControlButtonEnabled = false;
            Log($"Status: {StatusCode.Auhtorizing}");
            string logText = "";
            switch (await Translator.InitializeAsync(ApiKey))
            {
                case DeepLClientState.Initialized:
                    logText = StatusCode.Initialized;
                    _server = new HttpServer(Translator, Settings);
                    _ = _server.StartAsync();
                    Log("Status: Server started.");

                    IsServerRunning = true;

                    break;
                case DeepLClientState.ConnectionError:
                    logText = StatusCode.NetworkError;
                    break;
                case DeepLClientState.AuthorizationError:
                    logText = StatusCode.AuthorizationError;
                    break;
                case DeepLClientState.LimitReached:
                    logText = StatusCode.LimitReached;
                    break;
                case DeepLClientState.NotInitialized:
                    logText = StatusCode.NotInitialized;
                    break;
                default:
                    logText = StatusCode.Unkwnon;
                    break;
            }
            Log($"Status: {logText}");
            IsControlButtonEnabled = true;
        }

        private void SaveApiKey()
        {
            ApiKeyManager.SaveApiKey();
        }

        private void Stop()
        {
            _server?.Stop();
            _server = null;
            IsServerRunning = false;
            Log("Status: Server stopped.");
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
