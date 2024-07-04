using skill_composer.Models;
using skill_composer.Helper;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace skill_composer.SpecialActions
{
    public class ConcatenateAllInputFiles : ISpecialAction
    {
        public async Task<Models.Task> ExecuteAsync(Models.Task task, Skill selectedSkill, Settings settings)
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
