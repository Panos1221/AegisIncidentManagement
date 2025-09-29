import React, { useMemo, useEffect } from 'react';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import { Card, CardContent, CardHeader, CardTitle } from '../../components/ui/card';
import { Badge } from '../../components/ui/badge';
import { Button } from '../../components/ui/button';
import { 
  Anchor, 
  Ship, 
  Users, 
  AlertTriangle, 
  Clock, 
  MapPin,
  Waves,
  Eye,
  Phone,
} from 'lucide-react';
import { useUserStore } from '../../lib/userStore';
import { vehiclesApi, incidentsApi, personnelApi } from '../../lib/api';
import { IncidentStatus, ResourceType } from '../../types';
import { LoadingSpinner, ErrorBoundary } from '../../components';
import { useTranslation } from '../../hooks/useTranslation';
import VesselFinderIFrame  from "../../components/VesselFinderIFrame";
import { useMarineWeather } from '../../hooks/useMarineWeather';
import { useGeolocation } from '../../hooks/useGeolocation';
import { WindDirection, getWindDirectionLabel } from '../../components/Weather/WindDirection';
import { getSeaState, getWindSpeedUnit } from '../../lib/weatherHelpers';
import { useIncidentNotification } from '../../lib/incidentNotificationContext';
import { useSignalR } from '../../hooks/useSignalR';
import { getIncidentCardBackgroundColor, getIncidentStatusBadgeColor, getIncidentStatusTranslation, getIncidentCardTextColor, getIncidentCardSecondaryTextColor, getStatusBadgeColor, getVehicleStatusTranslation } from '../../utils/incidentUtils';

