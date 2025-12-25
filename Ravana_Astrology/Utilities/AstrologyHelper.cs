using Ravana_Astrology.Enums;
using Ravana_Astrology.Models.Responses;

namespace Ravana_Astrology.Utilities
{
    /// <summary>
    /// Helper utilities for astrological calculations and conversions.
    /// </summary>
    public static class AstrologyHelper
    {
        /// <summary>
        /// Get the zodiac sign for a given ecliptic longitude.
        /// </summary>
        public static Chart GetZodiacSign(double longitude)
        {
            var normalizedLongitude = NormalizeDegrees(longitude);
            var signIndex = (int)(normalizedLongitude / 30.0) + 1; // +1 because signs are now numbered 1-12
            return (Chart)signIndex;
        }

        /// <summary>
        /// Convert decimal degrees to ZodiacInfo with sign and DMS format.
        /// </summary>
        public static ZodiacInfo ConvertToZodiacInfo(double longitude)
        {
            var normalizedLongitude = NormalizeDegrees(longitude);
            var sign = GetZodiacSign(normalizedLongitude);
            var degreeInSign = normalizedLongitude % 30.0;

            var (degree, minutes, seconds) = ConvertToDMS(degreeInSign);

            return new ZodiacInfo
            {
                Sign = sign,
                DegreeInSign = degreeInSign,
                Degree = degree,
                Minutes = minutes,
                Seconds = seconds,
                FormattedPosition = $"{degree}°{minutes:D2}'{seconds:D2}\" {sign}"
            };
        }

        /// <summary>
        /// Convert decimal degrees to Degrees, Minutes, Seconds format.
        /// </summary>
        public static (int Degree, int Minutes, int Seconds) ConvertToDMS(double decimalDegree)
        {
            var degree = (int)decimalDegree;
            var minutesDecimal = (decimalDegree - degree) * 60.0;
            var minutes = (int)minutesDecimal;
            var seconds = (int)((minutesDecimal - minutes) * 60.0);

            return (degree, minutes, seconds);
        }

        /// <summary>
        /// Convert HouseSystem enum to Swiss Ephemeris character code.
        /// </summary>
        public static char HouseSystemToCode(HouseSystem houseSystem)
        {
            return houseSystem switch
            {
                HouseSystem.Placidus => 'P',
                HouseSystem.WholeSign => 'W',
                HouseSystem.Equal => 'E',
                HouseSystem.Koch => 'K',
                HouseSystem.Campanus => 'C',
                HouseSystem.Regiomontanus => 'R',
                _ => 'P'
            };
        }

        /// <summary>
        /// Convert Swiss Ephemeris house system code to HouseSystem enum.
        /// </summary>
        public static HouseSystem CodeToHouseSystem(char code)
        {
            return char.ToUpper(code) switch
            {
                'P' => HouseSystem.Placidus,
                'W' => HouseSystem.WholeSign,
                'E' => HouseSystem.Equal,
                'K' => HouseSystem.Koch,
                'C' => HouseSystem.Campanus,
                'R' => HouseSystem.Regiomontanus,
                _ => HouseSystem.Placidus
            };
        }

        /// <summary>
        /// Convert Planet enum to Swiss Ephemeris planet ID.
        /// </summary>
        public static int PlanetToSwissEphId(Planet planet)
        {
            return (int)planet;
        }

        /// <summary>
        /// Normalize degrees to 0-360 range.
        /// </summary>
        public static double NormalizeDegrees(double degrees)
        {
            var normalized = degrees % 360.0;
            if (normalized < 0)
                normalized += 360.0;
            return normalized;
        }

        /// <summary>
        /// Calculate Ketu (South Lunar Node) position from Rahu (North Lunar Node).
        /// Ketu is always 180° opposite to Rahu.
        /// </summary>
        public static double CalculateKetu(double rahuLongitude)
        {
            return NormalizeDegrees(rahuLongitude + 180.0);
        }

        /// <summary>
        /// Get planet name as string.
        /// </summary>
        public static string GetPlanetName(Planet planet)
        {
            return planet switch
            {
                Planet.MeanNode => "Rahu",
                Planet.TrueNode => "Rahu",
                _ => planet.ToString()
            };
        }

