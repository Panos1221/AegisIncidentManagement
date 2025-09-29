import { useState, useMemo, useEffect } from 'react'
import { useQuery, useQueryClient } from '@tanstack/react-query'
import { incidentsApi, vehiclesApi } from '../lib/api'
import { IncidentStatus, Incident, VehicleStatus } from '../types'
import { Clock, MapPin, AlertTriangle, Truck, Users, Phone, Calendar } from 'lucide-react'
import { useTranslation } from '../hooks/useTranslation'
import { useIncidentNotification } from '../lib/incidentNotificationContext'
import { useSignalR } from '../hooks/useSignalR'
import { useUserStore } from '../lib/userStore'
import { formatInLocalTimezone } from '../utils/dateUtils'
import { getIncidentCardBackgroundColor, getIncidentStatusBadgeColor, getIncidentStatusTranslation, getIncidentCardTextColor, getIncidentCardSecondaryTextColor, getAssignmentStatusTranslation, getIncidentPriorityTranslation } from '../utils/incidentUtils'
import IncidentDetailsPanel from '../components/IncidentDetailsPanel'

// Helper functions for vehicle status
const getVehicleStatusColor = (status: VehicleStatus | string) => {
  switch (status) {
    case VehicleStatus.Available:
    case 'Available':
      return 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200'
    case VehicleStatus.Busy:
    case 'Busy':
    case 'On Scene':
      return 'bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-200'
    case VehicleStatus.Maintenance:
    case 'Maintenance':
      return 'bg-yellow-100 text-yellow-800 dark:bg-yellow-900 dark:text-yellow-200'
    case 'Notified':
      return 'bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-200'
    case 'En Route':
      return 'bg-orange-100 text-orange-800 dark:bg-orange-900 dark:text-orange-200'
    case 'Finished':
    case 'Completed':
      return 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200'
    default:
      return 'bg-gray-100 text-gray-800 dark:bg-gray-900 dark:text-gray-200'
  }
}

// Removed hardcoded getVehicleStatusText - now using getVehicleStatusTranslation from utils

