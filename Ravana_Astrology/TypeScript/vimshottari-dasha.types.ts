/**
 * TypeScript types for Ravana Astrology - Vimshottari Dasha API
 * Generated from C# models in Ravana_Astrology.Models.Responses
 */

/**
 * Location information
 */
export interface LocationInfo {
  latitude: number;
  longitude: number;
  timeZone: string;
}

/**
 * Information about Moon's nakshatra at birth
 */
export interface NakshatraInfo {
  /** Nakshatra number (1-27) */
  nakshatraNumber: number;
  /** Nakshatra name (Sanskrit) */
  nakshatraName: string;
  /** Nakshatra lord (ruling planet) */
  lord: string;
  /** Moon's longitude at birth (sidereal) */
  moonLongitude: number;
  /** Degrees traversed in the nakshatra (0-13.333333) */
  degreesInNakshatra: number;
  /** Percentage completed in nakshatra (0-100) */
  percentageCompleted: number;
  /** Pada (quarter) of nakshatra (1-4) */
  pada: number;
}

/**
 * Base information for any dasha period
 */
export interface DashaPeriodInfo {
  /** Planet ruling this period */
  planet: string;
  /** Start date-time (UTC) */
  startDateUtc: string;
  /** End date-time (UTC) */
  endDateUtc: string;
  /** Start date-time (Local) */
  startDateLocal: string;
  /** End date-time (Local) */
  endDateLocal: string;
  /** Duration in days */
  durationDays: number;
  /** Duration in years (decimal) */
  durationYears: number;
  /** Whether this period is currently active */
  isCurrentPeriod: boolean;
}

/**
 * Sookshma (4th level) dasha period information
 * This is the deepest level with no further sub-periods
 */
export interface SookshmaInfo extends DashaPeriodInfo {
  // No additional properties - deepest level
}

/**
 * Pratyantardasha (3rd level) dasha period information
 */
export interface PratyantardashaInfo extends DashaPeriodInfo {
  /** Sub-periods (Sookshma) within this Pratyantardasha. Only populated if DetailLevel >= 4 */
  sookshmaPeriods: SookshmaInfo[];
}

/**
 * Antardasha (2nd level) dasha period information
 */
export interface AntardashaInfo extends DashaPeriodInfo {
  /** Sub-periods (Pratyantardasha) within this Antardasha. Only populated if DetailLevel >= 3 */
  pratyantardashaPeriods: PratyantardashaInfo[];
}

/**
 * Mahadasha (1st level) dasha period information
 */
export interface MahadashaInfo extends DashaPeriodInfo {
  /** Whether this Mahadasha started at birth with a balance period */
  isBalancePeriod: boolean;
  /** Remaining years at birth (for balance period) */
  balanceYears: number | null;
  /** Sub-periods (Antardasha) within this Mahadasha. Only populated if DetailLevel >= 2 */
  antardashaPeriods: AntardashaInfo[];
}

/**
 * Complete Vimshottari Dasha calculation response
 */
export interface VimshottariDashaResponse {
  /** Birth date-time (UTC) */
  birthDateTimeUtc: string;
  /** Birth date-time (Local) */
  birthDateTimeLocal: string;
  /** Location information */
  location: LocationInfo;
  /** Moon's nakshatra information at birth */
  birthNakshatra: NakshatraInfo;
  /** Detail level of calculation (1-4) */
  detailLevel: number;
  /** Number of years calculated */
  yearsCalculated: number;
  /** List of Mahadasha periods */
  mahadashaPeriods: MahadashaInfo[];
  /** Current active Mahadasha */
  currentMahadasha: MahadashaInfo | null;
  /** Current active Antardasha (if DetailLevel >= 2) */
  currentAntardasha: AntardashaInfo | null;
  /** Current active Pratyantardasha (if DetailLevel >= 3) */
  currentPratyantardasha: PratyantardashaInfo | null;
  /** Current active Sookshma (if DetailLevel >= 4) */
  currentSookshma: SookshmaInfo | null;
  /** Total number of periods calculated (all levels) */
  totalPeriodsCount: number;
}

/**
 * Request parameters for Vimshottari Dasha calculation
 */
