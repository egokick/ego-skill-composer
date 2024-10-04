using skill_composer.Models;

namespace skill_composer.SpecialActions
{
    /// <summary>
    /// Reads the content of a specified file and sets the content as the task's output.
    /// If no file path is provided, it will use a file from the input folder.
    /// The file content is prefixed with the file name in the output.
    /// </summary>
    public class StringReplace : ISpecialAction
    {
        public async Task<Models.Task> Execute(Models.Task task, Skill selectedSkill)
        {
            var inputLines = task.Input.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

            var replace = inputLines.FirstOrDefault(line => line.StartsWith("replace:"))?["replace:".Length..]?.Trim();
            var replacewith = inputLines.FirstOrDefault(line => line.StartsWith("replacewith:"))?["replacewith:".Length..]?.Trim();

            var contentLines = inputLines.Where(line => !line.StartsWith("replace:") && !line.StartsWith("replacewith:"));
            var content = string.Join("\n", contentLines).Trim();

            task.Output = content.Replace(replace, replacewith);

            return task;
        }

    }
}