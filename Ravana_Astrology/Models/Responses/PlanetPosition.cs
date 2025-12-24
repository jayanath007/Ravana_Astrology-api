namespace Ravana_Astrology.Models.Responses
{
    /// <summary>
    /// Position and details of a planet in the birth chart.
    /// </summary>
    public class PlanetPosition
    {
        public string Planet { get; set; } = string.Empty;
        public double EclipticLongitude { get; set; }
        public ZodiacInfo ZodiacPosition { get; set; } = new();
        public int House { get; set; }
        public double Latitude { get; set; }
        public double Distance { get; set; }
        public double Speed { get; set; }
    }
}
