using skill_composer.Models;
using skill_composer.Helper;

namespace skill_composer.SpecialActions
{
    public class FilesMoveToOutputFolder : ISpecialAction
    {
        public async Task<Models.Task> Execute(Models.Task task, Skill selectedSkill)
        {
            var inputDirectory = FilePathHelper.GetDataInputDirectory();
            var outputDirectory = FilePathHelper.GetDataOutputDirectory();

            // Get all files in the input directory, including files in subdirectories
            var inputFiles = Directory.GetFiles(inputDirectory, "*.*", SearchOption.AllDirectories);

            int i = 0;
            foreach (var inputFile in inputFiles)
            {
                // Generate the relative path for the file in the output directory
                var relativePath = Path.GetRelativePath(inputDirectory, inputFile);
                var outputFile = Path.Combine(outputDirectory, relativePath);

                // Ensure the directory structure is created in the output directory
                Directory.CreateDirectory(Path.GetDirectoryName(outputFile));

                // Move the file to the output directory
                // This will overwrite the file in the output directory if it already exists
                File.Move(inputFile, outputFile, true);
                i++;
            }

            // Clean up empty directories in the input directory
            DeleteEmptyDirectories(inputDirectory);

            task.Output = $"Moved {i} files to output directory: {outputDirectory}";

            return task;
        }

        private void DeleteEmptyDirectories(string startDirectory)
        {
            foreach (var directory in Directory.GetDirectories(startDirectory, "*", SearchOption.AllDirectories))
            {
                if (Directory.GetFiles(directory).Length == 0 && Directory.GetDirectories(directory).Length == 0)
                {
                    Directory.Delete(directory, false);
                }
            }
        }
    }
}
