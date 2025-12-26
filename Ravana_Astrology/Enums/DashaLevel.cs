namespace Ravana_Astrology.Enums
{
    /// <summary>
    /// Levels of Vimshottari Dasha periods.
    /// </summary>
    public enum DashaLevel
    {
        /// <summary>
        /// Mahadasha - Main period (7-20 years)
        /// </summary>
        Mahadasha = 1,

        /// <summary>
        /// Antardasha - Sub-period within Mahadasha
        /// </summary>
        Antardasha = 2,

        /// <summary>
        /// Pratyantardasha - Sub-sub-period within Antardasha
        /// </summary>
        Pratyantardasha = 3,

        /// <summary>
        /// Sookshma - Micro-period within Pratyantardasha (days to weeks)
        /// </summary>
        Sookshma = 4
    }
}
