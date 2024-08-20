using skill_composer.Models;
using skill_composer.Helper;
using skill_composer.Services;
using Newtonsoft.Json;
using System.Data;
using System.Text.RegularExpressions;
using System.Text;

namespace skill_composer.SpecialActions
{
    /// <summary>
    /// Useful if you are raw dogging AI query execution. If the query fails the AI will attempt to fix it and rerun it.
    /// </summary>
    public class DatabaseRunQueryWithAIRetry : ISpecialAction
    {
        public async Task<Models.Task> Execute(Models.Task task, Skill selectedSkill)
        {
            var databaseService = new DatabaseService();
            
            var query = task.Input;

            if (query.Contains('\\'))
            {
                // Replace single \ with \\ (but don't replace if already doubled)
                query = Regex.Replace(query, @"(?<!\\)\\(?!\\)", "\\\\");
            }

            DataSet dbResult = null;

            try
            {
                // attempt 1
                dbResult = await databaseService.ExecuteQueryMultiTable(query);
            }
            catch (Exception ex) 
            {
                try
                {
                    // attempt 2 - attempt to escape apostrophes
                    query = query
                        .Replace("'", "''")
                        .Replace("'',", "',")
                        .Replace("'')", "')")
                        .Replace("(''", "('")
                        .Replace(", ''", ", '")
                        .Replace(",  ''", ", '");

                    dbResult = await databaseService.ExecuteQueryMultiTable(query);
                }
                catch(Exception ex2) 
                {
                        // attempt 3
                    var aiMessage = $"The following mysql query failed to execute successfully: {task.Input}\n\nException: {ex2.Message} .\nModify the mysql query to fix the error. IMPORTANT! Return mysql code only, your response will be directly passed to another program.";

                    var api = new ApiHandler();

                    var aiResponse = await api.GetAIResponse(aiMessage);

                    var dbQueryV2 = RemoveFirstAndLastLines(aiResponse);

                    dbResult = await databaseService.ExecuteQueryMultiTable(query);
                }
                
            }

            if (dbResult != null)
            {
                task.Output = SerializeDataSetWithTableNames(dbResult);
            }

            return task;
        }

        static string CorrectInternalQuotes(string query)
        {
            StringBuilder output = new StringBuilder();
            bool insideQuotes = false;

            for (int i = 0; i < query.Length; i++)
            {
                char currentChar = query[i];

                if (currentChar == '\'')
                {
                    if (insideQuotes)
                    {
                        // Check if the next character is not another single quote, it's an internal quote
                        if (i + 1 < query.Length && query[i + 1] != '\'')
                        {
                            // Internal single quote, so duplicate it
                            output.Append("''");
                        }
                        else
                        {
                            // It's a closing quote
                            insideQuotes = false;
                            output.Append("'");
                        }
                    }
                    else
                    {
                        // It's an opening quote
                        insideQuotes = true;
                        output.Append("'");
                    }
                }
                else
                {
                    // Regular character, just append it
                    output.Append(currentChar);
                }
            }

            return output.ToString();
        }

        private string SerializeDataSetWithTableNames(DataSet dataSet)
        {
            var dictionary = new Dictionary<string, object>();

            // If the operation is a non-query and successful, add a success message
            if (dataSet.Tables.Count == 0)
            {
                dictionary["Result"] =  "Operation succeeded"  ;
            }
            else
            {
                foreach (DataTable table in dataSet.Tables)
                {
                    var tableData = new List<Dictionary<string, object>>();

                    foreach (DataRow row in table.Rows)
                    {
                        var rowData = new Dictionary<string, object>();
                        foreach (DataColumn column in table.Columns)
                        {
                            rowData[column.ColumnName] = row[column];
                        }
                        tableData.Add(rowData);
                    }

                    dictionary[table.TableName] = tableData;
                }
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
