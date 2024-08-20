using skill_composer.Helper;
using skill_composer.Models;

namespace skill_composer.SpecialActions
{
    public class FilesDeleteOutputFolder : ISpecialAction
    {
        public async Task<Models.Task> Execute(Models.Task task, Skill selectedSkill)
        {
            var outputDirectory = FilePathHelper.GetDataOutputDirectory();

            // Get all files in the output directory, including files in subdirectories
            var outputFiles = Directory.GetFiles(outputDirectory, "*", SearchOption.AllDirectories);

            int filesDeleted = 0;
            int filesFailed = 0;

            foreach (var outputFile in outputFiles.Where(x => !x.Contains(".git")))
            {
                try
                {
                    // Remove the ReadOnly attribute if it's set
                    File.SetAttributes(outputFile, FileAttributes.Normal);

                    // Delete the file
                    File.Delete(outputFile);
                    filesDeleted++;
                }
                catch (UnauthorizedAccessException ex)
                {
                    filesFailed++;
                    Console.WriteLine($"UnauthorizedAccessException: Unable to delete {outputFile}. Error: {ex.Message}");
                }
                catch (Exception ex)
                {
                    filesFailed++;
                    Console.WriteLine($"Exception: Unable to delete {outputFile}. Error: {ex.Message}");
                }
            }

            var outputDirectories = Directory.GetDirectories(outputDirectory, "*", SearchOption.AllDirectories);
            foreach (var directory in outputDirectories.OrderBy(x => x.Length).Where(x => !x.Contains(".git")))
            {
                try
                {
                    Directory.Delete(directory, recursive: true); // Delete directory and its contents
                }
                catch (UnauthorizedAccessException ex)
                {
                    filesFailed++;
                    Console.WriteLine($"UnauthorizedAccessException: Unable to delete directory {directory}. Error: {ex.Message}");
                }
                catch (Exception ex)
                {
                    filesFailed++;
                    Console.WriteLine($"Exception: Unable to delete directory {directory}. Error: {ex.Message}");
                }
            }

            await System.Threading.Tasks.Task.Delay(1000);

            task.Output = $"Deleted {filesDeleted} files from the output directory: {outputDirectory}. Failed to delete {filesFailed} files/directories.";

            return task;
        }
    }
}
