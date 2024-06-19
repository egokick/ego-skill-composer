namespace skill_composer.Helper
{
    /// <summary>
    /// Date Time extension.
    /// </summary>
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Gets the date as a string in MySql format.
        /// </summary>
        /// <param name="datetime"></param>
        /// <returns></returns>
        public static string ToMySqlDateTime(this DateTime datetime)
        {
            var stringDateTime = datetime.ToString("yyyy-MM-dd HH:mm:ss");
            return stringDateTime;
        }

        /// <summary>
        /// Gets the date as a string in MySql format.
        /// </summary>
        /// <param name="datetime"></param>
        /// <returns></returns>
        public static string ToMySqlDate(this DateTime datetime)
        {
            return datetime.ToString("yyyy-MM-dd");
        }

        /// <summary>
        /// Converts a unix time stamp to c-sharp date time.
        /// </summary>
        /// <param name="timeStamp"></param>
        /// <returns></returns>
        public static DateTime FromUnixTimeStamp(double timeStamp)
        {
            var datetime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            datetime = datetime.AddSeconds(timeStamp).ToLocalTime();
            return datetime;
        }
    }
}
