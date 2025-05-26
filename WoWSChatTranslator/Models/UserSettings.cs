using System.IO;
using Newtonsoft.Json;
using DeepL;

namespace WoWSChatTranslator.Models
{
    public class UserSettings
    {
        public string TargetLangCode { get; set; } = LanguageCode.EnglishBritish;
        public string TargetUrl { get; set; } = "http://localhost:5000/wowschat/";

        private static readonly string SettingsPath = "userSettings.json";

        public static UserSettings Load()
        {
            if (File.Exists(SettingsPath))
            {
                UserSettings? userSettings = JsonConvert.DeserializeObject<UserSettings>(File.ReadAllText(SettingsPath));
                if (userSettings != null)
                {
                    return userSettings; // Loaded saved data
                }
            }
            return new UserSettings();
        }

        public void Save()
        {
            File.WriteAllText(SettingsPath, JsonConvert.SerializeObject(this));
        }
    }
}
