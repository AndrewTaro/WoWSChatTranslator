using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DeepL;
using Microsoft.Extensions.Logging.Abstractions;

namespace WoWSChatTranslator.Models
{
    public class HttpServer
    {
        private readonly HttpListener _listener;
        private readonly Translator _translator;
        private readonly UserSettings _settings;
        private readonly Action<string>? _log;
        private CancellationTokenSource? _cancellationTokenSource;
        private bool _isRunning = false;
        public bool IsRunning => _listener.IsListening && _isRunning;

        public HttpServer(Translator translator, UserSettings settings, Action<string>? log = null)
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add(settings.TargetUrl);
            _translator = translator;
            _settings = settings;
            _log = log;
        }

        public async Task StartAsync()
        {
            if (_translator.ClientState != DeepLClientState.Initialized)
            {
                throw new InvalidOperationException("Translator is not initialized. Please set the API key and initialize the translator first.");
            }
            if (_isRunning)
            {
                return;
            }
            _isRunning = true;

            _cancellationTokenSource = new CancellationTokenSource();
            var token = _cancellationTokenSource.Token;
            _listener.Start();

            await Task.Run(() =>
            {
                while (_isRunning && !token.IsCancellationRequested)
                {
                    try
                    {
                        var context = _listener.GetContextAsync();
                        context.Wait(token);
                        if (!token.IsCancellationRequested)
                        {
                            HandleRequest(context.Result);
                        }
                    }
                    catch (OperationCanceledException) // Operation aborted
                    {
                        // Listener was stopped, exit the loop
                    }
                    catch (Exception)
                    {
                        //throw; // Handle other exceptions as needed
                    }
                }
            }, token).ConfigureAwait(false);
        }

        private async void HandleRequest(HttpListenerContext context)
        {
            var request = context.Request;
            string? translated = null;
            if (request.QueryString["text"] is string text && !string.IsNullOrWhiteSpace(text))
            {
                Log($"Received: {text}");
                translated = await _translator.TranslateAsync(text, _settings.TargetLangCode);
                if (string.IsNullOrEmpty(translated) || translated == text)
                {
                    translated = null; // No translation needed or no change
                    Log("Skipping: No translation needed or no change detected.");
                }
                else
                {
                    Log($"Translated: {translated}");
                }
            }

            byte[] buffer = Encoding.UTF8.GetBytes(translated ?? "");
            context.Response.ContentType = "text/plain";
            context.Response.OutputStream.Write(buffer, 0, buffer.Length);
            context.Response.Close();
        }

        public void Stop()
        {
            if (!_isRunning)
            {
                return;
            }
            _cancellationTokenSource?.Cancel();
            _listener.Stop();
            _isRunning = false;
        }

        private void Log(string message)
        {
            _log?.Invoke(message);
        }
    }
}