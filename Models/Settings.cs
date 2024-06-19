namespace skill_composer.Models
{

    public class Settings
    {
        public string OpenAiKey { get; set; }
        public string OpenAiModel { get; set; }
        public string OpenaAIVerifierModel { get; set; }
        public string AssemblyAIApiKey { get; set; }
        public List<DatabaseSettings> Databases { get; set; }


    }
    public class DatabaseSettings
    {
        public string Name { get; set; }
        public string ConnectionString { get; set; }
    }

}
