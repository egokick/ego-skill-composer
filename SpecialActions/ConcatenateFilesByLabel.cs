using skill_composer.Models;
using skill_composer.Helper;

namespace skill_composer.SpecialActions
{
    public class ConcatenateFilesByLabel : ISpecialAction
    {
        public async Task<Models.Task> Execute(Models.Task task, Skill selectedSkill, Settings settings)
        {
            var labels = task.Input.Split(',').ToList();
            labels.RemoveAll(label => string.IsNullOrEmpty(label) || label == ".");
            var dataInputDirectory = FilePathHelper.GetDataInputDirectory();
            var outputDirectory = FilePathHelper.GetDataOutputDirectory();

            foreach (var label in labels)
            {
                var labelTrimmed = label.Trim(); // Ensure no leading or trailing spaces
                var filesWithLabel = Directory.GetFiles(dataInputDirectory, "*" + labelTrimmed + "*");

                var concatenatedContent = filesWithLabel.SelectMany(file => File.ReadAllLines(file)).ToArray();

                var outputFile = Path.Combine(outputDirectory, labelTrimmed + ".txt");
                File.WriteAllLines(outputFile, concatenatedContent);

                task.FilePath = outputFile;

                Console.WriteLine($"Created {outputFile} with concatenated content of {filesWithLabel.Length} files.");
            }

            return task;
        }
    }
}
