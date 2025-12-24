namespace Ravana_Astrology.Configuration
{
    /// <summary>
    /// Configuration options for Swiss Ephemeris calculations.
    /// </summary>
    public class SwissEphemerisOptions
    {
        /// <summary>
        /// Path to ephemeris data files. Set to null to use built-in Moshier ephemeris.
        /// </summary>
        public string? EphemerisPath { get; set; }

        /// <summary>
        /// Default house system code (P=Placidus, W=WholeSign, E=Equal, K=Koch, C=Campanus, R=Regiomontanus).
        /// </summary>
        public string DefaultHouseSystem { get; set; } = "P";

        /// <summary>
        /// Whether to use high precision external ephemeris files.
        /// </summary>
        public bool UseHighPrecision { get; set; } = false;
    }
}
