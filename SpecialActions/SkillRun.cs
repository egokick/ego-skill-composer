using skill_composer.Helper;
using skill_composer.Models;

namespace skill_composer.SpecialActions
{
    public class SkillRun : ISpecialAction
    {
        public async Task<Models.Task> Execute(Models.Task task, Skill selectedSkill)
        {
            var inputLines = task.Input.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

            var skillNameToRun = task.Input;

            var inputDirectory = "";

            var specialinput = inputLines.FirstOrDefault(line => line.StartsWith("specialinput:"))?["specialinput:".Length..]?.Trim();

            if (specialinput is not null)
            {
                inputDirectory = specialinput;
                skillNameToRun = string.Join('\n', inputLines.ToList().Where(x => !x.StartsWith("specialinput:")));
            } 

            string skillName = skillNameToRun.Trim();

            // Load all skills from a JSON file
            //var allSkillsJson = File.ReadAllText(FilePathHelper.GetSkillFilePath());
            //var allSkills = JsonConvert.DeserializeObject<SkillSet>(allSkillsJson);
            var allSkills = await SkillHelper.GetSkillsFromS3();

            // Find the skill by name
            var skillToRun = allSkills?.Skills.FirstOrDefault(s => s.SkillName.Equals(skillName, StringComparison.OrdinalIgnoreCase));

            if (skillToRun == null)
            {
                task.Output = $"SkillName: '{skillName}' not found.";
                task.HaltProcessing = true;
            }
            else
            {
                task.Output = $"Initiating skill: {skillName}, directory: {inputDirectory}";

                if (!string.IsNullOrEmpty(inputDirectory))
                {
                    skillToRun
                        .Tasks
                        .Where(x=>x.SpecialAction == "ExternalInput")
                        .ToList()
                        .ForEach(x=>x.Output = inputDirectory);
                }

                // Recursive fun!
                task.Output = Program.ProcessSkill(skillToRun); // Process each task and update it
            }

            return task;
        } 
    }
}
