using skill_composer.Models;
using skill_composer.Helper;

namespace skill_composer.SpecialActions
{
    public class MoveOutputFilesToInputFolder : ISpecialAction
    {        
        public async Task<Models.Task> ExecuteAsync(Models.Task task, Skill selectedSkill, Settings settings)
        {
            var outputDirectory = FilePathHelper.GetDataOutputDirectory();
            var inputDirectory = FilePathHelper.GetDataInputDirectory();

            // Get all files in the output directory
            var outputFiles = Directory.GetFiles(outputDirectory);

            foreach (var outputFile in outputFiles)
            {
                // Generate the path for the file in the input directory
                var fileName = Path.GetFileName(outputFile);
                var inputFile = Path.Combine(inputDirectory, fileName);

                // Move the file to the input directory
                // This will overwrite the file in the output directory if it already exists
                File.Move(outputFile, inputFile, true);
            }

            return task;
        }
    }
}
