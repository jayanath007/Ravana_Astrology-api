using Ravana_Astrology.Models.Requests;
using Ravana_Astrology.Models.Responses;

namespace Ravana_Astrology.Services.Interfaces
{
    /// <summary>
    /// Service for calculating birth charts and astrological data.
    /// </summary>
    public interface IAstrologyCalculationService
    {
        /// <summary>
        /// Calculate a complete birth chart for the given birth data.
        /// </summary>
        Task<BirthChartResponse> CalculateBirthChart(BirthChartRequest request);

        /// <summary>
        /// Calculate when each planet will next change zodiac signs.
        /// Returns the next sign change date for all 9 planets (Sun, Moon, Mars, Mercury, Jupiter, Venus, Saturn, Rahu, Ketu).
        /// </summary>
        Task<List<PlanetSignChangeResponse>> CalculatePlanetSignChanges(PlanetSignRequest request);
    }
}
