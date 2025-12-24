namespace Ravana_Astrology.Models.Responses
{
    /// <summary>
    /// Ascendant or Midheaven position information.
    /// </summary>
    public class AscendantInfo
    {
        public double Degree { get; set; }
        public ZodiacInfo ZodiacPosition { get; set; } = new();
    }
}
