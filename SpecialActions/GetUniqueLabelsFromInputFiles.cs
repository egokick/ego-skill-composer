using skill_composer.Models;
using skill_composer.Helper;

namespace skill_composer.SpecialActions
{
    public class GetUniqueLabelsFromInputFiles : ISpecialAction
    {
        public async Task<Models.Task> ExecuteAsync(Models.Task task, Skill selectedSkill, Settings settings)
        {
            var inputDirectory = FilePathHelper.GetDataInputDirectory();

            // Ensure the directory exists to avoid exceptions
            if (Directory.Exists(inputDirectory))
            {
                var allFiles = Directory.GetFiles(inputDirectory);

                var labelGroups = allFiles
                    .SelectMany(file => Path.GetFileName(file).Split('-')) // Split each filename by '-'
                    .GroupBy(label => label) // Group by label
                    .Where(group => group.Count() > 1) // Filter groups by those that have more than one occurrence
                    .Select(group => group.Key) // Select the label from the group
                    .ToList(); // Convert to List

                task.Output = string.Join(",", labelGroups);
            }
            else
            {
                Console.WriteLine($"GetUniqueLabelsFromInputFiles -> No files in the input directory");
            }

            return task;
        }
    }
}
