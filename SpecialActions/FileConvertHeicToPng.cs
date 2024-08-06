using skill_composer.Models;
using skill_composer.Helper;
using ImageMagick; // Add ImageMagick for image processing

namespace skill_composer.SpecialActions
{
    public class FileConvertHeicToPng : ISpecialAction
    {
        public async Task<Models.Task> Execute(Models.Task task, Skill selectedSkill)
        {
            var heicFilePath = task.FilePath;

            // Get the output directory using the helper method
            var outputDirectory = FilePathHelper.GetDataOutputDirectory();

            // Create the output file path in the output directory
            var pngFilePath = Path.Combine(outputDirectory, Path.GetFileNameWithoutExtension(heicFilePath) + ".png");

            // Convert HEIC to PNG using ImageMagick
            using (var image = new MagickImage(heicFilePath))
            {
                image.Write(pngFilePath);
            }

            // Update the task with the new file path
            task.FilePath = pngFilePath;
            task.Output = $"Converted HEIC to PNG: {pngFilePath}";

            return task;
        }
    }
}
