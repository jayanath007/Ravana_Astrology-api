/**
 * Example React Hook and Component for Vimshottari Dasha API
 * This demonstrates how to use the TypeScript types and utilities
 */

import { useState, useEffect } from 'react';
import {
  VimshottariDashaRequest,
  VimshottariDashaResponse,
  ParsedVimshottariDashaResponse,
  DashaLevel,
} from './vimshottari-dasha.types';
import {
  parseVimshottariDashaResponse,
  formatDashaPeriod,
  formatDateRange,
  formatDuration,
  formatRemainingTime,
  getPlanetColor,
  getElapsedPercentage,
} from './vimshottari-dasha.utils';

/**
 * Custom hook for fetching Vimshottari Dasha data
 */
export function useVimshottariDasha(request: VimshottariDashaRequest) {
  const [data, setData] = useState<ParsedVimshottariDashaResponse | null>(null);
  const [loading, setLoading] = useState<boolean>(false);
  const [error, setError] = useState<Error | null>(null);

  useEffect(() => {
    const fetchDasha = async () => {
      setLoading(true);
      setError(null);

      try {
        const response = await fetch('http://localhost:5188/api/dasha/vimshottari', {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
          },
          body: JSON.stringify(request),
        });

        if (!response.ok) {
          throw new Error(`HTTP error! status: ${response.status}`);
        }

        const rawData: VimshottariDashaResponse = await response.json();
        const parsedData = parseVimshottariDashaResponse(rawData);
        setData(parsedData);
      } catch (err) {
        setError(err instanceof Error ? err : new Error('An error occurred'));
      } finally {
        setLoading(false);
      }
    };

    fetchDasha();
  }, [
    request.birthDate,
    request.birthTime,
    request.latitude,
    request.longitude,
    request.timeZoneId,
    request.detailLevel,
    request.yearsToCalculate,
  ]);

  return { data, loading, error };
}

/**
 * Example component displaying current dasha periods
 */
