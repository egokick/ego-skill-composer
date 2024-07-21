using skill_composer.Models;
using skill_composer.Helper;

namespace skill_composer.SpecialActions
{
    public class FileWriteSplitOutput : ISpecialAction
    {
        public async Task<Models.Task> Execute(Models.Task task, Skill selectedSkill, Settings settings)
        {
            var outputDirectory = FilePathHelper.GetDataOutputDirectory();
            
            var fileContent = task.Output.Split("--NEWFILE--");

            foreach(var content in  fileContent)
            {
                if (string.IsNullOrWhiteSpace(content))
                    continue;

                // Split the content into lines
                var lines = content.Split(new[] { "\n" }, StringSplitOptions.None);
                if (lines.Length < 2) // Check to ensure there's at least one line for the filename and one for content
                    continue;

                var fileName = lines[1].Trim(); // The first line is the file name.

                
                    fileName = $"{selectedSkill.RepeatCount}.txt";
 

                var fileData = string.Join(Environment.NewLine, lines.Skip(2)); // The rest is the file data.

                File.WriteAllText(Path.Combine(outputDirectory, fileName), fileData);
            }

            return task;
        }
    }
}
