using skill_composer.Models;
using skill_composer.Helper;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace skill_composer.SpecialActions
{
    public class FilesPathGetByDirectory : ISpecialAction
    {
        public async Task<Models.Task> Execute(Models.Task task, Skill selectedSkill)
        {
            // Get the directory from task.Input
            var inputDirectory = task.Input;

            // Ensure the directory exists
            if (!Directory.Exists(inputDirectory))
            {
                throw new DirectoryNotFoundException($"The directory {inputDirectory} does not exist.");
            }

            // Get all files in the input directory and its subdirectories
            var inputFiles = Directory.GetFiles(inputDirectory, "*.*", SearchOption.AllDirectories);

            // Set the list of files to task.Output
            task.Output = string.Join('\n', inputFiles);

            return task;
        }
    }
}
