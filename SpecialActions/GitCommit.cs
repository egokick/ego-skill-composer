using skill_composer.Models;
using skill_composer.Services;
using Newtonsoft.Json;
using LibGit2Sharp;

namespace skill_composer.SpecialActions
{
    public class GitCommit : ISpecialAction
    {
        public async Task<Models.Task> Execute(Models.Task task, Skill selectedSkill)
        { 
            // Get task input
            var lines = task.Input.Split("\n").ToList();

            var directory = lines.FirstOrDefault(line => line.StartsWith("directory:"))?["directory:".Length..]?.Trim();
            var author = lines.FirstOrDefault(line => line.StartsWith("author:"))?["author:".Length..]?.Trim();
            var commitmessage = lines.FirstOrDefault(line => line.StartsWith("commitmessage:"))?["commitmessage:".Length..]?.Trim();
            var newbranchname = lines.FirstOrDefault(line => line.StartsWith("newbranchname:"))?["newbranchname:".Length..]?.Trim();            

            var repository = new LibGit2Sharp.Repository(directory);

            var gitService = new GitService();

            // todo log branch creation
            _ = gitService.CreateBranch(repository, newbranchname);

            // Stage all file changes
            Commands.Stage(repository, "*");

            var gitAuthor = new LibGit2Sharp.Signature("AIGenerated", author, DateTime.Now);
         
            var commitOptions = new CommitOptions() { }; 

            var commit = repository.Commit(commitmessage, gitAuthor, gitAuthor, commitOptions);

            task.Output = JsonConvert.SerializeObject(new 
            { 
                Message = commit.Message,
                Encoding = commit.Encoding,
                AuthorName = commit.Author.Name,
                AuthorEmail = commit.Author.Email,
                WhenCreated = commit.Author.When,
                SHA = commit.Sha
            });            

            return task;
        }
 
    }
}
