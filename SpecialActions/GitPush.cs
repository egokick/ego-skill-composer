using skill_composer.Models;
using LibGit2Sharp;
using skill_composer.Services;

namespace skill_composer.SpecialActions
{
    public class GitPush : ISpecialAction
    {
        public async Task<Models.Task> Execute(Models.Task task, Skill selectedSkill)
        { 
            // Get task input
            var lines = task.Input.Split("\n").ToList();

            var directory = lines.FirstOrDefault(line => line.StartsWith("directory:"))?["directory:".Length..]?.Trim();
            var branchname = lines.FirstOrDefault(line => line.StartsWith("branchname:"))?["branchname:".Length..]?.Trim();
            var githubtoken = lines.FirstOrDefault(line => line.StartsWith("githubtoken:"))?["githubtoken:".Length..]?.Trim();
            
            var repository = new Repository(directory); 
             
            var gitService = new GitService();
                        
            gitService.Push(repository, githubtoken, branchname);

            task.Output = "Git push successful";

            return task;
        }
 
    }
}
