using skill_composer.Models;
using skill_composer.Helper;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace skill_composer.SpecialActions
{
    public class SpecialActionAdd : ISpecialAction
    {
        public async Task<Models.Task> Execute(Models.Task task, Skill selectedSkill)
        {
            if (task.Input.ToLower() == "continue" ) return task;

            // Extract the class name and class code from the task input
            var className = ExtractClassName(task.Input);
            var classCode = ExtractClassCode(task.Input);

            if (string.IsNullOrEmpty(className) || string.IsNullOrEmpty(classCode))
            {
                task.Output = "Invalid input. Could not extract class name or code.";
                return task;
            }

            // Get the root directory of the project and then combine with the "SpecialActions" folder
            var rootDirectory = FilePathHelper.GetRootDirectory();
            var specialActionsDirectory = Path.Combine(rootDirectory, "SpecialActions");

            // Ensure the directory exists
            Directory.CreateDirectory(specialActionsDirectory);

            var newClassFilePath = Path.Combine(specialActionsDirectory, $"{className}.cs");

            // Write the class code to the new file
            await File.WriteAllTextAsync(newClassFilePath, classCode);

            task.Output = $"Successfully added new special action: {className} at {newClassFilePath}";

            return task;
        }

        private string ExtractClassName(string input)
        {
            // Improved regex to account for attributes and access modifiers
            var match = Regex.Match(input, @"\bclass\s+(\w+)", RegexOptions.Multiline);
            return match.Success ? match.Groups[1].Value : null;
        }

        private string ExtractClassCode(string input)
        {
            // Extract the code block containing the class definition
            var lines = input.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
            var insideCodeBlock = false;
            var classCodeLines = new List<string>();

            foreach (var line in lines)
            {
                if (line.TrimStart().StartsWith("```"))
                {
                    insideCodeBlock = !insideCodeBlock;
                    continue;
                }

                if (insideCodeBlock)
                {
                    classCodeLines.Add(line);
                }
            }

            return string.Join(Environment.NewLine, classCodeLines);
        }
    }
}
