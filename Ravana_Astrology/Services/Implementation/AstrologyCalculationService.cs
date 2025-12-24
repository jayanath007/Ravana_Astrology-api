using Microsoft.Extensions.Options;
using Ravana_Astrology.Configuration;
using Ravana_Astrology.Enums;
using Ravana_Astrology.Models.Requests;
using Ravana_Astrology.Models.Responses;
using Ravana_Astrology.Services.Interfaces;
using Ravana_Astrology.Utilities;
using SwissEphNet;
using TimeZoneConverter;

namespace Ravana_Astrology.Services.Implementation
{
    /// <summary>
    /// Service implementation for astrological calculations using Swiss Ephemeris.
    /// </summary>
    public class AstrologyCalculationService : IAstrologyCalculationService, IDisposable
    {
        private readonly SwissEph _swissEph;
        private readonly ILogger<AstrologyCalculationService> _logger;
        private readonly SwissEphemerisOptions _options;

        public AstrologyCalculationService(
            IOptions<SwissEphemerisOptions> options,
            ILogger<AstrologyCalculationService> logger)
        {
            _options = options.Value;
            _logger = logger;
            _swissEph = new SwissEph();

            // Set ephemeris path (null for Moshier built-in ephemeris)
            if (!string.IsNullOrEmpty(_options.EphemerisPath))
            {
                _swissEph.swe_set_ephe_path(_options.EphemerisPath);
            }
        }

