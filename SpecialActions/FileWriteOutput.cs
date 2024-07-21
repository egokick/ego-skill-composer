using skill_composer.Models;
using skill_composer.Helper;
using System.Text.RegularExpressions;

namespace skill_composer.SpecialActions
{
    public class FileWriteOutput : ISpecialAction
    {
        public async Task<Models.Task> Execute(Models.Task task, Skill selectedSkill, Settings settings)
        {
            var outputDirectory = FilePathHelper.GetDataOutputDirectory();

            var fileName = $"{selectedSkill.RepeatCount}.txt";

            var lines = task.Output.Split("\n");

            if (lines.FirstOrDefault() is not null && lines.First().Count(c => c == '-') == 4)
            {
                fileName = lines.First();
                fileName = Regex.Replace(fileName, "[^a-zA-Z0-9._]", "");
            }

            var outputFilePath = Path.Combine(outputDirectory, fileName);

            File.WriteAllText(outputFilePath, task.Output);

            task.FilePath = outputFilePath;
            return task;
        }
    }
}
