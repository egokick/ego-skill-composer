namespace skill_composer.Models
{
    /// <summary>
    /// Booking response for available slots.
    /// </summary>
    public class BookingResponse
    {
        /// <summary>
        /// Resource Identity.
        /// </summary>
        public int ResourceId { get; set; }

        /// <summary>
        /// Start Date Time.
        /// </summary>
        public DateTime StartDateTime { get; set; }

        /// <summary>
        /// End Date Time.
        /// </summary>
        public DateTime EndDateTime { get; set; }

    }
}
