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

            var lines = task.Input.Split("\n").ToList();

            var directory = lines.FirstOrDefault(line => line.StartsWith("directory:"))?["directory:".Length..]?.Trim();
            var outputsubdirectory = lines.FirstOrDefault(line => line.StartsWith("outputsubdirectory:"))?["outputsubdirectory:".Length..]?.Trim();
            var filename = lines.FirstOrDefault(line => line.StartsWith("filename:"))?["filename:".Length..]?.Trim();
            var content = string.Join('\n', lines.Where(line => !line.StartsWith("outputsubdirectory:") && !line.StartsWith("filename:") && !line.StartsWith("directory:")).ToList());
            content = content.Replace("content:", "");

            // Check if the first line contains the filename and extract it
            if (!string.IsNullOrEmpty(filename))
            {                
                filename = Path.GetFileName(filename); // Extract just the filename if it's a path
                filename = Regex.Replace(filename, "[^a-zA-Z0-9._-]", "");                 
            }
            else
            {
                filename = $"{GenerateRandomFileName(6)}.txt";
            }

            if (!string.IsNullOrEmpty(outputsubdirectory))
            {
                outputDirectory = Path.Combine(outputDirectory, outputsubdirectory);
            }

            // this allows you to write to the input directory 
            if (!string.IsNullOrEmpty(directory)) 
            {                
                outputDirectory = Path.GetDirectoryName(directory);
            }

            var outputFilePath = Path.Combine(outputDirectory, filename);

            string directoryPath = Path.GetDirectoryName(outputFilePath);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            // Write the remaining lines to the file, excluding the filename line
            File.WriteAllText(outputFilePath, content);

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
