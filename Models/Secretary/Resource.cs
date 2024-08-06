namespace skill_composer.Models
{
    /// <summary>
    /// Represents a resource entity in the database.
    /// </summary>
    public class Resource
    {
        /// <summary>
        /// Gets or sets the unique identifier for the resource.
        /// </summary>
        public int ResourceId { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the service this
        /// resource belongs too.
        /// </summary>
        public int ServiceId { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the business to which the resource belongs.
        /// </summary>
        public int BusinessId { get; set; }

        /// <summary>
        /// Gets or sets the name of the resource.
        /// </summary>
        public string ResourceName { get; set; }

        /// <summary>
        /// Gets or sets the description of the resource.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the quantity available of the resource.
        /// </summary>
        public int QuantityAvailable { get; set; }

        /// <summary>
        /// MySql Period's :
        /// '1', 'Year'
        /// '2', 'Month'
        /// '3', 'Week'
        /// '4', 'Day'
        /// '5', 'Hour'
        /// '6', 'Minute'
        /// Resources that are consumed within a given period eg.,
        /// Log Cabin can only be booked for whole days, so the
        /// resource period would be '4', 'Day'
        /// </summary>
        public int ResourcePeriodId { get; set; }

        /// <summary>
        /// Gets or sets the period name either:
        /// '1', 'Year'
        /// '2', 'Month'
        /// '3', 'Week'
        /// '4', 'Day'
        /// '5', 'Hour'
        /// '6', 'Minute'
        /// </summary>
        public string Period { get; set; }

        /// <summary>
        /// Gets or sets the location of the resource.
        /// </summary>
        public string Location { get; set; }
    }
}
