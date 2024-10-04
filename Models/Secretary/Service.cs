namespace skill_composer.Models
{
    /// <summary>
    /// Represents a service offered by a business in the booking application.
    /// </summary>
    public class Service
    {
        /// <summary>
        /// Gets or sets the unique identifier for the service.
        /// </summary>
        public int ServiceId { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the business offering the service.
        /// </summary>
        public int BusinessId { get; set; }

        /// <summary>
        /// Gets or sets the category of the service.
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the address associated with the service.
        /// Nullable to accommodate cases where the service is not associated with a specific address.
        /// </summary>
        public int? AddressId { get; set; }

        /// <summary>
        /// Gets or sets the name of the service.
        /// </summary>
        public string ServiceName { get; set; }

        /// <summary>
        /// Gets or sets the description of the service.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the duration of the service in seconds.
        /// </summary>
        public int Duration { get; set; }

        /// <summary>
        /// Gets or sets the price of the service, assuming a two-decimal place currency.
        /// eg., 88.77
        /// </summary>
        public decimal Price { get; set; }
    }
}
