using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CredentialManagement;
using DeepL;
using Microsoft.Extensions.Options;

namespace WoWSChatTranslator.Model
{
    public class ApiKeyManager
    {
        public const string DefaultApiKey = "Insert your auth key from DeepL";
        private const string CredentialName = "Wows-Chat-Translator-DeepL-API-Key";
        private const string UserName = "deepl";
        public string ApiKey { get; set; } = DefaultApiKey;

        public ApiKeyManager(string? apiKey=null)
        {
            if (!string.IsNullOrWhiteSpace(apiKey) && apiKey != DefaultApiKey)
            {
                ApiKey = apiKey;
            }
            else
            {
                LoadApiKey(); // Try to load the API key from Windows Credential Manager
            }
        }

        public bool LoadApiKey()
        {
            var cred = new Credential { Target = CredentialName };
            if (cred.Load())
            {
                ApiKey = cred.Password;
                return true;
            }
            ApiKey = DefaultApiKey;
            return false; // Credential not found, using default fake API key
        }

        public static async Task<bool> ValidateApiKey(string apiKey)
        {
            if (string.IsNullOrWhiteSpace(apiKey) || apiKey.Contains(DefaultApiKey))
            {
                return false;
            }
            DeepLClient client = new DeepLClient(apiKey);
            try
            {
                var usage = await client.GetUsageAsync(); // Check if the API key is valid
                return !usage.AnyLimitReached;
            }
            catch (Exception)
            {
                return false; // Invalid API key
            }
        }

        public bool SaveApiKey()
        {
            // Save the API key to Windows Credential Manager
            var cred = new Credential
            {
                Target = CredentialName,
                Username = UserName,
                Password = ApiKey,
                Type = CredentialType.Generic,
                PersistanceType = PersistanceType.LocalComputer
            };
            return cred.Save();
        }

        public bool RemvoeApiKey()
        {
            var cred = new Credential { Target = CredentialName };
            if (cred.Load())
            {
                return cred.Delete();
            }
            else
            {
                return false; // Credential not found
            }
        }
    }

    public class ApiKey : IEquatable<ApiKey>
    {
        public string Key { get; private set; }
        public bool IsValid { get; private set; } = false;
        public ApiKey(string key)
        {
            Key = key;
        }
        public async Task<bool> ValidateKey()
        {
            IsValid = await ApiKeyManager.ValidateApiKey(Key);
            return IsValid;
        }
        public bool Equals(ApiKey? other)
        {
            if (other is null) return false;
            return Key == other.Key;
        }
        public override bool Equals(object? obj)
        {
            return Equals(obj as ApiKey);
        }
        public override int GetHashCode()
        {
            return Key.GetHashCode();
        }
    }
}
