using Ravana_Astrology.Constants;
using Ravana_Astrology.Enums;
using Ravana_Astrology.Models.Requests;
using Ravana_Astrology.Models.Responses;
using Ravana_Astrology.Services.Interfaces;
using Ravana_Astrology.Utilities;
using TimeZoneConverter;

namespace Ravana_Astrology.Services.Implementation
{
    /// <summary>
    /// Service implementation for Vimshottari Dasha calculations.
    /// </summary>
    public class VimshottariDashaService : IVimshottariDashaService
    {
        private readonly IAstrologyCalculationService _astrologyService;
        private readonly ILogger<VimshottariDashaService> _logger;

        public VimshottariDashaService(
            IAstrologyCalculationService astrologyService,
            ILogger<VimshottariDashaService> logger)
        {
            _astrologyService = astrologyService;
            _logger = logger;
        }

        public async Task<VimshottariDashaResponse> CalculateVimshottariDasha(VimshottariDashaRequest request)
        {
            try
            {
                _logger.LogInformation("Calculating Vimshottari Dasha for birth date {Date} at {Lat}, {Lon}",
                    request.BirthDate, request.Latitude, request.Longitude);

                // 1. Get birth chart to calculate Moon's sidereal position
                var birthChartRequest = new BirthChartRequest
                {
                    BirthDate = request.BirthDate,
                    BirthTime = request.BirthTime,
                    Latitude = request.Latitude,
                    Longitude = request.Longitude,
                    TimeZoneId = request.TimeZoneId,
                    HouseSystem = HouseSystem.WholeSign,
                    IncludeTrueNode = false,
                    CalculationType = CalculationType.Vedic // Use Vedic/Sidereal for nakshatra
                };

                var birthChart = await _astrologyService.CalculateBirthChart(birthChartRequest);

                // 2. Extract Moon's sidereal longitude
                var moonPosition = birthChart.Planets.FirstOrDefault(p => p.Planet == "Moon");
                if (moonPosition == null)
                {
                    throw new Exception("Could not calculate Moon position");
                }

                var moonLongitude = moonPosition.EclipticLongitude;
                _logger.LogInformation("Moon sidereal longitude: {Longitude}°", moonLongitude);

                // 3. Determine nakshatra and calculate balance
                var nakshatra = DetermineNakshatra(moonLongitude);
                var nakshatraLord = AstrologyHelper.GetNakshatraLord(nakshatra.NakshatraNumber);
                var balanceYears = CalculateBalance(nakshatra, nakshatraLord);

                _logger.LogInformation("Birth Nakshatra: {Name} (Lord: {Lord}), Balance: {Balance} years",
                    nakshatra.NakshatraName, nakshatra.Lord, balanceYears);

                // 4. Generate Mahadasha periods
                var mahadashas = GenerateMahadashas(
                    birthChart.BirthDateTimeUtc,
                    birthChart.BirthDateTimeLocal,
                    nakshatraLord,
                    balanceYears,
                    request.YearsToCalculate,
                    request.TimeZoneId);

                // 5. Generate nested periods based on DetailLevel
                if (request.DetailLevel >= 2)
                {
                    foreach (var mahadasha in mahadashas)
                    {
                        mahadasha.AntardashaPeriods = GenerateAntardashas(mahadasha, request.TimeZoneId);

                        if (request.DetailLevel >= 3)
                        {
                            foreach (var antardasha in mahadasha.AntardashaPeriods)
                            {
                                antardasha.PratyantardashaPeriods = GeneratePratyantardashas(antardasha, request.TimeZoneId);

                                if (request.DetailLevel >= 4)
                                {
                                    foreach (var pratyantardasha in antardasha.PratyantardashaPeriods)
                                    {
                                        pratyantardasha.SookshmaPeriods = GenerateSookshmas(pratyantardasha, request.TimeZoneId);
                                    }
                                }
                            }
                        }
                    }
                }

                // 6. Build response
                var response = new VimshottariDashaResponse
                {
                    BirthDateTimeUtc = birthChart.BirthDateTimeUtc,
                    BirthDateTimeLocal = birthChart.BirthDateTimeLocal,
                    Location = birthChart.Location,
                    BirthNakshatra = nakshatra,
                    DetailLevel = request.DetailLevel,
                    YearsCalculated = request.YearsToCalculate,
                    MahadashaPeriods = mahadashas,
                    TotalPeriodsCount = CountTotalPeriods(mahadashas, request.DetailLevel)
                };

                // 7. Identify current active periods
                IdentifyCurrentPeriods(response, DateTime.UtcNow);

                _logger.LogInformation("Vimshottari Dasha calculation complete. Total periods: {Count}",
                    response.TotalPeriodsCount);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating Vimshottari Dasha");
                throw;
            }
        }

