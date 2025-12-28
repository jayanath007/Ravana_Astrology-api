# Vimshottari Dasha TypeScript Types & Utilities

TypeScript definitions and utilities for integrating the Ravana Astrology Vimshottari Dasha API with React applications.

## Files Overview

### 📘 `vimshottari-dasha.types.ts`
Complete TypeScript type definitions matching the C# API models.

**Key Types:**
- `VimshottariDashaRequest` - Request parameters
- `VimshottariDashaResponse` - API response
- `MahadashaInfo`, `AntardashaInfo`, `PratyantardashaInfo`, `SookshmaInfo` - Period types
- `NakshatraInfo` - Moon's nakshatra information
- `DashaPlanet` - Union type for planet names
- `DashaLevel` - Enum for detail levels
- `ParsedVimshottariDashaResponse` - Response with Date objects instead of strings

### 🛠️ `vimshottari-dasha.utils.ts`
Utility functions for working with dasha data in React.

**Key Functions:**
- `parseVimshottariDashaResponse()` - Convert date strings to Date objects
- `formatDashaPeriod()` - Format period names
- `formatDateRange()` - Format date ranges
- `formatDuration()` - Format durations (e.g., "10 years, 3 months")
- `formatRemainingTime()` - Format remaining time
- `getPlanetColor()` - Get planet colors for UI
- `getElapsedPercentage()` - Calculate progress percentage
- `findActiveMahadasha()` - Find active period for a date

### ⚛️ `useVimshottariDasha.example.tsx`
Example React hook and component demonstrating usage.

**Includes:**
- `useVimshottariDasha` - Custom hook for API calls
- `CurrentDashaDisplay` - Full-featured React component
- Example CSS styles

## Installation

Copy the TypeScript files to your React project:

```bash
# Copy to your project's types directory
src/
  types/
    vimshottari-dasha.types.ts
    vimshottari-dasha.utils.ts
  hooks/
    useVimshottariDasha.tsx
  components/
    VimshottariDasha.tsx
```

## Basic Usage

### 1. Simple API Call

```typescript
import { VimshottariDashaRequest } from './types/vimshottari-dasha.types';

const request: VimshottariDashaRequest = {
  birthDate: '1987-10-09',
  birthTime: '14:41',
  latitude: 6.9271,
  longitude: 79.8612,
  timeZoneId: 'Asia/Colombo',
  detailLevel: 2, // Mahadasha + Antardasha
  yearsToCalculate: 120,
};

const response = await fetch('http://localhost:5188/api/dasha/vimshottari', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify(request),
});

const data = await response.json();
```

### 2. Using the Custom Hook

```typescript
import { useVimshottariDasha } from './hooks/useVimshottariDasha';
import { DashaLevel } from './types/vimshottari-dasha.types';

function MyComponent() {
  const { data, loading, error } = useVimshottariDasha({
    birthDate: '1987-10-09',
    birthTime: '14:41',
    latitude: 6.9271,
    longitude: 79.8612,
    timeZoneId: 'Asia/Colombo',
    detailLevel: DashaLevel.Antardasha,
    yearsToCalculate: 120,
  });

  if (loading) return <div>Loading...</div>;
  if (error) return <div>Error: {error.message}</div>;
  if (!data) return null;

  return (
    <div>
      <h2>Current Mahadasha: {data.currentMahadasha?.planet}</h2>
      <h3>Current Antardasha: {data.currentAntardasha?.planet}</h3>
    </div>
  );
}
```

### 3. Parsing Dates

```typescript
import { parseVimshottariDashaResponse } from './utils/vimshottari-dasha.utils';

// Raw response from API (dates are strings)
const rawResponse = await fetch(...).then(r => r.json());

// Parsed response (dates are Date objects)
const parsedData = parseVimshottariDashaResponse(rawResponse);

// Now you can use Date methods
const startDate = parsedData.currentMahadasha.startDateLocal; // Date object
const formattedDate = startDate.toLocaleDateString(); // "10/9/1987"
```

### 4. Formatting Utilities

```typescript
import {
  formatDashaPeriod,
  formatDateRange,
  formatDuration,
  formatRemainingTime,
  getPlanetColor,
} from './utils/vimshottari-dasha.utils';

// Format period names
formatDashaPeriod('Rahu', 'Mahadasha'); // "Rahu Mahadasha"
formatDashaPeriod('Rahu', 'Antardasha', 'Jupiter'); // "Rahu-Jupiter Antardasha"

// Format dates
formatDateRange(startDate, endDate); // "May 15, 1990 - Jun 19, 2026"

// Format durations
formatDuration(10.5); // "10 years, 6 months"
formatDuration(7); // "7 years"

// Format remaining time
formatRemainingTime(endDate); // "2 years, 3 months remaining"

// Get planet colors
getPlanetColor('Rahu'); // "#696969"
```

## API Response Structure

