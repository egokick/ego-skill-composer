using skill_composer.Models;
using skill_composer.Services;

namespace skill_composer.SpecialActions
{
    public class GitPullRequest : ISpecialAction
    {
        public async Task<Models.Task> Execute(Models.Task task, Skill selectedSkill)
        { 
            // Get task input
            var lines = task.Input.Split("\n").ToList();

            var repositoryurl = lines.FirstOrDefault(line => line.StartsWith("repositoryurl:"))?["repositoryurl:".Length..]?.Trim();
            var mergeintobranch = lines.FirstOrDefault(line => line.StartsWith("mergeintobranch:"))?["mergeintobranch:".Length..]?.Trim();
            var prbranch = lines.FirstOrDefault(line => line.StartsWith("prbranch:"))?["prbranch:".Length..]?.Trim();
            var githubtoken = lines.FirstOrDefault(line => line.StartsWith("githubtoken:"))?["githubtoken:".Length..]?.Trim();

            var gitService = new GitService();

            var pullRequestUrl = await gitService.CreatePullRequest(repositoryurl, githubtoken, mergeintobranch, prbranch);
  
            task.Output = pullRequestUrl;

            return task;
        }
 
    }
}
