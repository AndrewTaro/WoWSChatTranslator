using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DeepL;
using DeepL.Model;
using System.Text.RegularExpressions;
using System.Windows;
using System.CodeDom.Compiler;

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

        public static List<LanguageOption> AvailableLanguageCodes { get; private set; } = InitializeAvailableLanguageOptions();

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

        private static List<LanguageOption> InitializeAvailableLanguageOptions()
        {
            // Clear existing items
            List<LanguageOption> languageOptions = new List<LanguageOption>();

            // Use reflection to get all public static fields of the LanguageCode class
            var fields = typeof(LanguageCode).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

            foreach (var field in fields)
            {
                if (UnavailableLanguageCodes.Contains(field.GetValue(null)))
                {
                    continue; // Skip unavailable languages
                }
                // Get the field name and value
                var name = field.Name; // Field name
                var value = field.GetValue(null); // Field value (static fields don't require an instance)
                if (value != null && value?.ToString() is string code)
                {
                    // Add the name and value to the ComboBox
                    languageOptions.Add(new LanguageOption(name, code));
                }
            }
            return languageOptions;
        }
    }

    public partial class LanguageOption
    {
        [GeneratedRegex(@"(?<=[A-Z])(?=[A-Z][a-z])|(?<=[^A-Z])(?=[A-Z])|(?<=[0-9])(?=[^A-Z0-9])|(?=[0-9])(?<=[^0-9])|(?<=[^A-Za-z0-9])(?=[a-z])|(?=[^A-Za-z0-9])(?<=[A-Za-z])")]
        private static partial Regex GetRegex();
        public string Code { get; set; }
        public string Name { get; set; }
        public LanguageOption(string name, string code)
        {
            Code = code;
            Name = string.Join(" ", GetRegex().Split(name));
        }
        public override string ToString() => $"{Name} ({Code})";
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