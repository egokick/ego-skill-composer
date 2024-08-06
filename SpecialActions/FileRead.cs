using skill_composer.Models;
using skill_composer.Helper;

namespace skill_composer.SpecialActions
{
    /// <summary>
    /// Reads the content of a specified file and sets the content as the task's output.
    /// If no file path is provided, it will use a file from the input folder.
    /// The file content is prefixed with the file name in the output.
    /// </summary>

    public class FileRead : ISpecialAction
    { 
        public async Task<Models.Task> Execute(Models.Task task, Skill selectedSkill)
        {
            if (string.IsNullOrEmpty(task.FilePath))
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
                    if (task.Input.StartsWith("\"") && task.Input.EndsWith("\""))
                    {
                        task.FilePath = task.Input.Substring(1, task.Input.Length - 2);
                    }
                    else
                    {
                        task.FilePath = task.Input;
                    }
                }
            }

            var fileName = Path.GetFileName(task.FilePath);
            var fileContent = File.ReadAllText(task.FilePath);

            task.Output = $"Title: {fileName} {fileContent}";
            
            return task;
        }
    }
}
