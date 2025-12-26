using Microsoft.AspNetCore.Mvc;
using Ravana_Astrology.Models.Requests;
using Ravana_Astrology.Models.Responses;
using Ravana_Astrology.Services.Interfaces;

namespace Ravana_Astrology.Controllers
{
    /// <summary>
    /// Controller for Vimshottari Dasha calculations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class DashaController : ControllerBase
    {
        private readonly IVimshottariDashaService _dashaService;
        private readonly ILogger<DashaController> _logger;

        public DashaController(
            IVimshottariDashaService dashaService,
            ILogger<DashaController> logger)
        {
            _dashaService = dashaService;
            _logger = logger;
        }

        /// <summary>
        /// Calculate Vimshottari Dasha periods from birth data.
        /// Returns the complete dasha cycle with configurable detail levels:
        /// - Level 1: Mahadasha only (~9-10 periods)
        /// - Level 2: Mahadasha + Antardasha (~90 periods) [Default]
        /// - Level 3: + Pratyantardasha (~810 periods)
        /// - Level 4: + Sookshma (~7,290 periods) [Large response]
        /// </summary>
        /// <param name="request">Birth data and calculation parameters</param>
        /// <returns>Complete Vimshottari Dasha calculation with all requested period levels</returns>
        [HttpPost("vimshottari")]
        [ProducesResponseType(typeof(VimshottariDashaResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<VimshottariDashaResponse>> CalculateVimshottariDasha(
            [FromBody] VimshottariDashaRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _dashaService.CalculateVimshottariDasha(request);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request parameters");
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid Request",
                    Detail = ex.Message,
                    Status = StatusCodes.Status400BadRequest
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating Vimshottari Dasha");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ProblemDetails
                    {
                        Title = "Calculation Error",
                        Detail = "An error occurred while calculating Vimshottari Dasha",
                        Status = StatusCodes.Status500InternalServerError
                    });
            }
        }
    }
}
