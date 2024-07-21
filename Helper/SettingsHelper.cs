using Newtonsoft.Json;
using skill_composer.Models;

namespace skill_composer.Helper
{
    public static class SettingsHelper
    {
        public static Settings Load()
        {
            var settingsJson = File.ReadAllText(FilePathHelper.GetSettingsFile());

            var settings = JsonConvert.DeserializeObject<Settings>(settingsJson);

            return FilePathHelper.InitializeSettings(settings);
        }
    }
}
