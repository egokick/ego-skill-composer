using skill_composer.Models;
using skill_composer.Helper;

namespace skill_composer.SpecialActions
{
    public class FileMoveToOutput : ISpecialAction
    {
        public async Task<Models.Task> Execute(Models.Task task, Skill selectedSkill)
        {   
            // Moves the file to the output folder and assigns FilePath to the new filePath
            task.FilePath = FilePathHelper.MoveFileToOutputDirectory(task.FilePath);

            return task;
        }
    }
}
