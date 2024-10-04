 
namespace skill_composer.Models
{
    /// <summary>
    /// Basic Business Hour.
    /// </summary>
    public class BasicBusinessHour
    {
        /// <summary>
        /// Gets or sets the unique identifier for the BusinessHour.
        /// </summary>
        public int BusinessHourId { get; set; }

        /// <summary>
        /// Gets or sets the time at which the business opens on the specified day.
        /// In HH:mm:ss format.
        /// </summary>
        public string BusinessOpen { get; set; }

        /// <summary>
        /// Gets or sets the time at which the business closes on the specified day.
        /// In HH:mm:ss format.
        /// </summary>
        public string BusinessClose { get; set; }

        /// <summary>
        /// Day Name eg., Monday.
        /// </summary>
        public string DayName { get; set; }

        /// <summary>
        /// Enum Value of DayOfWeek.
        /// </summary>
        public int Value { get; set; }

        /// <summary>
        /// Gets the business hour as a comma separated list.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var bh = string.Empty;

            if (!string.IsNullOrEmpty(DayName)) { bh += $"{DayName}"; }
            if (!string.IsNullOrEmpty(BusinessOpen)) { bh += $": {BusinessOpen.ToTwelveHourFormat()}"; }
            if (!string.IsNullOrEmpty(BusinessClose)) { bh += $" to {BusinessClose.ToTwelveHourFormat()}, "; }

            return bh;
        }
    }
}
