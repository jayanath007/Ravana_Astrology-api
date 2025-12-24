namespace Ravana_Astrology.Models.Responses
{
    /// <summary>
    /// Birth location information.
    /// </summary>
    public class LocationInfo
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string TimeZone { get; set; } = string.Empty;
    }
}
