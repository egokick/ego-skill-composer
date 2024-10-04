using LibGit2Sharp; 

namespace skill_composer.Services
{
    /// <summary>
    /// Service for performing Git operations.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="GitService"/> class with the specified application settings.
    /// </remarks> 
    public class GitService() 
    {
        /// <inheritdoc />
        public Repository Clone(string repoUrl, string repoPath, string gitHubToken)
        {
            if (string.IsNullOrEmpty(repoUrl)) throw new ArgumentNullException(nameof(repoUrl));
            if (string.IsNullOrEmpty(repoPath)) throw new ArgumentNullException(nameof(repoPath));
            if (string.IsNullOrEmpty(gitHubToken)) throw new ArgumentNullException(nameof(gitHubToken));

            try
            {
                Console.WriteLine($"GitService.Clone: Info: Cloning repository: repository URL: {repoUrl}, repository path: {repoPath}");

                var cloneOptions = new CloneOptions
                {
                    FetchOptions =
                    {
                        CredentialsProvider = (_, _, _) => new UsernamePasswordCredentials // requires delegate to be constructed
                        {
                            Username = gitHubToken,
                            Password = string.Empty
                        }
                    }
                };

                Repository.Clone(repoUrl, repoPath, cloneOptions);
                var repository = new Repository(repoPath);
                return repository;
            }
            catch (LibGit2SharpException ge)
            {
                Console.WriteLine($"GitService.Clone: Error: {ge}");
                throw;
            }
            catch (Exception e)
            {
                Console.WriteLine($"GitService.Clone: Error: {e}");
                throw;
            }
        }

        /// <inheritdoc />        
        public Branch CreateBranch(Repository repository, string branchName = "")
        {
            ArgumentNullException.ThrowIfNull(repository);

            try
            {
                branchName = string.IsNullOrEmpty(branchName) ? "main" : branchName;

                Console.WriteLine($"Creating branch and checking out: {branchName}");
                _ = repository.CreateBranch(branchName);
                var branch = Commands.Checkout(repository, branchName);
                return branch;
            }
            catch (LibGit2SharpException ge)
            {
                Console.WriteLine($"GitService.CreateBranch: Error: {ge}");
                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine($"GitService.CreateBranch: Error: {e}");
                return null;
            }
        }

        /// <inheritdoc />
        public Commit Commit(Repository repository, IList<string> paths)
        {
            ArgumentNullException.ThrowIfNull(repository);

            try
            {
                foreach (var path in paths)
                { 
                    Console.WriteLine($"Staging and committing changes: {path}");
                    Commands.Stage(repository, path);
                }
                var author = new Signature("GitHubReadmeGenerator", "mipmapper@live.co.uk", DateTimeOffset.Now);
                var commit = repository.Commit("Add README containing code analysis", author, author);
                return commit;
            }
            catch (LibGit2SharpException ge)
            {
                Console.WriteLine($"GitService.Commit: Error: {ge}");
                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine($"GitService.Commit: Error: {e}");
                return null;
            }
        }

        /// <inheritdoc />
        public void Push(Repository repository, string gitHubToken, string branchName = "")
        {
            ArgumentNullException.ThrowIfNull(repository);

            try
            {
                branchName = string.IsNullOrEmpty(branchName) ? "main" : branchName;
                Console.WriteLine($"Pushing branch: {branchName}");
                var remote = repository.Network.Remotes["origin"];
                repository.Network.Push(remote, $@"+refs/heads/{branchName}", new PushOptions
                {
                    CredentialsProvider = (_, _, _) => new UsernamePasswordCredentials { Username = gitHubToken, Password = string.Empty }
                });
            }
            catch (LibGit2SharpException ge)
            {
                Console.WriteLine($"GitService.Push: Error: {ge}");
                throw;
            }
            catch (Exception e)
            {
                Console.WriteLine($"GitService.Push: Error: {e}");
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<string> CreatePullRequest(string repositoryUrl, string gitHubToken, string originBranch, string branchName = "")
        {
            if (string.IsNullOrEmpty(repositoryUrl)) throw new ArgumentNullException(nameof(repositoryUrl));
            if (string.IsNullOrEmpty(originBranch)) throw new ArgumentNullException(nameof(originBranch));

            branchName = string.IsNullOrEmpty(branchName) ? "main" : branchName;

            try
            {
                Console.WriteLine($"Creating pull request for repository URL: {repositoryUrl}");

                var client = new Octokit.GitHubClient(new Octokit.ProductHeaderValue("GitHubReadmeGenerator"))
                {
                    Credentials = new Octokit.Credentials(gitHubToken)
                };
                var repoOwner = repositoryUrl.Split('/')[3];
                var repoName = repositoryUrl.Split('/')[4].Replace(".git", "");
                var pullRequest = new Octokit.NewPullRequest("Add README with code analysis", branchName, originBranch)
                {
                    Body = "This pull request adds a README file with code analysis."
                };

                var pr = await client.PullRequest.Create(repoOwner, repoName, pullRequest);

                Console.WriteLine($"Pull request created : {pr.HtmlUrl}.");

                // Force garbage collection
                GC.Collect();
                GC.WaitForPendingFinalizers();

                return pr.HtmlUrl;
            }
            catch (Exception e)
            {
                Console.WriteLine($"GitService.CreatePullRequest: Error: {e}");
            }

            return null;
        }
    }
}
