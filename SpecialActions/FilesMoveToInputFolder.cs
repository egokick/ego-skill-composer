using skill_composer.Models;
using skill_composer.Helper;

namespace skill_composer.SpecialActions
{
    public class FilesMoveToInputFolder : ISpecialAction
    {        
        public async Task<Models.Task> Execute(Models.Task task, Skill selectedSkill)
        {
            int i = 0;
            var outputDirectory = FilePathHelper.GetDataOutputDirectory();
            var inputDirectory = FilePathHelper.GetDataInputDirectory();

            var inputLines = task.Input.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

            var inputFormatted = task.Input;
           
            var subdirectory = inputLines.FirstOrDefault(line => line.StartsWith("subdirectory:"))?["subdirectory:".Length..]?.Trim();
            if (subdirectory is not null)
            {
                var outputSubFolder = Path.Combine(outputDirectory, subdirectory);
                if (Directory.Exists(outputSubFolder))
                {
                    // Get all files and directories in the outputSubFolder
                    var files = Directory.GetFiles(outputSubFolder, "*", SearchOption.AllDirectories);
                    var directories = Directory.GetDirectories(outputSubFolder, "*", SearchOption.AllDirectories);

                    // Replace "output" with "input" in the directory structure
                    foreach (var dir in directories)
                    {
                        var correspondingInputDir = dir.Replace(outputDirectory, inputDirectory);
                        if (!Directory.Exists(correspondingInputDir))
                        {
                            Directory.CreateDirectory(correspondingInputDir);
                        }
                    }

                    // Move all files to the corresponding input directory
                    foreach (var file in files)
                    {
                        i++;
                        var correspondingInputFile = file.Replace(outputDirectory, inputDirectory);
                        var correspondingInputDir = Path.GetDirectoryName(correspondingInputFile);

                        if (!Directory.Exists(correspondingInputDir))
                        {
                            Directory.CreateDirectory(correspondingInputDir);
                        }

                        // Overwrite the file if it already exists
                        File.Copy(file, correspondingInputFile, true); // true flag overwrites the file
                        File.Delete(file); // Delete the original file after copying
                    }
                }

                task.Output = $"Moved {i} files to the input folder.";

                return task;
            }

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
                i++;
            }

            task.Output = $"Moved {i} files to the input folder.";

            return task;
        }
    }
}