```typescript
{
  // Birth information
  birthDateTimeUtc: "1987-10-09T09:11:00Z",
  birthDateTimeLocal: "1987-10-09T14:41:00",
  location: {
    latitude: 6.9271,
    longitude: 79.8612,
    timeZone: "Asia/Colombo"
  },

  // Nakshatra at birth
  birthNakshatra: {
    nakshatraNumber: 15,
    nakshatraName: "Swati",
    lord: "Rahu",
    moonLongitude: 193.45,
    degreesInNakshatra: 6.78,
    percentageCompleted: 50.85,
    pada: 3
  },

  // Calculation parameters
  detailLevel: 2,
  yearsCalculated: 120,
  totalPeriodsCount: 87,

  // Mahadasha periods
  mahadashaPeriods: [
    {
      planet: "Rahu",
      startDateUtc: "1987-10-09T09:11:00Z",
      endDateUtc: "1996-08-06T09:11:00Z",
      isBalancePeriod: true,
      balanceYears: 8.847,
      antardashaPeriods: [...] // 9 Antardashas
    },
    // ... more Mahadashas
  ],

  // Currently active periods
  currentMahadasha: {...},
  currentAntardasha: {...},
  currentPratyantardasha: null,
  currentSookshma: null
}
```

## Detail Levels

| Level | Name | Periods | Use Case |
|-------|------|---------|----------|
| 1 | Mahadasha | ~9-10 | Quick overview |
| 2 | Antardasha | ~90 | **Default** - Balanced detail |
| 3 | Pratyantardasha | ~810 | Detailed analysis |
| 4 | Sookshma | ~7,290 | **Very large** - Complete precision |

## Constants

### Vimshottari Sequence
```typescript
['Ketu', 'Venus', 'Sun', 'Moon', 'Mars', 'Rahu', 'Jupiter', 'Saturn', 'Mercury']
```

### Mahadasha Durations (Years)
```typescript
{
  Sun: 6,
  Moon: 10,
  Mars: 7,
  Rahu: 18,
  Jupiter: 16,
  Saturn: 19,
  Mercury: 17,
  Ketu: 7,
  Venus: 20
}
// Total: 120 years
```

### Planet Colors (Vedic)
```typescript
{
  Sun: '#FFA500',    // Orange
  Moon: '#E0E0E0',   // Silver
  Mars: '#DC143C',   // Crimson
  Mercury: '#32CD32', // Green
  Jupiter: '#FFD700', // Gold
  Venus: '#FF69B4',   // Pink
  Saturn: '#4B0082',  // Indigo
  Rahu: '#696969',    // Dark Gray
  Ketu: '#808080'     // Gray
}
```

## Advanced Examples

### Finding Active Periods

```typescript
import { findActiveMahadasha, findActiveAntardasha } from './utils/vimshottari-dasha.utils';

const mahadasha = findActiveMahadasha(data.mahadashaPeriods);
const antardasha = mahadasha ? findActiveAntardasha(mahadasha) : null;

console.log(`Current: ${mahadasha?.planet} - ${antardasha?.planet}`);
```

### Progress Indicator

```typescript
import { getElapsedPercentage } from './utils/vimshottari-dasha.utils';

function ProgressBar({ period }) {
  const progress = getElapsedPercentage(
    period.startDateLocal,
    period.endDateLocal
  );

  return (
    <div style={{ width: '100%', background: '#eee' }}>
      <div
        style={{
          width: `${progress}%`,
          background: getPlanetColor(period.planet),
          height: '8px',
        }}
      />
    </div>
  );
}
```

### Timeline Visualization

```typescript
function DashaTimeline({ mahadashas }) {
  return (
    <div className="timeline">
      {mahadashas.map((m, i) => (
        <div
          key={i}
          className={m.isCurrentPeriod ? 'active' : ''}
          style={{ borderLeftColor: getPlanetColor(m.planet) }}
        >
          <h4>{m.planet}</h4>
          <p>{formatDateRange(m.startDateLocal, m.endDateLocal)}</p>
          <p>{formatDuration(m.durationYears)}</p>
          {m.isCurrentPeriod && <span>Current</span>}
        </div>
      ))}
    </div>
  );
}
```

## Performance Tips

1. **Choose appropriate detail level:**
   - Use Level 1 for overviews
   - Use Level 2 for most applications (default)
   - Avoid Level 4 unless absolutely necessary

2. **Limit years calculated:**
   - Use `yearsToCalculate: 50` for near-term analysis
   - Full 120-year cycle only when needed

3. **Cache responses:**
   - Dasha periods don't change for a birth chart
   - Cache by birth data + detail level

4. **Lazy load nested periods:**
   - Load Mahadashas first
   - Load Antardashas on demand when user expands

## API Endpoint

```
POST http://localhost:5188/api/dasha/vimshottari
Content-Type: application/json
```

For production, update the base URL in your API client.

## Type Safety Benefits

✅ **Autocomplete** - IntelliSense for all properties
✅ **Type checking** - Catch errors at compile time
✅ **Refactoring** - Safe property renames
✅ **Documentation** - Types serve as inline docs
✅ **Consistency** - Matches C# API models exactly

## License

These TypeScript definitions are provided as part of the Ravana Astrology API project.
