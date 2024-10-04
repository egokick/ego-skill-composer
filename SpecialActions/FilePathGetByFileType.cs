using skill_composer.Models;
using skill_composer.Helper;

namespace skill_composer.SpecialActions
{
    public class FilePathGetByFileType : ISpecialAction
    {
        public async Task<Models.Task> Execute(Models.Task task, Skill selectedSkill)
        {
            // Get the initial directory where input files are stored
            var inputDirectory = FilePathHelper.GetDataInputDirectory();

            // Split the input into lines and check for a custom directory
            var inputLines = task.Input.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

            // Check if the input specifies a custom directory
            var directory = inputLines.FirstOrDefault(line => line.StartsWith("directory:"))?[10..]?.Trim();
            if (directory is not null)
            {
                inputDirectory = directory;
                // Remove the directory line from the input
                task.Input = string.Join('\n', inputLines.Where(line => !line.StartsWith("directory:")));
            }

            // Split the remaining input by comma to get multiple file types
            var fileTypes = task.Input.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                      .Select(ft => ft.Trim())
                                      .ToArray();

            List<string> matchingFiles = new List<string>();

            // Loop through each file type and get matching files
            foreach (var fileType in fileTypes)
            {
                var files = Directory.GetFiles(inputDirectory, $"*{fileType}", SearchOption.AllDirectories)
                                     .OrderBy(filename => filename)
                                     .ToArray();

                if (files.Length > 0)
                {
                    matchingFiles.AddRange(files);
                    break; // Only need to find one file for this special action
                }
            }

            if (matchingFiles.Count == 0)
            {
                task.HaltProcessing = true;
                task.Output = "No files of the specified types found.";
                return task;
            }

            // Select the first matching file in ascending order
            task.FilePath = matchingFiles.OrderBy(filename => filename).First();

            var fileName = Path.GetFileName(task.FilePath);
            var fileContent = File.ReadAllText(task.FilePath);

            task.Output = fileContent;

            return task;
        }
    }
}
