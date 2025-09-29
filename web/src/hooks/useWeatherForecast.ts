import { useQuery } from '@tanstack/react-query';

export type WeatherForecastData = {
  datetime: string;
  temperature: number;           // °C
  temperatureMax: number;        // °C
  temperatureMin: number;        // °C
  humidity: number;              // %
  pressure: number;              // hPa
  windSpeed: number;             // knots
  windDirection: number;         // degrees (0-360)
  windGust: number;              // knots
  precipitation: number;         // mm
  precipitationProbability: number; // %
  cloudCover: number;            // %
  visibility: number;            // km
  uvIndex: number;               // UV index
  dewPoint: number;              // °C
  apparentTemperature: number;   // °C (feels like)
  weatherCode: number;           // WMO weather code
  // Marine specific (only for coastal/marine locations)
  waveHeight?: number;           // meters
  waveDirection?: number;        // degrees
  wavePeriod?: number;           // seconds
  seaTemperature?: number;       // °C
  swellHeight?: number;          // meters
  swellDirection?: number;       // degrees
  swellPeriod?: number;          // seconds
};

export type WeatherForecast = {
  location: {
    latitude: number;
    longitude: number;
    timezone: string;
    elevation: number;
  };
  current: WeatherForecastData;
  hourly: WeatherForecastData[];
  daily: WeatherForecastData[];
  raw: {
    weather: any;
    marine?: any;
  };
};

/**
 * Get weather description from WMO weather code
 */
export function getWeatherDescription(code: number, t: (key: string) => string): string {
    const weatherCodes: { [key: number]: string } = {
      0: t('weatherClear'),
      1: t('weatherMainlyClear'),
      2: t('weatherPartlyCloudy'),
      3: t('weatherOvercast'),
      45: t('weatherFog'),
      48: t('weatherFog'),
      51: t('weatherDrizzleLight'),
      53: t('weatherDrizzleModerate'),
      55: t('weatherDrizzleDense'),
      56: t('weatherDrizzleLight'),
      57: t('weatherDrizzleDense'),
      61: t('weatherRainLight'),
      63: t('weatherRainModerate'),
      65: t('weatherRainHeavy'),
      66: t('weatherRainLight'),
      67: t('weatherRainHeavy'),
      71: t('weatherSnowLight'),
      73: t('weatherSnowModerate'),
      75: t('weatherSnowHeavy'),
      77: t('weatherSnowLight'),
      80: t('weatherShowers'),
      81: t('weatherShowers'),
      82: t('weatherShowers'),
      85: t('weatherSnowLight'),
      86: t('weatherSnowHeavy'),
      95: t('weatherThunderstorm'),
      96: t('weatherThunderstorm'),
      99: t('weatherThunderstorm')
    };
    return weatherCodes[code] || 'Unknown';
  }

/**
 * Fetch comprehensive weather forecast from Open-Meteo APIs
 * Combines Weather API with Marine API for coastal locations
 */
