namespace Ravana_Astrology.Enums
{
    /// <summary>
    /// Supported astrological house systems.
    /// </summary>
    public enum HouseSystem
    {
        Placidus,       // 'P' - Most popular in Western astrology
        WholeSign,      // 'W' - Common in Vedic astrology
        Equal,          // 'E' - Equal 30° houses from Ascendant
        Koch,           // 'K' - Birthplace system
        Campanus,       // 'C' - Prime vertical system
        Regiomontanus   // 'R' - Rational system
    }
}
