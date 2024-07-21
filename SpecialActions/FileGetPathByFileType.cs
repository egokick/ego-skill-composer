using skill_composer.Models;
using skill_composer.Helper;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace skill_composer.SpecialActions
{
    public class FileGetPathByFileType : ISpecialAction
    {
        public async Task<Models.Task> Execute(Models.Task task, Skill selectedSkill, Settings settings)
        {
            // Get the directory where input files are stored
            var inputDirectory = FilePathHelper.GetDataInputDirectory();

            // Get all files of the specified type in the input directory
            var files = Directory.GetFiles(inputDirectory, $"*.{task.Input}")
                                 .OrderBy(filename => filename)
                                 .ToArray();

            if (files.Length == 0)
            {
                task.HaltProcessing = true;
                task.Output = "No files of the specified type found.";
                return task;
            }

            // Select the first file in ascending order
            task.FilePath = files[0];

            var fileName = Path.GetFileName(task.FilePath);
            var fileContent = File.ReadAllText(task.FilePath);

            task.Output = $"Title: {fileName}";

            return task;
        }
    }
}
