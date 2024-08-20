using skill_composer.Models;
using skill_composer.Helper;
using skill_composer.Services;
using Newtonsoft.Json;
using System.Data;

namespace skill_composer.SpecialActions
{
    public class GithubClone : ISpecialAction
    {
        public async Task<Models.Task> Execute(Models.Task task, Skill selectedSkill)
        {              
            var request = JsonConvert.DeserializeObject<CodeRequest>(task.Input);

            var guid = Guid.NewGuid().ToString(); // unique guid used to track the analysis.
            var path = Path.Combine(FilePathHelper.GetDataInputDirectory(), guid);

            var gitService = new GitService();
            var repository = gitService.Clone(request.RepositoryUrl, path, request.GitHubToken);
            var originBranch = repository.Head.FriendlyName;

            var stringOutput = JsonConvert.SerializeObject(repository);
            var cleanedOutput = JsonConvert.DeserializeObject<GithubResult>(stringOutput);
            
            cleanedOutput.Guid = guid;
            cleanedOutput.FilePath = path;
            cleanedOutput.RepositoryUrl = request.RepositoryUrl;
            cleanedOutput.Branch = request.BranchName;
            cleanedOutput.ClientLoginId = request.ClientLoginId;

            if (task.UserResponseShared is not null)
            {
                task.UserResponseShared.Enqueue($"guid:{guid}");
            } 

            task.Output = JsonConvert.SerializeObject(cleanedOutput);
            task.FilePath = path;

            return task;
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