export function CurrentDashaDisplay() {
  const request: VimshottariDashaRequest = {
    birthDate: '1987-10-09',
    birthTime: '14:41',
    latitude: 6.9271,
    longitude: 79.8612,
    timeZoneId: 'Asia/Colombo',
    detailLevel: DashaLevel.Pratyantardasha,
    yearsToCalculate: 120,
  };

  const { data, loading, error } = useVimshottariDasha(request);

  if (loading) {
    return <div className="loading">Calculating Vimshottari Dasha...</div>;
  }

  if (error) {
    return <div className="error">Error: {error.message}</div>;
  }

  if (!data) {
    return null;
  }

  return (
    <div className="vimshottari-dasha">
      <h2>Vimshottari Dasha</h2>

      {/* Birth Nakshatra Info */}
      <section className="nakshatra-info">
        <h3>Birth Nakshatra</h3>
        <div className="nakshatra-details">
          <p>
            <strong>Nakshatra:</strong> {data.birthNakshatra.nakshatraName} (
            {data.birthNakshatra.nakshatraNumber}/27)
          </p>
          <p>
            <strong>Lord:</strong> {data.birthNakshatra.lord}
          </p>
          <p>
            <strong>Pada:</strong> {data.birthNakshatra.pada}
          </p>
          <p>
            <strong>Completed:</strong> {data.birthNakshatra.percentageCompleted.toFixed(2)}%
          </p>
        </div>
      </section>

      {/* Current Periods */}
      <section className="current-periods">
        <h3>Current Dasha Periods</h3>

        {data.currentMahadasha && (
          <div className="current-period mahadasha">
            <div
              className="period-header"
              style={{ borderLeftColor: getPlanetColor(data.currentMahadasha.planet as any) }}
            >
              <h4>{formatDashaPeriod(data.currentMahadasha.planet, 'Mahadasha')}</h4>
              <span className="duration">{formatDuration(data.currentMahadasha.durationYears)}</span>
            </div>
            <div className="period-details">
              <p>
                {formatDateRange(
                  data.currentMahadasha.startDateLocal,
                  data.currentMahadasha.endDateLocal
                )}
              </p>
              <p className="remaining">
                {formatRemainingTime(data.currentMahadasha.endDateLocal)}
              </p>
              <div className="progress-bar">
                <div
                  className="progress-fill"
                  style={{
                    width: `${getElapsedPercentage(
                      data.currentMahadasha.startDateLocal,
                      data.currentMahadasha.endDateLocal
                    )}%`,
                    backgroundColor: getPlanetColor(data.currentMahadasha.planet as any),
                  }}
                />
              </div>
            </div>
          </div>
        )}

        {data.currentAntardasha && (
          <div className="current-period antardasha">
            <div
              className="period-header"
              style={{ borderLeftColor: getPlanetColor(data.currentAntardasha.planet as any) }}
            >
              <h4>
                {formatDashaPeriod(
                  data.currentMahadasha?.planet || '',
                  'Antardasha',
                  data.currentAntardasha.planet
                )}
              </h4>
              <span className="duration">{formatDuration(data.currentAntardasha.durationYears)}</span>
            </div>
            <div className="period-details">
              <p>
                {formatDateRange(
                  data.currentAntardasha.startDateLocal,
                  data.currentAntardasha.endDateLocal
                )}
              </p>
              <p className="remaining">
                {formatRemainingTime(data.currentAntardasha.endDateLocal)}
              </p>
              <div className="progress-bar">
                <div
                  className="progress-fill"
                  style={{
                    width: `${getElapsedPercentage(
                      data.currentAntardasha.startDateLocal,
                      data.currentAntardasha.endDateLocal
                    )}%`,
                    backgroundColor: getPlanetColor(data.currentAntardasha.planet as any),
                  }}
                />
              </div>
            </div>
          </div>
        )}

        {data.currentPratyantardasha && (
          <div className="current-period pratyantardasha">
            <div
              className="period-header"
              style={{ borderLeftColor: getPlanetColor(data.currentPratyantardasha.planet as any) }}
            >
              <h4>
                {data.currentMahadasha?.planet}-{data.currentAntardasha?.planet}-
                {data.currentPratyantardasha.planet} Pratyantardasha
              </h4>
              <span className="duration">
                {formatDuration(data.currentPratyantardasha.durationYears)}
              </span>
            </div>
            <div className="period-details">
              <p>
                {formatDateRange(
                  data.currentPratyantardasha.startDateLocal,
                  data.currentPratyantardasha.endDateLocal
                )}
              </p>
              <p className="remaining">
                {formatRemainingTime(data.currentPratyantardasha.endDateLocal)}
              </p>
            </div>
          </div>
        )}
      </section>

      {/* All Mahadashas Timeline */}
      <section className="mahadasha-timeline">
        <h3>Mahadasha Timeline</h3>
        <div className="timeline">
          {data.mahadashaPeriods.map((mahadasha, index) => (
            <div
              key={index}
              className={`timeline-item ${mahadasha.isCurrentPeriod ? 'active' : ''}`}
            >
              <div
                className="timeline-marker"
                style={{ backgroundColor: getPlanetColor(mahadasha.planet as any) }}
              />
              <div className="timeline-content">
                <h4>
                  {mahadasha.planet}
                  {mahadasha.isBalancePeriod && ' (Balance)'}
                </h4>
                <p className="dates">
                  {formatDateRange(mahadasha.startDateLocal, mahadasha.endDateLocal)}
                </p>
                <p className="duration">
                  {formatDuration(mahadasha.durationYears)}
                  {mahadasha.isBalancePeriod &&
                    mahadasha.balanceYears &&
                    ` (${formatDuration(mahadasha.balanceYears)} balance)`}
                </p>
                {mahadasha.isCurrentPeriod && (
                  <div className="current-badge">Current</div>
                )}
              </div>
            </div>
          ))}
        </div>
      </section>

      {/* Summary Stats */}
      <section className="summary-stats">
        <h3>Summary</h3>
        <div className="stats-grid">
          <div className="stat">
            <span className="stat-label">Total Mahadashas:</span>
            <span className="stat-value">{data.mahadashaPeriods.length}</span>
          </div>
          <div className="stat">
            <span className="stat-label">Total Periods:</span>
            <span className="stat-value">{data.totalPeriodsCount}</span>
          </div>
          <div className="stat">
            <span className="stat-label">Detail Level:</span>
            <span className="stat-value">
              {data.detailLevel === 1 && 'Mahadasha'}
              {data.detailLevel === 2 && 'Antardasha'}
              {data.detailLevel === 3 && 'Pratyantardasha'}
              {data.detailLevel === 4 && 'Sookshma'}
            </span>
          </div>
          <div className="stat">
            <span className="stat-label">Years Calculated:</span>
            <span className="stat-value">{data.yearsCalculated}</span>
          </div>
        </div>
      </section>
    </div>
  );
}

/**
 * Example CSS styles (use with CSS-in-JS or a stylesheet)
 */
export const exampleStyles = `
.vimshottari-dasha {
  max-width: 1200px;
  margin: 0 auto;
  padding: 20px;
}

.nakshatra-info,
.current-periods,
.mahadasha-timeline,
.summary-stats {
  background: white;
  border-radius: 8px;
  padding: 20px;
  margin-bottom: 20px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

.current-period {
  margin-bottom: 15px;
  border-left: 4px solid;
  padding-left: 15px;
}

.period-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 10px;
}

.progress-bar {
  height: 8px;
  background: #e0e0e0;
  border-radius: 4px;
  overflow: hidden;
  margin-top: 10px;
}

.progress-fill {
  height: 100%;
  transition: width 0.3s ease;
}

.timeline-item {
  display: flex;
  gap: 15px;
  margin-bottom: 20px;
  opacity: 0.6;
}

.timeline-item.active {
  opacity: 1;
}

.timeline-marker {
  width: 12px;
  height: 12px;
  border-radius: 50%;
  margin-top: 6px;
  flex-shrink: 0;
}

.current-badge {
  display: inline-block;
  background: #4caf50;
  color: white;
  padding: 2px 8px;
  border-radius: 4px;
  font-size: 12px;
  margin-top: 5px;
}

.stats-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 15px;
}

.stat {
  display: flex;
  flex-direction: column;
}

.stat-label {
  font-size: 14px;
  color: #666;
}

.stat-value {
  font-size: 24px;
  font-weight: bold;
  color: #333;
}
`;
