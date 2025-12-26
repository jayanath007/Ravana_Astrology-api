using System.ComponentModel.DataAnnotations;

namespace Ravana_Astrology.Models.Requests
{
    /// <summary>
    /// Request model for Vimshottari Dasha calculation.
    /// </summary>
    public class VimshottariDashaRequest
    {
        /// <summary>
        /// Birth date
        /// </summary>
        [Required(ErrorMessage = "Birth date is required")]
        public DateTime BirthDate { get; set; }

        /// <summary>
        /// Birth time in HH:MM format (24-hour)
        /// </summary>
        [Required(ErrorMessage = "Birth time is required")]
        [RegularExpression(@"^([01]\d|2[0-3]):([0-5]\d)$",
            ErrorMessage = "Birth time must be in HH:MM format (24-hour)")]
        public string BirthTime { get; set; } = string.Empty;

        /// <summary>
        /// Latitude in degrees (-90 to +90)
        /// </summary>
        [Required(ErrorMessage = "Latitude is required")]
        [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90 degrees")]
        public double Latitude { get; set; }

        /// <summary>
        /// Longitude in degrees (-180 to +180)
        /// </summary>
        [Required(ErrorMessage = "Longitude is required")]
        [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180 degrees")]
        public double Longitude { get; set; }

        /// <summary>
        /// IANA timezone identifier (e.g., "Asia/Kolkata", "America/New_York")
        /// </summary>
        [Required(ErrorMessage = "TimeZoneId is required")]
        public string TimeZoneId { get; set; } = string.Empty;

        /// <summary>
        /// Maximum detail level to calculate (Mahadasha=1, Antardasha=2,
        /// Pratyantardasha=3, Sookshma=4). Default is 2 (Antardasha).
        /// </summary>
        [Range(1, 4, ErrorMessage = "Detail level must be between 1 (Mahadasha) and 4 (Sookshma)")]
        public int DetailLevel { get; set; } = 2;

        /// <summary>
        /// Number of years from birth to calculate. Default is 120 (full cycle).
        /// </summary>
        [Range(1, 120, ErrorMessage = "Years must be between 1 and 120")]
        public int YearsToCalculate { get; set; } = 120;
    }
}