export async function fetchWeatherForecast(
  lat: number, 
  lon: number, 
  days: number = 7,
  includeMarine: boolean = true
): Promise<WeatherForecast> {
  console.log('Fetching weather for coordinates:', { lat, lon, days });
  // Comprehensive weather parameters
  const weatherParams = [
    'temperature_2m',
    'temperature_2m_max',
    'temperature_2m_min',
    'relative_humidity_2m',
    'dew_point_2m',
    'apparent_temperature',
    'surface_pressure',
    'cloud_cover',
    'visibility',
    'wind_speed_10m',
    'wind_direction_10m',
    'wind_gusts_10m',
    'precipitation',
    'precipitation_probability',
    'weather_code',
    'uv_index'
  ].join(',');

  // Weather API URL with comprehensive parameters
  const weatherUrl = `https://api.open-meteo.com/v1/forecast?` +
    `latitude=${lat}&longitude=${lon}&` +
    `current=${weatherParams}&` +
    `hourly=${weatherParams}&` +
    `daily=temperature_2m_max,temperature_2m_min,precipitation_sum,precipitation_probability_max,wind_speed_10m_max,wind_gusts_10m_max,weather_code,uv_index_max&` +
    `windspeed_unit=kn&` +
    `timezone=UTC&` +
    `forecast_days=${days}`;

  // Marine API URL (if requested)
  const marineUrl = includeMarine ? 
    `https://marine-api.open-meteo.com/v1/marine?` +
    `latitude=${lat}&longitude=${lon}&` +
    `current=wave_height,wave_direction,wave_period,swell_wave_height,swell_wave_direction,swell_wave_period,sea_surface_temperature&` +
    `hourly=wave_height,wave_direction,wave_period,swell_wave_height,swell_wave_direction,swell_wave_period,sea_surface_temperature&` +
    `daily=wave_height_max,swell_wave_height_max&` +
    `timezone=UTC&` +
    `forecast_days=${days}` : null;

  // Log API URLs for debugging
  console.log('Weather API URL:', weatherUrl);
  if (marineUrl) {
    console.log('Marine API URL:', marineUrl);
  }

  // Fetch APIs
  const promises = [fetch(weatherUrl)];
  if (marineUrl) {
    promises.push(fetch(marineUrl));
  }

  const responses = await Promise.all(promises);
  const weatherRes = responses[0];
  const marineRes = responses[1];

  if (!weatherRes.ok) {
    throw new Error(`Weather API fetch failed: ${weatherRes.status} ${weatherRes.statusText}`);
  }

  const weatherData = await weatherRes.json();
  console.log('Weather API Response:', weatherData);
  let marineData = null;

  if (marineRes) {
    if (marineRes.ok) {
      marineData = await marineRes.json();
      console.log('Marine API Response:', marineData);
    } else {
      console.warn('Marine API failed, continuing with weather data only');
    }
  }

  // Helper function to create weather data object
  const createWeatherData = (
    index: number,
    timeArray: string[],
    weatherHourly: any,
    marineHourly?: any
  ): WeatherForecastData => {
    const datetime = timeArray[index];
    
    const weatherData = {
      datetime,
      temperature: Number(weatherHourly.temperature_2m?.[index] ?? 0),
      temperatureMax: Number(weatherHourly.temperature_2m_max?.[index] ?? weatherHourly.temperature_2m?.[index] ?? 0),
      temperatureMin: Number(weatherHourly.temperature_2m_min?.[index] ?? weatherHourly.temperature_2m?.[index] ?? 0),
      humidity: Number(weatherHourly.relative_humidity_2m?.[index] ?? 0),
      pressure: Number(weatherHourly.pressure_msl?.[index] ?? weatherHourly.surface_pressure?.[index] ?? 1013),
      windSpeed: Number(weatherHourly.wind_speed_10m?.[index] ?? 0),
      windDirection: Number(weatherHourly.wind_direction_10m?.[index] ?? 0),
      windGust: Number(weatherHourly.wind_gusts_10m?.[index] ?? 0),
      precipitation: Number(weatherHourly.precipitation?.[index] ?? weatherHourly.precipitation_sum?.[index] ?? 0),
      precipitationProbability: Number(weatherHourly.precipitation_probability?.[index] ?? weatherHourly.precipitation_probability_max?.[index] ?? 0),
      cloudCover: Number(weatherHourly.cloud_cover?.[index] ?? 0),
      visibility: Number(weatherHourly.visibility?.[index] ?? 10000) / 1000, // Convert to km
      uvIndex: Number(weatherHourly.uv_index?.[index] ?? weatherHourly.uv_index_max?.[index] ?? 0),
      dewPoint: Number(weatherHourly.dew_point_2m?.[index] ?? 0),
      apparentTemperature: Number(weatherHourly.apparent_temperature?.[index] ?? weatherHourly.temperature_2m?.[index] ?? 0),
      weatherCode: Number(weatherHourly.weather_code?.[index] ?? 0),
      // Marine data (if available)
      waveHeight: marineHourly ? Number(marineHourly.wave_height?.[index] ?? 0) : undefined,
      waveDirection: marineHourly ? Number(marineHourly.wave_direction?.[index] ?? 0) : undefined,
      wavePeriod: marineHourly ? Number(marineHourly.wave_period?.[index] ?? 0) : undefined,
      seaTemperature: marineHourly ? Number(marineHourly.sea_surface_temperature?.[index] ?? 0) : undefined,
      swellHeight: marineHourly ? Number(marineHourly.swell_wave_height?.[index] ?? 0) : undefined,
      swellDirection: marineHourly ? Number(marineHourly.swell_wave_direction?.[index] ?? 0) : undefined,
      swellPeriod: marineHourly ? Number(marineHourly.swell_wave_period?.[index] ?? 0) : undefined,
    };
    //console.log('Creating weather data for index', index, ':', weatherData);
    return weatherData;
  };

  // Process current weather
  const currentTime = weatherData.current?.time || new Date().toISOString();
  const current = {
    datetime: currentTime,
    temperature: Number(weatherData.current?.temperature_2m ?? 0),
    temperatureMax: Number(weatherData.current?.temperature_2m ?? 0), // Use current temp for max
    temperatureMin: Number(weatherData.current?.temperature_2m ?? 0), // Use current temp for min
    humidity: Number(weatherData.current?.relative_humidity_2m ?? 0),
    pressure: Number(weatherData.current?.pressure_msl ?? weatherData.current?.surface_pressure ?? 1013),
    windSpeed: Number(weatherData.current?.wind_speed_10m ?? 0),
    windDirection: Number(weatherData.current?.wind_direction_10m ?? 0),
    windGust: Number(weatherData.current?.wind_gusts_10m ?? 0),
    precipitation: Number(weatherData.current?.precipitation ?? 0),
    precipitationProbability: 0, // Not available in current data
    cloudCover: Number(weatherData.current?.cloud_cover ?? 0),
    visibility: Number(weatherData.current?.visibility ?? 10000) / 1000, // Convert to km
    uvIndex: Number(weatherData.current?.uv_index ?? 0),
    dewPoint: 0, // Not available in current data
    apparentTemperature: Number(weatherData.current?.apparent_temperature ?? 0),
    weatherCode: Number(weatherData.current?.weather_code ?? 0),
    // Marine data for current conditions
    waveHeight: marineData?.current ? Number(marineData.current.wave_height ?? 0) : undefined,
    waveDirection: marineData?.current ? Number(marineData.current.wave_direction ?? 0) : undefined,
    wavePeriod: marineData?.current ? Number(marineData.current.wave_period ?? 0) : undefined,
    seaTemperature: marineData?.current ? Number(marineData.current.sea_surface_temperature ?? 0) : undefined,
    swellHeight: marineData?.current ? Number(marineData.current.swell_wave_height ?? 0) : undefined,
    swellDirection: marineData?.current ? Number(marineData.current.swell_wave_direction ?? 0) : undefined,
    swellPeriod: marineData?.current ? Number(marineData.current.swell_wave_period ?? 0) : undefined,
  };
  console.log('Current weather data:', current);

  // Process hourly forecast
  const hourlyTimes = weatherData.hourly?.time ?? [];
  const hourly = hourlyTimes.map((_: string, index: number) => 
    createWeatherData(index, hourlyTimes, weatherData.hourly, marineData?.hourly)
  );

  // Process daily forecast
  const dailyTimes = weatherData.daily?.time ?? [];
  const daily = dailyTimes.map((_: string, index: number) => 
    createWeatherData(index, dailyTimes, weatherData.daily, marineData?.daily)
  );

  return {
    location: {
      latitude: lat,
      longitude: lon,
      timezone: weatherData.timezone || 'UTC',
      elevation: weatherData.elevation || 0,
    },
    current,
    hourly,
    daily,
    raw: {
      weather: weatherData,
      marine: marineData,
    },
  };
}

