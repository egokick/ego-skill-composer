using skill_composer.Models;
using skill_composer.Helper;

namespace skill_composer.SpecialActions
{
    public class FilesMoveToInputFolderByFileType : ISpecialAction
    {
        public async Task<Models.Task> Execute(Models.Task task, Skill selectedSkill)
        {
            var outputDirectory = FilePathHelper.GetDataOutputDirectory();
            var inputDirectory = FilePathHelper.GetDataInputDirectory();

            // Get the file extension from task input
            var fileExtension = task.Input?.Trim();
            if (string.IsNullOrEmpty(fileExtension))
            {
                throw new ArgumentException("File extension must be specified in the task input.");
            }

            // Get all files in the output directory and its subdirectories with the specified extension
            var outputFiles = Directory.GetFiles(outputDirectory, $"*{fileExtension}", SearchOption.AllDirectories);

            foreach (var outputFile in outputFiles)
            {
                // Generate the path for the file in the input directory
                var fileName = Path.GetFileName(outputFile);
                var inputFile = Path.Combine(inputDirectory, fileName);

                // Move the file to the input directory
                // This will overwrite the file in the input directory if it already exists
                File.Move(outputFile, inputFile, true);
            }

            return task;
        }
    }
}
