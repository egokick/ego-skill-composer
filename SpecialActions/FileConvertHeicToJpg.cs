using skill_composer.Models;
using skill_composer.Helper;
using ImageMagick; // Add ImageMagick for image processing

namespace skill_composer.SpecialActions
{
    public class FileConvertHeicToJpg : ISpecialAction
    {
        public async Task<Models.Task> Execute(Models.Task task, Skill selectedSkill)
        {
            var heicFilePath = task.FilePath;

            // Get the output directory using the helper method
            var outputDirectory = FilePathHelper.GetDataOutputDirectory();

            // Create the output file path in the output directory
            var jpgFilePath = Path.Combine(outputDirectory, Path.GetFileNameWithoutExtension(heicFilePath) + ".jpg");

            // Convert HEIC to JPG using ImageMagick
            using (var image = new MagickImage(heicFilePath))
            {
                // Check if the image is larger than 2000x2000
                if (image.Width > 2000 || image.Height > 2000)
                {
                    // Calculate the new dimensions while maintaining the aspect ratio
                    var size = new MagickGeometry(2000, 2000)
                    {
                        IgnoreAspectRatio = false
                    };

                    // Resize the image
                    image.Resize(size);
                }

                // Set the format to JPG and save the image
                image.Format = MagickFormat.Jpg;
                image.Write(jpgFilePath);
            }

            // Update the task with the new file path
            task.FilePath = jpgFilePath;
            task.Output = $"Converted HEIC to JPG: {jpgFilePath}";

            return task;
        }
    }
}