const CoastGuardDashboard: React.FC = () => {
  const { user, isDispatcher } = useUserStore();
  const t = useTranslation();
  const navigate = useNavigate();
  const { isIncidentFlashing } = useIncidentNotification();
  const queryClient = useQueryClient();
  const signalR = useSignalR();

  // Helper function to get vessel callsign by resourceId
  const getVesselCallsign = (resourceId: number) => {
    const vessel = vehicles?.find(v => v.id === resourceId);
    return vessel?.callsign || `Vessel ${resourceId}`;
  };

  // Fetch real data filtered by user's agency
  const { data: vehicles, isLoading: vehiclesLoading } = useQuery({
    queryKey: ['vehicles', 'coast-guard-dashboard'],
    queryFn: () => vehiclesApi.getAll().then(res => res.data),
    enabled: !!user?.agencyId,
    staleTime: 30000,
  });

  const { data: incidents, isLoading: incidentsLoading } = useQuery({
    queryKey: ['incidents', 'coast-guard-dashboard'],
    queryFn: () => incidentsApi.getAll().then(res => res.data),
    enabled: !!user?.agencyId,
    staleTime: 30000,
  });

  // Fetch personnel data for dynamic count
  const { data: personnel } = useQuery({
    queryKey: ['personnel', 'coast-guard-dashboard'],
    queryFn: () => personnelApi.getAll({ isActive: true }).then(res => res.data),
    enabled: !!user?.agencyId,
    staleTime: 30000,
  });

  // Set up SignalR event handlers to invalidate queries when incidents change
  useEffect(() => {
    if (!signalR) return;

    // Add handlers and store cleanup functions
    const cleanupCreated = signalR.addIncidentCreatedHandler(() => {
      queryClient.invalidateQueries({ queryKey: ['incidents', 'coast-guard-dashboard'] });
    });

    const cleanupStatusChanged = signalR.addIncidentStatusChangedHandler(() => {
      queryClient.invalidateQueries({ queryKey: ['incidents', 'coast-guard-dashboard'] });
    });

    const cleanupUpdate = signalR.addIncidentUpdateHandler(() => {
      queryClient.invalidateQueries({ queryKey: ['incidents', 'coast-guard-dashboard'] });
    });

    const cleanupResourceAssigned = signalR.addResourceAssignedHandler(() => {
      queryClient.invalidateQueries({ queryKey: ['incidents', 'coast-guard-dashboard'] });
    });

    // Return cleanup function
     return () => {
       cleanupCreated();
       cleanupStatusChanged();
       cleanupUpdate();
       cleanupResourceAssigned();
     };
   }, [signalR, queryClient]);

  // Total active incidents count for statistics
  const totalActiveIncidents = useMemo(() => {
    if (!incidents) return 0;
    return incidents.filter(incident =>
      incident.status === IncidentStatus.OnGoing ||
      incident.status === IncidentStatus.PartialControl ||
      incident.status === IncidentStatus.Controlled
    ).length;
  }, [incidents]);

  // Process data for dashboard
  const processedIncidents = useMemo(() => {
    const filtered = incidents?.filter(incident => 
      incident.status === IncidentStatus.OnGoing || 
      incident.status === IncidentStatus.PartialControl ||
      incident.status === IncidentStatus.Controlled
    ) || [];
    
    if (isDispatcher()) {
      // For dispatchers, show only the latest 5 incidents
      return filtered
        .sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime())
        .slice(0, 5);
    } else {
      // For non-dispatchers, show incidents from their station
      return filtered.filter(incident => incident.stationId === user?.stationId);
    }
  }, [incidents, isDispatcher, user?.stationId]);

  const availableVessels = vehicles?.map(vehicle => ({
    id: vehicle.id.toString(),
    name: vehicle.callsign,
    status: getVehicleStatusTranslation(vehicle.status, t),
    crew: 6, // Mock crew data for now
    type: vehicle.type,
    location: vehicle.latitude && vehicle.longitude ? 
      `${vehicle.latitude.toFixed(4)}°N ${vehicle.longitude.toFixed(4)}°E` : t.Unknown
  })) || [];

  // Use user's location with fallback to Athens, Greece coordinates
  const geolocation = useGeolocation();
  const lat = geolocation.latitude ?? 38.0;
  const lon = geolocation.longitude ?? 23.7;
  const { data: marine, isLoading: weatherLoading, isError: weatherError } = useMarineWeather(lat, lon);

  // Using the new vibrant status color function from utils
  const getStatusColor = (status: string) => {
    return getStatusBadgeColor(status);
  };



  if (vehiclesLoading || incidentsLoading) {
    return (
      <div className="p-6 flex items-center justify-center h-64">
        <LoadingSpinner size="lg" text={t.loadingData} />
      </div>
    );
  }

  return (
    <ErrorBoundary>
    <div className="p-6 space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-gray-900 dark:text-white flex items-center gap-2">
            <Anchor className="h-8 w-8 text-blue-600" />
            {t.dashboard} - {t.coastGuard}
          </h1>
          <p className="text-gray-600 dark:text-gray-400 mt-1">
            {t.welcomeBack}, {user?.name} - {user?.agencyName}
          </p>
        </div>
        <div className="flex flex-col gap-2">
          <Button className="bg-red-600 hover:bg-red-700 text-white">
            <Phone className="h-4 w-4 mr-2" />
            108
          </Button>
        </div>
      </div>

      {/* Stats Overview */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        {/* Active Incidents */}
        <Card className="bg-white dark:bg-gray-800 border-gray-200 dark:border-gray-700">
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium text-gray-900 dark:text-white">
              {t.activeIncidents}
            </CardTitle>
            <AlertTriangle className="h-4 w-4 text-red-500" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-gray-900 dark:text-white">{totalActiveIncidents}</div>
            <p className="text-xs text-gray-600 dark:text-gray-400">{t.totalActiveIncidents}</p>
          </CardContent>
        </Card>

        {/* Resources Available */}
        <Card className="bg-white dark:bg-gray-800 border-gray-200 dark:border-gray-700">
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium text-gray-900 dark:text-white">
              {t.resourcesAvailable}
            </CardTitle>
            <Ship className="h-4 w-4 text-blue-500" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-gray-900 dark:text-white">
              {availableVessels.filter(vessel => vessel.status === 'Available').length}
            </div>
            <p className="text-xs text-gray-600 dark:text-gray-400">
              {t.outOfnumber} {availableVessels.length} {t.total}
            </p>
          </CardContent>
        </Card>

        {/* Personnel On Duty */}
        <Card className="bg-white dark:bg-gray-800 border-gray-200 dark:border-gray-700">
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium text-gray-900 dark:text-white">
              {t.personnel} {t.onDuty}
            </CardTitle>
            <Users className="h-4 w-4 text-green-500" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-gray-900 dark:text-white">
              {personnel?.length || 0}
            </div>
            <p className="text-xs text-gray-600 dark:text-gray-400">
              {t.onAllStations}
            </p>
          </CardContent>
        </Card>
      </div>


      <div className="grid grid-cols-1 gap-6">
        {/* Maritime Map (focused on Greece) */}
        <Card className="bg-white dark:bg-gray-800 border-gray-200 dark:border-gray-700">
          <CardHeader>
            <CardTitle>Public AIS — Greece</CardTitle>
          </CardHeader>
          <CardContent>
            <VesselFinderIFrame lat={lat} lon={lon} zoom={6} height={600} />
          </CardContent>
        </Card>

        {/* Incidents + Weather */}
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          {/* Active Incidents */}
          <Card className="lg:col-span-2 bg-white dark:bg-gray-800 border-gray-200 dark:border-gray-700">
            <CardHeader>
              <CardTitle className="flex items-center gap-2 text-gray-900 dark:text-white">
                <AlertTriangle className="h-5 w-5 text-red-500" />
                {t.activeIncidents}
                {isDispatcher() && (
                  <Badge variant="outline" className="ml-2 text-xs">
                    {t.latestFive}
                  </Badge>
                )}
              </CardTitle>
            </CardHeader>
                <CardContent className="flex-1">
                  <div className="space-y-4 flex-1 overflow-y-auto">
                    {processedIncidents.length > 0 ? (
                      processedIncidents.map((incident) => {
                        const isFlashing = isIncidentFlashing(incident.id);
                        return (
                        <div 
                          key={incident.id} 
                          className={`border-l-4 border-gray-200 dark:border-gray-600 cursor-pointer hover:shadow-md transition-shadow rounded-lg border ${getIncidentCardBackgroundColor(incident.status)} ${isFlashing ? 'incident-flash' : ''}`}
                          onClick={() => navigate(`/incidents/${incident.id}`)}
                        >
                          <CardContent className="p-4 space-y-2">
                            <div className="flex items-center justify-between">
                              <div className="flex flex-col">
                                <h3 className={`font-semibold ${getIncidentCardTextColor(incident.status)}`}>{incident.mainCategory}</h3>
                                <h4 className={`text-sm font-medium ${getIncidentCardTextColor(incident.status)}`}>{incident.subCategory}</h4>
                              </div>
                              <div className="flex items-center gap-2">
                                <Badge className={`${getIncidentStatusBadgeColor(incident.status)}`}>
                                  {getIncidentStatusTranslation(incident.status, t)}
                                </Badge>
                                <Eye className="h-4 w-4 text-gray-400" />
                              </div>
                            </div>
                            <div className={`flex items-center gap-2 text-sm ${getIncidentCardSecondaryTextColor(incident.status)}`}>
                              <MapPin className="h-4 w-4" />
                              {(() => {
                                if (incident.street || incident.city) {
                                  const parts = [
                                    incident.street && incident.streetNumber ? `${incident.street} ${incident.streetNumber}` : incident.street,
                                    incident.city
                                  ].filter(Boolean)
                                  return parts.join(', ') || `${incident.latitude}, ${incident.longitude}`
                                }
                                return incident.address || `${incident.latitude}, ${incident.longitude}`
                              })()}
                            </div>
                            <div className={`flex items-center gap-2 text-sm ${getIncidentCardSecondaryTextColor(incident.status)}`}>
                              <Clock className="h-4 w-4" />
                              {t.reported}: {new Date(incident.createdAt).toLocaleTimeString()}
                            </div>
                            {incident.notes && (
                              <div className={`text-sm ${getIncidentCardSecondaryTextColor(incident.status)}`}>
                                {incident.notes}
                              </div>
                            )}
                            <div className="flex flex-wrap gap-1">
                              {incident.assignments
                                .filter(assignment => assignment.resourceType === ResourceType.Vehicle)
                                .map((assignment) => (
                                <Badge key={assignment.id} variant="outline" className="text-xs border-gray-300 dark:border-gray-500 text-gray-700 dark:text-gray-300">
                                  {getVesselCallsign(assignment.resourceId)}
                                </Badge>
                              ))}
                            </div>
                          </CardContent>
                        </div>
                          );
                        })
                    ) : (
                      <div className="text-center py-8 text-gray-500 dark:text-gray-400">
                        No active maritime incidents at this time
                      </div>
                    )}
                  </div>
                </CardContent>
          </Card>

          {/* Weather Conditions */}
          <Card className="bg-white dark:bg-gray-800 border-gray-200 dark:border-gray-700">
            <CardHeader>
              <CardTitle className="flex items-center gap-2 text-gray-900 dark:text-white">
                <Waves className="h-5 w-5 text-blue-500" />
                {t.weatherConditions}
              </CardTitle>
            </CardHeader>

            <CardContent>
              {weatherLoading ? (
                <div className="flex items-center justify-center p-6">
                  <LoadingSpinner size="md" text={t.loadingData} />
                </div>
              ) : weatherError || !marine ? (
                <div className="text-center text-sm text-gray-500 dark:text-gray-400 p-6">
                  Failed to load weather — showing last known values.
                </div>
              ) : (
                <div className="space-y-4">
                  <div className="flex items-center justify-between">
                    <div className="flex items-center gap-4">
                      <WindDirection degrees={marine.windDirection} t={t} />
                      <div>
                        <div className="text-sm text-gray-700 dark:text-gray-300">{t.wind}</div>
                        <div className="font-medium text-gray-900 dark:text-white">
                          {Math.round(marine.windSpeed)} {getWindSpeedUnit(Math.round(marine.windSpeed), t)} {getWindDirectionLabel(marine.windDirection, t)}
                        </div>
                      </div>
                    </div>
                  </div>

                  <div className="flex items-center justify-between">
                    <div className="text-sm text-gray-700 dark:text-gray-300">{t.waveHeight}</div>
                    <div className="font-medium text-gray-900 dark:text-white">{marine.waveHeight.toFixed(1)} m</div>
                  </div>

                  <div className="flex items-center justify-between">
                    <div className="text-sm text-gray-700 dark:text-gray-300">{t.temperature}</div>
                    <div className="font-medium text-gray-900 dark:text-white">{marine.temperature.toFixed(1)} °C</div>
                  </div>

                  <div className="flex items-center justify-between">
                    <div className="text-sm text-gray-700 dark:text-gray-300">{t.visibility}</div>
                    <div className="font-medium text-gray-900 dark:text-white">{marine.visibilityNmi.toFixed(1)} {t.nauticalmiles}</div>
                  </div>

                  <div className="flex items-center justify-between">
                    <div className="text-sm text-gray-700 dark:text-gray-300">{t.seaState}</div>
                    <Badge variant="outline" className="border-gray-300 dark:border-gray-500 text-gray-700 dark:text-gray-300">
                      {t[getSeaState(marine.waveHeight) as keyof typeof t]}
                    </Badge>
                  </div>
                </div>
              )}
            </CardContent>
          </Card>

        </div>
      </div>


      {/* Vessel Status */}
      <Card className="bg-white dark:bg-gray-800 border-gray-200 dark:border-gray-700">
        <CardHeader>
          <CardTitle className="flex items-center gap-2 text-gray-900 dark:text-white">
            <Ship className="h-5 w-5 text-blue-500" />
            {t.status} {t.fleet}
          </CardTitle>
        </CardHeader>
        <CardContent className="flex-1">
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4 flex-1 overflow-y-auto">
            {availableVessels.length > 0 ? (
              availableVessels.map((vessel) => (
                <div key={vessel.id} className="flex items-center justify-between p-3 border rounded-lg border-gray-200 dark:border-gray-600 bg-gray-50 dark:bg-gray-700">
                  <div className="flex items-center gap-3">
                    <div className={`w-3 h-3 rounded-full ${getStatusColor(vessel.status)}`} />
                    <div>
                      <div className="font-medium text-gray-900 dark:text-white">{vessel.name}</div>
                      <div className="text-sm text-gray-600 dark:text-gray-300">
                        {vessel.type} • {t.crew}: {vessel.crew}
                      </div>
                      <div className="text-xs text-gray-500 dark:text-gray-400">
                        {vessel.location}
                      </div>
                    </div>
                  </div>
                  <Badge variant="outline" className="border-gray-300 dark:border-gray-500 text-gray-700 dark:text-gray-300">{vessel.status}</Badge>
                </div>
              ))
            ) : (
              <div className="text-center py-8 text-gray-500 dark:text-gray-400">
                {t.noVehiclesFound}
              </div>
            )}
          </div>
        </CardContent>
      </Card>
    </div>
    </ErrorBoundary>
  );
};

export default CoastGuardDashboard;