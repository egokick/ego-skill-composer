using Newtonsoft.Json;
using skill_composer.Helper;
using skill_composer.Models;

namespace skill_composer.SpecialActions
{
    public class SkillUpdate : ISpecialAction
    {
        public async Task<Models.Task> Execute(Models.Task task, Skill selectedSkill)
        { 
            var skillJson = ExtractContentBetweenCodeBlocks(task.Input);

            // Attempt to deserialize the input to a Skill object
            Skill? updatedSkill = JsonConvert.DeserializeObject<Skill>(skillJson);

            if (updatedSkill == null)
            {
                task.Output = "Invalid skill JSON format.";
                return task;
            }

            // Load all skills from the JSON file
            var skillFilePath = FilePathHelper.GetSkillFilePath();
            var allSkillsJson = await File.ReadAllTextAsync(skillFilePath);
            var skillSet = JsonConvert.DeserializeObject<SkillSet>(allSkillsJson) ?? new SkillSet { Skills = new List<Skill>() };

            // Find the skill by name and update it
            var existingSkill = skillSet.Skills.FirstOrDefault(s => s.SkillName.Equals(updatedSkill.SkillName, StringComparison.OrdinalIgnoreCase));
            if (existingSkill != null)
            {
                // Update the existing skill's properties
                UpdateSkillProperties(existingSkill, updatedSkill);

                // Define the JsonSerializerSettings to ignore null values and format indented
                var jsonSerializerSettings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    Formatting = Formatting.Indented
                };

                // Save the updated skills back to the JSON file
                var updatedSkillsJson = JsonConvert.SerializeObject(skillSet, jsonSerializerSettings);
                await File.WriteAllTextAsync(skillFilePath, updatedSkillsJson);

                task.Output = $"Skill '{updatedSkill.SkillName}' updated successfully.";
            }
            else
            {
                task.Output = $"Skill '{updatedSkill.SkillName}' not found.";
            }
            return task;
        }

        private void UpdateSkillProperties(Skill existingSkill, Skill updatedSkill)
        {
            // Update only the necessary properties
            existingSkill.Temperature = updatedSkill.Temperature;
            existingSkill.OpenAiModel = updatedSkill.OpenAiModel;
            existingSkill.DisableFileLogging = updatedSkill.DisableFileLogging;
            existingSkill.AppendFileLogging = updatedSkill.AppendFileLogging;
            existingSkill.Tasks = updatedSkill.Tasks;
        }

        private static string ExtractContentBetweenCodeBlocks(string input)
        {
            var lines = input.Split(new[] { "\n" }, StringSplitOptions.None);

            var insideCodeBlock = false;
            var extractedLines = new List<string>();

            foreach (var line in lines)
            {
                if (line.TrimStart().StartsWith("```"))
                {
                    insideCodeBlock = !insideCodeBlock;
                    continue; // Skip the lines starting with ```
                }

                if (insideCodeBlock)
                {
                    extractedLines.Add(line);
                }
            }

            return string.Join("\n", extractedLines);
        }
    }
}