        public async Task<BirthChartResponse> CalculateBirthChart(BirthChartRequest request)
        {
            try
            {
                // 1. Convert local birth time to UTC
                var localDateTime = CombineDateAndTime(request.BirthDate, request.BirthTime);
                var utcDateTime = ConvertToUtc(localDateTime, request.TimeZoneId);

                _logger.LogInformation("Calculating birth chart for {Date} at {Lat}, {Lon}",
                    utcDateTime, request.Latitude, request.Longitude);

                // 2. Calculate Julian Day
                var julianDay = CalculateJulianDay(utcDateTime);

                // 3. Calculate Ayanamsa for Vedic/Sidereal calculations
                double ayanamsa = 0.0;
                if (request.CalculationType == Enums.CalculationType.Vedic)
                {
                    ayanamsa = CalculateAyanamsa(julianDay);
                    _logger.LogInformation("Using Vedic/Sidereal calculation with Lahiri Ayanamsa: {Ayanamsa}°", ayanamsa);
                }

                // 4. Determine house system
                var houseSystem = request.HouseSystem ?? AstrologyHelper.CodeToHouseSystem(_options.DefaultHouseSystem[0]);
                var houseSystemCode = AstrologyHelper.HouseSystemToCode(houseSystem);

                // 5. Calculate houses and angles
                var (houses, ascendant, midheaven) = CalculateHouses(
                    julianDay,
                    request.Latitude,
                    request.Longitude,
                    houseSystemCode,
                    ayanamsa);

                // 6. Calculate planetary positions
                var nodeToUse = request.IncludeTrueNode ? Planet.TrueNode : Planet.MeanNode;
                var planets = CalculatePlanetaryPositions(julianDay, houses, nodeToUse, ayanamsa);

                // 6. Build response
                return new BirthChartResponse
                {
                    BirthDateTimeUtc = utcDateTime,
                    BirthDateTimeLocal = localDateTime,
                    JulianDay = julianDay,
                    Location = new LocationInfo
                    {
                        Latitude = request.Latitude,
                        Longitude = request.Longitude,
                        TimeZone = request.TimeZoneId
                    },
                    HouseSystem = houseSystem.ToString(),
                    Planets = planets,
                    Houses = houses,
                    Ascendant = ascendant,
                    Midheaven = midheaven
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating birth chart");
                throw;
            }
        }

        private DateTime CombineDateAndTime(DateTime date, string time)
        {
            var timeParts = time.Split(':');
            var hours = int.Parse(timeParts[0]);
            var minutes = int.Parse(timeParts[1]);

            return new DateTime(date.Year, date.Month, date.Day, hours, minutes, 0);
        }

        private DateTime ConvertToUtc(DateTime localDateTime, string timeZoneId)
        {
            try
            {
                // Convert IANA timezone to TimeZoneInfo
                var timeZoneInfo = TZConvert.GetTimeZoneInfo(timeZoneId);
                return TimeZoneInfo.ConvertTimeToUtc(localDateTime, timeZoneInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Invalid timezone ID: {TimeZoneId}", timeZoneId);
                throw new ArgumentException($"Invalid timezone ID: {timeZoneId}", nameof(timeZoneId));
            }
        }

        private double CalculateJulianDay(DateTime utcDateTime)
        {
            var year = utcDateTime.Year;
            var month = utcDateTime.Month;
            var day = utcDateTime.Day;
            var hour = utcDateTime.Hour + (utcDateTime.Minute / 60.0) + (utcDateTime.Second / 3600.0);

            return _swissEph.swe_julday(year, month, day, hour, SwissEph.SE_GREG_CAL);
        }

        private double CalculateAyanamsa(double julianDay)
        {
            // Use Lahiri (Chitrapaksha) Ayanamsa - most commonly used in Vedic astrology
            _swissEph.swe_set_sid_mode(SwissEph.SE_SIDM_LAHIRI, 0, 0);
            return _swissEph.swe_get_ayanamsa_ut(julianDay);
        }

        private (List<HousePosition> Houses, AscendantInfo Ascendant, AscendantInfo Midheaven) CalculateHouses(
            double julianDay,
            double latitude,
            double longitude,
            char houseSystemCode,
            double ayanamsa = 0.0)
        {
            var cusps = new double[13]; // cusps[1..12] are the house cusps
            var ascmc = new double[10]; // ascmc[0]=Ascendant, ascmc[1]=MC, etc.
            var error = string.Empty;

            var result = _swissEph.swe_houses_ex(
                julianDay,
                SwissEph.SEFLG_SWIEPH,
                latitude,
                longitude,
                houseSystemCode,
                cusps,
                ascmc);

            if (result < 0)
            {
                _logger.LogError("Error calculating houses: {Error}", error);
                throw new Exception($"Failed to calculate houses: {error}");
            }

            var houses = new List<HousePosition>();
            for (int i = 1; i <= 12; i++)
            {
                var cuspLongitude = cusps[i] - ayanamsa; // Apply Ayanamsa for sidereal
                houses.Add(new HousePosition
                {
                    HouseNumber = i,
                    Cusp = cuspLongitude,
                    ZodiacPosition = AstrologyHelper.ConvertToZodiacInfo(cuspLongitude)
                });
            }

            var ascendantLongitude = ascmc[0] - ayanamsa; // Apply Ayanamsa
            var ascendant = new AscendantInfo
            {
                Degree = ascendantLongitude,
                ZodiacPosition = AstrologyHelper.ConvertToZodiacInfo(ascendantLongitude)
            };

            var midheavenLongitude = ascmc[1] - ayanamsa; // Apply Ayanamsa
            var midheaven = new AscendantInfo
            {
                Degree = midheavenLongitude,
                ZodiacPosition = AstrologyHelper.ConvertToZodiacInfo(midheavenLongitude)
            };

            return (houses, ascendant, midheaven);
        }

        private List<PlanetPosition> CalculatePlanetaryPositions(
            double julianDay,
            List<HousePosition> houses,
            Planet nodeType,
            double ayanamsa = 0.0)
        {
            var planets = new List<PlanetPosition>();

            // Calculate traditional planets
            var planetsToCalculate = new[]
            {
                Planet.Sun,
                Planet.Moon,
                Planet.Mercury,
                Planet.Venus,
                Planet.Mars,
                Planet.Jupiter,
                Planet.Saturn,
                nodeType // Rahu (either Mean or True Node)
            };

            foreach (var planet in planetsToCalculate)
            {
                var position = CalculatePlanetPosition(julianDay, planet, houses, ayanamsa);
                planets.Add(position);
            }

            // Calculate Ketu (South Node) as opposite of Rahu
            var rahu = planets.Last(); // Last one is Rahu
            var ketuLongitude = AstrologyHelper.CalculateKetu(rahu.EclipticLongitude);

            planets.Add(new PlanetPosition
            {
                Planet = "Ketu",
                EclipticLongitude = ketuLongitude,
                ZodiacPosition = AstrologyHelper.ConvertToZodiacInfo(ketuLongitude),
                House = DetermineHouse(ketuLongitude, houses),
                Latitude = -rahu.Latitude, // Opposite latitude
                Distance = rahu.Distance,
                Speed = -rahu.Speed // Opposite speed
            });

            return planets;
        }

        private PlanetPosition CalculatePlanetPosition(
            double julianDay,
            Planet planet,
            List<HousePosition> houses,
            double ayanamsa = 0.0)
        {
            var xx = new double[6];
            var error = string.Empty;
            var planetId = AstrologyHelper.PlanetToSwissEphId(planet);

            var result = _swissEph.swe_calc_ut(
                julianDay,
                planetId,
                SwissEph.SEFLG_SWIEPH | SwissEph.SEFLG_SPEED,
                xx,
                ref error);

            if (result < 0)
            {
                _logger.LogError("Error calculating {Planet}: {Error}", planet, error);
                throw new Exception($"Failed to calculate {planet}: {error}");
            }

            var longitude = xx[0] - ayanamsa;  // Ecliptic longitude - apply Ayanamsa for sidereal
            var latitude = xx[1];   // Ecliptic latitude
            var distance = xx[2];   // Distance in AU
            var speedLon = xx[3];   // Speed in longitude (degrees/day)

            return new PlanetPosition
            {
                Planet = AstrologyHelper.GetPlanetName(planet),
                EclipticLongitude = longitude,
                ZodiacPosition = AstrologyHelper.ConvertToZodiacInfo(longitude),
                House = DetermineHouse(longitude, houses),
                Latitude = latitude,
                Distance = distance,
                Speed = speedLon
            };
        }

        private int DetermineHouse(double planetLongitude, List<HousePosition> houses)
        {
            var normalizedPlanetLon = AstrologyHelper.NormalizeDegrees(planetLongitude);

            for (int i = 0; i < 12; i++)
            {
                var currentHouse = houses[i];
                var nextHouse = houses[(i + 1) % 12];

                var currentCusp = currentHouse.Cusp;
                var nextCusp = nextHouse.Cusp;

                // Handle wraparound at 360°/0°
                if (currentCusp > nextCusp)
                {
                    // House crosses 0° Aries
                    if (normalizedPlanetLon >= currentCusp || normalizedPlanetLon < nextCusp)
                    {
                        return currentHouse.HouseNumber;
                    }
                }
                else
                {
                    // Normal case
                    if (normalizedPlanetLon >= currentCusp && normalizedPlanetLon < nextCusp)
                    {
                        return currentHouse.HouseNumber;
                    }
                }
            }

            // Fallback (should not happen)
            return 1;
        }

        public void Dispose()
        {
            _swissEph?.Dispose();
        }
    }
}
