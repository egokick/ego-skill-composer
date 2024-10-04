namespace skill_composer.Models
{
    public class Incident
    {
        public int IncidentId { get; set; }
        public DateTime IncidentDateTime { get; set; }
        public string Location { get; set; }
        public string Description  { get; set; }
        public string OtherVehiclesPartiesInvolved { get; set; }
        public string WitnessInformation { get; set; }
    }
}
