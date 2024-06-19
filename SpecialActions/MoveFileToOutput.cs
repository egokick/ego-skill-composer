using skill_composer.Models;
using skill_composer.Helper;

namespace skill_composer.SpecialActions
{
    public class MoveFileToOutput : ISpecialAction
    {
        public async Task<Models.Task> ExecuteAsync(Models.Task task, Skill selectedSkill, Settings settings)
        {   
            // Moves the file to the output folder and assigns FilePath to the new filePath
            task.FilePath = FilePathHelper.MoveFileToOutputDirectory(task.Input);

            return task;
        }
    }
}
