using skill_composer.Models;
using skill_composer.Helper;
using Newtonsoft.Json;

namespace skill_composer.SpecialActions
{
    public class SkillGetByName : ISpecialAction
    {
        public async Task<Models.Task> Execute(Models.Task task, Skill selectedSkill)
        {
            if (string.IsNullOrEmpty(task.Input))
            {
                task.Output = "Skill name is empty. Please provide a valid skill name.";
                return task;
            }

            string skillName = task.Input.Trim();

            try
            {
                // Load the skills JSON file
                var skillFilePath = FilePathHelper.GetSkillFilePath();
                var allSkillsJson = await File.ReadAllTextAsync(skillFilePath);

                if (string.IsNullOrEmpty(allSkillsJson))
                {
                    task.Output = "Skills file is empty or could not be read.";
                    return task;
                }

                // Deserialize the skills
                var skillSet = JsonConvert.DeserializeObject<SkillSet>(allSkillsJson);
                if (skillSet == null || skillSet.Skills == null)
                {
                    task.Output = "No skills found in the skills file.";
                    return task;
                }

                // Find the skill by name
                var skillToGet = skillSet.Skills.FirstOrDefault(s => s.SkillName.Equals(skillName, StringComparison.OrdinalIgnoreCase));
                if (skillToGet == null)
                {
                    task.Output = $"Skill '{skillName}' not found.";
                    return task;
                }

                // Define the JsonSerializerSettings to format the skill output
                var jsonSerializerSettings = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented
                };

                // Set the task output to the serialized skill
                task.Output = JsonConvert.SerializeObject(skillToGet, jsonSerializerSettings);
            }
            catch (Exception ex)
            {
                task.Output = $"An error occurred: {ex.Message}";
            }

            return task;
        }
    }
}