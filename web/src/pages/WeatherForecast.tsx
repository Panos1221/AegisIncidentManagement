import React, { useState, useMemo } from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '../components/ui/card';
import { Button } from '../components/ui/button';
import {
  Cloud,
  CloudRain,
  Sun,
  Thermometer,
  Droplets,
  Eye,
  Gauge,
  Waves,
  Calendar,
  Clock,
  Filter,
  RefreshCw,
  MapPin
} from 'lucide-react';
import { useWeatherForecast, getWeatherDescription } from '../hooks/useWeatherForecast';
import { useGeolocation } from '../hooks/useGeolocation';
import { useTranslation } from '../hooks/useTranslation';
import { useUserStore } from '../lib/userStore';
import { WindDirection } from '../components/Weather/WindDirection';
import { getSeaState, getWindSpeedUnit } from '../lib/weatherHelpers';
import { LoadingSpinner } from '../components';
import { format, addDays, startOfDay, endOfDay, isWithinInterval } from 'date-fns';

type ViewMode = 'current' | 'hourly' | 'daily';
type FilterPeriod = 'today' | 'tomorrow' | '3days' | '7days' | 'custom';

const WeatherForecast: React.FC = () => {
  const { user } = useUserStore();
  const t = useTranslation();
  const { latitude, longitude, error: locationError } = useGeolocation();
  
  // State for filters and view mode
  const [viewMode, setViewMode] = useState<ViewMode>('current');
  const [filterPeriod, setFilterPeriod] = useState<FilterPeriod>('today');
  const [customStartDate, setCustomStartDate] = useState<string>(
    format(new Date(), 'yyyy-MM-dd')
  );
  const [customEndDate, setCustomEndDate] = useState<string>(
    format(addDays(new Date(), 7), 'yyyy-MM-dd')
  );
  const [showMarineData, setShowMarineData] = useState<boolean>(
    user?.agencyName === 'Hellenic Coast Guard'
  );

  // Determine forecast days based on filter
  const forecastDays = useMemo(() => {
    switch (filterPeriod) {
      case 'today':
      case 'tomorrow':
        return 2;
      case '3days':
        return 3;
      case '7days':
        return 7;
      case 'custom':
        const start = new Date(customStartDate);
        const end = new Date(customEndDate);
        const diffTime = Math.abs(end.getTime() - start.getTime());
        const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
        return Math.min(Math.max(diffDays, 1), 16); // API limit is 16 days
      default:
        return 7;
    }
  }, [filterPeriod, customStartDate, customEndDate]);

  // Fetch weather data
  const {
    data: forecast,
    isLoading,
    error,
    refetch
  } = useWeatherForecast(
    latitude || 37.9755, // Default to Athens
    longitude || 23.7348,
    forecastDays,
    showMarineData,
    !!(latitude && longitude)
  );

  // Filter data based on selected period
  const filteredData = useMemo(() => {
    if (!forecast) return { hourly: [], daily: [] };

    const now = new Date();
    let startDate: Date;
    let endDate: Date;

    switch (filterPeriod) {
      case 'today':
        startDate = startOfDay(now);
        endDate = endOfDay(now);
        break;
      case 'tomorrow':
        const tomorrow = addDays(now, 1);
        startDate = startOfDay(tomorrow);
        endDate = endOfDay(tomorrow);
        break;
      case '3days':
        startDate = startOfDay(now);
        endDate = endOfDay(addDays(now, 2));
        break;
      case '7days':
        startDate = startOfDay(now);
        endDate = endOfDay(addDays(now, 6));
        break;
      case 'custom':
        startDate = startOfDay(new Date(customStartDate));
        endDate = endOfDay(new Date(customEndDate));
        break;
      default:
        startDate = startOfDay(now);
        endDate = endOfDay(addDays(now, 6));
    }

    const hourly = forecast.hourly.filter(item => {
      const itemDate = new Date(item.datetime);
      return isWithinInterval(itemDate, { start: startDate, end: endDate });
    });

    const daily = forecast.daily.filter(item => {
      const itemDate = new Date(item.datetime);
      return isWithinInterval(itemDate, { start: startDate, end: endDate });
    });

    return { hourly, daily };
  }, [forecast, filterPeriod, customStartDate, customEndDate]);

  // Get weather icon based on weather code
  const getWeatherIcon = (code: number, size: number = 24) => {
    if (code === 0 || code === 1) return <Sun size={size} className="text-yellow-500" />;
    if (code === 2 || code === 3) return <Cloud size={size} className="text-gray-500" />;
    if (code >= 51 && code <= 67) return <CloudRain size={size} className="text-blue-500" />;
    if (code >= 71 && code <= 86) return <Cloud size={size} className="text-blue-300" />;
    if (code >= 95) return <CloudRain size={size} className="text-purple-500" />;
    return <Cloud size={size} className="text-gray-400" />;
  };

  // Render current weather card
  const renderCurrentWeather = () => {
    if (!forecast?.current) return null;

    const current = forecast.current;
    return (
      <Card className="bg-white dark:bg-gray-800 border-gray-200 dark:border-gray-700">
        <CardHeader>
          <CardTitle className="flex items-center gap-2 text-gray-900 dark:text-white">
            {getWeatherIcon(current.weatherCode, 32)}
            {t.currentWeather}
          </CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            {/* Temperature */}
            <div className="space-y-4">
              <div className="flex items-center gap-3">
                <Thermometer className="h-5 w-5 text-red-500" />
                <div>
                  <div className="text-sm text-gray-600 dark:text-gray-400">{t.temperature}</div>
                  <div className="text-2xl font-bold text-gray-900 dark:text-white">
                    {Math.round(current.temperature)}°C
                  </div>
                  <div className="text-sm text-gray-500 dark:text-gray-400">
                    {t.feelsLike} {Math.round(current.apparentTemperature)}°C
                  </div>
                </div>
              </div>
              
              <div className="flex items-center gap-3">
                <Droplets className="h-5 w-5 text-blue-500" />
                <div>
                  <div className="text-sm text-gray-600 dark:text-gray-400">{t.humidity}</div>
                  <div className="text-lg font-semibold text-gray-900 dark:text-white">
                    {Math.round(current.humidity)}%
                  </div>
                </div>
              </div>
            </div>

            {/* Wind */}
            <div className="space-y-4">
              <div className="flex items-center gap-4">
                <WindDirection degrees={current.windDirection} size={48} t={t} />
                <div>
                  <div className="text-sm text-gray-600 dark:text-gray-400">{t.wind}</div>
                  <div className="text-lg font-semibold text-gray-900 dark:text-white">
                    {Math.round(current.windSpeed)} {getWindSpeedUnit(Math.round(current.windSpeed), t)}
                  </div>
                  <div className="text-sm text-gray-500 dark:text-gray-400">
                    {t.gusts}: {Math.round(current.windGust)} {getWindSpeedUnit(Math.round(current.windGust), t)}
                  </div>
                </div>
              </div>
            </div>

            {/* Additional Data */}
            <div className="space-y-4">
              <div className="flex items-center gap-3">
                <Gauge className="h-5 w-5 text-purple-500" />
                <div>
                  <div className="text-sm text-gray-600 dark:text-gray-400">{t.pressure}</div>
                  <div className="text-lg font-semibold text-gray-900 dark:text-white">
                    {Math.round(current.pressure)} hPa
                  </div>
                </div>
              </div>
              
              <div className="flex items-center gap-3">
                <Eye className="h-5 w-5 text-green-500" />
                <div>
                  <div className="text-sm text-gray-600 dark:text-gray-400">{t.visibility}</div>
                  <div className="text-lg font-semibold text-gray-900 dark:text-white">
                    {Math.round(current.visibility)} km
                  </div>
                </div>
              </div>
            </div>
          </div>

          {/* Marine Data */}
          {showMarineData && current.waveHeight !== undefined && (
            <div className="mt-6 pt-6 border-t border-gray-200 dark:border-gray-600">
              <h4 className="text-lg font-semibold text-gray-900 dark:text-white mb-4 flex items-center gap-2">
                <Waves className="h-5 w-5 text-blue-500" />
                {t.marineConditions}
              </h4>
              <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                <div>
                  <div className="text-sm text-gray-600 dark:text-gray-400">{t.waveHeight}</div>
                  <div className="text-lg font-semibold text-gray-900 dark:text-white">
                    {current.waveHeight?.toFixed(1)} m
                  </div>
                  <div className="text-sm text-gray-500 dark:text-gray-400">
                    {t[getSeaState(current.waveHeight) as keyof typeof t]}
                  </div>
                </div>
                <div>
                  <div className="text-sm text-gray-600 dark:text-gray-400">{t.seaTemperature}</div>
                  <div className="text-lg font-semibold text-gray-900 dark:text-white">
                    {current.seaTemperature?.toFixed(1)}°C
                  </div>
                </div>
                <div>
                  <div className="text-sm text-gray-600 dark:text-gray-400">{t.swellHeight}</div>
                  <div className="text-lg font-semibold text-gray-900 dark:text-white">
                    {current.swellHeight?.toFixed(1)} m
                  </div>
                </div>
              </div>
            </div>
          )}

          {/* Weather Description */}
          <div className="mt-6 pt-6 border-t border-gray-200 dark:border-gray-600">
            <div className="text-center">
              <div className="text-lg font-medium text-gray-900 dark:text-white">
                {getWeatherDescription(current.weatherCode, (key: string) => t[key as keyof typeof t] || key)}
              </div>
              <div className="text-sm text-gray-500 dark:text-gray-400 mt-1">
                {t.cloudCover}: {Math.round(current.cloudCover)}% • {t.uvIndex}: {current.uvIndex}
              </div>
            </div>
          </div>
        </CardContent>
      </Card>
    );
  };

  // Group hourly data by day
  const groupedHourlyData = useMemo(() => {
    if (!filteredData.hourly.length) return {};
    
    return filteredData.hourly.reduce((groups, hour) => {
      const date = format(new Date(hour.datetime), 'yyyy-MM-dd');
      if (!groups[date]) {
        groups[date] = [];
      }
      groups[date].push(hour);
      return groups;
    }, {} as Record<string, typeof filteredData.hourly>);
  }, [filteredData.hourly]);

  // Render hourly forecast
  const renderHourlyForecast = () => {
    if (!filteredData.hourly.length) return null;

    return (
      <Card className="bg-white dark:bg-gray-800 border-gray-200 dark:border-gray-700">
        <CardHeader>
          <CardTitle className="flex items-center gap-2 text-gray-900 dark:text-white">
            <Clock className="h-5 w-5" />
            {t.hourlyForecast}
          </CardTitle>
        </CardHeader>
        <CardContent>
          <div className="space-y-6">
            {Object.entries(groupedHourlyData).map(([date, hours]) => (
              <div key={date} className="">
                <div className="mb-4">
                  <h3 className="text-lg font-semibold text-gray-900 dark:text-white border-b border-gray-200 dark:border-gray-600 pb-2">
                    {format(new Date(date), 'EEEE, MMMM dd')}
                  </h3>
                </div>
                <div className="overflow-x-auto">
                  <div className="flex gap-4 pb-4" style={{ minWidth: `${hours.length * 200}px` }}>
                    {hours.map((hour, index) => (
                      <div key={index} className="flex-shrink-0 w-48 p-4 bg-gray-50 dark:bg-gray-700 rounded-lg">
                        <div className="text-center">
                          <div className="text-sm font-medium text-gray-900 dark:text-white mb-2">
                            {format(new Date(hour.datetime), 'HH:mm')}
                          </div>
                          <div className="flex justify-center mb-2">
                            {getWeatherIcon(hour.weatherCode, 32)}
                          </div>
                          <div className="text-lg font-bold text-gray-900 dark:text-white mb-2">
                            {Math.round(hour.temperature)}°C
                          </div>
                          <div className="space-y-1 text-xs text-gray-600 dark:text-gray-400">
                            <div className="flex items-center justify-between">
                              <span>{t.wind}:</span>
                              <span>{Math.round(hour.windSpeed)} {getWindSpeedUnit(Math.round(hour.windSpeed), t)}</span>
                            </div>
                            <div className="flex items-center justify-between">
                              <span>{t.humidity}:</span>
                              <span>{Math.round(hour.humidity)}%</span>
                            </div>
                            <div className="flex items-center justify-between">
                              <span>{t.precipitation}:</span>
                              <span>{hour.precipitation.toFixed(1)}mm</span>
                            </div>
                            {showMarineData && hour.waveHeight !== undefined && (
                              <div className="flex items-center justify-between">
                                <span>{t.waves}:</span>
                                <span>{hour.waveHeight.toFixed(1)}m</span>
                              </div>
                            )}
                          </div>
                        </div>
                      </div>
                    ))}
                  </div>
                </div>
              </div>
            ))}
          </div>
        </CardContent>
      </Card>
    );
  };

  // Render daily forecast
  const renderDailyForecast = () => {
    if (!filteredData.daily.length) return null;

    return (
      <Card className="bg-white dark:bg-gray-800 border-gray-200 dark:border-gray-700">
        <CardHeader>
          <CardTitle className="flex items-center gap-2 text-gray-900 dark:text-white">
            <Calendar className="h-5 w-5" />
            {t.dailyForecast}
          </CardTitle>
        </CardHeader>
        <CardContent>
          <div className="space-y-4">
            {filteredData.daily.map((day, index) => (
              <div key={index} className="flex items-center justify-between p-4 bg-gray-50 dark:bg-gray-700 rounded-lg">
                <div className="flex items-center gap-4">
                  <div className="text-sm font-medium text-gray-900 dark:text-white w-20">
                    {format(new Date(day.datetime), 'MMM dd')}
                  </div>
                  <div className="flex items-center gap-2">
                    {getWeatherIcon(day.weatherCode, 24)}
                    <span className="text-sm text-gray-600 dark:text-gray-400">
                      {getWeatherDescription(day.weatherCode, (key: string) => t[key as keyof typeof t] || key)}
                    </span>
                  </div>
                </div>
                
                <div className="flex items-center gap-6">
                  <div className="text-right">
                    <div className="text-lg font-bold text-gray-900 dark:text-white">
                      {Math.round(day.temperatureMax)}°
                    </div>
                    <div className="text-sm text-gray-500 dark:text-gray-400">
                      {Math.round(day.temperatureMin)}°
                    </div>
                  </div>
                  
                  <div className="text-right text-sm text-gray-600 dark:text-gray-400">
                    <div>{Math.round(day.windSpeed)} {getWindSpeedUnit(Math.round(day.windSpeed), t)}</div>
                    <div>{Math.round(day.precipitationProbability)}%</div>
                  </div>
                  
                  {showMarineData && day.waveHeight !== undefined && (
                    <div className="text-right text-sm text-gray-600 dark:text-gray-400">
                      <div>{day.waveHeight.toFixed(1)}m</div>
                      <div className="text-xs">{t.waves}</div>
                    </div>
                  )}
                </div>
              </div>
            ))}
          </div>
        </CardContent>
      </Card>
    );
  };

  if (locationError) {
    return (
      <div className="container mx-auto px-4 py-8">
        <Card className="bg-white dark:bg-gray-800 border-gray-200 dark:border-gray-700">
          <CardContent className="text-center py-8">
            <MapPin className="h-12 w-12 text-gray-400 mx-auto mb-4" />
            <h3 className="text-lg font-semibold text-gray-900 dark:text-white mb-2">
              {t.locationRequired}
            </h3>
            <p className="text-gray-600 dark:text-gray-400">
              {t.enableLocationForWeather}
            </p>
          </CardContent>
        </Card>
      </div>
    );
  }

  return (
    <div className="container mx-auto px-4 py-8">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4 mb-8">
        <div>
          <h1 className="text-3xl font-bold text-gray-900 dark:text-white">
            {t.weatherForecast}
          </h1>
          <p className="text-gray-600 dark:text-gray-400 mt-1">
            {forecast && (
              <span className="flex items-center gap-1">
                <MapPin className="h-4 w-4" />
                {forecast.location.latitude.toFixed(4)}, {forecast.location.longitude.toFixed(4)}
              </span>
            )}
          </p>
        </div>
        
        <div className="flex items-center gap-2">
          <Button
            variant="outline"
            size="sm"
            onClick={() => refetch()}
            disabled={isLoading}
            className={`flex items-center gap-2
              border-gray-300 dark:border-gray-600
              text-gray-700 dark:text-gray-200
              hover:bg-gray-100 dark:hover:bg-gray-700
              disabled:opacity-50 disabled:cursor-not-allowed
            `}
          >
            <RefreshCw className={`h-4 w-4 ${isLoading ? 'animate-spin' : ''}`} />
            {t.refresh}
          </Button>
        </div>
      </div>

      {/* Filters */}
      <Card className="bg-white dark:bg-gray-800 border-gray-200 dark:border-gray-700 mb-6">
        <CardHeader>
          <CardTitle className="flex items-center justify-between text-gray-900 dark:text-white">
            <div className="flex items-center gap-2">
              <Filter className="h-5 w-5" />
              {t.filters}
            </div>
          </CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
            {/* View Mode */}
            <div>
              <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                {t.viewMode}
              </label>
              <select
                value={viewMode}
                onChange={(e) => setViewMode(e.target.value as ViewMode)}
                className="w-full p-2 border border-gray-300 dark:border-gray-600 rounded-md bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
              >
                <option value="current">{t.current}</option>
                <option value="hourly">{t.hourly}</option>
                <option value="daily">{t.daily}</option>
              </select>
            </div>

            {/* Time Period */}
            <div>
              <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                {t.timePeriod}
              </label>
              <select
                value={filterPeriod}
                onChange={(e) => {
                  const newPeriod = e.target.value as FilterPeriod;
                  setFilterPeriod(newPeriod);
                  // Auto-switch to hourly view when moving away from current time
                  if (viewMode === 'current' && newPeriod !== 'today') {
                    setViewMode('hourly');
                  }
                }}
                className="w-full p-2 border border-gray-300 dark:border-gray-600 rounded-md bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
              >
                <option value="today">{t.today}</option>
                <option value="tomorrow">{t.tomorrow}</option>
                <option value="3days">{t.next3Days}</option>
                <option value="7days">{t.next7Days}</option>
                <option value="custom">{t.customRange}</option>
              </select>
            </div>

            {/* Custom Date Range */}
            {filterPeriod === 'custom' && (
              <>
                <div>
                  <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                    {t.startDate}
                  </label>
                  <input
                    type="date"
                    value={customStartDate}
                    onChange={(e) => setCustomStartDate(e.target.value)}
                    className="w-full p-2 border border-gray-300 dark:border-gray-600 rounded-md bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                    {t.endDate}
                  </label>
                  <input
                    type="date"
                    value={customEndDate}
                    onChange={(e) => setCustomEndDate(e.target.value)}
                    className="w-full p-2 border border-gray-300 dark:border-gray-600 rounded-md bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
                  />
                </div>
              </>
            )}

            {/* Marine Data Toggle - Only show for Fire Service */}
            {user?.agencyName === 'Hellenic Fire Service' && (
              <div className="flex items-center gap-3 mt-4">
                <label htmlFor="marine-data" className="flex items-center cursor-pointer select-none">
                  {/* Switch */}
                  <div className="relative">
                    <input
                      type="checkbox"
                      id="marine-data"
                      checked={showMarineData}
                      onChange={(e) => setShowMarineData(e.target.checked)}
                      className="sr-only" // hide default checkbox
                    />
                    <div
                      className={`w-11 h-6 rounded-full shadow-inner transition-colors duration-300 ${
                        showMarineData ? 'bg-blue-500' : 'bg-gray-300 dark:bg-gray-700'
                      }`}
                    ></div>
                    <div
                      className={`absolute left-0 top-0.5 w-5 h-5 bg-white rounded-full shadow transform transition-transform duration-300 ${
                        showMarineData ? 'translate-x-5' : ''
                      }`}
                    ></div>
                  </div>
                  {/* Label */}
                  <span className="ml-3 text-sm font-medium text-gray-700 dark:text-gray-300">
                    {t.showMarineData}
                  </span>
                </label>
              </div>
            )}
          </div>
        </CardContent>
      </Card>

      {/* Loading State */}
      {isLoading && (
        <div className="flex items-center justify-center py-12">
          <LoadingSpinner size="lg" text={t.loadingWeatherData} />
        </div>
      )}

      {/* Error State */}
      {error && (
        <Card className="bg-red-50 dark:bg-red-900/20 border-red-200 dark:border-red-800 mb-6">
          <CardContent className="text-center py-8">
            <div className="text-red-600 dark:text-red-400">
              {t.weatherLoadError}: {error.message}
            </div>
          </CardContent>
        </Card>
      )}

      {/* Weather Content */}
      {forecast && !isLoading && (
        <div className="space-y-6">
          {viewMode === 'current' && renderCurrentWeather()}
          {viewMode === 'hourly' && renderHourlyForecast()}
          {viewMode === 'daily' && renderDailyForecast()}
        </div>
      )}
    </div>
  );
};

export default WeatherForecast;