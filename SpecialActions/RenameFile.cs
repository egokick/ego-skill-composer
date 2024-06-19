using skill_composer.Models;
using skill_composer.Helper;
using System.Text.RegularExpressions;

namespace skill_composer.SpecialActions
{
    public class RenameFile : ISpecialAction
    { 

        public async Task<Models.Task> ExecuteAsync(Models.Task task, Skill selectedSkill, Settings settings)
        { 
            var fileName = Path.GetFileName(task.FilePath);
            var directory = Path.GetDirectoryName(task.FilePath);

            // replace commas with hyphens 
            var labels = task.Input.Replace(" ", "").Replace(",", "-");

            fileName = labels + " " + fileName;

            // Use Regex to strip non-alphanumeric characters from the fileName
            fileName = Regex.Replace(fileName, "[^-a-zA-Z0-9. ]", "");

            var newFilePath = Path.Combine(directory, fileName);

            File.Move(task.FilePath, newFilePath);

            // Set the new filepath so that it can be correctly referenced in subsequent task steps
            task.FilePath = newFilePath;

            return task;
        }
    }
}
