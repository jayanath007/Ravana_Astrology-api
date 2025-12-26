using Ravana_Astrology.Enums;

namespace Ravana_Astrology.Constants
{
    /// <summary>
    /// Constants and reference data for Vimshottari Dasha calculations.
    /// </summary>
    public static class VimshottariConstants
    {
        /// <summary>
        /// Total duration of Vimshottari cycle in years
        /// </summary>
        public const double TOTAL_YEARS = 120.0;

        /// <summary>
        /// Degrees per nakshatra (13°20' = 13.333333°)
        /// </summary>
        public const double NAKSHATRA_SPAN = 13.333333333333334;

        /// <summary>
        /// Total number of nakshatras
        /// </summary>
        public const int TOTAL_NAKSHATRAS = 27;

        /// <summary>
        /// Nakshatra names in order (1-27)
        /// </summary>
        public static readonly string[] NakshatraNames = new[]
        {
            "Ashwini",           // 1
            "Bharani",           // 2
            "Krittika",          // 3
            "Rohini",            // 4
            "Mrigashira",        // 5
            "Ardra",             // 6
            "Punarvasu",         // 7
            "Pushya",            // 8
            "Ashlesha",          // 9
            "Magha",             // 10
            "Purva Phalguni",    // 11
            "Uttara Phalguni",   // 12
            "Hasta",             // 13
            "Chitra",            // 14
            "Swati",             // 15
            "Vishakha",          // 16
            "Anuradha",          // 17
            "Jyeshtha",          // 18
            "Mula",              // 19
            "Purva Ashadha",     // 20
            "Uttara Ashadha",    // 21
            "Shravana",          // 22
            "Dhanishta",         // 23
            "Shatabhisha",       // 24
            "Purva Bhadrapada",  // 25
            "Uttara Bhadrapada", // 26
            "Revati"             // 27
        };

        /// <summary>
        /// Nakshatra lords in sequence (cycles through 9 planets, 3 complete cycles for 27 nakshatras)
        /// </summary>
        public static readonly DashaPlanet[] NakshatraLords = new[]
        {
            DashaPlanet.Ketu,    // 1. Ashwini
            DashaPlanet.Venus,   // 2. Bharani
            DashaPlanet.Sun,     // 3. Krittika
            DashaPlanet.Moon,    // 4. Rohini
            DashaPlanet.Mars,    // 5. Mrigashira
            DashaPlanet.Rahu,    // 6. Ardra
            DashaPlanet.Jupiter, // 7. Punarvasu
            DashaPlanet.Saturn,  // 8. Pushya
            DashaPlanet.Mercury, // 9. Ashlesha
            DashaPlanet.Ketu,    // 10. Magha
            DashaPlanet.Venus,   // 11. Purva Phalguni
            DashaPlanet.Sun,     // 12. Uttara Phalguni
            DashaPlanet.Moon,    // 13. Hasta
            DashaPlanet.Mars,    // 14. Chitra
            DashaPlanet.Rahu,    // 15. Swati
            DashaPlanet.Jupiter, // 16. Vishakha
            DashaPlanet.Saturn,  // 17. Anuradha
            DashaPlanet.Mercury, // 18. Jyeshtha
            DashaPlanet.Ketu,    // 19. Mula
            DashaPlanet.Venus,   // 20. Purva Ashadha
            DashaPlanet.Sun,     // 21. Uttara Ashadha
            DashaPlanet.Moon,    // 22. Shravana
            DashaPlanet.Mars,    // 23. Dhanishta
            DashaPlanet.Rahu,    // 24. Shatabhisha
            DashaPlanet.Jupiter, // 25. Purva Bhadrapada
            DashaPlanet.Saturn,  // 26. Uttara Bhadrapada
            DashaPlanet.Mercury  // 27. Revati
        };

        /// <summary>
        /// Mahadasha durations in years for each planet
        /// </summary>
        public static readonly Dictionary<DashaPlanet, double> MahadashaDurations = new()
        {
            { DashaPlanet.Sun, 6.0 },
            { DashaPlanet.Moon, 10.0 },
            { DashaPlanet.Mars, 7.0 },
            { DashaPlanet.Rahu, 18.0 },
            { DashaPlanet.Jupiter, 16.0 },
            { DashaPlanet.Saturn, 19.0 },
            { DashaPlanet.Mercury, 17.0 },
            { DashaPlanet.Ketu, 7.0 },
            { DashaPlanet.Venus, 20.0 }
        };

        /// <summary>
        /// Vimshottari dasha sequence (standard order of planetary periods)
        /// </summary>
        public static readonly DashaPlanet[] DashaSequence = new[]
        {
            DashaPlanet.Ketu,
            DashaPlanet.Venus,
            DashaPlanet.Sun,
            DashaPlanet.Moon,
            DashaPlanet.Mars,
            DashaPlanet.Rahu,
            DashaPlanet.Jupiter,
            DashaPlanet.Saturn,
            DashaPlanet.Mercury
        };

        /// <summary>
        /// Planet names for display
        /// </summary>
        public static readonly Dictionary<DashaPlanet, string> PlanetNames = new()
        {
            { DashaPlanet.Sun, "Sun" },
            { DashaPlanet.Moon, "Moon" },
            { DashaPlanet.Mars, "Mars" },
            { DashaPlanet.Rahu, "Rahu" },
            { DashaPlanet.Jupiter, "Jupiter" },
            { DashaPlanet.Saturn, "Saturn" },
            { DashaPlanet.Mercury, "Mercury" },
            { DashaPlanet.Ketu, "Ketu" },
            { DashaPlanet.Venus, "Venus" }
        };
    }
}
