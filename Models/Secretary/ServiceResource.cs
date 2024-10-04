namespace skill_composer.Models
{
    /// <summary>
    /// Service Resource.
    /// </summary>
    public class ServiceResource
    {
        /// <summary>
        /// Gets or sets the unique identifier for the service.
        /// </summary>
        public int ServiceResourceId { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the business to which the resource belongs.
        /// </summary>
        public int BusinessId { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the service.
        /// </summary>
        public int ServiceId { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the resource.
        /// </summary>
        public int ResourceId { get; set; }
    }
}
