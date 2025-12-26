namespace Ravana_Astrology.Models.Responses
{
    /// <summary>
    /// Antardasha (2nd level) dasha period information.
    /// </summary>
    public class AntardashaInfo : DashaPeriodInfo
    {
        /// <summary>
        /// Sub-periods (Pratyantardasha) within this Antardasha.
        /// Only populated if DetailLevel >= 3.
        /// </summary>
        public List<PratyantardashaInfo> PratyantardashaPeriods { get; set; } = new();
    }
}
