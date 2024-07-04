using Newtonsoft.Json;
using skill_composer.Helper;
using skill_composer.Models;
using Task = System.Threading.Tasks.Task;

namespace skill_composer.SpecialActions
{
    public class RunSkill : ISpecialAction
    {
        public async Task<Models.Task> ExecuteAsync(Models.Task task, Skill selectedSkill, Settings settings)
        {
            string skillName = task.Input.Trim();

            // Load all skills from a JSON file
            var allSkillsJson = File.ReadAllText(FilePathHelper.GetSkillFile());
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
                ApiHandler api = new ApiHandler(settings);
                // Recursive fun!
                Program.AISkillGeneration(skillToRun); // Process each task and update it
            }

            return task;
        } 
    }
}
