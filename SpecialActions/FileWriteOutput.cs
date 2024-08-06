using skill_composer.Models;
using skill_composer.Helper;
using System.Text.RegularExpressions;

namespace skill_composer.SpecialActions
{
    public class FileWriteOutput : ISpecialAction
    {
        public async Task<Models.Task> Execute(Models.Task task, Skill selectedSkill)
        {
            var outputDirectory = FilePathHelper.GetDataOutputDirectory();

            var fileName = $"{GenerateRandomFileName(6)}.txt";

            var lines = task.Input.Split("\n");

            if (lines.FirstOrDefault() is not null && lines.First().Count(c => c == '-') == 4)
            {
                fileName = lines.First();
                fileName = Regex.Replace(fileName, "[^a-zA-Z0-9._]", "");
            }

            var outputFilePath = Path.Combine(outputDirectory, fileName);

            File.WriteAllText(outputFilePath, task.Input);

            task.FilePath = outputFilePath;
            return task;
        }

        string GenerateRandomFileName(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            var fileName = new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());

            return $"{fileName}";
        }

    }
}
