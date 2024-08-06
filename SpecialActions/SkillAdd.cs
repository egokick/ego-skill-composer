using Newtonsoft.Json;
using skill_composer.Helper;
using skill_composer.Models;

namespace skill_composer.SpecialActions
{
    public class SkillAdd : ISpecialAction
    {
        public async Task<Models.Task> Execute(Models.Task task, Skill selectedSkill)
        {
            var skillJson = ExtractContentBetweenCodeBlocks(task.Input);

            // Attempt to deserialize the input to a Skill object
            Skill newSkill = JsonConvert.DeserializeObject<Skill>(skillJson);

            foreach(var t in newSkill.Tasks)
            {
                if (t.PrintOutput is not null && t.PrintOutput == true) t.PrintOutput = null;
                if (t.HaltProcessing is null || t.HaltProcessing == false) t.HaltProcessing = null;
                if (t.BestOf is not null && t.BestOf == 0) t.BestOf = null;
            } 

            // Load all skills from the JSON file
            var skillFilePath = FilePathHelper.GetSkillFilePath();
            var allSkillsJson = await File.ReadAllTextAsync(skillFilePath);
            var skillSet = JsonConvert.DeserializeObject<SkillSet>(allSkillsJson) ?? new SkillSet { Skills = new List<Skill>() };

            foreach (var skill in skillSet.Skills)
            {
                if (skill.DisableFileLogging is not null && skill.DisableFileLogging == false) skill.DisableFileLogging = null;
                if (skill.AppendFileLogging is not null && skill.AppendFileLogging == false) skill.AppendFileLogging = null;
              
                foreach (var t in skill.Tasks)
                {
                    if (t.PrintOutput is not null && t.PrintOutput == true) t.PrintOutput = null;
                    if (t.HaltProcessing is null || t.HaltProcessing == false) t.HaltProcessing = null;
                    if(t.BestOf==0) t.BestOf = null;
                }
            }

            // Append the new skill to the existing skills
            skillSet.Skills.Add(newSkill);

            // Define the JsonSerializerSettings to ignore null values and format indented
            var jsonSerializerSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.Indented
            };

            // Save the updated skills back to the JSON file
            var updatedSkillsJson = JsonConvert.SerializeObject(skillSet, jsonSerializerSettings);
            await File.WriteAllTextAsync(skillFilePath, updatedSkillsJson);

            task.Output = $"Skill '{newSkill.SkillName}' added successfully.";

            return task;
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