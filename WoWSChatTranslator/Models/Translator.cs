using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DeepL;
using DeepL.Model;

namespace WoWSChatTranslator.Models
{
    public class Translator
    {
        private DeepLClient? _translator;
        private TextTranslateOptions _translateOptions = new TextTranslateOptions();
        private static List<string> UnavailableLanguageCodes = new List<string>
        {
            LanguageCode.English,
        };

        public DeepLClientState ClientState {  get; private set; }

        public static List<string> SupportedLanguageCodes { get; private set; } = InitializeSupportedLanguageCodes();

        public async Task<DeepLClientState> InitializeAsync(string apiKey)
        {
            _translateOptions.TagHandling = "html";
            DeepLClientState state = DeepLClientState.NotInitialized;
            try
            {
                _translator = new DeepLClient(apiKey);
                Usage usage = await _translator.GetUsageAsync();
                if (usage.AnyLimitReached)
                {
                    state = DeepLClientState.LimitReached;
                }
                state = DeepLClientState.Initialized;
            }
            catch (DeepL.ConnectionException)
            {
                state = DeepLClientState.ConnectionError;
            }
            catch (AuthorizationException)
            {
                state = DeepLClientState.AuthorizationError;
            }
            catch (Exception)
            {
                state = DeepLClientState.UnkownError;
            }
            finally
            {
                ClientState = state;
            }
            return state;
        }

        public async Task<string?> TranslateAsync(string text, string targetLangCode)
        {
            if (ClientState != DeepLClientState.Initialized || _translator == null)
                return null;
            var result = await _translator.TranslateTextAsync(text, null, targetLangCode, _translateOptions);
            if (result.Text == text)
            {
                return null;
            }
            return result.Text;
        }

        private static List<string> InitializeSupportedLanguageCodes()
        {
            // Clear existing items
            List<string> languageCodes = new List<string>();

            // Use reflection to get all public static fields of the LanguageCode class
            var fields = typeof(LanguageCode).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

            foreach (var field in fields)
            {
                if (UnavailableLanguageCodes.Contains(field.GetValue(null)))
                {
                    continue; // Skip unavailable languages
                }
                // Get the field name and value
                var value = field.GetValue(null); // Field value (static fields don't require an instance)
                if (value != null)
                {
                    // Add the name and value to the ComboBox
                    var code = value?.ToString();
                    languageCodes.Add(code ?? "Invalid Language Code");
                }
            }
            return languageCodes;
        }
    }

    public enum DeepLClientState
    {
        NotInitialized,
        Initialized,
        LimitReached,
        ConnectionError,
        AuthorizationError,
        UnkownError,
    }
}