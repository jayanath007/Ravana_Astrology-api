namespace Ravana_Astrology.Models.Responses
{
    /// <summary>
    /// Pratyantardasha (3rd level) dasha period information.
    /// </summary>
    public class PratyantardashaInfo : DashaPeriodInfo
    {
        /// <summary>
        /// Sub-periods (Sookshma) within this Pratyantardasha.
        /// Only populated if DetailLevel >= 4.
        /// </summary>
        public List<SookshmaInfo> SookshmaPeriods { get; set; } = new();
    }
}
