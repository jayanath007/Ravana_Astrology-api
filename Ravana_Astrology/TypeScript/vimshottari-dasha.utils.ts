/**
 * Utility functions for working with Vimshottari Dasha API in React
 */

import {
  VimshottariDashaResponse,
  ParsedVimshottariDashaResponse,
  MahadashaInfo,
  ParsedMahadasha,
  AntardashaInfo,
  ParsedAntardasha,
  PratyantardashaInfo,
  ParsedPratyantardasha,
  SookshmaInfo,
  ParsedSookshma,
  DashaPlanet,
} from './vimshottari-dasha.types';

/**
 * Parse a Sookshma period, converting date strings to Date objects
 */
function parseSookshma(sookshma: SookshmaInfo): ParsedSookshma {
  return {
    ...sookshma,
    startDateUtc: new Date(sookshma.startDateUtc),
    endDateUtc: new Date(sookshma.endDateUtc),
    startDateLocal: new Date(sookshma.startDateLocal),
    endDateLocal: new Date(sookshma.endDateLocal),
  };
}

/**
 * Parse a Pratyantardasha period, converting date strings to Date objects
 */
function parsePratyantardasha(pratyantardasha: PratyantardashaInfo): ParsedPratyantardasha {
  return {
    ...pratyantardasha,
    startDateUtc: new Date(pratyantardasha.startDateUtc),
    endDateUtc: new Date(pratyantardasha.endDateUtc),
    startDateLocal: new Date(pratyantardasha.startDateLocal),
    endDateLocal: new Date(pratyantardasha.endDateLocal),
    sookshmaPeriods: pratyantardasha.sookshmaPeriods.map(parseSookshma),
  };
}

/**
 * Parse an Antardasha period, converting date strings to Date objects
 */
function parseAntardasha(antardasha: AntardashaInfo): ParsedAntardasha {
  return {
    ...antardasha,
    startDateUtc: new Date(antardasha.startDateUtc),
    endDateUtc: new Date(antardasha.endDateUtc),
    startDateLocal: new Date(antardasha.startDateLocal),
    endDateLocal: new Date(antardasha.endDateLocal),
    pratyantardashaPeriods: antardasha.pratyantardashaPeriods.map(parsePratyantardasha),
  };
}

/**
 * Parse a Mahadasha period, converting date strings to Date objects
 */
function parseMahadasha(mahadasha: MahadashaInfo): ParsedMahadasha {
  return {
    ...mahadasha,
    startDateUtc: new Date(mahadasha.startDateUtc),
    endDateUtc: new Date(mahadasha.endDateUtc),
    startDateLocal: new Date(mahadasha.startDateLocal),
    endDateLocal: new Date(mahadasha.endDateLocal),
    antardashaPeriods: mahadasha.antardashaPeriods.map(parseAntardasha),
  };
}

/**
 * Parse the entire Vimshottari Dasha response, converting all date strings to Date objects
 * This makes it easier to work with dates in React components
 */
export function parseVimshottariDashaResponse(
  response: VimshottariDashaResponse
): ParsedVimshottariDashaResponse {
  return {
    ...response,
    birthDateTimeUtc: new Date(response.birthDateTimeUtc),
    birthDateTimeLocal: new Date(response.birthDateTimeLocal),
    mahadashaPeriods: response.mahadashaPeriods.map(parseMahadasha),
    currentMahadasha: response.currentMahadasha ? parseMahadasha(response.currentMahadasha) : null,
    currentAntardasha: response.currentAntardasha ? parseAntardasha(response.currentAntardasha) : null,
    currentPratyantardasha: response.currentPratyantardasha
      ? parsePratyantardasha(response.currentPratyantardasha)
      : null,
    currentSookshma: response.currentSookshma ? parseSookshma(response.currentSookshma) : null,
  };
}

/**
 * Format a dasha period as a readable string
 * Example: "Rahu Mahadasha" or "Rahu-Jupiter Antardasha"
 */
export function formatDashaPeriod(
  planet: string,
  level: 'Mahadasha' | 'Antardasha' | 'Pratyantardasha' | 'Sookshma',
  subPlanet?: string
): string {
  if (subPlanet) {
    return `${planet}-${subPlanet} ${level}`;
  }
  return `${planet} ${level}`;
}

/**
 * Format a date range for display
 * Example: "May 15, 1990 - Jun 19, 2026"
 */
export function formatDateRange(
  startDate: Date,
  endDate: Date,
  locale: string = 'en-US'
): string {
  const options: Intl.DateTimeFormatOptions = {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
  };

  const start = startDate.toLocaleDateString(locale, options);
  const end = endDate.toLocaleDateString(locale, options);

  return `${start} - ${end}`;
}

/**
 * Format duration in a human-readable way
 * Example: "10 years, 3 months" or "7 years"
 */
export function formatDuration(years: number): string {
  const wholeYears = Math.floor(years);
  const remainingMonths = Math.round((years - wholeYears) * 12);

  if (remainingMonths === 0) {
    return `${wholeYears} ${wholeYears === 1 ? 'year' : 'years'}`;
  }

  const yearsPart = wholeYears > 0 ? `${wholeYears} ${wholeYears === 1 ? 'year' : 'years'}` : '';
  const monthsPart = `${remainingMonths} ${remainingMonths === 1 ? 'month' : 'months'}`;

  return wholeYears > 0 ? `${yearsPart}, ${monthsPart}` : monthsPart;
}

/**
 * Check if a specific date falls within a dasha period
 */
export function isDateInPeriod(
  date: Date,
  startDate: Date,
  endDate: Date
): boolean {
  return date >= startDate && date < endDate;
}

/**
 * Find the active Mahadasha for a given date
 */
