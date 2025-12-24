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
    }
}
