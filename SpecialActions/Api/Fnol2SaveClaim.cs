using skill_composer.Models;
using skill_composer.Helper;
using skill_composer.Services;
using Newtonsoft.Json; 

namespace skill_composer.SpecialActions
{
    public class Fnol2SaveClaim : ISpecialAction
    {
        public async Task<Models.Task> Execute(Models.Task task, Skill selectedSkill)
        {
            var claimString = RemoveFirstAndLastLines(task.Input);

            var request = JsonConvert.DeserializeObject<Claim>(claimString);
            request.ClaimStatus = "Pending";
            request.ActionsTaken = "Initial assessment completed, documents collected";
            request.PendingTasks = "Obtain police report, schedule vehicle inspection";

            var claimResult = await SaveClaim(request);

            task.Output = claimResult;

            return task;
        }
        private static string RemoveFirstAndLastLines(string input)
        {
            var lines = input.Split(new[] { "\n" }, StringSplitOptions.None);

            var filteredLines = lines.Where(line => !line.TrimStart().StartsWith("```")).ToArray();

            filteredLines = filteredLines.Where(line => !line.TrimStart().StartsWith("--")).ToArray();

            return string.Join("\n", filteredLines);
        }

        private async Task<string> SaveClaim(Claim request)
        {
            try
            { 
                var databaseService = new DatabaseService(Program._settings);

                var parameters = new Dictionary<string, string>
                {
                    { "p_ClaimId", request.IncidentId.ToString() },
                    { "p_CustomerId", request.CustomerId.ToString() },
                    { "p_IncidentId", request.IncidentId.ToString() },
                    { "p_ClaimStatus", request.ClaimStatus.ToString() },
                    { "p_ActionsTaken", request.ActionsTaken.ToString() },
                    { "p_PendingTasks", request.PendingTasks.ToString() }                    
                };

                var dataSet = await databaseService.Execute("insurance.Claim_Save", parameters);
                
                if (dataSet.Tables[0].Columns.Contains("ClaimId")) // booking has been added
                {
                    request.ClaimId = databaseService.GetInteger(dataSet.Tables[0].Rows[0], "ClaimId");
                    return JsonConvert.SerializeObject(request);
                }
                return "Unable to save claim";
            }
            catch (Exception e)
            {
                Console.WriteLine($"SaveClaim error: {e.Message}");
                return $"SaveClaim error: {e.Message}";
            }
        }
    }
}
