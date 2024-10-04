namespace skill_composer.Models
{
    public class Skill
    {
        public string SkillName { get; set; }
        public string Description { get; set; }
        public string OpenAiModel { get; set; }
        public double Temperature { get; set; } = 0.1;
        public int RepeatCount = 1; 
        public List<Task> Tasks { get; set; }

        // Allows the user to disable the file logging, this is particularly useful for large data labelling tasks where the logging may be redundant 
        public bool? DisableFileLogging { get; set; } = false;
        public bool? AppendFileLogging { get; set; } = false;
    }
}
