namespace Ravana_Astrology.Models.Responses
{
    /// <summary>
    /// Mahadasha (1st level) dasha period information.
    /// </summary>
    public class MahadashaInfo : DashaPeriodInfo
    {
        /// <summary>
        /// Whether this Mahadasha started at birth with a balance period
        /// </summary>
        public bool IsBalancePeriod { get; set; }

        /// <summary>
        /// Remaining years at birth (for balance period)
        /// </summary>
        public double? BalanceYears { get; set; }

        /// <summary>
        /// Sub-periods (Antardasha) within this Mahadasha.
        /// Only populated if DetailLevel >= 2.
        /// </summary>
        public List<AntardashaInfo> AntardashaPeriods { get; set; } = new();
    }
}
