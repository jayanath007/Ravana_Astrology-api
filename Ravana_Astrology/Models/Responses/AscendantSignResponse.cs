namespace Ravana_Astrology.Models.Responses
{
    /// <summary>
    /// Response containing the Ascendant (Lagna) zodiac sign number.
    /// </summary>
    public class AscendantSignResponse
    {
        /// <summary>
        /// Zodiac sign number (0=Aries, 1=Taurus, ..., 11=Pisces)
        /// </summary>
        public int Sign { get; set; }
    }
}
