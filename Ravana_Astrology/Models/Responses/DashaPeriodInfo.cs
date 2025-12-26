namespace Ravana_Astrology.Models.Responses
{
    /// <summary>
    /// Base information for any dasha period.
    /// </summary>
    public class DashaPeriodInfo
    {
        /// <summary>
        /// Planet ruling this period
        /// </summary>
        public string Planet { get; set; } = string.Empty;

        /// <summary>
        /// Start date-time (UTC)
        /// </summary>
        public DateTime StartDateUtc { get; set; }

        /// <summary>
        /// End date-time (UTC)
        /// </summary>
        public DateTime EndDateUtc { get; set; }

        /// <summary>
        /// Start date-time (Local)
        /// </summary>
        public DateTime StartDateLocal { get; set; }

        /// <summary>
        /// End date-time (Local)
        /// </summary>
        public DateTime EndDateLocal { get; set; }

        /// <summary>
        /// Duration in days
        /// </summary>
        public double DurationDays { get; set; }

        /// <summary>
        /// Duration in years (decimal)
        /// </summary>
        public double DurationYears { get; set; }

        /// <summary>
        /// Whether this period is currently active
        /// </summary>
        public bool IsCurrentPeriod { get; set; }
    }
}