/**
 * React Query hook for weather forecast
 */
export const useWeatherForecast = (
  lat: number, 
  lon: number, 
  days: number = 7,
  includeMarine: boolean = true,
  enabled: boolean = true
) => {
  return useQuery<WeatherForecast>({
    queryKey: ['weather-forecast', lat, lon, days, includeMarine],
    queryFn: () => fetchWeatherForecast(lat, lon, days, includeMarine),
    staleTime: 10 * 60_000,        // 10 minutes
    refetchInterval: 30 * 60_000,  // auto refresh every 30 minutes
    retry: 2,
    enabled: enabled && !isNaN(lat) && !isNaN(lon),
  });
};

/**
 * Hook for current weather only (faster loading)
 */
export const useCurrentWeather = (
  lat: number, 
  lon: number, 
  includeMarine: boolean = true,
  enabled: boolean = true
) => {
  return useQuery<WeatherForecast>({
    queryKey: ['current-weather', lat, lon, includeMarine],
    queryFn: () => fetchWeatherForecast(lat, lon, 1, includeMarine),
    staleTime: 5 * 60_000,         // 5 minutes
    refetchInterval: 10 * 60_000,  // auto refresh every 10 minutes
    retry: 2,
    enabled: enabled && !isNaN(lat) && !isNaN(lon),
  });
};