export default function CAD() {
  const t  = useTranslation()
  const { isIncidentFlashing } = useIncidentNotification()
  const [selectedIncident, setSelectedIncident] = useState<Incident | null>(null)
  const queryClient = useQueryClient()
  const signalR = useSignalR()
  const { user } = useUserStore()

  // Fetch incidents excluding resolved, closed, and cancelled statuses
  const { data: incidents = [], isLoading: incidentsLoading } = useQuery({
    queryKey: ['incidents'],
    queryFn: () => incidentsApi.getAll().then(res => res.data),
  })

  const { data: vehicles = [] } = useQuery({
    queryKey: ['vehicles'],
    queryFn: () => vehiclesApi.getAll().then(res => res.data),
  })



  // Set up SignalR event handlers to invalidate queries when incidents change
  useEffect(() => {
    if (!signalR) return

    // Add handlers and store cleanup functions
    const cleanupCreated = signalR.addIncidentCreatedHandler(() => {
      queryClient.invalidateQueries({ queryKey: ['incidents'] })
    })

    const cleanupStatusChanged = signalR.addIncidentStatusChangedHandler(() => {
      queryClient.invalidateQueries({ queryKey: ['incidents'] })
    })

    const cleanupUpdate = signalR.addIncidentUpdateHandler(() => {
      queryClient.invalidateQueries({ queryKey: ['incidents'] })
    })

    const cleanupResourceAssigned = signalR.addResourceAssignedHandler(() => {
      queryClient.invalidateQueries({ queryKey: ['incidents'] })
      queryClient.invalidateQueries({ queryKey: ['vehicles'] })
    })

    // New handlers for real-time updates
    const cleanupAssignmentStatusChanged = signalR.addAssignmentStatusChangedHandler(() => {
      queryClient.invalidateQueries({ queryKey: ['incidents'] })
    })

    const cleanupIncidentLogAdded = signalR.addIncidentLogAddedHandler(() => {
      queryClient.invalidateQueries({ queryKey: ['incidents'] })
    })

    const cleanupVehicleAssignmentChanged = signalR.addVehicleAssignmentChangedHandler(() => {
      queryClient.invalidateQueries({ queryKey: ['vehicles'] })
      queryClient.invalidateQueries({ queryKey: ['incidents'] })
    })

    // Return cleanup function
     return () => {
       cleanupCreated();
       cleanupStatusChanged();
       cleanupUpdate();
       cleanupResourceAssigned();
       cleanupAssignmentStatusChanged();
       cleanupIncidentLogAdded();
       cleanupVehicleAssignmentChanged();
     };
   }, [signalR, queryClient]);

  // Update selectedIncident with fresh data when incidents are re-fetched
  useEffect(() => {
    if (selectedIncident && incidents) {
      const updatedIncident = incidents.find(i => i.id === selectedIncident.id)
      if (updatedIncident && JSON.stringify(updatedIncident) !== JSON.stringify(selectedIncident)) {
        setSelectedIncident(updatedIncident)
      }
    }
  }, [incidents, selectedIncident])

  // Get vehicle assignments from selected incident, filtered to only show user's station vehicles
  const vehicleAssignments = useMemo(() => {
    if (!selectedIncident || !user?.stationId) return []

    return selectedIncident.assignments
      ?.filter(a => a.resourceType === 0) // ResourceType.Vehicle = 0
      ?.filter(a => {
        const vehicle = vehicles.find(v => v.id === a.resourceId)
        return vehicle && vehicle.stationId === user.stationId
      }) || []
  }, [selectedIncident, vehicles, user?.stationId])

  const assignmentsLoading = false

  // Filter incidents to exclude resolved, closed, and cancelled
  const activeIncidents = useMemo(() => {
    if (!incidents) return []
    return incidents.filter(incident => 
      incident.status !== IncidentStatus.Closed &&
      incident.status !== IncidentStatus.FullyControlled
    ).sort((a, b) => {
      // Sort by creation date first (newest first) to show new incidents at the top
      const dateComparison = new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
      if (dateComparison !== 0) {
        return dateComparison
      }
      // If same creation time, then sort by priority (Critical first)
      return a.priority - b.priority // Lower number = higher priority
    })
  }, [incidents])



  if (incidentsLoading) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600"></div>
      </div>
    )
  }

  return (
    <div className="h-full px-6 py-4">
      <div className="mb-4">
        <h1 className="text-2xl font-bold text-gray-900 dark:text-gray-100">
          {t.cad || 'Computer-aided dispatch (CAD)'}
        </h1>
        <p className="mt-1 text-sm text-gray-600 dark:text-gray-400">
          {t.cadDescription || 'Central command center for emergency response personnel'}
        </p>
      </div>

      <div className="grid grid-cols-12 gap-4 h-[calc(100vh-140px)]">
        {/* Left Column - Incidents List */}
        <div className="col-span-4 bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 overflow-hidden">
          <div className="p-4 border-b border-gray-200 dark:border-gray-700">
            <h2 className="text-lg font-semibold text-gray-900 dark:text-gray-100 flex items-center">
              <AlertTriangle className="h-5 w-5 mr-2 text-red-500" />
              {t.activeIncidents || 'Active Incidents'}
            </h2>
            <p className="text-sm text-gray-500 dark:text-gray-400 mt-1">
              {activeIncidents.length} {t.incidents || 'incidents'}
            </p>
          </div>
          <div className="overflow-y-auto h-[calc(100%-80px)]">
            {activeIncidents.length === 0 ? (
              <div className="p-4 text-center text-gray-500 dark:text-gray-400">
                {t.noActiveIncidents || 'No active incidents'}
              </div>
            ) : (
              <div className="space-y-2 p-2 pb-4">
                {activeIncidents.map((incident) => {
                  const isFlashing = isIncidentFlashing(incident.id);
                  return (
                    <div
                      key={incident.id}
                      onClick={() => setSelectedIncident(incident)}
                      className={`p-3 rounded-lg cursor-pointer transition-all duration-200 border-2 ${
                        selectedIncident?.id === incident.id
                          ? 'border-blue-500 shadow-lg ring-2 ring-blue-200 dark:ring-blue-800'
                          : 'border-transparent hover:border-gray-300 dark:hover:border-gray-600'
                      } ${getIncidentCardBackgroundColor(incident.status)} ${isFlashing ? 'incident-flash' : ''}`}
                    >
                      <div className="flex items-start justify-between mb-2">
                        <div className="flex items-center gap-2">
                          <span className={`inline-flex items-center px-2 py-1 rounded-full text-xs font-medium ${getIncidentStatusBadgeColor(incident.status)}`}>
                            {getIncidentStatusTranslation(incident.status, t)}
                          </span>
                          {incident.participationType === 'Reinforcement' && (
                            <span className="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-200">
                              {t.reinforcement || 'Reinforcement'}
                            </span>
                          )}
                        </div>
                        <span className="text-xs text-black dark:text-white">
                          #{incident.id}
                        </span>
                      </div>
                      <h3 className={`font-medium ${getIncidentCardTextColor(incident.status)} text-sm mb-1`}>
                        <div className="flex flex-col">
                          <span className="font-semibold">{incident.mainCategory}</span>
                          <span className="text-xs text-gray-0 dark:text-gray-200">{incident.subCategory}</span>
                        </div>
                      </h3>
                      <div className={`flex items-center text-xs ${getIncidentCardSecondaryTextColor(incident.status)} mb-1`}>
                        <MapPin className="h-3 w-3 mr-1" />
                        {/* Show detailed address or fallback to full address */}
                        {(() => {
                          if (incident.street || incident.city) {
                            const parts = [
                              incident.street && incident.streetNumber ? `${incident.street} ${incident.streetNumber}` : incident.street,
                              incident.city
                            ].filter(Boolean)
                            return parts.join(', ') || 'No address'
                          }
                          return incident.address || 'No address'
                        })()}
                      </div>
                      <div className={`flex items-center justify-between text-xs ${getIncidentCardSecondaryTextColor(incident.status)}`}>
                        <div className="flex items-center">
                          <Clock className="h-3 w-3 mr-1" />
                          {formatInLocalTimezone(incident.createdAt, 'HH:mm dd/MM')}
                        </div>
                        <div className="flex items-center gap-3 text-black dark:text-white">
                          <div className="flex items-center">
                            <Truck className="h-3 w-3 mr-1" />
                            {incident.assignments?.filter(a => a.resourceType === 0).length || 0}
                          </div>
                          <div className="flex items-center">
                            <Users className="h-3 w-3 mr-1" />
                            {incident.assignments?.filter(a => a.resourceType === 1).length || 0}
                          </div>
                        </div>
                      </div>
                    </div>
                  );
                })}
              </div>
            )}
          </div>
        </div>

        {/* Middle Column - Incident Details */}
        <div className="col-span-5 bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 overflow-hidden">
          {!selectedIncident ? (
            <div className="flex items-center justify-center h-full text-gray-500 dark:text-gray-400">
              <div className="text-center">
                <AlertTriangle className="h-12 w-12 mx-auto mb-4 text-gray-300 dark:text-gray-600" />
                <p>{t.selectIncidentToView || 'Select an incident to view details'}</p>
              </div>
            </div>
          ) : (
            <IncidentDetailsPanel incident={selectedIncident} />
          )}
        </div>

        {/* Right Column - Assigned Vehicles */}
        <div className="col-span-3 bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 overflow-hidden">
          <div className="p-4 border-b border-gray-200 dark:border-gray-700">
            <h2 className="text-lg font-semibold text-gray-900 dark:text-gray-100 flex items-center">
              <Truck className="h-5 w-5 mr-2 text-green-500" />
              {t.assignedVehicles || 'Assigned Vehicles'}
            </h2>
            {selectedIncident && (
              <p className="text-sm text-gray-500 dark:text-gray-400 mt-1">
                {t.incidentId || 'Incident'} #{selectedIncident.id}
                {selectedIncident.participationType === 'Reinforcement' && (
                  <span className="ml-2 text-xs text-blue-600 dark:text-blue-400">
                    ({t.reinforcement || 'Reinforcement'})
                  </span>
                )}
              </p>
            )}
          </div>
          <div className="overflow-y-auto h-[calc(100%-80px)]">
            {!selectedIncident ? (
              <div className="p-4 text-center text-gray-500 dark:text-gray-400">
                <Truck className="h-8 w-8 mx-auto mb-2 text-gray-300 dark:text-gray-600" />
                <p className="text-sm">{t.selectIncidentToViewVehicles || 'Select an incident to view assigned vehicles'}</p>
              </div>
            ) : assignmentsLoading ? (
              <div className="p-4 text-center">
                <div className="animate-spin rounded-full h-6 w-6 border-b-2 border-primary-600 mx-auto"></div>
              </div>
            ) : !vehicleAssignments || vehicleAssignments.length === 0 ? (
              <div className="p-4 text-center text-gray-500 dark:text-gray-400">
                <Truck className="h-8 w-8 mx-auto mb-2 text-gray-300 dark:text-gray-600" />
                <p className="text-sm">{t.noVehiclesAssigned || 'No vehicles assigned to this incident'}</p>
              </div>
            ) : (
              <div className="p-4 space-y-4 pb-6">
                {vehicleAssignments.map((assignment) => {
                  const vehicle = vehicles.find(v => v.id === assignment.resourceId)
                  return (
                    <div key={assignment.id} className="bg-gray-50 dark:bg-gray-900 rounded-lg p-3 border border-gray-200 dark:border-gray-700">
                      <div className="flex items-center justify-between mb-2">
                        <h4 className="font-medium text-gray-900 dark:text-gray-100">
                          {vehicle?.callsign || `Vehicle ${assignment.resourceId}`}
                        </h4>
                        <span className={`inline-flex items-center px-2 py-1 rounded-full text-xs font-medium ${getVehicleStatusColor(assignment.status)}`}>
                          {getAssignmentStatusTranslation(assignment.status, t)}
                        </span>
                      </div>
                      
                      <div className="space-y-2 text-xs">
                        <div className="grid grid-cols-2 gap-2">
                          <div>
                            <span className="font-medium text-gray-600 dark:text-gray-400">{t.plateNumber || 'Plate'}:</span>
                            <div className="text-gray-900 dark:text-gray-100">{vehicle?.plateNumber || 'N/A'}</div>
                          </div>
                          <div>
                            <span className="font-medium text-gray-600 dark:text-gray-400">{t.vehicleType || 'Type'}:</span>
                            <div className="text-gray-900 dark:text-gray-100">{vehicle?.type || 'N/A'}</div>
                          </div>
                        </div>
                        
                        {/* Timeline */}
                        <div className="border-t border-gray-200 dark:border-gray-700 pt-2 mt-2">
                          <div className="space-y-2">
                            <div className="flex items-center justify-between">
                              <div className="flex items-center">
                                <div className={`w-2 h-2 rounded-full mr-2 ${assignment.dispatchedAt ? 'bg-blue-500' : 'bg-gray-300 dark:bg-gray-600'}`}></div>
                                <span className="text-gray-600 dark:text-gray-400">{t.notified || 'Notified'}</span>
                              </div>
                              <span className="text-gray-900 dark:text-gray-100">
                                {assignment.dispatchedAt ? formatInLocalTimezone(assignment.dispatchedAt, 'HH:mm') : '--:--'}
                              </span>
                            </div>
                            
                            <div className="flex items-center justify-between">
                              <div className="flex items-center">
                                <div className={`w-2 h-2 rounded-full mr-2 ${assignment.onSceneAt ? 'bg-orange-500' : 'bg-gray-300 dark:bg-gray-600'}`}></div>
                                <span className="text-gray-600 dark:text-gray-400">{t.onScene || 'On Scene'}</span>
                              </div>
                              <span className="text-gray-900 dark:text-gray-100">
                                {assignment.onSceneAt ? formatInLocalTimezone(assignment.onSceneAt, 'HH:mm') : '--:--'}
                              </span>
                            </div>
                            
                            <div className="flex items-center justify-between">
                              <div className="flex items-center">
                                <div className={`w-2 h-2 rounded-full mr-2 ${assignment.completedAt ? 'bg-green-500' : 'bg-gray-300 dark:bg-gray-600'}`}></div>
                                <span className="text-gray-600 dark:text-gray-400">{t.finished || 'Finished'}</span>
                              </div>
                              <span className="text-gray-900 dark:text-gray-100">
                                {assignment.completedAt ? formatInLocalTimezone(assignment.completedAt, 'HH:mm') : '--:--'}
                              </span>
                            </div>
                            
                            {/* Duration calculation */}
                            {assignment.dispatchedAt && assignment.completedAt && (
                              <div className="flex items-center justify-between pt-1 border-t border-gray-200 dark:border-gray-700">
                                <span className="text-gray-600 dark:text-gray-400 font-medium">{t.duration || 'Duration'}:</span>
                                <span className="text-gray-900 dark:text-gray-100 font-medium">
                                  {Math.round((new Date(assignment.completedAt).getTime() - new Date(assignment.dispatchedAt).getTime()) / (1000 * 60))} min
                                </span>
                              </div>
                            )}
                          </div>
                        </div>
                      </div>
                    </div>
                  )
                })}
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  )
}