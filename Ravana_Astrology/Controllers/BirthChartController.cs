using Microsoft.AspNetCore.Mvc;
using Ravana_Astrology.Enums;
using Ravana_Astrology.Models.Requests;
using Ravana_Astrology.Models.Responses;
using Ravana_Astrology.Services.Interfaces;

namespace Ravana_Astrology.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BirthChartController : ControllerBase
    {
        private readonly IAstrologyCalculationService _astrologyService;
        private readonly ILogger<BirthChartController> _logger;

        public BirthChartController(
            IAstrologyCalculationService astrologyService,
            ILogger<BirthChartController> logger)
        {
            _astrologyService = astrologyService;
            _logger = logger;
        }

        /// <summary>
        /// Calculate a complete birth chart based on birth date, time, and location.
        /// </summary>
        /// <param name="request">Birth chart calculation request</param>
        /// <returns>Complete birth chart with planetary positions and houses</returns>
        [HttpPost("calculate")]
        [ProducesResponseType(typeof(BirthChartResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<BirthChartResponse>> CalculateBirthChart(
            [FromBody] BirthChartRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _astrologyService.CalculateBirthChart(request);
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
                _logger.LogError(ex, "Error calculating birth chart");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ProblemDetails
                    {
                        Title = "Calculation Error",
                        Detail = "An error occurred while calculating the birth chart",
                        Status = StatusCodes.Status500InternalServerError
                    });
            }
        }

        /// <summary>
        /// Calculate planetary positions and return a simplified list of planets with their zodiac signs.
        /// </summary>
        /// <param name="request">Simplified planet sign request</param>
        /// <returns>Simplified list of planets with their zodiac signs</returns>
        [HttpPost("planet-signs")]
        [ProducesResponseType(typeof(List<PlanetSignResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<PlanetSignResponse>>> GetPlanetSigns(
            [FromBody] PlanetSignRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Convert to full request with default values
                var fullRequest = new BirthChartRequest
                {
                    BirthDate = request.BirthDate,
                    BirthTime = request.BirthTime,
                    Latitude = request.Latitude,
                    Longitude = request.Longitude,
                    TimeZoneId = request.TimeZoneId,
                    HouseSystem = Enums.HouseSystem.WholeSign, // Default for Vedic
                    IncludeTrueNode = false, // Use Mean Node
                    CalculationType = Enums.CalculationType.Vedic // Use Vedic/Kundli system
                };

                var result = await _astrologyService.CalculateBirthChart(fullRequest);

                var planetSigns = result.Planets.Select(p => new PlanetSignResponse
                {
                    Planet = Utilities.AstrologyHelper.GetSinhalaPlanetName(p.Planet),
                    Sign = (int)p.ZodiacPosition.Sign
                }).ToList();

                return Ok(planetSigns);
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
                _logger.LogError(ex, "Error calculating planet signs");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ProblemDetails
                    {
                        Title = "Calculation Error",
                        Detail = "An error occurred while calculating planet signs",
                        Status = StatusCodes.Status500InternalServerError
                    });
            }
        }

        /// <summary>
        /// Calculate planetary Navāṁśa positions (D9 divisional chart) and return a
        /// simplified list of planets with their Navāṁśa signs.
        /// </summary>
        /// <param name="request">Simplified planet sign request</param>
        /// <returns>Simplified list of planets with their Navāṁśa zodiac signs</returns>
        [HttpPost("navamsa-signs")]
        [ProducesResponseType(typeof(List<PlanetSignResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<PlanetSignResponse>>> GetNavamsaSigns(
            [FromBody] PlanetSignRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Convert to full request with default values
                var fullRequest = new BirthChartRequest
                {
                    BirthDate = request.BirthDate,
                    BirthTime = request.BirthTime,
                    Latitude = request.Latitude,
                    Longitude = request.Longitude,
                    TimeZoneId = request.TimeZoneId,
                    HouseSystem = Enums.HouseSystem.WholeSign, // Default for Vedic
                    IncludeTrueNode = false, // Use Mean Node
                    CalculationType = Enums.CalculationType.Vedic // Use Vedic/Kundli system
                };

                var result = await _astrologyService.CalculateBirthChart(fullRequest);

                // Calculate Navamsa positions for each planet
                var navamsaSigns = result.Planets.Select(p => new PlanetSignResponse
                {
                    Planet = Utilities.AstrologyHelper.GetSinhalaPlanetName(p.Planet),
                    Sign = (int)Utilities.AstrologyHelper.CalculateNavamsaSign(p.EclipticLongitude)
                }).ToList();

                return Ok(navamsaSigns);
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
                _logger.LogError(ex, "Error calculating Navamsa signs");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ProblemDetails
                    {
                        Title = "Calculation Error",
                        Detail = "An error occurred while calculating Navamsa signs",
                        Status = StatusCodes.Status500InternalServerError
                    });
            }
        }

        /// <summary>
        /// Get the Ascendant (Lagna) zodiac sign based on birth date, time, and location.
        /// </summary>
        /// <param name="request">Simplified planet sign request</param>
        /// <returns>Ascendant zodiac sign in Sinhala</returns>
        [HttpPost("ascendant")]
        [ProducesResponseType(typeof(AscendantSignResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AscendantSignResponse>> GetAscendantSign(
            [FromBody] PlanetSignRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Convert to full request with default values
                var fullRequest = new BirthChartRequest
                {
                    BirthDate = request.BirthDate,
                    BirthTime = request.BirthTime,
                    Latitude = request.Latitude,
                    Longitude = request.Longitude,
                    TimeZoneId = request.TimeZoneId,
                    HouseSystem = Enums.HouseSystem.WholeSign, // Default for Vedic
                    IncludeTrueNode = false, // Use Mean Node
                    CalculationType = Enums.CalculationType.Vedic // Use Vedic/Kundli system
                };

                var result = await _astrologyService.CalculateBirthChart(fullRequest);

                var ascendantSign = new AscendantSignResponse
                {
                    Sign = (int)result.Ascendant.ZodiacPosition.Sign
                };

                return Ok(ascendantSign);
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
                _logger.LogError(ex, "Error calculating ascendant sign");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ProblemDetails
                    {
                        Title = "Calculation Error",
                        Detail = "An error occurred while calculating the ascendant sign",
                        Status = StatusCodes.Status500InternalServerError
                    });
            }
        }

        /// <summary>
        /// Get the Ascendant (Lagna) Navāṁśa zodiac sign based on birth date, time, and location.
        /// Returns the D9 divisional chart position of the ascendant.
        /// </summary>
        /// <param name="request">Simplified planet sign request</param>
        /// <returns>Navāṁśa ascendant zodiac sign</returns>
        [HttpPost("navamsa-ascendant")]
        [ProducesResponseType(typeof(AscendantSignResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AscendantSignResponse>> GetNavamsaAscendantSign(
            [FromBody] PlanetSignRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Convert to full request with default values
                var fullRequest = new BirthChartRequest
                {
                    BirthDate = request.BirthDate,
                    BirthTime = request.BirthTime,
                    Latitude = request.Latitude,
                    Longitude = request.Longitude,
                    TimeZoneId = request.TimeZoneId,
                    HouseSystem = Enums.HouseSystem.WholeSign, // Default for Vedic
                    IncludeTrueNode = false, // Use Mean Node
                    CalculationType = Enums.CalculationType.Vedic // Use Vedic/Kundli system
                };

                var result = await _astrologyService.CalculateBirthChart(fullRequest);

                // Calculate Navamsa position of the ascendant
                var navamsaAscendantSign = new AscendantSignResponse
                {
                    Sign = (int)Utilities.AstrologyHelper.CalculateNavamsaSign(result.Ascendant.Degree)
                };

                return Ok(navamsaAscendantSign);
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
                _logger.LogError(ex, "Error calculating Navamsa ascendant sign");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ProblemDetails
                    {
                        Title = "Calculation Error",
                        Detail = "An error occurred while calculating the Navamsa ascendant sign",
                        Status = StatusCodes.Status500InternalServerError
                    });
            }
        }

        /// <summary>
        /// Get the list of supported house systems.
        /// </summary>
        /// <returns>List of house system names</returns>
        [HttpGet("supported-house-systems")]
        [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<string>> GetSupportedHouseSystems()
        {
            return Ok(Enum.GetNames<HouseSystem>());
        }
    }
}
