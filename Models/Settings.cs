using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Reflection;

namespace skill_composer.Models
{
    public static class Settings
    {
        public static string AiUrl { get; set; }
        public static string OpenAiKey { get; set; }
        public static string OpenAiApiVersion { get; set; }
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

    }


    public class DatabaseSettings
    {
        public string Name { get; set; }
        public string ConnectionString { get; set; }
    }

}
