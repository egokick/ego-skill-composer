namespace skill_composer.Models
{
    public class GithubResult
    {
        public string Branch { get; set; }
        public string RepositoryUrl { get; set; }
        public string Guid { get; set; }
        public string FilePath { get; set; }
        public Head Head { get; set; }
        public int ClientLoginId { get; set; }
    }

    public class Head
    {
        public bool IsRemote { get; set; }
        public bool IsTracking { get; set; }
        public bool IsCurrentRepositoryHead { get; set; }
        public string UpstreamBranchCanonicalName { get; set; }
        public string RemoteName { get; set; }
        public string CanonicalName { get; set; }
        public string FriendlyName { get; set; }
    }
}
