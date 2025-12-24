namespace Ravana_Astrology.Models.Responses
{
    /// <summary>
    /// Position of an astrological house cusp.
    /// </summary>
    public class HousePosition
    {
        public int HouseNumber { get; set; }
        public double Cusp { get; set; }
        public ZodiacInfo ZodiacPosition { get; set; } = new();
    }
}
