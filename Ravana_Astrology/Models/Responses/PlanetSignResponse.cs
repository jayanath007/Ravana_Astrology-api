namespace Ravana_Astrology.Models.Responses
{
    /// <summary>
    /// Simplified response containing planet name and zodiac sign number.
    /// </summary>
    public class PlanetSignResponse
    {
        public string Planet { get; set; } = string.Empty;

        /// <summary>
        /// Zodiac sign number (0=Aries, 1=Taurus, ..., 11=Pisces)
        /// </summary>
        public int Sign { get; set; }
    }
}
