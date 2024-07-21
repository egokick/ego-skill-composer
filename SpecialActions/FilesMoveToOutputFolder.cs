using skill_composer.Models;
using skill_composer.Helper;
using System;
using System.IO;
using System.Threading.Tasks;

namespace skill_composer.SpecialActions
{
    public class FilesMoveToOutputFolder : ISpecialAction
    {
        public async Task<Models.Task> Execute(Models.Task task, Skill selectedSkill, Settings settings)
        {
            var inputDirectory = FilePathHelper.GetDataInputDirectory();
            var outputDirectory = FilePathHelper.GetDataOutputDirectory();

            // Get all files in the input directory
            var inputFiles = Directory.GetFiles(inputDirectory);

            foreach (var inputFile in inputFiles)
            {
                // Generate the path for the file in the output directory
                var fileName = Path.GetFileName(inputFile);
                var outputFile = Path.Combine(outputDirectory, fileName);

                // Move the file to the output directory
                // This will overwrite the file in the output directory if it already exists
                File.Move(inputFile, outputFile, true);
            }

            return task;
        }
    }
}
