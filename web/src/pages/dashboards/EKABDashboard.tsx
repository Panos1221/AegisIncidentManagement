import React, { useMemo, useEffect } from 'react';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import { Card, CardContent, CardHeader, CardTitle } from '../../components/ui/card';
import { Badge } from '../../components/ui/badge';
import { Button } from '../../components/ui/button';
import { 
  Heart, 
  Truck, 
  AlertTriangle, 
  Clock, 
  MapPin,
  Phone,
  Activity,
  Stethoscope,
  Building,
  Eye
} from 'lucide-react';
import { useUserStore } from '../../lib/userStore';
import { vehiclesApi, incidentsApi, personnelApi } from '../../lib/api';
import { IncidentStatus, ResourceType } from '../../types';
import { LoadingSpinner, ErrorBoundary } from '../../components';
import { useTranslation } from '../../hooks/useTranslation';
import { useIncidentNotification } from '../../lib/incidentNotificationContext';
import { useSignalR } from '../../hooks/useSignalR';
import { getIncidentCardBackgroundColor, getIncidentStatusBadgeColor, getIncidentStatusTranslation, getIncidentCardTextColor, getIncidentCardSecondaryTextColor, getStatusBadgeColor, getVehicleStatusTranslation, getIncidentPriorityBadgeColor } from '../../utils/incidentUtils';

