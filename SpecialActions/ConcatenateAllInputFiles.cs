using skill_composer.Models;
using skill_composer.Helper;

namespace skill_composer.SpecialActions
{
    /// <summary>
    /// Concatenates the contents of all files in the input data directory
    /// and writes the combined content to a single output file.
    /// The result file path is set in the task's FilePath field.
    /// </summary>

    public class ConcatenateAllInputFiles : ISpecialAction
    {
        public async Task<Models.Task> Execute(Models.Task task, Skill selectedSkill, Settings settings)
        {
            var dataInputDirectory = FilePathHelper.GetDataInputDirectory();
            var outputDirectory = FilePathHelper.GetDataOutputDirectory();

            // Get all files in the input data directory
            var allFiles = Directory.GetFiles(dataInputDirectory).OrderBy(f => f).ToList();

            // Read and concatenate the contents of all files
            var concatenatedContent = allFiles.SelectMany(file => File.ReadAllLines(file)).ToArray();

            // Define the output file path
            var outputFile = Path.Combine(outputDirectory, "AllConcatenatedFiles.txt");
            File.WriteAllLines(outputFile, concatenatedContent);

            // Set the file path in the task
            task.FilePath = outputFile;

            Console.WriteLine($"Created {outputFile} with concatenated content of {allFiles.Count} files.");

            return task;
        }
    }
}
