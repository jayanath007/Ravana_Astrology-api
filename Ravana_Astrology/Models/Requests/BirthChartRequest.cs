using System.ComponentModel.DataAnnotations;
using Ravana_Astrology.Enums;

namespace Ravana_Astrology.Models.Requests
{
    /// <summary>
    /// Request model for birth chart calculation.
    /// </summary>
    public class BirthChartRequest
    {
        [Required]
        public DateTime BirthDate { get; set; }

        [Required]
        [RegularExpression(@"^([01]\d|2[0-3]):([0-5]\d)$", ErrorMessage = "Birth time must be in HH:MM format (24-hour)")]
        public string BirthTime { get; set; } = string.Empty;

        [Required]
        [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90 degrees")]
        public double Latitude { get; set; }

        [Required]
        [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180 degrees")]
        public double Longitude { get; set; }

        [Required]
        public string TimeZoneId { get; set; } = string.Empty;

        public HouseSystem? HouseSystem { get; set; }

        public bool IncludeTrueNode { get; set; } = false;

        public CalculationType CalculationType { get; set; } = CalculationType.Vedic;
    }
}
