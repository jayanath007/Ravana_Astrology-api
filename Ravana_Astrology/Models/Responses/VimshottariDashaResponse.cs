namespace Ravana_Astrology.Models.Responses
{
    /// <summary>
    /// Complete Vimshottari Dasha calculation response.
    /// </summary>
    public class VimshottariDashaResponse
    {
        /// <summary>
        /// Birth date-time (UTC)
        /// </summary>
        public DateTime BirthDateTimeUtc { get; set; }

        /// <summary>
        /// Birth date-time (Local)
        /// </summary>
        public DateTime BirthDateTimeLocal { get; set; }

        /// <summary>
        /// Location information
        /// </summary>
        public LocationInfo Location { get; set; } = new();

        /// <summary>
        /// Moon's nakshatra information at birth
        /// </summary>
        public NakshatraInfo BirthNakshatra { get; set; } = new();

        /// <summary>
        /// Detail level of calculation (1-4)
        /// </summary>
        public int DetailLevel { get; set; }

        /// <summary>
        /// Number of years calculated
        /// </summary>
        public int YearsCalculated { get; set; }

        /// <summary>
        /// List of Mahadasha periods
        /// </summary>
        public List<MahadashaInfo> MahadashaPeriods { get; set; } = new();

        /// <summary>
        /// Current active Mahadasha
        /// </summary>
        public MahadashaInfo? CurrentMahadasha { get; set; }

        /// <summary>
        /// Current active Antardasha (if DetailLevel >= 2)
        /// </summary>
        public AntardashaInfo? CurrentAntardasha { get; set; }

        /// <summary>
        /// Current active Pratyantardasha (if DetailLevel >= 3)
        /// </summary>
        public PratyantardashaInfo? CurrentPratyantardasha { get; set; }

        /// <summary>
        /// Current active Sookshma (if DetailLevel >= 4)
        /// </summary>
        public SookshmaInfo? CurrentSookshma { get; set; }

        /// <summary>
        /// Total number of periods calculated (all levels)
        /// </summary>
        public int TotalPeriodsCount { get; set; }
    }
}