        /// <summary>
        /// Determine nakshatra from Moon's sidereal longitude.
        /// </summary>
        private NakshatraInfo DetermineNakshatra(double moonLongitude)
        {
            var normalizedLongitude = AstrologyHelper.NormalizeDegrees(moonLongitude);
            var nakshatraNumber = AstrologyHelper.GetNakshatra(normalizedLongitude);
            var nakshatraName = AstrologyHelper.GetNakshatraName(nakshatraNumber);
            var lord = AstrologyHelper.GetNakshatraLord(nakshatraNumber);
            var lordName = AstrologyHelper.GetDashaPlanetName(lord);

            // Calculate position within nakshatra
            var degreesInNakshatra = normalizedLongitude % VimshottariConstants.NAKSHATRA_SPAN;
            var percentageCompleted = (degreesInNakshatra / VimshottariConstants.NAKSHATRA_SPAN) * 100.0;
            var pada = AstrologyHelper.GetNakshatraPada(degreesInNakshatra);

            return new NakshatraInfo
            {
                NakshatraNumber = nakshatraNumber,
                NakshatraName = nakshatraName,
                Lord = lordName,
                MoonLongitude = normalizedLongitude,
                DegreesInNakshatra = degreesInNakshatra,
                PercentageCompleted = percentageCompleted,
                Pada = pada
            };
        }

        /// <summary>
        /// Calculate balance of Mahadasha at birth.
        /// </summary>
        private double CalculateBalance(NakshatraInfo nakshatra, DashaPlanet lord)
        {
            var totalYears = VimshottariConstants.MahadashaDurations[lord];
            var balance = totalYears * (1 - (nakshatra.PercentageCompleted / 100.0));
            return balance;
        }

        /// <summary>
        /// Generate all Mahadasha periods.
        /// </summary>
        private List<MahadashaInfo> GenerateMahadashas(
            DateTime birthDateTimeUtc,
            DateTime birthDateTimeLocal,
            DashaPlanet startingPlanet,
            double balanceYears,
            int yearsToCalculate,
            string timeZoneId)
        {
            var mahadashas = new List<MahadashaInfo>();
            var currentPlanet = startingPlanet;
            var currentStartUtc = birthDateTimeUtc;
            var currentStartLocal = birthDateTimeLocal;
            var totalYearsElapsed = 0.0;

            // First Mahadasha with balance period
            var balanceDays = balanceYears * 365.25;
            var balanceEndUtc = currentStartUtc.AddDays(balanceDays);
            var balanceEndLocal = ConvertToLocal(balanceEndUtc, timeZoneId);

            mahadashas.Add(new MahadashaInfo
            {
                Planet = AstrologyHelper.GetDashaPlanetName(currentPlanet),
                StartDateUtc = currentStartUtc,
                EndDateUtc = balanceEndUtc,
                StartDateLocal = currentStartLocal,
                EndDateLocal = balanceEndLocal,
                DurationDays = balanceDays,
                DurationYears = balanceYears,
                IsBalancePeriod = true,
                BalanceYears = balanceYears,
                IsCurrentPeriod = false
            });

            totalYearsElapsed += balanceYears;
            currentStartUtc = balanceEndUtc;
            currentStartLocal = balanceEndLocal;
            currentPlanet = AstrologyHelper.GetNextDashaPlanet(currentPlanet);

            // Subsequent full Mahadashas
            while (totalYearsElapsed < yearsToCalculate)
            {
                var durationYears = VimshottariConstants.MahadashaDurations[currentPlanet];

                // Check if this would exceed yearsToCalculate
                if (totalYearsElapsed + durationYears > yearsToCalculate)
                {
                    durationYears = yearsToCalculate - totalYearsElapsed;
                }

                var durationDays = durationYears * 365.25;
                var endUtc = currentStartUtc.AddDays(durationDays);
                var endLocal = ConvertToLocal(endUtc, timeZoneId);

                mahadashas.Add(new MahadashaInfo
                {
                    Planet = AstrologyHelper.GetDashaPlanetName(currentPlanet),
                    StartDateUtc = currentStartUtc,
                    EndDateUtc = endUtc,
                    StartDateLocal = currentStartLocal,
                    EndDateLocal = endLocal,
                    DurationDays = durationDays,
                    DurationYears = durationYears,
                    IsBalancePeriod = false,
                    BalanceYears = null,
                    IsCurrentPeriod = false
                });

                totalYearsElapsed += durationYears;
                currentStartUtc = endUtc;
                currentStartLocal = endLocal;
                currentPlanet = AstrologyHelper.GetNextDashaPlanet(currentPlanet);
            }

            return mahadashas;
        }

