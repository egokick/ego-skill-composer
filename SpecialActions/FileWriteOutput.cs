using skill_composer.Models;
using skill_composer.Helper;
using System.Text.RegularExpressions;

namespace skill_composer.SpecialActions
{
    public class FileWriteOutput : ISpecialAction
    {
        public async Task<Models.Task> ExecuteAsync(Models.Task task, Skill selectedSkill, Settings settings)
        {
            var outputDirectory = FilePathHelper.GetDataOutputDirectory();

            var fileName = $"{selectedSkill.RepeatCount}.txt";

            var lines = task.Output.Split("\n");

            if (lines.FirstOrDefault() is not null && lines.First().Count(c => c == '-') == 4)
            {
                fileName = lines.First();
                fileName = Regex.Replace(fileName, "[^a-zA-Z0-9._]", "");
            }

            File.WriteAllText(Path.Combine(outputDirectory, fileName), task.Output);

            return task;
        }
    }
}
