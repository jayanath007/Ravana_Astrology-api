using Ravana_Astrology.Models.Requests;
using Ravana_Astrology.Models.Responses;

namespace Ravana_Astrology.Services.Interfaces
{
    /// <summary>
    /// Service for calculating Vimshottari Dasha periods.
    /// </summary>
    public interface IVimshottariDashaService
    {
        /// <summary>
        /// Calculate complete Vimshottari Dasha for the given birth data.
        /// </summary>
        /// <param name="request">Birth data and calculation parameters</param>
        /// <returns>Complete Vimshottari Dasha response with all requested period levels</returns>
        Task<VimshottariDashaResponse> CalculateVimshottariDasha(VimshottariDashaRequest request);
    }
}
