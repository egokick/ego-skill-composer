using Newtonsoft.Json;
using skill_composer.Helper;
using skill_composer.Models;
using Task = System.Threading.Tasks.Task;

namespace skill_composer.SpecialActions
{
    public class SkillRun : ISpecialAction
    {
        public async Task<Models.Task> Execute(Models.Task task, Skill selectedSkill)
        {
            string skillName = task.Input.Trim();

            // Load all skills from a JSON file
            var allSkillsJson = File.ReadAllText(FilePathHelper.GetSkillFilePath());
            var allSkills = JsonConvert.DeserializeObject<SkillSet>(allSkillsJson);

            // Find the skill by name
            var skillToRun = allSkills?.Skills.FirstOrDefault(s => s.SkillName.Equals(skillName, StringComparison.OrdinalIgnoreCase));

            if (skillToRun == null)
            {
                task.Output = "Skill not found.";
                task.HaltProcessing = true;
            }
            else
            {
                task.Output = $"Initiating skill: {skillName}";
                // Setting up an ApiHandler and Skill execution context as required
                ApiHandler api = new ApiHandler();
                // Recursive fun!
                Program.ProcessSkill(skillToRun); // Process each task and update it
            }

            return task;
        } 
    }
}