export interface VimshottariDashaRequest {
  /** Birth date (ISO 8601 format: YYYY-MM-DD) */
  birthDate: string;
  /** Birth time in HH:MM format (24-hour) */
  birthTime: string;
  /** Latitude in degrees (-90 to +90) */
  latitude: number;
  /** Longitude in degrees (-180 to +180) */
  longitude: number;
  /** IANA timezone identifier (e.g., "Asia/Kolkata", "America/New_York") */
  timeZoneId: string;
  /** Maximum detail level to calculate (1=Mahadasha, 2=Antardasha, 3=Pratyantardasha, 4=Sookshma). Default: 2 */
  detailLevel?: number;
  /** Number of years from birth to calculate (1-120). Default: 120 */
  yearsToCalculate?: number;
}

/**
 * Dasha detail levels
 */
export enum DashaLevel {
  /** Mahadasha only (~9-10 periods) */
  Mahadasha = 1,
  /** Mahadasha + Antardasha (~90 periods) */
  Antardasha = 2,
  /** + Pratyantardasha (~810 periods) */
  Pratyantardasha = 3,
  /** + Sookshma (~7,290 periods) - Very large response */
  Sookshma = 4,
}

/**
 * Planets used in Vimshottari Dasha system
 */
export type DashaPlanet =
  | 'Sun'      // 6 years
  | 'Moon'     // 10 years
  | 'Mars'     // 7 years
  | 'Rahu'     // 18 years
  | 'Jupiter'  // 16 years
  | 'Saturn'   // 19 years
  | 'Mercury'  // 17 years
  | 'Ketu'     // 7 years
  | 'Venus';   // 20 years

/**
 * Helper type for parsed dates
 */
export interface ParsedDashaPeriod extends Omit<DashaPeriodInfo, 'startDateUtc' | 'endDateUtc' | 'startDateLocal' | 'endDateLocal'> {
  startDateUtc: Date;
  endDateUtc: Date;
  startDateLocal: Date;
  endDateLocal: Date;
}

/**
 * Helper type for parsed Mahadasha with Date objects
 */
export interface ParsedMahadasha extends Omit<MahadashaInfo, 'startDateUtc' | 'endDateUtc' | 'startDateLocal' | 'endDateLocal' | 'antardashaPeriods'> {
  startDateUtc: Date;
  endDateUtc: Date;
  startDateLocal: Date;
  endDateLocal: Date;
  antardashaPeriods: ParsedAntardasha[];
}

/**
 * Helper type for parsed Antardasha with Date objects
 */
export interface ParsedAntardasha extends Omit<AntardashaInfo, 'startDateUtc' | 'endDateUtc' | 'startDateLocal' | 'endDateLocal' | 'pratyantardashaPeriods'> {
  startDateUtc: Date;
  endDateUtc: Date;
  startDateLocal: Date;
  endDateLocal: Date;
  pratyantardashaPeriods: ParsedPratyantardasha[];
}

/**
 * Helper type for parsed Pratyantardasha with Date objects
 */
export interface ParsedPratyantardasha extends Omit<PratyantardashaInfo, 'startDateUtc' | 'endDateUtc' | 'startDateLocal' | 'endDateLocal' | 'sookshmaPeriods'> {
  startDateUtc: Date;
  endDateUtc: Date;
  startDateLocal: Date;
  endDateLocal: Date;
  sookshmaPeriods: ParsedSookshma[];
}

/**
 * Helper type for parsed Sookshma with Date objects
 */
export interface ParsedSookshma extends Omit<SookshmaInfo, 'startDateUtc' | 'endDateUtc' | 'startDateLocal' | 'endDateLocal'> {
  startDateUtc: Date;
  endDateUtc: Date;
  startDateLocal: Date;
  endDateLocal: Date;
}

/**
 * Helper type for parsed response with Date objects instead of strings
 */
export interface ParsedVimshottariDashaResponse extends Omit<VimshottariDashaResponse, 'birthDateTimeUtc' | 'birthDateTimeLocal' | 'mahadashaPeriods' | 'currentMahadasha' | 'currentAntardasha' | 'currentPratyantardasha' | 'currentSookshma'> {
  birthDateTimeUtc: Date;
  birthDateTimeLocal: Date;
  mahadashaPeriods: ParsedMahadasha[];
  currentMahadasha: ParsedMahadasha | null;
  currentAntardasha: ParsedAntardasha | null;
  currentPratyantardasha: ParsedPratyantardasha | null;
  currentSookshma: ParsedSookshma | null;
}
