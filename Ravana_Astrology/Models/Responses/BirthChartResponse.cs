namespace Ravana_Astrology.Models.Responses
{
    /// <summary>
    /// Complete birth chart calculation response.
    /// </summary>
    public class BirthChartResponse
    {
        public DateTime BirthDateTimeUtc { get; set; }
        public DateTime BirthDateTimeLocal { get; set; }
        public double JulianDay { get; set; }
        public LocationInfo Location { get; set; } = new();
        public string HouseSystem { get; set; } = string.Empty;
        public List<PlanetPosition> Planets { get; set; } = new();
        public List<HousePosition> Houses { get; set; } = new();
        public AscendantInfo Ascendant { get; set; } = new();
        public AscendantInfo Midheaven { get; set; } = new();
    }
}
