namespace skill_composer.Models
{
    /// <summary>
    /// Time in Hours, Minutes and Seconds.
    /// </summary>
    public class Time
    {
        /// <summary>
        /// Default Constructor.
        /// </summary>
        /// <param name="value"></param>
        public Time(TimeSpan value)
        {
            SetTime(value);
        }

        /// <summary>
        /// Parameter less constructor for serialisation.
        /// </summary>
        public Time()
        {

        }

        /// <summary>
        /// Sets the time from a time span.
        /// </summary>
        /// <param name="value"></param>
        public void SetTime(TimeSpan value)
        {
            Hours = value.Hours;
            Minutes = value.Minutes;
            Seconds = value.Seconds;
        }

        /// <summary>
        /// Hours.
        /// </summary>
        public int Hours { get; set; }

        /// <summary>
        /// Minutes.
        /// </summary>
        public int Minutes { get; set; }

        /// <summary>
        /// Seconds.
        /// </summary>
        public int Seconds { get; set; }

        /// <summary>
        /// To Time String ; hh:mm:ss.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{Hours:D2}:{Minutes:D2}:{Seconds:D2}";
        }

        /// <summary>
        /// Converts Time to TimeSpan.
        /// </summary>
        /// <returns></returns>
        public TimeSpan ToTimeSpan()
        {
            return TimeSpan.Parse(ToString());
        }
    }
}
