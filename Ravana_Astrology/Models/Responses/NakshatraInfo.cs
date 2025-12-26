namespace Ravana_Astrology.Models.Responses
{
    /// <summary>
    /// Information about Moon's nakshatra at birth.
    /// </summary>
    public class NakshatraInfo
    {
        /// <summary>
        /// Nakshatra number (1-27)
        /// </summary>
        public int NakshatraNumber { get; set; }

        /// <summary>
        /// Nakshatra name (Sanskrit)
        /// </summary>
        public string NakshatraName { get; set; } = string.Empty;

        /// <summary>
        /// Nakshatra lord (ruling planet)
        /// </summary>
        public string Lord { get; set; } = string.Empty;

        /// <summary>
        /// Moon's longitude at birth (sidereal)
        /// </summary>
        public double MoonLongitude { get; set; }

        /// <summary>
        /// Degrees traversed in the nakshatra (0-13.333333)
        /// </summary>
        public double DegreesInNakshatra { get; set; }

        /// <summary>
        /// Percentage completed in nakshatra (0-100)
        /// </summary>
        public double PercentageCompleted { get; set; }

        /// <summary>
        /// Pada (quarter) of nakshatra (1-4)
        /// </summary>
        public int Pada { get; set; }
    }
}
