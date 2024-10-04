namespace skill_composer.Models
{
     public class CodeRequest
    {
        /// <summary>
        /// Gets or sets the URL to the repository. This property holds the URL where the repository is located.
        /// </summary>
        public string RepositoryUrl { get; set; }

        /// <summary>
        /// The GitHub token used for accessing the GitHub API.
        /// </summary>
        public string GitHubToken { get; set; }

        /// <summary>
        /// Gets or sets the OpenAI key.
        /// Only required if not on the white list.
        /// </summary>
        public string OpenAiKey { get; set; }

        /// <summary>
        /// Gets or sets the prompt type identity.
        /// If not provided the system default's to 1, which performs the code analysis and produces the readme files
        /// </summary>
        public string SkillName { get; set; }

        /// <summary>
        /// The branch name to use for the pull request.
        /// </summary>
        public string BranchName { get; set; }

        public int ClientLoginId { get; set; }
    }
}
