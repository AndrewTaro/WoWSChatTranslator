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

        private string _status = "Not initialized.";
        public string Status
        {
            get => _status;
            set { _status = value; OnPropertyChanged(nameof(Status)); }
        }

        public string ApiKey
        {
            get => ApiKeyManager.ApiKey;
            set { ApiKeyManager.ApiKey = value; OnPropertyChanged(nameof(ApiKey)); }
        }

        public string TargetLangCode
        {
            get => Settings.TargetLangCode;
            set { Settings.TargetLangCode = value; OnPropertyChanged(nameof(TargetLangCode)); }
        }

        public ObservableCollection<string> TargetLanguageCodes { get; } = new ObservableCollection<string>(Translator.SupportedLanguageCodes);

        public ICommand SaveAndStartCommand { get; }
        public ICommand StopCommand { get; }

        public MainViewModel()
        {
            SaveAndStartCommand = new RelayCommand(async () => await SaveAndStartAsync());
            StopCommand = new RelayCommand(Stop);
        }

        private async Task SaveAndStartAsync()
        {
            switch (await Translator.InitializeAsync(ApiKey))
            {
                case DeepLClientState.Initialized:
                    Status = "DeepL client was successfully initialized.";
                    break;
                case DeepLClientState.ConnectionError:
                    Status = "Network Error occurred.";
                    break;
                case DeepLClientState.AuthorizationError:
                    Status = "Authorization failed.";
                    break;
                case DeepLClientState.LimitReached:
                    Status = "API Limit has reached.";
                    break;
                case DeepLClientState.NotInitialized:
                    Status = "Not Initialized.";
                    break;
                default:
                    Status = "Unknown State";
                    break;
            }

            Settings.Save();
            ApiKeyManager.SaveApiKey();
            _server = new HttpServer(Translator, Settings);
            await Task.Run(() => _server.StartAsync());
        }

        private void Stop()
        {
            _server?.Stop();
            _server = null;
            Status = "Server stopped.";
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
