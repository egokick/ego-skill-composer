using skill_composer.Models;
using skill_composer.Helper;
using skill_composer.Services;
using MySqlX.XDevAPI.Relational;
using Newtonsoft.Json;
using System.Data;

namespace skill_composer.SpecialActions
{
    public class RunDatabaseQuery : ISpecialAction
    {
        public async Task<Models.Task> ExecuteAsync(Models.Task task, Skill selectedSkill, Settings settings)
        {
            var databaseService = new DatabaseService(settings);

            var query = RemoveFirstAndLastLines(task.Output);

            DataSet dbResult = await databaseService.ExecuteQueryMultiTable(query);

            if (dbResult != null)
            {
                var jsonData = SerializeDataSetWithTableNames(dbResult);

                if (string.IsNullOrEmpty(task.Output))
                {
                    task.Output = jsonData;
                }
                else
                {
                    // Append JSON data in a way that maintains valid JSON format.
                    task.Output += "," + jsonData;
                }
            }

            return task;
        }

        private string SerializeDataSetWithTableNames(DataSet dataSet)
        {
            var dictionary = new Dictionary<string, DataTable>();
            foreach (DataTable table in dataSet.Tables)
            {
                dictionary[table.TableName] = table;
            }

            return JsonConvert.SerializeObject(dictionary, Formatting.Indented);
        }


        private static string RemoveFirstAndLastLines(string input)
        {
            // Split the input string into lines using CRLF as the delimiter
            var lines = input.Split(new[] { "\n" }, StringSplitOptions.None);

            // Check if there are at least three lines to remove the first and last lines
            if (lines.Length <= 2)
            {
                // Return an empty string if there are not enough lines
                return string.Empty;
            }

            // Skip the first and last lines and join the remaining lines
            var result = string.Join("\r\n", lines.Skip(1).Take(lines.Length - 2));
            return result;
        }

    }
}
