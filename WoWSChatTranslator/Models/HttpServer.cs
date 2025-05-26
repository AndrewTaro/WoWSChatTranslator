using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DeepL;

namespace WoWSChatTranslator.Models
{
    public class HttpServer
    {
        private readonly HttpListener _listener;
        private readonly Translator _translator;
        private readonly UserSettings _settings;
        public bool IsRunning => _listener.IsListening;

        public HttpServer(Translator translator, UserSettings settings)
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add(settings.TargetUrl);
            _translator = translator;
            _settings = settings;
        }

        public async Task StartAsync()
        {
            if (_translator.ClientState != DeepLClientState.Initialized)
            {
                throw new InvalidOperationException("Translator is not initialized. Please set the API key and initialize the translator first.");
            }
            _listener.Start();
            while (true)
            {
                try
                {
                    var context = await _listener.GetContextAsync();
                    HandleRequest(context);
                }
                catch (Exception)
                {
                    
                }
                
            }
        }

        private async void HandleRequest(HttpListenerContext context)
        {
            var request = context.Request;
            string? translated = null;
            if (request.QueryString["text"] is string text && !string.IsNullOrWhiteSpace(text))
            {
                translated = await _translator.TranslateAsync(text, _settings.TargetLangCode);
            }

            byte[] buffer = Encoding.UTF8.GetBytes(translated ?? "");
            context.Response.ContentType = "text/plain";
            context.Response.OutputStream.Write(buffer, 0, buffer.Length);
            context.Response.Close();
        }

        public void Stop() => _listener.Stop();
    }
}