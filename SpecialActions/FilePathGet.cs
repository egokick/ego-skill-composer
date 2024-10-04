using skill_composer.Models;
using skill_composer.Helper;

namespace skill_composer.SpecialActions
{

    public class FilePathGet : ISpecialAction
    { 
        public async Task<Models.Task> Execute(Models.Task task, Skill selectedSkill)
        {
            if (string.IsNullOrEmpty(task.Input))
            {
                task.FilePath = FilePathHelper.GetDataInputFilePath(); // Gets any file in the input folder

                // no more files to process
                if (string.IsNullOrEmpty(task.FilePath))
                {
                    // so stop processing for this skill
                    task.HaltProcessing = true;
                    return task;
                }
            }
            else
            {
                task.FilePath = task.Input;
            }

            var fileName = Path.GetFileName(task.FilePath);
            var fileContent = File.ReadAllText(task.FilePath);

            task.Output = $"Title: {fileName}";
            
            return task;
        }
    }
}
