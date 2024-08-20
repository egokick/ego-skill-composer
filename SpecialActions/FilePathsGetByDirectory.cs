using skill_composer.Helper;
using skill_composer.Models;

namespace skill_composer.SpecialActions
{
    public class FilePathsGetByDirectory : ISpecialAction
    {
        public async Task<Models.Task> Execute(Models.Task task, Skill selectedSkill)
        {
            // Get the directory from task.Input
            var inputDirectory = task.Input;

            if (string.IsNullOrEmpty(task.Input))
            {
                inputDirectory = FilePathHelper.GetDataInputDirectory();
            } 

            // Ensure the directory exists
            if (!Directory.Exists(inputDirectory))
            {
                throw new DirectoryNotFoundException($"The directory {inputDirectory} does not exist.");
            }

            // Get all files in the input directory and its subdirectories
            var inputFiles = Directory.GetFiles(inputDirectory, "*.*", SearchOption.AllDirectories);
            
            var cleanFiles = inputFiles.ToList().Where(x => !x.Contains(".git"));

            // Set the list of files to task.Output
            task.Output = string.Join('\n', cleanFiles);

            return task;
        }
    }
}
