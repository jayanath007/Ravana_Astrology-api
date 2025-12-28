namespace Ravana_Astrology.Models.Responses
{
    /// <summary>
    /// Response containing planet name, current sign, and the dates when it last changed and will next change signs.
    /// </summary>
    public class PlanetSignChangeResponse
    {
        /// <summary>
        /// Planet name (Sinhala abbreviated form)
        /// </summary>
        public string Planet { get; set; } = string.Empty;

        /// <summary>
        /// Current zodiac sign number (1=Aries, 2=Taurus, ..., 12=Pisces)
        /// </summary>
        public int Sign { get; set; }

        /// <summary>
        /// Date and time when the planet will next change to a different zodiac sign
        /// </summary>
        public DateTime NextSignChangeDate { get; set; }

        /// <summary>
        /// Date and time when the planet last changed from a different zodiac sign to the current sign
        /// </summary>
        public DateTime LastSignChangeDate { get; set; }
    }
}
