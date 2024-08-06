namespace skill_composer.Models
{
    public class Claim
    {
        public int ClaimId { get; set; }
        public int CustomerId { get; set; }
        public int IncidentId { get; set; }
        public string ClaimStatus { get; set; }
        public string ActionsTaken { get; set; }
        public string PendingTasks { get; set; }

    }
}
