namespace Ravana_Astrology.Enums
{
    /// <summary>
    /// Type of astrological calculation system.
    /// </summary>
    public enum CalculationType
    {
        /// <summary>
        /// Western/Tropical zodiac (aligned with seasons, default)
        /// </summary>
        Tropical = 0,

        /// <summary>
        /// Vedic/Sidereal zodiac (aligned with constellations, uses Lahiri Ayanamsa)
        /// Used in Kundli and Hindu astrology
        /// </summary>
        Vedic = 1
    }
}