        /// <summary>
        /// Get Sinhala zodiac name for a given ZodiacSign.
        /// </summary>
        public static string GetSinhalaZodiacName(Chart sign)
        {
            return sign switch
            {
                Chart.Aries => "මෙස",           // Mesha
                Chart.Taurus => "වෘෂභ",         // Vrushabha
                Chart.Gemini => "මිතුන",        // Mithuna
                Chart.Cancer => "කරක",          // Karka
                Chart.Leo => "සිංහ",            // Simha
                Chart.Virgo => "කන්‍යා",        // Kanya
                Chart.Libra => "තුලා",          // Tula
                Chart.Scorpio => "වෘෂ්චික",     // Vrischika
                Chart.Sagittarius => "ධනුව",    // Dhanu
                Chart.Capricorn => "මකර",       // Makara
                Chart.Aquarius => "කුම්භ",      // Kumbha
                Chart.Pisces => "මීන",          // Meena
                _ => sign.ToString()
            };
        }

        /// <summary>
        /// Get Sinhala planet name for a given planet name.
        /// </summary>
        public static string GetSinhalaPlanetName(string planetName)
        {
            return planetName switch
            {
                "Sun" => "ර",        // Surya
                "Moon" => "ච",      // Chandra
                "Mars" => "කු",       // Mangala
                "Mercury" => "බු",     // Budha
                "Jupiter" => "ගු",     // Guru
                "Venus" => "ශු",     // Shukra
                "Saturn" => "ශ",      // Shani
                "Rahu" => "රා",       // Rahu
                "Ketu" => "කේ",       // Ketu
                _ => planetName
            };
        }

        /// <summary>
        /// Calculate the Navāṁśa (D9) sign for a given sidereal longitude.
        /// The Navāṁśa is a divisional chart where each sign is divided into 9 parts of 3°20' each.
        /// </summary>
        /// <param name="sidereaLongitude">Sidereal ecliptic longitude in degrees</param>
        /// <returns>Navāṁśa zodiac sign (1-12)</returns>
        public static Chart CalculateNavamsaSign(double sidereaLongitude)
        {
            var normalizedLongitude = NormalizeDegrees(sidereaLongitude);

            // Get the natal (Rāśi) sign (1-12)
            int natalSign = (int)(normalizedLongitude / 30.0) + 1;

            // Get position within the natal sign (0-30 degrees)
            double degreeInSign = normalizedLongitude % 30.0;

            // Calculate which Navāṁśa division (0-8) within the sign
            // Each division is 3°20' (3.333333 degrees)
            int division = (int)(degreeInSign / (30.0 / 9.0));

            // Ensure division is within bounds (0-8)
            if (division > 8) division = 8;

            // Get the starting Navāṁśa sign based on natal sign's element
            int navamsaStartSign = GetNavamsaStartSign(natalSign);

            // Calculate final Navāṁśa sign using circular addition
            int navamsaSignNumber = ((navamsaStartSign - 1 + division) % 12) + 1;

            return (Chart)navamsaSignNumber;
        }

        /// <summary>
        /// Determine the starting Navāṁśa sign based on the natal sign's element.
        /// Fire signs start from Aries, Earth from Capricorn, Air from Libra, Water from Cancer.
        /// </summary>
        /// <param name="natalSign">Natal (Rāśi) sign number (1-12)</param>
        /// <returns>Starting Navāṁśa sign number (1-12)</returns>
        private static int GetNavamsaStartSign(int natalSign)
        {
            // Element pattern repeats every 4 signs:
            // 1, 5, 9 (Aries, Leo, Sagittarius) = Fire → Start at Aries (1)
            // 2, 6, 10 (Taurus, Virgo, Capricorn) = Earth → Start at Capricorn (10)
            // 3, 7, 11 (Gemini, Libra, Aquarius) = Air → Start at Libra (7)
            // 4, 8, 12 (Cancer, Scorpio, Pisces) = Water → Start at Cancer (4)

            int elementGroup = (natalSign - 1) % 4;

            return elementGroup switch
            {
                0 => 1,  // Fire → Aries
                1 => 10, // Earth → Capricorn
                2 => 7,  // Air → Libra
                3 => 4,  // Water → Cancer
                _ => 1   // Default fallback
            };
        }
    }
}