const EKABDashboard: React.FC = () => {
  const { user, isDispatcher } = useUserStore();
  const t = useTranslation();
  const navigate = useNavigate();
  const { isIncidentFlashing } = useIncidentNotification();
  const queryClient = useQueryClient();
  const signalR = useSignalR();

  // Helper function to get ambulance callsign by resourceId
  const getAmbulanceCallsign = (resourceId: number) => {
    const ambulance = vehicles?.find(v => v.id === resourceId);
    return ambulance?.callsign || `Ambulance ${resourceId}`;
  };

  // Fetch real data filtered by user's agency
  const { data: vehicles, isLoading: vehiclesLoading } = useQuery({
    queryKey: ['vehicles', 'ekab-dashboard'],
    queryFn: () => vehiclesApi.getAll().then(res => res.data),
    enabled: !!user?.agencyId,
    staleTime: 30000,
  });

  const { data: incidents, isLoading: incidentsLoading } = useQuery({
    queryKey: ['incidents', 'ekab-dashboard'],
    queryFn: () => incidentsApi.getAll().then(res => res.data),
    enabled: !!user?.agencyId,
    staleTime: 30000,
  });

  // Fetch personnel data for dynamic count
  const { data: personnel } = useQuery({
    queryKey: ['personnel', 'ekab-dashboard'],
    queryFn: () => personnelApi.getAll({ isActive: true }).then(res => res.data),
    enabled: !!user?.agencyId,
    staleTime: 30000,
  });

  // Set up SignalR event handlers to invalidate queries when incidents change
  useEffect(() => {
    if (!signalR) return;

    // Add handlers and store cleanup functions
    const cleanupCreated = signalR.addIncidentCreatedHandler(() => {
      queryClient.invalidateQueries({ queryKey: ['incidents', 'ekab-dashboard'] });
    });

    const cleanupStatusChanged = signalR.addIncidentStatusChangedHandler(() => {
      queryClient.invalidateQueries({ queryKey: ['incidents', 'ekab-dashboard'] });
    });

    const cleanupUpdate = signalR.addIncidentUpdateHandler(() => {
      queryClient.invalidateQueries({ queryKey: ['incidents', 'ekab-dashboard'] });
    });

    const cleanupResourceAssigned = signalR.addResourceAssignedHandler(() => {
      queryClient.invalidateQueries({ queryKey: ['incidents', 'ekab-dashboard'] });
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

  const availableUnits = vehicles?.map(vehicle => ({
    id: vehicle.id.toString(),
    name: vehicle.callsign,
    status: getVehicleStatusTranslation(vehicle.status, t),
    crew: 2, // Typical ambulance crew
    type: vehicle.type.includes('ALS') ? 'ALS' : 'BLS',
    equipment: vehicle.type.includes('ALS') ? 'Advanced Life Support' : 'Basic Life Support'
  })) || [];

  const hospitalStatus = [
    { name: 'General Hospital Athens', beds: 15, status: 'Available', waitTime: '5 min' },
    { name: 'Evangelismos Hospital', beds: 8, status: 'Limited', waitTime: '12 min' },
    { name: 'KAT Hospital', beds: 22, status: 'Available', waitTime: '3 min' },
    { name: 'Laiko Hospital', beds: 3, status: 'Critical', waitTime: '25 min' }
  ];

  // Using the new vibrant status color function from utils
  const getStatusColor = (status: string) => {
    return getStatusBadgeColor(status);
  };

  // Using the new vibrant priority color function from utils
  const getPriorityColor = (priority: string) => {
    // Convert string to IncidentPriority enum and use the vibrant color function
    const priorityMap: { [key: string]: any } = {
      'Critical': 0, // IncidentPriority.Critical
      'High': 1,     // IncidentPriority.High  
      'Normal': 2,   // IncidentPriority.Normal
      'Low': 3       // IncidentPriority.Low
    };
    return getIncidentPriorityBadgeColor(priorityMap[priority] ?? 2);
  };

  const getHospitalStatusColor = (status: string) => {
    switch (status) {
      case 'Available': return 'text-green-600';
      case 'Limited': return 'text-yellow-600';
      case 'Critical': return 'text-red-600';
      default: return 'text-gray-600';
    }
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
            <Heart className="h-8 w-8 text-red-500" />
            {t.dashboard} - {t.ekab}
          </h1>
          <p className="text-gray-600 dark:text-gray-400 mt-1">
            {t.welcomeBack}, {user?.name} - {user?.agencyName}
          </p>
        </div>
        <div className="flex gap-2">
          <Button className="bg-red-600 hover:bg-red-700">
            <Phone className="h-4 w-4 mr-2" />
            166
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
          <CardContent>
            <div className="text-2xl font-bold text-gray-900 dark:text-white">{totalActiveIncidents}</div>
            <p className="text-xs text-gray-600 dark:text-gray-400">{t.totalActiveIncidents}</p>
          </CardContent>
        </Card>

        <Card className="bg-white dark:bg-gray-800 border-gray-200 dark:border-gray-700">
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium text-gray-900 dark:text-white">{t.resourcesAvailable}</CardTitle>
            <Truck className="h-4 w-4 text-green-500" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-gray-900 dark:text-white">
              {availableUnits.filter(unit => unit.status === 'Available').length}
            </div>
            <p className="text-xs text-gray-600 dark:text-gray-400">{t.outOfnumber} {availableUnits.length} {t.total}</p>
          </CardContent>
        </Card>

        <Card className="bg-white dark:bg-gray-800 border-gray-200 dark:border-gray-700">
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium text-gray-900 dark:text-white">Paramedics on Duty</CardTitle>
            <Stethoscope className="h-4 w-4 text-blue-500" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-gray-900 dark:text-white">{personnel?.length || 0}</div>
            <p className="text-xs text-gray-600 dark:text-gray-400">{t.onAllStations}</p>
          </CardContent>
        </Card>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Active Medical Incidents */}
        <Card className="lg:col-span-2 bg-white dark:bg-gray-800 border-gray-200 dark:border-gray-700">
          <CardHeader>
            <CardTitle className="flex items-center gap-2 text-gray-900 dark:text-white">
              <Activity className="h-5 w-5 text-red-500" />
              {t.activeIncidents} - Medical
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
                    <CardContent className="p-4 space-y-3">
                      <div className="flex items-center justify-between">
                        <div className="flex flex-col">
                          <h3 className={`font-semibold ${getIncidentCardTextColor(incident.status)}`}>{incident.mainCategory}</h3>
                          <h4 className={`text-sm font-medium ${getIncidentCardTextColor(incident.status)}`}>{incident.subCategory}</h4>
                        </div>
                        <div className="flex items-center gap-2">
                          <Badge className={`${getPriorityColor(incident.priority.toString())} text-white`}>
                            {incident.priority}
                          </Badge>
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
                        <div className="bg-gray-50 dark:bg-gray-600 rounded p-3">
                          <div className={`text-sm font-medium mb-1 ${getIncidentCardTextColor(incident.status)}`}>{t.incidentDetails}</div>
                          <div className={`text-sm ${getIncidentCardSecondaryTextColor(incident.status)}`}>
                            {incident.notes}
                          </div>
                        </div>
                      )}
                      
                      <div className="flex flex-wrap gap-1">
                        {incident.assignments
                          .filter(assignment => assignment.resourceType === ResourceType.Vehicle)
                          .map((assignment) => (
                          <Badge key={assignment.id} variant="outline" className="text-xs border-gray-300 dark:border-gray-500 text-gray-700 dark:text-gray-300">
                            {getAmbulanceCallsign(assignment.resourceId)}
                          </Badge>
                        ))}
                      </div>
                    </CardContent>
                  </div>
                  );
                })
              ) : (
                <div className="text-center py-8 text-gray-500 dark:text-gray-400">
                  No active medical calls at this time
                </div>
              )}
            </div>
          </CardContent>
        </Card>

        {/* Hospital Status */}
        <Card className="bg-white dark:bg-gray-800 border-gray-200 dark:border-gray-700">
          <CardHeader>
            <CardTitle className="flex items-center gap-2 text-gray-900 dark:text-white">
              <Building className="h-5 w-5 text-blue-500" />
               {t.status} {t.of} {t.hospitalsOff}
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="space-y-4">
              {hospitalStatus.map((hospital, index) => (
                <div key={index} className="border rounded-lg p-3 border-gray-200 dark:border-gray-600 bg-gray-50 dark:bg-gray-700">
                  <div className="font-medium text-sm mb-2 text-gray-900 dark:text-white">{hospital.name}</div>
                  <div className="flex items-center justify-between mb-1">
                    <span className="text-xs text-gray-600 dark:text-gray-300">Available Beds</span>
                    <span className="font-medium text-gray-900 dark:text-white">{hospital.beds}</span>
                  </div>
                  <div className="flex items-center justify-between mb-2">
                    <span className="text-xs text-gray-600 dark:text-gray-300">Wait Time</span>
                    <span className="font-medium text-gray-900 dark:text-white">{hospital.waitTime}</span>
                  </div>
                  <Badge 
                    variant="outline" 
                    className={`text-xs border-gray-300 dark:border-gray-500 ${getHospitalStatusColor(hospital.status)} dark:text-gray-300`}
                  >
                    {hospital.status}
                  </Badge>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Unit Status */}
      <Card className="bg-white dark:bg-gray-800 border-gray-200 dark:border-gray-700">
        <CardHeader>
          <CardTitle className="flex items-center gap-2 text-gray-900 dark:text-white">
            <Truck className="h-5 w-5 text-green-500" />
            {t.resourcesCapital}
          </CardTitle>
        </CardHeader>
        <CardContent className="flex-1">
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4 flex-1 overflow-y-auto">
            {availableUnits.length > 0 ? (
              availableUnits.map((unit) => (
                <div key={unit.id} className="flex items-center justify-between p-3 border rounded-lg border-gray-200 dark:border-gray-600 bg-gray-50 dark:bg-gray-700">
                  <div className="flex items-center gap-3">
                    <div className={`w-3 h-3 rounded-full ${getStatusColor(unit.status)}`} />
                    <div>
                      <div className="font-medium text-gray-900 dark:text-white">{unit.name}</div>
                      <div className="text-sm text-gray-600 dark:text-gray-300">
                        {unit.type} • {t.crew}: {unit.crew}
                      </div>
                      <div className="text-xs text-gray-500 dark:text-gray-400">
                        {unit.equipment}
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
    </ErrorBoundary>
  );
};

export default EKABDashboard;