        /// <summary>
        /// Generate Antardasha periods within a Mahadasha.
        /// </summary>
        private List<AntardashaInfo> GenerateAntardashas(MahadashaInfo mahadasha, string timeZoneId)
        {
            var antardashas = new List<AntardashaInfo>();
            var mahadashaDurationDays = mahadasha.DurationDays;
            var currentStartUtc = mahadasha.StartDateUtc;

            // Get the Mahadasha planet to start the Antardasha sequence
            var mahadashaPlanet = GetDashaPlanetFromName(mahadasha.Planet);

            // Antardasha sequence starts with the Mahadasha planet
            var currentPlanet = mahadashaPlanet;

            for (int i = 0; i < 9; i++) // 9 Antardashas per Mahadasha
            {
                var antardashaPlanetYears = VimshottariConstants.MahadashaDurations[currentPlanet];
                var antardashaDurationYears = (antardashaPlanetYears / VimshottariConstants.TOTAL_YEARS) * mahadasha.DurationYears;
                var antardashaDurationDays = antardashaDurationYears * 365.25;

                var endUtc = currentStartUtc.AddDays(antardashaDurationDays);
                var startLocal = ConvertToLocal(currentStartUtc, timeZoneId);
                var endLocal = ConvertToLocal(endUtc, timeZoneId);

                antardashas.Add(new AntardashaInfo
                {
                    Planet = AstrologyHelper.GetDashaPlanetName(currentPlanet),
                    StartDateUtc = currentStartUtc,
                    EndDateUtc = endUtc,
                    StartDateLocal = startLocal,
                    EndDateLocal = endLocal,
                    DurationDays = antardashaDurationDays,
                    DurationYears = antardashaDurationYears,
                    IsCurrentPeriod = false
                });

                currentStartUtc = endUtc;
                currentPlanet = AstrologyHelper.GetNextDashaPlanet(currentPlanet);
            }

            return antardashas;
        }

        /// <summary>
        /// Generate Pratyantardasha periods within an Antardasha.
        /// </summary>
        private List<PratyantardashaInfo> GeneratePratyantardashas(AntardashaInfo antardasha, string timeZoneId)
        {
            var pratyantardashas = new List<PratyantardashaInfo>();
            var currentStartUtc = antardasha.StartDateUtc;

            // Pratyantardasha sequence starts with the Antardasha planet
            var antardashaPlanet = GetDashaPlanetFromName(antardasha.Planet);
            var currentPlanet = antardashaPlanet;

            for (int i = 0; i < 9; i++) // 9 Pratyantardashas per Antardasha
            {
                var pratyantardashaPlanetYears = VimshottariConstants.MahadashaDurations[currentPlanet];
                var pratyantardashaDurationYears = (pratyantardashaPlanetYears / VimshottariConstants.TOTAL_YEARS) * antardasha.DurationYears;
                var pratyantardashaDurationDays = pratyantardashaDurationYears * 365.25;

                var endUtc = currentStartUtc.AddDays(pratyantardashaDurationDays);
                var startLocal = ConvertToLocal(currentStartUtc, timeZoneId);
                var endLocal = ConvertToLocal(endUtc, timeZoneId);

                pratyantardashas.Add(new PratyantardashaInfo
                {
                    Planet = AstrologyHelper.GetDashaPlanetName(currentPlanet),
                    StartDateUtc = currentStartUtc,
                    EndDateUtc = endUtc,
                    StartDateLocal = startLocal,
                    EndDateLocal = endLocal,
                    DurationDays = pratyantardashaDurationDays,
                    DurationYears = pratyantardashaDurationYears,
                    IsCurrentPeriod = false
                });

                currentStartUtc = endUtc;
                currentPlanet = AstrologyHelper.GetNextDashaPlanet(currentPlanet);
            }

            return pratyantardashas;
        }

        /// <summary>
        /// Generate Sookshma periods within a Pratyantardasha.
        /// </summary>
        private List<SookshmaInfo> GenerateSookshmas(PratyantardashaInfo pratyantardasha, string timeZoneId)
        {
            var sookshmas = new List<SookshmaInfo>();
            var currentStartUtc = pratyantardasha.StartDateUtc;

            // Sookshma sequence starts with the Pratyantardasha planet
            var pratyantardashaPlanet = GetDashaPlanetFromName(pratyantardasha.Planet);
            var currentPlanet = pratyantardashaPlanet;

            for (int i = 0; i < 9; i++) // 9 Sookshmas per Pratyantardasha
            {
                var sookshmaPlanetYears = VimshottariConstants.MahadashaDurations[currentPlanet];
                var sookshmaDurationYears = (sookshmaPlanetYears / VimshottariConstants.TOTAL_YEARS) * pratyantardasha.DurationYears;
                var sookshmaDurationDays = sookshmaDurationYears * 365.25;

                var endUtc = currentStartUtc.AddDays(sookshmaDurationDays);
                var startLocal = ConvertToLocal(currentStartUtc, timeZoneId);
                var endLocal = ConvertToLocal(endUtc, timeZoneId);

                sookshmas.Add(new SookshmaInfo
                {
                    Planet = AstrologyHelper.GetDashaPlanetName(currentPlanet),
                    StartDateUtc = currentStartUtc,
                    EndDateUtc = endUtc,
                    StartDateLocal = startLocal,
                    EndDateLocal = endLocal,
                    DurationDays = sookshmaDurationDays,
                    DurationYears = sookshmaDurationYears,
                    IsCurrentPeriod = false
                });

                currentStartUtc = endUtc;
                currentPlanet = AstrologyHelper.GetNextDashaPlanet(currentPlanet);
            }

            return sookshmas;
        }

