import { useQuery } from '@tanstack/react-query';

export type MarineWeather = {
  waveHeight: number;      // meters
  windSpeed: number;       // knots
  windDirection: number;   // degrees (0-360)
  temperature: number;     // °C
  visibilityKm: number;    // km
  visibilityNmi: number;   // nautical miles
  raw: any;
};

/**
 * Fetch marine weather from Open-Meteo APIs.
 * Combines Marine API (waves, sea temperature) with Weather API (wind, visibility).
 * Returns values for the nearest hour (UTC) using correct API parameters.
 */
export async function fetchMarineWeather(lat: number, lon: number): Promise<MarineWeather> {
  // Marine API for wave and sea surface temperature data
  const marineUrl = `https://marine-api.open-meteo.com/v1/marine?latitude=${lat}&longitude=${lon}&hourly=wave_height,sea_surface_temperature&windspeed_unit=kn&timezone=UTC`;
  
  // Weather API for wind and visibility data
  const weatherUrl = `https://api.open-meteo.com/v1/forecast?latitude=${lat}&longitude=${lon}&hourly=wind_speed_10m,wind_direction_10m,visibility&windspeed_unit=kn&timezone=UTC`;

  // Fetch both APIs in parallel
  const [marineRes, weatherRes] = await Promise.all([
    fetch(marineUrl),
    fetch(weatherUrl)
  ]);

  if (!marineRes.ok) {
    throw new Error(`Marine API fetch failed: ${marineRes.status} ${marineRes.statusText}`);
  }
  if (!weatherRes.ok) {
    throw new Error(`Weather API fetch failed: ${weatherRes.status} ${weatherRes.statusText}`);
  }

  const [marineData, weatherData] = await Promise.all([
    marineRes.json(),
    weatherRes.json()
  ]);

  const marineTimes: string[] = marineData?.hourly?.time ?? [];
  const weatherTimes: string[] = weatherData?.hourly?.time ?? [];

  if (!marineTimes || marineTimes.length === 0) {
    throw new Error('No hourly data returned from marine API');
  }
  if (!weatherTimes || weatherTimes.length === 0) {
    throw new Error('No hourly data returned from weather API');
  }

  // Current UTC hour (format matches API, e.g., "2025-09-12T09:00")
  const nowUtc = new Date();
  const currentHourIso = nowUtc.toISOString().slice(0, 13) + ':00';

  // Find index for marine data
  let marineIdx = marineTimes.findIndex((t) => t === currentHourIso);
  if (marineIdx === -1) {
    const nowMs = nowUtc.getTime();
    marineIdx = marineTimes
      .map((t) => ({ t, ms: new Date(t).getTime() }))
      .filter((x) => !isNaN(x.ms) && x.ms <= nowMs)
      .reduce((acc, cur, i) => cur.ms >= new Date(marineTimes[acc]).getTime() ? i : acc, 0);

    if (marineIdx < 0 || marineIdx >= marineTimes.length) marineIdx = Math.max(0, marineTimes.length - 1);
  }

  // Find index for weather data
  let weatherIdx = weatherTimes.findIndex((t) => t === currentHourIso);
  if (weatherIdx === -1) {
    const nowMs = nowUtc.getTime();
    weatherIdx = weatherTimes
      .map((t) => ({ t, ms: new Date(t).getTime() }))
      .filter((x) => !isNaN(x.ms) && x.ms <= nowMs)
      .reduce((acc, cur, i) => cur.ms >= new Date(weatherTimes[acc]).getTime() ? i : acc, 0);

    if (weatherIdx < 0 || weatherIdx >= weatherTimes.length) weatherIdx = Math.max(0, weatherTimes.length - 1);
  }

  const waveHeight = Number(marineData.hourly?.wave_height?.[marineIdx] ?? 0);
  const temperature = Number(marineData.hourly?.sea_surface_temperature?.[marineIdx] ?? 0);
  const windSpeed = Number(weatherData.hourly?.wind_speed_10m?.[weatherIdx] ?? 0);
  const windDirection = Number(weatherData.hourly?.wind_direction_10m?.[weatherIdx] ?? 0);
  
  // Convert visibility from meters to km and nautical miles
  const visibilityMeters = Number(weatherData.hourly?.visibility?.[weatherIdx] ?? 10000); // Default 10km in meters
  const visibilityKm = visibilityMeters / 1000;
  const visibilityNmi = visibilityKm * 0.539957; // km → nautical miles

  return { waveHeight, windSpeed, windDirection, temperature, visibilityKm, visibilityNmi, raw: { marine: marineData, weather: weatherData } };
}

/**
 * React Query v5 hook wrapper for marine weather
 */
export const useMarineWeather = (lat: number, lon: number) => {
  return useQuery<MarineWeather>({
    queryKey: ['marine-weather', lat, lon],
    queryFn: () => fetchMarineWeather(lat, lon),
    staleTime: 60_000,           // 1 minute
    refetchInterval: 5 * 60_000, // auto refresh every 5 minutes
    retry: 1,
  });
};