export function findActiveMahadasha(
  mahadashas: ParsedMahadasha[],
  date: Date = new Date()
): ParsedMahadasha | undefined {
  return mahadashas.find((m) =>
    isDateInPeriod(date, m.startDateUtc, m.endDateUtc)
  );
}

/**
 * Find the active Antardasha within a Mahadasha for a given date
 */
export function findActiveAntardasha(
  mahadasha: ParsedMahadasha,
  date: Date = new Date()
): ParsedAntardasha | undefined {
  return mahadasha.antardashaPeriods.find((a) =>
    isDateInPeriod(date, a.startDateUtc, a.endDateUtc)
  );
}

/**
 * Find the active Pratyantardasha within an Antardasha for a given date
 */
export function findActivePratyantardasha(
  antardasha: ParsedAntardasha,
  date: Date = new Date()
): ParsedPratyantardasha | undefined {
  return antardasha.pratyantardashaPeriods.find((p) =>
    isDateInPeriod(date, p.startDateUtc, p.endDateUtc)
  );
}

/**
 * Get the remaining time in a period as a percentage
 * Returns a number between 0 and 100
 */
export function getRemainingPercentage(
  startDate: Date,
  endDate: Date,
  currentDate: Date = new Date()
): number {
  const total = endDate.getTime() - startDate.getTime();
  const elapsed = currentDate.getTime() - startDate.getTime();
  const remaining = total - elapsed;

  return Math.max(0, Math.min(100, (remaining / total) * 100));
}

/**
 * Get the elapsed time in a period as a percentage
 * Returns a number between 0 and 100
 */
export function getElapsedPercentage(
  startDate: Date,
  endDate: Date,
  currentDate: Date = new Date()
): number {
  return 100 - getRemainingPercentage(startDate, endDate, currentDate);
}

/**
 * Get planet color for UI styling (common Vedic astrology colors)
 */
export function getPlanetColor(planet: DashaPlanet): string {
  const colors: Record<DashaPlanet, string> = {
    Sun: '#FFA500',      // Orange
    Moon: '#E0E0E0',     // Silver/White
    Mars: '#DC143C',     // Crimson
    Mercury: '#32CD32',  // Green
    Jupiter: '#FFD700',  // Gold
    Venus: '#FF69B4',    // Pink
    Saturn: '#4B0082',   // Indigo
    Rahu: '#696969',     // Dark Gray
    Ketu: '#808080',     // Gray
  };

  return colors[planet] || '#000000';
}

/**
 * Get nakshatra lord color
 */
export function getNakshatraLordColor(lord: string): string {
  return getPlanetColor(lord as DashaPlanet);
}

/**
 * Calculate days until a period starts or ends
 */
export function daysUntil(date: Date, currentDate: Date = new Date()): number {
  const diff = date.getTime() - currentDate.getTime();
  return Math.ceil(diff / (1000 * 60 * 60 * 24));
}

/**
 * Format remaining time in a human-readable way
 * Example: "2 years, 3 months remaining" or "Ended 5 days ago"
 */
export function formatRemainingTime(
  endDate: Date,
  currentDate: Date = new Date()
): string {
  const days = daysUntil(endDate, currentDate);

  if (days < 0) {
    return `Ended ${Math.abs(days)} ${Math.abs(days) === 1 ? 'day' : 'days'} ago`;
  }

  if (days === 0) {
    return 'Ends today';
  }

  if (days === 1) {
    return 'Ends tomorrow';
  }

  const years = Math.floor(days / 365.25);
  const months = Math.floor((days % 365.25) / 30.44);
  const remainingDays = Math.floor((days % 365.25) % 30.44);

  const parts: string[] = [];

  if (years > 0) {
    parts.push(`${years} ${years === 1 ? 'year' : 'years'}`);
  }

  if (months > 0) {
    parts.push(`${months} ${months === 1 ? 'month' : 'months'}`);
  }

  if (remainingDays > 0 && years === 0) {
    parts.push(`${remainingDays} ${remainingDays === 1 ? 'day' : 'days'}`);
  }

  return parts.length > 0 ? `${parts.join(', ')} remaining` : 'Less than a day remaining';
}

/**
 * Get all planets in Vimshottari sequence
 */
export const VIMSHOTTARI_SEQUENCE: DashaPlanet[] = [
  'Ketu',
  'Venus',
  'Sun',
  'Moon',
  'Mars',
  'Rahu',
  'Jupiter',
  'Saturn',
  'Mercury',
];

/**
 * Get Mahadasha duration in years for each planet
 */
export const MAHADASHA_DURATIONS: Record<DashaPlanet, number> = {
  Sun: 6,
  Moon: 10,
  Mars: 7,
  Rahu: 18,
  Jupiter: 16,
  Saturn: 19,
  Mercury: 17,
  Ketu: 7,
  Venus: 20,
};

/**
 * Get the next planet in Vimshottari sequence
 */
export function getNextPlanet(planet: DashaPlanet): DashaPlanet {
  const currentIndex = VIMSHOTTARI_SEQUENCE.indexOf(planet);
  const nextIndex = (currentIndex + 1) % VIMSHOTTARI_SEQUENCE.length;
  return VIMSHOTTARI_SEQUENCE[nextIndex];
}

/**
 * Get the previous planet in Vimshottari sequence
 */
export function getPreviousPlanet(planet: DashaPlanet): DashaPlanet {
  const currentIndex = VIMSHOTTARI_SEQUENCE.indexOf(planet);
  const prevIndex = (currentIndex - 1 + VIMSHOTTARI_SEQUENCE.length) % VIMSHOTTARI_SEQUENCE.length;
  return VIMSHOTTARI_SEQUENCE[prevIndex];
}
