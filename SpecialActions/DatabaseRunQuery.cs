using skill_composer.Models;
using skill_composer.Helper;
using skill_composer.Services;
using MySqlX.XDevAPI.Relational;
using Newtonsoft.Json;
using System.Data;

namespace skill_composer.SpecialActions
{
    public class DatabaseRunQuery : ISpecialAction
    {
        public async Task<Models.Task> Execute(Models.Task task, Skill selectedSkill)
        {
            var databaseService = new DatabaseService(Program._settings);

            var query = RemoveFirstAndLastLines(task.Input);
 
            DataSet dbResult = await databaseService.ExecuteQueryMultiTable(query);

            if (dbResult != null)
            {
                task.Output = SerializeDataSetWithTableNames(dbResult);
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
            var lines = input.Split(new[] { "\n" }, StringSplitOptions.None);

            var filteredLines = lines.Where(line => !line.TrimStart().StartsWith("```")).ToArray();
            
            filteredLines = filteredLines.Where(line => !line.TrimStart().StartsWith("--")).ToArray();

            return string.Join("\n", filteredLines);
        }

    }
}
