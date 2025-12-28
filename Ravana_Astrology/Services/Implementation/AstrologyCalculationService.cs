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

        public async Task<List<PlanetSignChangeResponse>> CalculatePlanetSignChanges(PlanetSignRequest request)
        {
            try
            {
                // 1. Convert local birth time to UTC
                var localDateTime = CombineDateAndTime(request.BirthDate, request.BirthTime);
                var utcDateTime = ConvertToUtc(localDateTime, request.TimeZoneId);

                _logger.LogInformation("Calculating planet sign changes for {Date} at {Lat}, {Lon}",
                    utcDateTime, request.Latitude, request.Longitude);

                // 2. Calculate Julian Day
                var julianDay = CalculateJulianDay(utcDateTime);

                // 3. Calculate Ayanamsa for Vedic/Sidereal calculations
                var ayanamsa = CalculateAyanamsa(julianDay);

                // 4. Get current planetary positions
                var planetsToCalculate = new[]
                {
                    Planet.Sun,
                    Planet.Moon,
                    Planet.Mercury,
                    Planet.Venus,
                    Planet.Mars,
                    Planet.Jupiter,
                    Planet.Saturn,
                    Planet.MeanNode // Rahu
                };

                var results = new List<PlanetSignChangeResponse>();

                // 5. Calculate sign change for each planet
                foreach (var planet in planetsToCalculate)
                {
                    var (longitude, speed) = GetPlanetPositionAtJulianDay(julianDay, planet, ayanamsa);
                    var currentSign = (int)AstrologyHelper.GetZodiacSign(longitude);

                    // Find when this planet will next change signs
                    var nextChangeDate = await FindSignChangeDate(
                        julianDay,
                        planet,
                        longitude,
                        speed,
                        ayanamsa,
                        request.TimeZoneId);

                    // Find when this planet last changed signs
                    var lastChangeDate = await FindLastSignChangeDate(
                        julianDay,
                        planet,
                        longitude,
                        speed,
                        ayanamsa,
                        request.TimeZoneId);

                    results.Add(new PlanetSignChangeResponse
                    {
                        Planet = AstrologyHelper.GetSinhalaPlanetName(AstrologyHelper.GetPlanetName(planet)),
                        Sign = currentSign,
                        NextSignChangeDate = nextChangeDate,
                        LastSignChangeDate = lastChangeDate
                    });
                }

                // 6. Calculate Ketu (opposite of Rahu)
                var rahuResult = results.Last();
                var rahuIndex = results.Count - 1;
                var (rahuLon, rahuSpeed) = GetPlanetPositionAtJulianDay(julianDay, Planet.MeanNode, ayanamsa);
                var ketuLon = AstrologyHelper.CalculateKetu(rahuLon);
                var ketuSpeed = -rahuSpeed; // Opposite speed
                var ketuSign = (int)AstrologyHelper.GetZodiacSign(ketuLon);

                var ketuNextChangeDate = await FindSignChangeDate(
                    julianDay,
                    Planet.MeanNode, // Use same planet but with Ketu longitude
                    ketuLon,
                    ketuSpeed,
                    ayanamsa,
                    request.TimeZoneId,
                    isKetu: true);

                var ketuLastChangeDate = await FindLastSignChangeDate(
                    julianDay,
                    Planet.MeanNode, // Use same planet but with Ketu longitude
                    ketuLon,
                    ketuSpeed,
                    ayanamsa,
                    request.TimeZoneId,
                    isKetu: true);

                results.Add(new PlanetSignChangeResponse
                {
                    Planet = "කේ", // Ketu in Sinhala
                    Sign = ketuSign,
                    NextSignChangeDate = ketuNextChangeDate,
                    LastSignChangeDate = ketuLastChangeDate
                });

                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating planet sign changes");
                throw;
            }
        }

        private async Task<DateTime> FindSignChangeDate(
            double currentJulianDay,
            Planet planet,
            double currentLongitude,
            double currentSpeed,
            double ayanamsa,
            string timeZoneId,
            bool isKetu = false)
        {
            const double TIME_PRECISION_MINUTES = 1.0;
            const double TIME_PRECISION_JD = TIME_PRECISION_MINUTES / (24.0 * 60.0); // 1 minute in JD
            const int MAX_SEARCH_DAYS = 400;

            // Determine if retrograde
            bool isRetrograde = currentSpeed < 0;

            // Get target boundary
            double targetBoundary = AstrologyHelper.GetNextSignBoundary(currentLongitude, isRetrograde);

            // Calculate degrees to boundary (handling 360° wraparound)
            double degreesToBoundary = CalculateDegreesToBoundary(currentLongitude, targetBoundary, isRetrograde);

            // Get search window multiplier based on planet type
            double multiplier = GetSearchWindowMultiplier(planet, Math.Abs(currentSpeed));

            // Calculate initial estimate
            double estimatedDays = Math.Abs(currentSpeed) > 0.001
                ? degreesToBoundary / Math.Abs(currentSpeed)
                : MAX_SEARCH_DAYS;

            double searchWindowDays = Math.Min(estimatedDays * multiplier, MAX_SEARCH_DAYS);

            // Set search range
            double startJD = currentJulianDay;
            double endJD = currentJulianDay + searchWindowDays;

            // Binary search for sign change
            int currentSign = (int)(AstrologyHelper.NormalizeDegrees(currentLongitude) / 30.0);

            // First check if sign change happens within window
            var (endLongitude, _) = isKetu
                ? GetKetuPositionAtJulianDay(endJD, ayanamsa)
                : GetPlanetPositionAtJulianDay(endJD, planet, ayanamsa);
            int endSign = (int)(AstrologyHelper.NormalizeDegrees(endLongitude) / 30.0);

            if (currentSign == endSign)
            {
                // No sign change found within search window
                // Return maximum date
                var timeZoneInfo = TZConvert.GetTimeZoneInfo(timeZoneId);
                var utcDateTime = JulianDayToDateTime(endJD);
                return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, timeZoneInfo);
            }

            // Binary search to find exact crossing point
            while ((endJD - startJD) > TIME_PRECISION_JD)
            {
                double midJD = (startJD + endJD) / 2.0;
                var (midLongitude, _) = isKetu
                    ? GetKetuPositionAtJulianDay(midJD, ayanamsa)
                    : GetPlanetPositionAtJulianDay(midJD, planet, ayanamsa);
                int midSign = (int)(AstrologyHelper.NormalizeDegrees(midLongitude) / 30.0);

                if (midSign != currentSign)
                {
                    // Sign change occurred before midpoint
                    endJD = midJD;
                }
                else
                {
                    // Sign change occurs after midpoint
                    startJD = midJD;
                }
            }

            // Convert result to DateTime in user's timezone
            var timeZone = TZConvert.GetTimeZoneInfo(timeZoneId);
            var resultUtc = JulianDayToDateTime(endJD);
            var resultLocal = TimeZoneInfo.ConvertTimeFromUtc(resultUtc, timeZone);

            return resultLocal;
        }

        private async Task<DateTime> FindLastSignChangeDate(
            double currentJulianDay,
            Planet planet,
            double currentLongitude,
            double currentSpeed,
            double ayanamsa,
            string timeZoneId,
            bool isKetu = false)
        {
            const double TIME_PRECISION_MINUTES = 1.0;
            const double TIME_PRECISION_JD = TIME_PRECISION_MINUTES / (24.0 * 60.0); // 1 minute in JD
            const int MAX_SEARCH_DAYS = 400;

            // Determine if retrograde
            bool isRetrograde = currentSpeed < 0;

            // For last sign change, we need to find when the planet entered the current sign
            // For direct motion: planet entered from previous sign (crossing start boundary)
            // For retrograde motion: planet entered from next sign (crossing end boundary)
            double targetBoundary;
            int currentSign = (int)(AstrologyHelper.NormalizeDegrees(currentLongitude) / 30.0);

            if (isRetrograde)
            {
                // Moving backward - entered current sign by crossing the end boundary
                targetBoundary = (currentSign + 1) * 30.0;
            }
            else
            {
                // Moving forward - entered current sign by crossing the start boundary
                targetBoundary = currentSign * 30.0;
            }

            // Calculate degrees from boundary
            double degreesFromBoundary = CalculateDegreesToBoundary(targetBoundary, currentLongitude, !isRetrograde);

            // Get search window multiplier based on planet type
            double multiplier = GetSearchWindowMultiplier(planet, Math.Abs(currentSpeed));

            // Calculate initial estimate
            double estimatedDays = Math.Abs(currentSpeed) > 0.001
                ? degreesFromBoundary / Math.Abs(currentSpeed)
                : MAX_SEARCH_DAYS;

            double searchWindowDays = Math.Min(estimatedDays * multiplier, MAX_SEARCH_DAYS);

            // Set search range (searching backward in time)
            double endJD = currentJulianDay;
            double startJD = currentJulianDay - searchWindowDays;

            // First check if sign change happened within window
            var (startLongitude, _) = isKetu
                ? GetKetuPositionAtJulianDay(startJD, ayanamsa)
                : GetPlanetPositionAtJulianDay(startJD, planet, ayanamsa);
            int startSign = (int)(AstrologyHelper.NormalizeDegrees(startLongitude) / 30.0);

            if (currentSign == startSign)
            {
                // No sign change found within search window
                // Return minimum date
                var timeZoneInfo = TZConvert.GetTimeZoneInfo(timeZoneId);
                var utcDateTime = JulianDayToDateTime(startJD);
                return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, timeZoneInfo);
            }

            // Binary search to find exact crossing point
            while ((endJD - startJD) > TIME_PRECISION_JD)
            {
                double midJD = (startJD + endJD) / 2.0;
                var (midLongitude, _) = isKetu
                    ? GetKetuPositionAtJulianDay(midJD, ayanamsa)
                    : GetPlanetPositionAtJulianDay(midJD, planet, ayanamsa);
                int midSign = (int)(AstrologyHelper.NormalizeDegrees(midLongitude) / 30.0);

                if (midSign != currentSign)
                {
                    // Sign change occurred after midpoint (planet was in different sign)
                    startJD = midJD;
                }
                else
                {
                    // Sign change occurred before midpoint (planet was already in current sign)
                    endJD = midJD;
                }
            }

            // Convert result to DateTime in user's timezone
            var timeZone = TZConvert.GetTimeZoneInfo(timeZoneId);
            var resultUtc = JulianDayToDateTime(endJD);
            var resultLocal = TimeZoneInfo.ConvertTimeFromUtc(resultUtc, timeZone);

            return resultLocal;
        }

        private (double longitude, double speed) GetPlanetPositionAtJulianDay(
            double julianDay,
            Planet planet,
            double ayanamsa)
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
                _logger.LogError("Error calculating {Planet} at JD {JD}: {Error}", planet, julianDay, error);
                throw new Exception($"Failed to calculate {planet}: {error}");
            }

            var longitude = xx[0] - ayanamsa;  // Apply Ayanamsa for sidereal
            var speed = xx[3];   // Speed in longitude (degrees/day)

            return (AstrologyHelper.NormalizeDegrees(longitude), speed);
        }

        private (double longitude, double speed) GetKetuPositionAtJulianDay(
            double julianDay,
            double ayanamsa)
        {
            // Get Rahu position first
            var (rahuLon, rahuSpeed) = GetPlanetPositionAtJulianDay(julianDay, Planet.MeanNode, ayanamsa);

            // Ketu is 180° opposite
            var ketuLon = AstrologyHelper.CalculateKetu(rahuLon);
            var ketuSpeed = -rahuSpeed;

            return (ketuLon, ketuSpeed);
        }

        private double CalculateDegreesToBoundary(double currentLongitude, double targetBoundary, bool isRetrograde)
        {
            var normalizedLongitude = AstrologyHelper.NormalizeDegrees(currentLongitude);
            var normalizedBoundary = AstrologyHelper.NormalizeDegrees(targetBoundary);

            double distance;

            if (isRetrograde)
            {
                // Moving backward
                if (normalizedLongitude > normalizedBoundary)
                {
                    distance = normalizedLongitude - normalizedBoundary;
                }
                else
                {
                    // Wrapped around 0°
                    distance = normalizedLongitude + (360.0 - normalizedBoundary);
                }
            }
            else
            {
                // Moving forward
                if (normalizedBoundary > normalizedLongitude)
                {
                    distance = normalizedBoundary - normalizedLongitude;
                }
                else
                {
                    // Wrapped around 360°
                    distance = (360.0 - normalizedLongitude) + normalizedBoundary;
                }
            }

            return distance;
        }

        private double GetSearchWindowMultiplier(Planet planet, double speed)
        {
            return planet switch
            {
                Planet.Moon => 1.5,              // Very fast, predictable
                Planet.Sun => 1.8,               // Fast, steady
                Planet.Mercury => 3.0,           // Variable, can go retrograde
                Planet.Venus => 2.5,             // Medium, can go retrograde
                Planet.Mars => 2.0,              // Medium-slow
                Planet.Jupiter => 3.0,           // Slow, goes retrograde
                Planet.Saturn => 3.0,            // Very slow, goes retrograde
                Planet.MeanNode => 2.0,          // Slow, always retrograde (Rahu/Ketu)
                _ => 2.5                         // Default
            };
        }

        private DateTime JulianDayToDateTime(double julianDay)
        {
            int year = 0, month = 0, day = 0;
            double hour = 0;

            _swissEph.swe_revjul(julianDay, SwissEph.SE_GREG_CAL, ref year, ref month, ref day, ref hour);

            var hours = (int)hour;
            var minutes = (int)((hour - hours) * 60.0);
            var seconds = (int)(((hour - hours) * 60.0 - minutes) * 60.0);

            return new DateTime(year, month, day, hours, minutes, seconds, DateTimeKind.Utc);
        }

        public void Dispose()
        {
            _swissEph?.Dispose();
        }
    }
}
