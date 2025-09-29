import React, { useState, useMemo, useEffect } from 'react';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import { Card, CardContent, CardHeader, CardTitle } from '../../components/ui/card';
import { Badge } from '../../components/ui/badge';
import { Button } from '../../components/ui/button';
import {
  Flame,
  Truck,
  Users,
  AlertTriangle,
  Clock,
  MapPin,
  Phone,
  Filter,
  Eye
} from 'lucide-react';
import { useUserStore } from '../../lib/userStore';
import { vehiclesApi, incidentsApi, fireStationsApi, personnelApi } from '../../lib/api';
import { IncidentStatus, FireStation, ResourceType } from '../../types';
import { LoadingSpinner, ErrorBoundary } from '../../components';
import { useTranslation } from '../../hooks/useTranslation';
import { useIncidentNotification } from '../../lib/incidentNotificationContext';
import { useSignalR } from '../../hooks/useSignalR';
import { getIncidentCardBackgroundColor, getIncidentStatusBadgeColor, getIncidentStatusTranslation, getIncidentCardTextColor, getIncidentCardSecondaryTextColor, getStatusBadgeColor, getVehicleStatusTranslation } from '../../utils/incidentUtils';

const FireDepartmentDashboard: React.FC = () => {
  const { user, isDispatcher } = useUserStore();
  const t = useTranslation();
  const navigate = useNavigate();
  const { isIncidentFlashing } = useIncidentNotification();
  const [selectedStationFilter, setSelectedStationFilter] = useState<number | 'all'>('all');
  const queryClient = useQueryClient();
  const signalR = useSignalR();

  // Helper function to get vehicle callsign by resourceId
  const getVehicleCallsign = (resourceId: number) => {
    const vehicle = vehicles?.find(v => v.id === resourceId);
    return vehicle?.callsign || `${t.resource} ${resourceId}`;
  };

  // Fetch real data filtered by user's agency
  const { data: vehicles, isLoading: vehiclesLoading } = useQuery({
    queryKey: ['vehicles', 'fire-dashboard'],
    queryFn: () => vehiclesApi.getAll().then(res => res.data),
    enabled: !!user?.agencyId,
    staleTime: 30000,
  });

  const { data: incidents, isLoading: incidentsLoading } = useQuery({
    queryKey: ['incidents', 'fire-dashboard'],
    queryFn: () => incidentsApi.getAll().then(res => res.data),
    enabled: !!user?.agencyId,
    staleTime: 30000,
  });

  // Fetch stations for filtering
  const { data: stationsData } = useQuery({
    queryKey: ['fire-stations'],
    queryFn: () => fireStationsApi.getStations(),
    staleTime: 5 * 60 * 1000,
  });

  const getStationName = (stationId: number) => {
    const station = stationsData?.find((s: FireStation) => s.id === stationId)
    return station?.name || `Station ${stationId}`
  }

  // Fetch personnel data for dynamic count
  const { data: personnel } = useQuery({
    queryKey: ['personnel', 'fire-dashboard'],
    queryFn: () => personnelApi.getAll({ isActive: true }).then(res => res.data),
    enabled: !!user?.agencyId,
    staleTime: 30000,
  });

  // Set up SignalR event handlers to invalidate queries when incidents change
  useEffect(() => {
    if (!signalR) return;

    // Add handlers and store cleanup functions
    const cleanupCreated = signalR.addIncidentCreatedHandler(() => {
      queryClient.invalidateQueries({ queryKey: ['incidents', 'fire-dashboard'] });
    });

    const cleanupStatusChanged = signalR.addIncidentStatusChangedHandler(() => {
      queryClient.invalidateQueries({ queryKey: ['incidents', 'fire-dashboard'] });
    });

    const cleanupUpdate = signalR.addIncidentUpdateHandler(() => {
      queryClient.invalidateQueries({ queryKey: ['incidents', 'fire-dashboard'] });
    });

    const cleanupResourceAssigned = signalR.addResourceAssignedHandler(() => {
      queryClient.invalidateQueries({ queryKey: ['incidents', 'fire-dashboard'] });
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

  // Process data for dashboard with role-based filtering
  const processedIncidents = useMemo(() => {
    if (!incidents) return [];
    
    let filteredIncidents = incidents.filter(incident =>
      incident.status === IncidentStatus.OnGoing ||
      incident.status === IncidentStatus.PartialControl ||
      incident.status === IncidentStatus.Controlled
    );

    // Role-based filtering
    if (!isDispatcher()) {
      // Members can only see incidents from their station
      filteredIncidents = filteredIncidents.filter(incident => 
        incident.stationId === user?.stationId
      );
    } else {
      // Dispatchers see latest 5 incidents only
      filteredIncidents = filteredIncidents
        .sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime())
        .slice(0, 5);
    }

    return filteredIncidents;
  }, [incidents, isDispatcher, user?.stationId]);

  // Process vehicles with station filtering
  const processedVehicles = useMemo(() => {
    if (!vehicles) return [];
    
    let filteredVehicles = vehicles;
    
    // Apply station filter if selected
    if (selectedStationFilter !== 'all') {
      filteredVehicles = filteredVehicles.filter(vehicle => 
        vehicle.stationId === selectedStationFilter
      );
    }

    return filteredVehicles.map(vehicle => ({
      id: vehicle.id.toString(),
      name: vehicle.callsign,
      status: getVehicleStatusTranslation(vehicle.status, t),
      crew: 4, // Mock crew data for now
      type: vehicle.type,
      stationId: vehicle.stationId
    }));
  }, [vehicles, selectedStationFilter]);

  const handleIncidentClick = (incidentId: number) => {
    navigate(`/incidents/${incidentId}`);
  };

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
              <Flame className="h-8 w-8 text-red-500" />
              {t.dashboard} - {t.fireService}
            </h1>
            <p className="text-gray-600 dark:text-gray-400 mt-1">
              {t.welcomeBack}, {user?.name} - {user?.agencyName}
            </p>
          </div>
          <div className="flex gap-2">
            <Button className="bg-red-600 hover:bg-red-700">
              <Phone className="h-4 w-4 mr-2" />
              199
            </Button>
          </div>
        </div>

        {/* Stats Overview */}
        <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
          <Card className="bg-white dark:bg-gray-800 border-gray-200 dark:border-gray-700">
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium text-gray-900 dark:text-white">{t.activeIncidents}</CardTitle>
              <AlertTriangle className="h-4 w-4 text-red-500" />
            </CardHeader>
            <CardContent className="flex-1">
              <div className="text-2xl font-bold text-gray-900 dark:text-white">{totalActiveIncidents}</div>
              <p className="text-xs text-gray-600 dark:text-gray-400">{t.totalActiveIncidents}</p>
            </CardContent>
          </Card>

          <Card className="bg-white dark:bg-gray-800 border-gray-200 dark:border-gray-700">
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium text-gray-900 dark:text-white">{t.resourcesAvailable}</CardTitle>
              <Truck className="h-4 w-4 text-green-500" />
            </CardHeader>
            <CardContent className="flex-1">
              <div className="text-2xl font-bold text-gray-900 dark:text-white">
                {processedVehicles.filter(unit => unit.status === 'Available').length}
              </div>
              <p className="text-xs text-gray-600 dark:text-gray-400">{t.outOfnumber} {processedVehicles.length} {t.total}</p>
            </CardContent>
          </Card>

          <Card className="bg-white dark:bg-gray-800 border-gray-200 dark:border-gray-700">
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium text-gray-900 dark:text-white">{t.personnel} {t.onDuty}</CardTitle>
              <Users className="h-4 w-4 text-blue-500" />
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold text-gray-900 dark:text-white">{personnel?.length || 0}</div>
              <p className="text-xs text-gray-600 dark:text-gray-400">{t.onAllStations}</p>
            </CardContent>
          </Card>
        </div>

        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 lg:items-start">
          {/* Active Incidents */}
          <Card className="bg-white dark:bg-gray-800 border-gray-200 dark:border-gray-700 h-full flex flex-col">
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
              <div className={`space-y-4 ${!isDispatcher() && processedIncidents.length > 3 ? 'overflow-y-auto pr-2' : ''} flex-1`}>
                {processedIncidents.length > 0 ? (
                  processedIncidents.map((incident) => {
                    const isFlashing = isIncidentFlashing(incident.id);
                    return (
                    <div 
                       key={incident.id} 
                       className={`border-l-4 border-gray-200 dark:border-gray-600 cursor-pointer hover:shadow-md transition-shadow rounded-lg border ${getIncidentCardBackgroundColor(incident.status)} ${isFlashing ? 'incident-flash' : ''}`}
                       onClick={() => handleIncidentClick(incident.id)}
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
                            <Badge key={assignment.id}variant="outline" className="text-xs border-gray-300 dark:border-gray-500 text-white dark:text-white">
                              {getVehicleCallsign(assignment.resourceId)}
                            </Badge>
                          ))}
                        </div>
                      </CardContent>
                    </div>
                    );
                  })
                ) : (
                  <div className="text-center py-8 text-gray-500 dark:text-gray-400">
                    {t.noIncidentsFound}
                  </div>
                )}
              </div>
            </CardContent>
          </Card>

          {/* Unit Status */}
          <Card className="bg-white dark:bg-gray-800 border-gray-200 dark:border-gray-700 h-full flex flex-col">
            <CardHeader>
              <CardTitle className="flex items-center gap-2 text-gray-900 dark:text-white">
                <Truck className="h-5 w-5 text-blue-500" />
                {t.status} - {t.vehicles}
              </CardTitle>
              {isDispatcher() && (
                <div className="flex items-center gap-2 mt-2">
                  <Filter className="h-4 w-4 text-gray-500" />
                  <select
                     value={selectedStationFilter}
                     onChange={(e) => setSelectedStationFilter(e.target.value === 'all' ? 'all' : Number(e.target.value))}
                     className="px-3 py-1 border border-gray-300 dark:border-gray-600 rounded-md bg-white dark:bg-gray-700 text-gray-900 dark:text-white text-sm"
                   >
                     <option value="all">{t.allStations}</option>
                     {stationsData?.map((station) => (
                       <option key={station.id} value={station.id}>
                          {station.name}
                       </option>
                     ))}
                  </select>
                </div>
              )}
            </CardHeader>
             <CardContent className="flex-1">
               <div className={`space-y-3 ${processedVehicles.length > 4 ? 'overflow-y-auto pr-2' : ''} flex-1`}>
                {processedVehicles.length > 0 ? (
                  processedVehicles.map((unit) => (
                    <div key={unit.id} className="flex items-center justify-between p-3 border rounded-lg border-gray-200 dark:border-gray-600 bg-gray-50 dark:bg-gray-700">
                      <div className="flex items-center gap-3">
                        <div className={`w-3 h-3 rounded-full ${getStatusColor(unit.status)}`} />
                        <div>
                          <div className="font-medium text-gray-900 dark:text-white">{unit.name}</div>
                          <div className="text-sm text-gray-600 dark:text-gray-300">
                            {unit.type} • {t.crew}: {unit.crew} • {getStationName(unit.stationId)}
                          </div>
                        </div>
                      </div>
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
      </div>
    </ErrorBoundary>
  );
};

export default FireDepartmentDashboard;