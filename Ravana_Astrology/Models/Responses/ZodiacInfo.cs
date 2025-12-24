using Ravana_Astrology.Enums;

namespace Ravana_Astrology.Models.Responses
{
    /// <summary>
    /// Zodiac position information for a celestial body.
    /// </summary>
    public class ZodiacInfo
    {
        public Chart Sign { get; set; }
        public double DegreeInSign { get; set; }
        public int Degree { get; set; }
        public int Minutes { get; set; }
        public int Seconds { get; set; }
        public string FormattedPosition { get; set; } = string.Empty;
    }
}
