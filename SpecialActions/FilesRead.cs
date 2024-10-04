using skill_composer.Models;

namespace skill_composer.SpecialActions
{
    /// <summary>
    /// Reads the content of specified files and appends the content to the task's output.
    /// If a file does not exist, it appends a not found message to the task's output.
    /// </summary>
    public class FilesRead : ISpecialAction
    {
        public async Task<Models.Task> Execute(Models.Task task, Skill selectedSkill)
        {
            var formattedInput = RemoveFirstAndLastLines(task.Input);
            // Split the input by both newlines and commas
            var filePaths = formattedInput.Split(new[] { '\n', ',' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var filePath in filePaths)
            {
                // Trim surrounding quotes and whitespace
                var trimmedFilePath = filePath.Trim('\"', ' ', '\r', '\n');

                if (File.Exists(trimmedFilePath))
                {
                    var fileContent = await File.ReadAllTextAsync(trimmedFilePath);
                    task.Output += $"FilePath:{trimmedFilePath}\n{fileContent}\n";
                }
                else
                {
                    task.Output += $"{trimmedFilePath} not found\n";
                }
            }

            return task;
        }

        private static string RemoveFirstAndLastLines(string input)
        {
            var lines = input.Split(new[] { "\n" }, StringSplitOptions.None);

            var filteredLines = lines.Where(line => !line.TrimStart().StartsWith("```")).ToArray();

            return string.Join("\n", filteredLines);
        }
    }
}
