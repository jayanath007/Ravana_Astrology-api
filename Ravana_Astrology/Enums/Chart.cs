namespace Ravana_Astrology.Enums
{
    /// <summary>
    /// The twelve zodiac signs in tropical zodiac order.
    /// </summary>
    public enum Chart
    {
        Aries = 1,       // 0° - 30°  // මේෂ
        Taurus = 2,      // 30° - 60° // වෘෂභ
        Gemini = 3,      // 60° - 90° // මිථුන
        Cancer = 4,      // 90° - 120° // කටක
        Leo = 5,         // 120° - 150° // සිංහ
        Virgo = 6,       // 150° - 180° // කන්‍යා
        Libra = 7,       // 180° - 210° // තුලා
        Scorpio = 8,     // 210° - 240° // වෘශ්චික
        Sagittarius = 9, // 240° - 270° // ධනු
        Capricorn = 10,  // 270° - 300° // මකර
        Aquarius = 11,   // 300° - 330° // කුම්භ
        Pisces = 12      // 330° - 360° // මීන

    }

    public enum Planet
    {
        Sun = 0,        // SwissEph.SE_SUN         // සූර්ය
        Moon = 1,       // SwissEph.SE_MOON        // චන්ද්‍ර
        Mercury = 2,    // SwissEph.SE_MERCURY     // බුධ
        Venus = 3,      // SwissEph.SE_VENUS       // ශුක්‍ර
        Mars = 4,       // SwissEph.SE_MARS        // මංගල
        Jupiter = 5,    // SwissEph.SE_JUPITER     // ගුරු
        Saturn = 6,     // SwissEph.SE_SATURN      // ශනි
        MeanNode = 10,  // SwissEph.SE_MEAN_NODE   // රාහු (උතුරු චන්ද්‍රග්‍රහ)
        TrueNode = 11   // SwissEph.SE_TRUE_NODE   // කේතු (නිර්වාර්ණ රාහු / දක්ෂිණ චන්ද්‍රග්‍රහ)

    }
}
