using skill_composer.Models;
using skill_composer.Helper;
using skill_composer.Services;
using Newtonsoft.Json; 

namespace skill_composer.SpecialActions
{
    public class Fnol1SaveIncident : ISpecialAction
    {
        public async Task<Models.Task> Execute(Models.Task task, Skill selectedSkill)
        {
            var incidentString = RemoveFirstAndLastLines(task.Input);

            Incident request = JsonConvert.DeserializeObject<Incident>(incidentString);
            
            if (string.IsNullOrEmpty(request.Location)) request.Location = "unknown";
            if(string.IsNullOrEmpty(request.WitnessInformation)) request.WitnessInformation = "unknown";

            var incidentResult = await SaveIncident(request);

            task.Output = incidentResult;

            return task;
        }
        private static string RemoveFirstAndLastLines(string input)
        {
            var lines = input.Split(new[] { "\n" }, StringSplitOptions.None);

            var filteredLines = lines.Where(line => !line.TrimStart().StartsWith("```")).ToArray();

            filteredLines = filteredLines.Where(line => !line.TrimStart().StartsWith("--")).ToArray();

            return string.Join("\n", filteredLines);
        }

        private async Task<string> SaveIncident(Incident request)
        {
            try
            { 
                var databaseService = new DatabaseService(Program._settings);
                
                var parameters = new Dictionary<string, string>
                {
                    { "p_IncidentId", request.IncidentId.ToString() },
                    { "p_IncidentDateTime", request.IncidentDateTime.ToString() },
                    { "p_Location", request.Location.ToString() },
                    { "p_Description", request.Description.ToString() },
                    { "p_OtherVehiclesPartiesInvolved", request.OtherVehiclesPartiesInvolved.ToString() },
                    { "p_WitnessInformation", request.WitnessInformation.ToString() }
                };

                var dataSet = await databaseService.Execute("insurance.Incident_Save", parameters);
                
                if (dataSet.Tables[0].Columns.Contains("IncidentId")) // booking has been added
                {
                    request.IncidentId = databaseService.GetInteger(dataSet.Tables[0].Rows[0], "IncidentId");
                    return JsonConvert.SerializeObject(request);
                }
                return "Unable to save incident";
            }
            catch (Exception e)
            {
                Console.WriteLine($"SaveIncident error: {e.Message}");
                return $"SaveIncident error: {e.Message}";
            }
        }
    }
}