        /// <summary>
        /// Convert UTC DateTime to local timezone.
        /// </summary>
        private DateTime ConvertToLocal(DateTime utcDateTime, string timeZoneId)
        {
            try
            {
                var timeZoneInfo = TZConvert.GetTimeZoneInfo(timeZoneId);
                return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, timeZoneInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error converting to timezone {TimeZoneId}", timeZoneId);
                throw new ArgumentException($"Invalid timezone ID: {timeZoneId}", nameof(timeZoneId));
            }
        }

        /// <summary>
        /// Identify currently active periods at all levels.
        /// </summary>
        private void IdentifyCurrentPeriods(VimshottariDashaResponse response, DateTime currentDateUtc)
        {
            // Find current Mahadasha
            response.CurrentMahadasha = response.MahadashaPeriods
                .FirstOrDefault(m => currentDateUtc >= m.StartDateUtc && currentDateUtc < m.EndDateUtc);

            if (response.CurrentMahadasha != null)
            {
                response.CurrentMahadasha.IsCurrentPeriod = true;

                // Find current Antardasha (if DetailLevel >= 2)
                if (response.DetailLevel >= 2)
                {
                    response.CurrentAntardasha = response.CurrentMahadasha.AntardashaPeriods
                        .FirstOrDefault(a => currentDateUtc >= a.StartDateUtc && currentDateUtc < a.EndDateUtc);

                    if (response.CurrentAntardasha != null)
                    {
                        response.CurrentAntardasha.IsCurrentPeriod = true;

                        // Find current Pratyantardasha (if DetailLevel >= 3)
                        if (response.DetailLevel >= 3)
                        {
                            response.CurrentPratyantardasha = response.CurrentAntardasha.PratyantardashaPeriods
                                .FirstOrDefault(p => currentDateUtc >= p.StartDateUtc && currentDateUtc < p.EndDateUtc);

                            if (response.CurrentPratyantardasha != null)
                            {
                                response.CurrentPratyantardasha.IsCurrentPeriod = true;

                                // Find current Sookshma (if DetailLevel >= 4)
                                if (response.DetailLevel >= 4)
                                {
                                    response.CurrentSookshma = response.CurrentPratyantardasha.SookshmaPeriods
                                        .FirstOrDefault(s => currentDateUtc >= s.StartDateUtc && currentDateUtc < s.EndDateUtc);

                                    if (response.CurrentSookshma != null)
                                    {
                                        response.CurrentSookshma.IsCurrentPeriod = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Count total number of periods across all levels.
        /// </summary>
        private int CountTotalPeriods(List<MahadashaInfo> mahadashas, int detailLevel)
        {
            int count = mahadashas.Count;

            if (detailLevel >= 2)
            {
                foreach (var mahadasha in mahadashas)
                {
                    count += mahadasha.AntardashaPeriods.Count;

                    if (detailLevel >= 3)
                    {
                        foreach (var antardasha in mahadasha.AntardashaPeriods)
                        {
                            count += antardasha.PratyantardashaPeriods.Count;

                            if (detailLevel >= 4)
                            {
                                foreach (var pratyantardasha in antardasha.PratyantardashaPeriods)
                                {
                                    count += pratyantardasha.SookshmaPeriods.Count;
                                }
                            }
                        }
                    }
                }
            }

            return count;
        }

        /// <summary>
        /// Get DashaPlanet enum from planet name string.
        /// </summary>
        private DashaPlanet GetDashaPlanetFromName(string planetName)
        {
            return planetName switch
            {
                "Sun" => DashaPlanet.Sun,
                "Moon" => DashaPlanet.Moon,
                "Mars" => DashaPlanet.Mars,
                "Rahu" => DashaPlanet.Rahu,
                "Jupiter" => DashaPlanet.Jupiter,
                "Saturn" => DashaPlanet.Saturn,
                "Mercury" => DashaPlanet.Mercury,
                "Ketu" => DashaPlanet.Ketu,
                "Venus" => DashaPlanet.Venus,
                _ => throw new ArgumentException($"Unknown planet name: {planetName}")
            };
        }
    }
}
