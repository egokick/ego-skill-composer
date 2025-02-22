using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Reflection;

namespace skill_composer.Models
{
    public static class Settings
    {
        public static string AiUrl { get; set; }
        public static string OpenAiKey { get; set; } 
        public static string OpenAiModel { get; set; } 
        public static string AzureClientId { get; set; }   
        public static string AzureTenantId { get; set; }
        public static string AzureSecretId { get; set; }
        public static string AssemblyAIApiKey { get; set; }
        public static string S3BucketName { get; set; }
        public static string S3KeyName { get; set; }
        public static string PusherAppId { get; set; }
        public static string PusherAppKey { get; set; }
        public static string PusherAppSecret { get; set; }

        public static List<DatabaseSettings> Databases { get; set; }

        public static void LoadSettings(string filePath)
        {
            var settingsJson = File.ReadAllText(filePath);
            var settingsDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(settingsJson);

            foreach (var property in typeof(Settings).GetProperties(BindingFlags.Static | BindingFlags.Public))
            {
                if (settingsDict.TryGetValue(property.Name, out var value))
                {
                    if (property.PropertyType == typeof(List<DatabaseSettings>) && value is JArray)
                    {
                        var databaseList = ((JArray)value).ToObject<List<DatabaseSettings>>();
                        property.SetValue(null, databaseList);
                    }
                    else
                    {
                        var convertedValue = Convert.ChangeType(value, property.PropertyType);
                        property.SetValue(null, convertedValue);
                    }
                }
            }
        }

        public static void CreateDefaultSettingsFile(string filePath)
        {
            var defaultSettings = new Dictionary<string, object>();

            foreach (var prop in typeof(Settings).GetProperties(BindingFlags.Static | BindingFlags.Public))
            {
                if (prop.PropertyType == typeof(List<DatabaseSettings>))
                {
                    defaultSettings[prop.Name] = new List<DatabaseSettings>();
                }
                else if (prop.PropertyType == typeof(string))
                {
                    string defaultVal = "";
                    if (prop.Name == "OpenAiModel")
                        defaultVal = "gpt-4o";
                    else if (prop.Name == "AiUrl")
                        defaultVal = "https://api.openai.com/v1/chat/completions";
                    defaultSettings[prop.Name] = defaultVal;
                }
            }

            var json = JsonConvert.SerializeObject(defaultSettings, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }

    }


    public class DatabaseSettings
    {
        public string Name { get; set; }
        public string ConnectionString { get; set; }
    }

}
