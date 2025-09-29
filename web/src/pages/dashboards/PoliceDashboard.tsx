import React, { useEffect } from 'react';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import { Card, CardContent, CardHeader, CardTitle } from '../../components/ui/card';
import { Badge } from '../../components/ui/badge';
import { Button } from '../../components/ui/button';
import {
    Shield,
    Car,
    Users,
    AlertTriangle,
    Clock,
    MapPin,
    Phone,
    Radio,
    UserCheck,
    FileText,
    Eye
} from 'lucide-react';
import { useUserStore } from '../../lib/userStore';
import { vehiclesApi, incidentsApi, personnelApi, patrolZonesApi } from '../../lib/api';
import { IncidentStatus, ResourceType } from '../../types';
import { LoadingSpinner, ErrorBoundary } from '../../components';
import { useTranslation } from '../../hooks/useTranslation';
import { useIncidentNotification } from '../../lib/incidentNotificationContext';
import { useSignalR } from '../../hooks/useSignalR';
import { getIncidentCardBackgroundColor, getIncidentStatusBadgeColor, getIncidentStatusTranslation, getIncidentCardTextColor, getIncidentCardSecondaryTextColor, getStatusBadgeColor, getVehicleStatusTranslation } from '../../utils/incidentUtils';

const PoliceDashboard: React.FC = () => {
    const { user, isDispatcher } = useUserStore();
    const t = useTranslation();
    const navigate = useNavigate();
    const { isIncidentFlashing } = useIncidentNotification();
    const queryClient = useQueryClient();
    const signalR = useSignalR();

    // Helper function to get patrol unit callsign by resourceId
    const getPatrolUnitCallsign = (resourceId: number) => {
        const unit = vehicles?.find(v => v.id === resourceId);
        return unit?.callsign || `Unit ${resourceId}`;
    };

    // Fetch real data filtered by user's agency
    const { data: vehicles, isLoading: vehiclesLoading } = useQuery({
        queryKey: ['vehicles', 'police-dashboard'],
        queryFn: () => vehiclesApi.getAll().then(res => res.data),
        enabled: !!user?.agencyId,
        staleTime: 30000,
    });

    const { data: incidents, isLoading: incidentsLoading } = useQuery({
        queryKey: ['incidents', 'police-dashboard'],
        queryFn: () => incidentsApi.getAll().then(res => res.data),
        enabled: !!user?.agencyId,
        staleTime: 30000,
    });

    // Fetch personnel data for dynamic count
    const { data: personnel } = useQuery({
        queryKey: ['personnel', 'police-dashboard'],
        queryFn: () => personnelApi.getAll({ isActive: true }).then(res => res.data),
        enabled: !!user?.agencyId,
        staleTime: 30000,
    });

    // Set up SignalR event handlers to invalidate queries when incidents change
    useEffect(() => {
        if (!signalR) return;

        // Add handlers and store cleanup functions
        const cleanupCreated = signalR.addIncidentCreatedHandler(() => {
            queryClient.invalidateQueries({ queryKey: ['incidents', 'police-dashboard'] });
        });

        const cleanupStatusChanged = signalR.addIncidentStatusChangedHandler(() => {
            queryClient.invalidateQueries({ queryKey: ['incidents', 'police-dashboard'] });
        });

        const cleanupUpdate = signalR.addIncidentUpdateHandler(() => {
            queryClient.invalidateQueries({ queryKey: ['incidents', 'police-dashboard'] });
        });

        const cleanupResourceAssigned = signalR.addResourceAssignedHandler(() => {
            queryClient.invalidateQueries({ queryKey: ['incidents', 'police-dashboard'] });
        });

        // Return cleanup function
         return () => {
             cleanupCreated();
             cleanupStatusChanged();
             cleanupUpdate();
             cleanupResourceAssigned();
         };
     }, [signalR, queryClient]);

    // Fetch patrol zones data
    const { data: patrolZones, isLoading: patrolZonesLoading } = useQuery({
        queryKey: ['patrolZones', 'police-dashboard', user?.stationId],
        queryFn: () => patrolZonesApi.getAll(user?.stationId ? { stationId: user.stationId } : {}).then(res => res.data),
        enabled: !!user?.agencyId,
        staleTime: 30000,
    });

    // Helper function to determine patrol zone status
    const getPatrolZoneStatus = (patrolZone: any) => {
        if (!patrolZone.vehicleAssignments || patrolZone.vehicleAssignments.length === 0) {
            return 'uncovered';
        }
        
        const activeAssignments = patrolZone.vehicleAssignments.filter((assignment: any) => assignment.isActive);
        
        if (activeAssignments.length === 0) {
            return 'uncovered';
        } else if (activeAssignments.length === 1) {
            return 'limited';
        } else {
            return 'covered';
        }
    };

    // Total active incidents count for statistics
    const totalActiveIncidents = React.useMemo(() => {
        if (!incidents) return 0;
        return incidents.filter(incident =>
            incident.status === IncidentStatus.OnGoing ||
            incident.status === IncidentStatus.PartialControl ||
            incident.status === IncidentStatus.Controlled
        ).length;
    }, [incidents]);

    // Process data for dashboard with role-based filtering
    const processedIncidents = React.useMemo(() => {
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

    const availableUnits = vehicles?.map(vehicle => ({
        id: vehicle.id.toString(),
        name: vehicle.callsign,
        status: getVehicleStatusTranslation(vehicle.status, t),
        crew: 2, // Typical police unit crew
        type: vehicle.type,
        location: vehicle.latitude && vehicle.longitude ?
            `${vehicle.latitude.toFixed(4)}, ${vehicle.longitude.toFixed(4)}` : t.unknown,
    })) || [];

    const getStatusColor = (status: string) => {
        switch (status) {
            case 'Available': return 'bg-green-500';
            case 'Busy': return 'bg-yellow-500';
            case 'Maintenance': return 'bg-red-500';
            case 'OutOfService': return 'bg-gray-500';
            default: return 'bg-gray-500';
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
                            <Shield className="h-8 w-8 text-blue-600" />
                            {t.dashboard} - {t.police}
                        </h1>
                        <p className="text-gray-600 dark:text-gray-400 mt-1">
                            {t.welcomeBack}, {user?.name} - {user?.agencyName}
                        </p>
                    </div>
                    <div className="flex flex-col gap-2">
                        <Button className="bg-red-600 hover:bg-red-700 text-white">
                            <Phone className="h-4 w-4 mr-2" />
                            100
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
                            <Car className="h-4 w-4 text-green-500" />
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
                            <CardTitle className="text-sm font-medium text-gray-900 dark:text-white">{t.personnel} {t.onDuty}</CardTitle>
                            <Users className="h-4 w-4 text-blue-500" />
                        </CardHeader>
                        <CardContent>
                            <div className="text-2xl font-bold text-gray-900 dark:text-white">{personnel?.length || 0}</div>
                            <p className="text-xs text-gray-600 dark:text-gray-400">{t.onAllStations}</p>
                        </CardContent>
                    </Card>
                </div>

                <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
                    {/* Active Incidents */}
                    <Card className="bg-white dark:bg-gray-800 border-gray-200 dark:border-gray-700">
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
                                           className={`cursor-pointer hover:shadow-md transition-shadow rounded-lg border border-gray-200 dark:border-gray-600 border-l-4 ${getIncidentCardBackgroundColor(incident.status)} ${isFlashing ? 'incident-flash' : ''}`}
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
                                                            {getPatrolUnitCallsign(assignment.resourceId)}
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
                    <Card className="bg-white dark:bg-gray-800 border-gray-200 dark:border-gray-700">
                        <CardHeader>
                            <CardTitle className="flex items-center gap-2 text-gray-900 dark:text-white">
                                <Car className="h-5 w-5 text-blue-500" />
                                {t.resourcesCapital}
                            </CardTitle>
                        </CardHeader>
                        <CardContent className="flex-1">
                            <div className="space-y-3 flex-1 overflow-y-auto">
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
                                                        {unit.location}
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

                {/* Additional Police-specific sections */}
                <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
                    {/* Recent Arrests */}
                    <Card className="bg-white dark:bg-gray-800 border-gray-200 dark:border-gray-700">
                        <CardHeader>
                            <CardTitle className="flex items-center gap-2 text-gray-900 dark:text-white">
                                <UserCheck className="h-5 w-5 text-gray-600 dark:text-gray-400" />
                                {t.recentActivity}
                            </CardTitle>
                        </CardHeader>
                        <CardContent>
                            <div className="space-y-3">
                                <div className="text-center py-4 text-gray-500 dark:text-gray-400 text-sm">
                                    Activity data will be available when connected to police records system
                                </div>
                            </div>
                        </CardContent>
                    </Card>

                    {/* Patrol Areas */}
                    <Card className="bg-white dark:bg-gray-800 border-gray-200 dark:border-gray-700">
                        <CardHeader>
                            <CardTitle className="flex items-center gap-2 text-gray-900 dark:text-white">
                                <MapPin className="h-5 w-5 text-green-500" />
                                {t.patrolCoverage}
                            </CardTitle>
                        </CardHeader>
                        <CardContent>
                            <div className="space-y-3">
                                {patrolZonesLoading ? (
                                    <div className="flex items-center justify-center py-4">
                                        <LoadingSpinner size="sm" />
                                        <span className="ml-2 text-sm text-gray-500 dark:text-gray-400">
                                            Loading patrol zones...
                                        </span>
                                    </div>
                                ) : patrolZones && patrolZones.length > 0 ? (
                                    patrolZones.map((zone: any) => {
                                        const status = getPatrolZoneStatus(zone);
                                        const statusText = status === 'covered' ? 'Covered' : 
                                                         status === 'limited' ? 'Limited' : 'Uncovered';
                                        
                                        return (
                                            <div key={zone.id} className="flex items-center justify-between">
                                                <span className="text-sm text-gray-700 dark:text-gray-300">
                                                    {zone.name}
                                                </span>
                                                <Badge className={getStatusBadgeColor(status)}>
                                                    {statusText}
                                                </Badge>
                                            </div>
                                        );
                                    })
                                ) : (
                                    <div className="text-center py-4 text-gray-500 dark:text-gray-400 text-sm">
                                        No patrol zones assigned to your station
                                    </div>
                                )}
                            </div>
                        </CardContent>
                    </Card>

                    {/* Quick Actions */}
                    <Card className="bg-white dark:bg-gray-800 border-gray-200 dark:border-gray-700">
                        <CardHeader>
                            <CardTitle className="flex items-center gap-2 text-gray-900 dark:text-white">
                                <Radio className="h-5 w-5 text-blue-500" />
                                {t.quickActions}
                            </CardTitle>
                        </CardHeader>
                        <CardContent>
                            <div className="space-y-2">
                                <Button className="w-full justify-start border-gray-300 dark:border-gray-500 text-gray-700 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-600" variant="outline">
                                    <FileText className="h-4 w-4 mr-2" />
                                    {t.newIncidentReport}
                                </Button>
                            </div>
                        </CardContent>
                    </Card>
                </div>
            </div>
        </ErrorBoundary>
    );
};

export default PoliceDashboard;