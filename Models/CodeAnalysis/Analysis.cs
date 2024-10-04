namespace skill_composer.Models
{
    /// <summary>
    /// Represents an analysis of a code repository.
    /// </summary>
    public class Analysis
    {
        /// <summary>
        /// Gets or sets the unique identifier for the analysis.
        /// </summary>
        public int AnalysisId { get; set; }

        /// <summary>
        /// Gets or sets the globally unique identifier (GUID) for the analysis.
        /// </summary>
        public string Guid { get; set; }

        /// <summary>
        /// Gets or sets the URL of the repository being analysed.
        /// </summary>
        public string RepositoryUrl { get; set; }

        /// <summary>
        /// Gets or sets the URL of the pull request.
        /// </summary>
        public string PullRequestUrl { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the prompt type.
        /// </summary>
        public string SkillName { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the client login.
        /// </summary>
        public int ClientLoginId { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the analysis started.
        /// </summary>
        public DateTime StartedOn { get; set; }
    }
}
