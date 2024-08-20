using skill_composer.Models;
using skill_composer.Helper;
using Newtonsoft.Json;

namespace skill_composer.SpecialActions
{
    public class FilePathsGetByFileType : ISpecialAction
    {
        public async Task<Models.Task> Execute(Models.Task task, Skill selectedSkill)
        {
            var inputDirectory = FilePathHelper.GetDataInputDirectory();

            var inputLines = task.Input.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            
            var inputFormatted = task.Input;

            var directory = inputLines.FirstOrDefault(line => line.StartsWith("directory:"))?[10..]?.Trim()!;
            if (directory is not null)
            {
                inputDirectory = directory;
                inputFormatted = string.Join('\n', inputLines.ToList().Where(x => !x.StartsWith("directory:")));
            }

            // Initialize a list to store all matching files
            var allMatchingFiles = new List<string>();

            // Split the task.Input by comma to handle multiple file types
            var fileTypes = inputFormatted.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var fileType in fileTypes)
            {
                // Get all files of the specified type in the directory and subdirectories
                var files = Directory.GetFiles(inputDirectory, $"*{fileType.Trim()}", SearchOption.AllDirectories)
                                     .OrderBy(filename => filename)
                                     .ToArray();

                // Add the files to the list of all matching files
                allMatchingFiles.AddRange(files);
            }

            if (allMatchingFiles.Count == 0)
            {
                task.HaltProcessing = true;
                task.Output = "No files of the specified type(s) found.";
                return task;
            }

            // Set the task.FilePath to the first file in ascending order (if needed)
            task.FilePath = allMatchingFiles[0];

            var multiLineFilePaths = string.Join(Environment.NewLine, allMatchingFiles);

            if (task.UserResponseShared is not null) 
            {
                task.UserResponseShared.Enqueue("filepaths:" + JsonConvert.SerializeObject(allMatchingFiles));
            }

            // Set the task.Output to all matching file paths, each on a separate line
            task.Output = multiLineFilePaths;

            return task;
        }
    }
}
