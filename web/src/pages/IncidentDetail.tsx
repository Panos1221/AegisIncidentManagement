import { useState, useEffect } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { incidentsApi, personnelApi, vehiclesApi } from '../lib/api'
import { IncidentStatus, ResourceType, IncidentClosureReason } from '../types'
import { MapPin, Clock, Users, Truck, MessageSquare, X, AlertTriangle, Minus, User, Activity, CheckCircle2, XCircle, AlertCircle, Info, Phone, ArrowLeft } from 'lucide-react'
import { format } from 'date-fns'
import { formatInLocalTimezone } from '../utils/dateUtils'
import { useUserStore } from '../lib/userStore'
import { useTranslation } from '../hooks/useTranslation'
import { useSignalR } from '../hooks/useSignalR'
import { getIncidentStatusBadgeColor, getIncidentStatusTranslation, getIncidentStatusUpdateOptions, IncidentStatusOption, getIncidentPriorityBadgeColor, getVehicleStatusTranslation, getAssignmentStatusTranslation, getIncidentPriorityTranslation, translateLogMessage } from '../utils/incidentUtils'
import EnhancedAssignmentModal from '../components/EnhancedAssignmentModal'
import IncidentDetailsPanel from '../components/IncidentDetailsPanel'

export default function IncidentDetail() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const { canAddIncidentNotes, canModifyIncident, user } = useUserStore()
  const t = useTranslation()
  const signalR = useSignalR()

  const [showAssignModal, setShowAssignModal] = useState(false)
  const [showLogModal, setShowLogModal] = useState(false)
  const [showStatusModal, setShowStatusModal] = useState(false)
  const [logMessage, setLogMessage] = useState('')
  const [selectedStatus, setSelectedStatus] = useState<IncidentStatus>(IncidentStatus.OnGoing)
  const [errorMessage, setErrorMessage] = useState('')
  const [successMessage, setSuccessMessage] = useState('')
  const [showUnassignModal, setShowUnassignModal] = useState(false)
  const [selectedAssignment, setSelectedAssignment] = useState<any>(null)
  const [showVehicleStatusModal, setShowVehicleStatusModal] = useState(false)
  const [selectedVehicleAssignment, setSelectedVehicleAssignment] = useState<any>(null)
  const [showPersonnelStatusModal, setShowPersonnelStatusModal] = useState(false)
  const [selectedPersonnelAssignment, setSelectedPersonnelAssignment] = useState<any>(null)
  const [showCloseModal, setShowCloseModal] = useState(false)
  const [selectedClosureReason, setSelectedClosureReason] = useState<IncidentClosureReason>(IncidentClosureReason.Action)
  const [showCloseConfirmation, setShowCloseConfirmation] = useState(false)
  const [showReopenModal, setShowReopenModal] = useState(false)
  const [showCallersModal, setShowCallersModal] = useState(false)

  const { data: incident, isLoading } = useQuery({
    queryKey: ['incident', id],
    queryFn: () => incidentsApi.getById(Number(id)).then(res => res.data),
    enabled: !!id,
    refetchOnWindowFocus: false,
    staleTime: 0, // Always consider data stale to ensure fresh fetches
  })

  const { data: vehicles = [] } = useQuery({
    queryKey: ['vehicles'],
    queryFn: () => vehiclesApi.getAll().then(res => res.data),
  })

  const { data: allIncidents = [] } = useQuery({
    queryKey: ['incidents-for-assignments'],
    queryFn: () => incidentsApi.getAll().then(res => res.data),
    enabled: showAssignModal,
  })

  const { data: personnel = [] } = useQuery({
    queryKey: ['personnel'],
    queryFn: () => personnelApi.getAll({ isActive: true }).then(res => res.data),
  })

  // Set up SignalR event handlers to invalidate queries when this incident changes
  useEffect(() => {
    if (!signalR || !id) return

    const currentIncidentId = Number(id)

    // Invalidate incident query when status changes for this incident
    const cleanupStatusChanged = signalR.addIncidentStatusChangedHandler((statusChange: { incidentId: number; newStatus: string; timestamp: string }) => {
      if (statusChange && statusChange.incidentId === currentIncidentId) {
        queryClient.invalidateQueries({ queryKey: ['incident', id] })
      }
    })

    // Invalidate incident query when this incident is updated
    const cleanupUpdate = signalR.addIncidentUpdateHandler((update: { incidentId: number; updateData: any; timestamp: string }) => {
      if (update && update.incidentId === currentIncidentId) {
        queryClient.invalidateQueries({ queryKey: ['incident', id] })
      }
    })

    // Invalidate incident query when resources are assigned to this incident
    const cleanupResourceAssigned = signalR.addResourceAssignedHandler((assignment: { incidentId: number; resourceType: string; resourceId: number; timestamp: string }) => {
      if (assignment && assignment.incidentId === currentIncidentId) {
        queryClient.invalidateQueries({ queryKey: ['incident', id] })
      }
    })

    // Invalidate incident query when assignment status changes
    const cleanupAssignmentStatusChanged = signalR.addAssignmentStatusChangedHandler((statusChange: { incidentId: number; assignmentId: number; newStatus: string; oldStatus: string; timestamp: string }) => {
      if (statusChange && statusChange.incidentId === currentIncidentId) {
        queryClient.invalidateQueries({ queryKey: ['incident', id] })
      }
    })

    // Invalidate incident query when logs are added
    const cleanupIncidentLogAdded = signalR.addIncidentLogAddedHandler((log: { incidentId: number; message: string; at: string; by?: string; timestamp: string }) => {
      if (log && log.incidentId === currentIncidentId) {
        queryClient.invalidateQueries({ queryKey: ['incident', id] })
      }
    })

    return () => {
      cleanupStatusChanged();
      cleanupUpdate();
      cleanupResourceAssigned();
      cleanupAssignmentStatusChanged();
      cleanupIncidentLogAdded();
    };
  }, [signalR, queryClient, id])

  const updateStatusMutation = useMutation({
    mutationFn: (status: IncidentStatus) => incidentsApi.updateStatus(Number(id), status),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['incident', id] })
      setShowStatusModal(false)
    }
  })

  const assignResourceMutation = useMutation({
    mutationFn: ({ resourceType, resourceId }: { resourceType: ResourceType, resourceId: number }) =>
      incidentsApi.assign(Number(id), resourceType, resourceId),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['incident', id] })
      // Only close modal for personnel assignments, keep it open for vehicle assignments
      if (variables.resourceType === ResourceType.Personnel) {
        setShowAssignModal(false)
      }
      setSuccessMessage(t.resourceAssignedSuccessfully)
      setErrorMessage('')
      setTimeout(() => setSuccessMessage(''), 3000)
    },
    onError: (error: any) => {
      const message = error?.message || 'Failed to assign resource'
      setErrorMessage(message)
      setTimeout(() => setErrorMessage(''), 5000)
    }
  })

  const unassignResourceMutation = useMutation({
    mutationFn: (assignmentId: number) =>
      incidentsApi.unassign(Number(id), assignmentId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['incident', id] })
      setSuccessMessage(t.resourceUnassignedSuccessfully)
      setErrorMessage('')
      setTimeout(() => setSuccessMessage(''), 3000)
    },
    onError: (error: any) => {
      const message = error?.message || 'Failed to unassign resource'
      setErrorMessage(message)
      setTimeout(() => setErrorMessage(''), 5000)
    }
  })

  const addLogMutation = useMutation({
    mutationFn: (message: string) => incidentsApi.addLog(Number(id), message, user?.name || 'Unknown User'),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['incident', id] })
      setShowLogModal(false)
      setLogMessage('')
    }
  })

  const closeIncidentMutation = useMutation({
    mutationFn: (closureReason: IncidentClosureReason) =>
      incidentsApi.close(Number(id), {
        closureReason,
        closedByUserId: user?.id || 0
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['incident', id] })
      setShowCloseModal(false)
      setShowCloseConfirmation(false)
      setSuccessMessage(t.incidentClosedSuccessfully || 'Incident closed successfully')
      setErrorMessage('')
      setTimeout(() => setSuccessMessage(''), 3000)
    },
    onError: (error: any) => {
      const message = error?.message || 'Failed to close incident'
      setErrorMessage(message)
      setTimeout(() => setErrorMessage(''), 5000)
    }
  })

  const reopenIncidentMutation = useMutation({
    mutationFn: () => incidentsApi.reopen(Number(id)),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['incident', id] })
      setShowReopenModal(false)
      setSuccessMessage(t.incidentReopenedSuccessfully || 'Incident reopened successfully')
      setErrorMessage('')
      setTimeout(() => setSuccessMessage(''), 3000)
    },
    onError: (error: any) => {
      const message = error?.message || 'Failed to reopen incident'
      setErrorMessage(message)
      setTimeout(() => setErrorMessage(''), 5000)
    }
  })

  const updateAssignmentStatusMutation = useMutation({
    mutationFn: ({ assignmentId, status }: { assignmentId: number, status: string }) => {
      return incidentsApi.updateAssignmentStatus(Number(id), assignmentId, status)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['incident', id] })
      setShowVehicleStatusModal(false)
      setShowPersonnelStatusModal(false)
      setSelectedVehicleAssignment(null)
      setSelectedPersonnelAssignment(null)
      setSuccessMessage(t.vehicleUpdatedSuccessfully || 'Status updated successfully')
      setErrorMessage('')
      setTimeout(() => setSuccessMessage(''), 3000)
    },
    onError: (error: any) => {
      const message = error?.message || 'Failed to update status'
      setErrorMessage(message)
      setTimeout(() => setErrorMessage(''), 5000)
    }
  })

  const getResourceName = (resourceType: number, resourceId: number) => {
    if (resourceType === ResourceType.Vehicle) {
      const vehicle = vehicles.find(v => v.id === resourceId)
      return vehicle ? `${vehicle.callsign} (${vehicle.type})` : `Vehicle #${resourceId}`
    } else {
      const person = personnel.find(p => p.id === resourceId)
      return person ? `${person.name} (${person.rank})` : `Personnel #${resourceId}`
    }
  }

  // Helper function to get next incident status
  const getNextIncidentStatus = (currentStatus: IncidentStatus): IncidentStatus => {
    const availableStatuses = getIncidentStatusUpdateOptions(t).map(option => option.value);
    const currentIndex = availableStatuses.indexOf(currentStatus);

    // If current status is not in available options or is the last one, return the first available
    if (currentIndex === -1 || currentIndex === availableStatuses.length - 1) {
      return availableStatuses[0];
    }

    // Return next status
    return availableStatuses[currentIndex + 1];
  };

  // Helper function to get next vehicle status
  const getNextVehicleStatus = (currentStatus: string): string => {
    const availableStatuses = getVehicleStatusOptions().map(option => option.value);
    const currentIndex = availableStatuses.indexOf(currentStatus);

    // If current status is not in available options or is the last one, return the first available
    if (currentIndex === -1 || currentIndex === availableStatuses.length - 1) {
      return availableStatuses[0];
    }

    // Return next status
    return availableStatuses[currentIndex + 1];
  };

  // Helper function to get next personnel status
  const getNextPersonnelStatus = (currentStatus: string): string => {
    const availableStatuses = getPersonnelStatusOptions().map(option => option.value);
    const currentIndex = availableStatuses.indexOf(currentStatus);

    // If current status is not in available options or is the last one, return the first available
    if (currentIndex === -1 || currentIndex === availableStatuses.length - 1) {
      return availableStatuses[0];
    }

    // Return next status
    return availableStatuses[currentIndex + 1];
  };

  const getVehicleStatusOptions = () => [
    { value: 'Notified', label: t.notified },
    { value: 'On Scene', label: t.onScene },
    { value: 'Finished', label: t.completed }
  ]

  const getPersonnelStatusOptions = () => [
    { value: 'Notified', label: t.notified },
    { value: 'On Scene', label: t.onScene },
    { value: 'Unavailable', label: t.unavailable }
  ]

  const handleAssign = (resourceType: ResourceType, resourceId: number) => {
    assignResourceMutation.mutate({ resourceType, resourceId })
  }

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600"></div>
      </div>
    )
  }

  if (!incident) {
    return (
      <div className="text-center py-12">
        <h3 className="text-lg font-medium text-gray-900 dark:text-gray-100 mb-2">{t.incidentNotFound}</h3>
        <p className="text-gray-600 dark:text-gray-400">{t.incidentNotFoundDescription}</p>
      </div>
    )
  }

  return (
    <div className="min-h-screen bg-gray-50 dark:bg-gray-900">
      {/* Header */}
      <div className="bg-white dark:bg-gray-800 border-b border-gray-200 dark:border-gray-700 shadow-sm">
        <div className="px-6 py-4">
          <div className="flex items-center justify-between">
            <div className="flex items-center">
              <button
                onClick={() => navigate('/incidents')}
                className="mr-4 p-2 text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-200 hover:bg-gray-100 dark:hover:bg-gray-700 rounded-lg transition-colors"
              >
                <ArrowLeft className="h-5 w-5" />
              </button>
              <div>
                <div className="flex items-center gap-3 mb-1">
                  <div className="flex items-center gap-2 px-3 py-1 bg-blue-50 dark:bg-blue-900/30 border border-blue-200 dark:border-blue-700 rounded-lg">
                    <AlertTriangle className="w-4 h-4 text-blue-600 dark:text-blue-400" />
                    <span className="text-sm font-semibold text-blue-700 dark:text-blue-300">
                      {t.incidentId} #{incident.id}
                    </span>
                  </div>
                  <div className="text-xs text-gray-500 dark:text-gray-400 font-mono">
                    {formatInLocalTimezone(incident.createdAt, 'yyyy-MM-dd HH:mm:ss')}
                  </div>
                </div>
                <h1 className="text-xl font-bold text-gray-900 dark:text-white">
                  {incident.mainCategory} - {incident.subCategory}
                </h1>
                {incident.address && (
                  <div className="flex items-center gap-2 text-sm text-gray-600 dark:text-gray-400 mt-1">
                    <MapPin className="w-4 h-4 text-red-500" />
                    <span>{incident.address}</span>
                  </div>
                )}
              </div>
            </div>

            {/* Status & Priority Badges */}
            <div className="flex flex-col gap-2">
              <span
                className={`px-4 py-2 text-sm font-semibold rounded-lg flex justify-center items-center shadow-sm border ${getIncidentStatusBadgeColor(incident.status)}`}
              >
                {getIncidentStatusTranslation(incident.status, t)}
              </span>
              <span
                className={`px-4 py-2 text-sm font-semibold rounded-lg flex justify-center items-center gap-2 shadow-sm border ${getIncidentPriorityBadgeColor(incident.priority)}`}
              >
                {incident.priority <= 2 && <AlertTriangle className="w-4 h-4" />}
                {getIncidentPriorityTranslation(incident.priority, t)}
              </span>
            </div>
          </div>
        </div>
      </div>

      {/* Error/Success Messages */}
      <div className="px-6 py-4">
        {errorMessage && (
          <div className="mb-4 p-4 bg-red-100 border border-red-400 text-red-700 rounded-lg dark:bg-red-900 dark:border-red-700 dark:text-red-200">
            <div className="flex items-center justify-between">
              <span>{errorMessage}</span>
              <button
                onClick={() => setErrorMessage('')}
                className="text-red-500 hover:text-red-700 dark:text-red-300 dark:hover:text-red-100"
              >
                <X className="w-4 h-4" />
              </button>
            </div>
          </div>
        )}

        {successMessage && (
          <div className="mb-4 p-4 bg-green-100 border border-green-400 text-green-700 rounded-lg dark:bg-green-900 dark:border-green-700 dark:text-green-200">
            <div className="flex items-center justify-between">
              <span>{successMessage}</span>
              <button
                onClick={() => setSuccessMessage('')}
                className="text-green-500 hover:text-green-700 dark:text-green-300 dark:hover:text-green-100"
              >
                <X className="w-4 h-4" />
              </button>
            </div>
          </div>
        )}
      </div>

      <div className="grid grid-cols-1 xl:grid-cols-4 gap-6 px-6">
        {/* Main Details - Use IncidentDetailsPanel */}
        <div className="xl:col-span-3">
          <div className="bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700 rounded-lg shadow-sm h-[calc(100vh-12rem)]">
            <IncidentDetailsPanel incident={incident} />
          </div>
        </div>

        {/* Sidebar */}
        <div className="space-y-6">
          {/* Quick Actions */}
          <div className="bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700 rounded-lg shadow-sm p-6">
            <div className="flex items-center mb-6 pb-4 border-b border-gray-200 dark:border-gray-700">
              <Activity className="w-6 h-6 text-blue-600 dark:text-blue-400 mr-3" />
              <h2 className="text-xl font-bold text-gray-900 dark:text-gray-100 tracking-wide">{t.quickActions}</h2>
            </div>
            <div className="space-y-3">
              {canModifyIncident() && !incident?.isClosed && (
                <>
                  <button
                    onClick={() => {
                      // Set default status to current status + 1
                      const nextStatus = getNextIncidentStatus(incident.status);
                      setSelectedStatus(nextStatus);
                      setShowStatusModal(true);
                    }}
                    className="w-full flex items-center justify-center px-4 py-3 bg-blue-600 hover:bg-blue-700 text-white font-semibold rounded-lg border border-blue-700 shadow-sm transition-all duration-200 hover:shadow-md focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 dark:focus:ring-offset-gray-800"
                  >
                    <AlertCircle className="w-5 h-5 mr-2" />
                    {t.updateStatus}
                  </button>
                  <button
                    onClick={() => setShowAssignModal(true)}
                    className="w-full flex items-center justify-center px-4 py-3 bg-green-600 hover:bg-green-700 text-white font-semibold rounded-lg border border-green-700 shadow-sm transition-all duration-200 hover:shadow-md focus:outline-none focus:ring-2 focus:ring-green-500 focus:ring-offset-2 dark:focus:ring-offset-gray-800"
                  >
                    <Users className="w-5 h-5 mr-2" />
                    {t.assignResource}
                  </button>
                </>
              )}
              <button
                onClick={() => navigate(`/map?incident=${incident.id}&lat=${incident.latitude}&lng=${incident.longitude}`)}
                className="w-full flex items-center justify-center px-4 py-3 bg-cyan-600 hover:bg-cyan-700 text-white font-semibold rounded-lg border border-cyan-700 shadow-sm transition-all duration-200 hover:shadow-md focus:outline-none focus:ring-2 focus:ring-cyan-500 focus:ring-offset-2 dark:focus:ring-offset-gray-800"
              >
                <MapPin className="w-5 h-5 mr-2" />
                {t.viewOnMap}
              </button>
              {incident.callers && incident.callers.length > 0 && (
                <button
                  onClick={() => setShowCallersModal(true)}
                  className="w-full flex items-center justify-center px-4 py-3 bg-indigo-600 hover:bg-indigo-700 text-white font-semibold rounded-lg border border-indigo-700 shadow-sm transition-all duration-200 hover:shadow-md focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2 dark:focus:ring-offset-gray-800"
                >
                  <Phone className="w-5 h-5 mr-2" />
                  {t.viewCallers || 'View Callers'} ({incident.callers.length})
                </button>
              )}
              {canAddIncidentNotes(incident?.stationId) && (
                <button
                  onClick={() => setShowLogModal(true)}
                  className="w-full flex items-center justify-center px-4 py-3 bg-purple-600 hover:bg-purple-700 text-white font-semibold rounded-lg border border-purple-700 shadow-sm transition-all duration-200 hover:shadow-md focus:outline-none focus:ring-2 focus:ring-purple-500 focus:ring-offset-2 dark:focus:ring-offset-gray-800"
                >
                  <MessageSquare className="w-5 h-5 mr-2" />
                  {t.addLogEntry}
                </button>
              )}
              {canModifyIncident() && (
                <>
                  {!incident?.isClosed && (
                    <button
                      onClick={() => setShowCloseModal(true)}
                      className="w-full flex items-center justify-center px-4 py-3 bg-red-600 hover:bg-red-700 text-white font-semibold rounded-lg border border-red-700 shadow-sm transition-all duration-200 hover:shadow-md focus:outline-none focus:ring-2 focus:ring-red-500 focus:ring-offset-2 dark:focus:ring-offset-gray-800"
                    >
                      <XCircle className="w-5 h-5 mr-2" />
                      {t.closeIncident || 'Close Incident'}
                    </button>
                  )}
                  {incident?.isClosed && (
                    <button
                      onClick={() => setShowReopenModal(true)}
                      className="w-full flex items-center justify-center px-4 py-3 bg-orange-600 hover:bg-orange-700 text-white font-semibold rounded-lg border border-orange-700 shadow-sm transition-all duration-200 hover:shadow-md focus:outline-none focus:ring-2 focus:ring-orange-500 focus:ring-offset-2 dark:focus:ring-offset-gray-800"
                    >
                      <CheckCircle2 className="w-5 h-5 mr-2" />
                      {t.reopenIncident || 'Reopen Incident'}
                    </button>
                  )}
                </>
              )}
            </div>
          </div>

          {/* Activity Log Summary */}
          <div className="bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700 rounded-lg shadow-sm p-6">
            <div className="flex items-center mb-6 pb-4 border-b border-gray-200 dark:border-gray-700">
              <MessageSquare className="w-6 h-6 text-purple-600 dark:text-purple-400 mr-3" />
              <h2 className="text-xl font-bold text-gray-900 dark:text-gray-100 tracking-wide">{t.activityLog}</h2>
            </div>
            {incident.logs.length === 0 ? (
              <div className="text-center py-8">
                <div className="w-16 h-16 mx-auto mb-4 bg-gray-100 dark:bg-gray-700 rounded-full flex items-center justify-center">
                  <MessageSquare className="w-8 h-8 text-gray-400" />
                </div>
                <p className="text-gray-500 dark:text-gray-400 font-medium">{t.noActivityLogged}</p>
              </div>
            ) : (
              <div className="space-y-3 max-h-64 overflow-y-auto">
                {incident.logs
                  .sort((a, b) => new Date(b.at).getTime() - new Date(a.at).getTime())
                  .slice(0, 3)
                  .map((log) => (
                    <div key={log.id} className="bg-gray-50 dark:bg-gray-700 rounded-lg p-3 border border-gray-200 dark:border-gray-600">
                      <p className="text-sm text-gray-900 dark:text-gray-100 leading-relaxed">{translateLogMessage(log.message, vehicles, personnel, t)}</p>
                      <div className="flex items-center justify-between mt-2 text-xs text-gray-500 dark:text-gray-400">
                        <div className="flex items-center">
                          <Clock className="w-3 h-3 mr-1" />
                          <span className="font-mono">{format(new Date(log.at), 'MMM d, HH:mm')}</span>
                        </div>
                        {log.by && (
                          <div className="flex items-center">
                            <User className="w-3 h-3 mr-1" />
                            <span>{log.by}</span>
                          </div>
                        )}
                      </div>
                    </div>
                  ))}
                {incident.logs.length > 3 && (
                  <p className="text-xs text-gray-500 dark:text-gray-400 text-center">
                    +{incident.logs.length - 3} more entries
                  </p>
                )}
              </div>
            )}
          </div>
        </div>
      </div>

      {/* Assigned Resources Section - Full Width Below Main Grid */}
      <div className="px-6 pb-6">
        <div className="bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700 rounded-lg shadow-sm p-6">
          <div className="flex items-center mb-6 pb-4 border-b border-gray-200 dark:border-gray-700">
            <Activity className="w-6 h-6 text-green-600 dark:text-green-400 mr-3" />
            <h2 className="text-xl font-bold text-gray-900 dark:text-gray-100 tracking-wide">{t.assignedResources}</h2>
          </div>
          {incident.assignments.length === 0 ? (
            <div className="text-center py-8">
              <div className="w-16 h-16 mx-auto mb-4 bg-gray-100 dark:bg-gray-700 rounded-full flex items-center justify-center">
                <Users className="w-8 h-8 text-gray-400" />
              </div>
              <p className="text-gray-500 dark:text-gray-400 font-medium">{t.noResourcesAssigned}</p>
            </div>
          ) : (
            <div className="space-y-4">
              {incident.assignments.map((assignment) => (
                <div key={assignment.id} className="bg-gray-50 dark:bg-gray-700 border border-gray-200 dark:border-gray-600 rounded-lg p-4 hover:shadow-md transition-shadow">
                  <div className="flex items-start justify-between">
                    <div className="flex items-start flex-1">
                      <div className={`p-2 rounded-lg mr-4 ${assignment.resourceType === ResourceType.Vehicle ? 'bg-blue-100 dark:bg-blue-900/30' : 'bg-green-100 dark:bg-green-900/30'}`}>
                        {assignment.resourceType === ResourceType.Vehicle ? (
                          <Truck className="w-6 h-6 text-blue-600 dark:text-blue-400" />
                        ) : (
                          <Users className="w-6 h-6 text-green-600 dark:text-green-400" />
                        )}
                      </div>
                      <div className="flex-1">
                        <div className="flex items-center mb-2">
                          <h3 className="font-bold text-lg text-gray-900 dark:text-gray-100 mr-3">
                            {getResourceName(assignment.resourceType, assignment.resourceId)}
                          </h3>
                          <span className={`px-3 py-1 text-xs font-bold rounded-full uppercase tracking-wider ${
                            assignment.status === 'Assigned' ? 'bg-yellow-100 text-yellow-800 dark:bg-yellow-900/30 dark:text-yellow-300' :
                            assignment.status === 'Dispatched' ? 'bg-orange-100 text-orange-800 dark:bg-orange-900/30 dark:text-orange-300' :
                            assignment.status === 'OnScene' ? 'bg-blue-100 text-blue-800 dark:bg-blue-900/30 dark:text-blue-300' :
                            assignment.status === 'Finished' ? 'bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-300' :
                            'bg-gray-100 text-gray-800 dark:bg-gray-900/30 dark:text-gray-300'
                          }`}>
                            {getAssignmentStatusTranslation(assignment.status, t)}
                          </span>
                        </div>
                        <div className="flex items-center text-sm text-gray-600 dark:text-gray-400 mb-3">
                          <Clock className="w-4 h-4 mr-1" />
                          <span className="font-medium">{t.assigned} {formatInLocalTimezone(assignment.createdAt)}</span>
                        </div>
                        {(assignment.dispatchedAt || assignment.onSceneAt || assignment.completedAt) && (
                          <div className="grid grid-cols-1 md:grid-cols-3 gap-2 mt-3">
                            {assignment.dispatchedAt && (
                              <div className="flex items-center p-2 bg-orange-50 dark:bg-orange-900/20 rounded-md border-l-2 border-orange-400">
                                <div className="w-2 h-2 bg-orange-500 rounded-full mr-2"></div>
                                <div className="text-xs">
                                  <div className="font-semibold text-orange-800 dark:text-orange-300">{t.notified || 'Notified'}</div>
                                  <div className="text-orange-600 dark:text-orange-400 font-mono">{formatInLocalTimezone(assignment.dispatchedAt, 'HH:mm:ss')}</div>
                                </div>
                              </div>
                            )}
                            {assignment.onSceneAt && (
                              <div className="flex items-center p-2 bg-blue-50 dark:bg-blue-900/20 rounded-md border-l-2 border-blue-400">
                                <div className="w-2 h-2 bg-blue-500 rounded-full mr-2"></div>
                                <div className="text-xs">
                                  <div className="font-semibold text-blue-800 dark:text-blue-300">{t.onScene || 'On Scene'}</div>
                                  <div className="text-blue-600 dark:text-blue-400 font-mono">{formatInLocalTimezone(assignment.onSceneAt, 'HH:mm:ss')}</div>
                                </div>
                              </div>
                            )}
                            {assignment.completedAt && (
                              <div className="flex items-center p-2 bg-green-50 dark:bg-green-900/20 rounded-md border-l-2 border-green-400">
                                <div className="w-2 h-2 bg-green-500 rounded-full mr-2"></div>
                                <div className="text-xs">
                                  <div className="font-semibold text-green-800 dark:text-green-300">{t.completed || 'Completed'}</div>
                                  <div className="text-green-600 dark:text-green-400 font-mono">{formatInLocalTimezone(assignment.completedAt, 'HH:mm:ss')}</div>
                                </div>
                              </div>
                            )}
                          </div>
                        )}
                      </div>
                    </div>
                    <div className="flex items-center space-x-2 mt-4">
                      {canModifyIncident() && assignment.resourceType === 0 && (
                        <button
                          onClick={() => {
                            // Set default status to current status + 1
                            const nextStatus = getNextVehicleStatus(assignment.status);
                            setSelectedVehicleAssignment({...assignment, status: nextStatus});
                            setShowVehicleStatusModal(true);
                          }}
                          disabled={updateAssignmentStatusMutation.isPending || assignment.status === 'Finished'}
                          className={`px-4 py-2 text-sm font-semibold rounded-lg transition-colors ${
                            assignment.status === 'Finished'
                              ? 'bg-gray-100 text-gray-500 dark:bg-gray-700 dark:text-gray-400 cursor-not-allowed border border-gray-300 dark:border-gray-600'
                              : 'bg-blue-600 text-white hover:bg-blue-700 dark:bg-blue-500 dark:hover:bg-blue-600 shadow-sm'
                          }`}
                          title={assignment.status === 'Finished' ? 'Status locked - vehicle finished' : (t.updateStatus || 'Change Status')}
                        >
                          {assignment.status === 'Finished' ? 'Locked' : (t.updateStatus || 'Update Status')}
                        </button>
                      )}
                      {canModifyIncident() && assignment.resourceType === 1 && (
                        <button
                          onClick={() => {
                            // Set default status to current status + 1
                            const nextStatus = getNextPersonnelStatus(assignment.status);
                            setSelectedPersonnelAssignment({...assignment, status: nextStatus});
                            setShowPersonnelStatusModal(true);
                          }}
                          disabled={updateAssignmentStatusMutation.isPending}
                          className="px-4 py-2 text-sm font-semibold bg-blue-600 text-white rounded-lg hover:bg-blue-700 dark:bg-blue-500 dark:hover:bg-blue-600 disabled:opacity-50 shadow-sm transition-colors"
                          title={t.updateStatus || 'Change Status'}
                        >
                          {t.updateStatus || 'Update Status'}
                        </button>
                      )}
                      {canModifyIncident() && (
                        <button
                          onClick={() => {
                            setSelectedAssignment(assignment)
                            setShowUnassignModal(true)
                          }}
                          disabled={unassignResourceMutation.isPending}
                          className="p-2 text-red-600 hover:text-red-700 dark:text-red-400 dark:hover:text-red-300 disabled:opacity-50 bg-red-50 dark:bg-red-900/20 rounded-lg hover:bg-red-100 dark:hover:bg-red-900/30 transition-colors"
                          title={t.unassign}
                        >
                          <Minus className="w-4 h-4" />
                        </button>
                      )}
                    </div>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      </div>

      {/* All modals here - keeping them for functionality */}

      {/* Update Status Modal */}
      {showStatusModal && (
        <div className="fixed inset-0 bg-black/60 backdrop-blur-sm flex items-center justify-center z-50 p-4">
          <div className="bg-white dark:bg-gray-800 rounded-xl shadow-2xl p-8 w-full max-w-md border border-gray-200 dark:border-gray-700 transform transition-all">
            <div className="flex items-center justify-between mb-6 pb-4 border-b border-gray-200 dark:border-gray-700">
              <div className="flex items-center">
                <AlertCircle className="w-6 h-6 text-blue-600 dark:text-blue-400 mr-3" />
                <h3 className="text-xl font-bold text-gray-900 dark:text-gray-100">{t.updateStatusModal}</h3>
              </div>
              <button
                onClick={() => setShowStatusModal(false)}
                className="p-2 text-gray-400 dark:text-gray-500 hover:text-gray-600 dark:hover:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-700 rounded-lg transition-colors"
              >
                <X className="w-5 h-5" />
              </button>
            </div>
            <div className="space-y-6">
              <div>
                <label className="block text-sm font-bold text-gray-700 dark:text-gray-300 mb-3">
                  {t.newStatus}
                </label>
                <select
                  value={selectedStatus}
                  onChange={(e) => setSelectedStatus(Number(e.target.value) as IncidentStatus)}
                  className="w-full px-4 py-3 bg-gray-50 dark:bg-gray-700 border border-gray-300 dark:border-gray-600 rounded-lg text-gray-900 dark:text-gray-100 font-medium focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all"
                >
                  {getIncidentStatusUpdateOptions(t).map(({ value, label, key }: IncidentStatusOption) => (
                    <option key={key} value={value}>
                      {label}
                    </option>
                  ))}
                </select>
              </div>
              <div className="flex justify-end space-x-3 pt-4">
                <button
                  onClick={() => setShowStatusModal(false)}
                  className="px-6 py-3 bg-gray-100 dark:bg-gray-700 text-gray-700 dark:text-gray-300 font-semibold rounded-lg border border-gray-300 dark:border-gray-600 hover:bg-gray-200 dark:hover:bg-gray-600 transition-colors focus:outline-none focus:ring-2 focus:ring-gray-500 focus:ring-offset-2 dark:focus:ring-offset-gray-800"
                >
                  {t.cancel}
                </button>
                <button
                  onClick={() => updateStatusMutation.mutate(selectedStatus)}
                  disabled={updateStatusMutation.isPending}
                  className="px-6 py-3 bg-blue-600 hover:bg-blue-700 disabled:bg-blue-400 text-white font-semibold rounded-lg shadow-sm transition-all focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 dark:focus:ring-offset-gray-800 disabled:cursor-not-allowed"
                >
                  {updateStatusMutation.isPending ? t.updating : t.update}
                </button>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Enhanced Assign Resource Modal */}
      <EnhancedAssignmentModal
        isOpen={showAssignModal}
        onClose={() => setShowAssignModal(false)}
        incident={incident!}
        allIncidents={allIncidents}
        onAssign={handleAssign}
        isAssigning={assignResourceMutation.isPending}
      />

      {/* Add Log Modal */}
      {showLogModal && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white dark:bg-gray-800 rounded-lg p-6 w-full max-w-md border border-gray-200 dark:border-gray-700">
            <div className="flex items-center justify-between mb-4">
              <h3 className="text-lg font-semibold text-gray-900 dark:text-gray-100">{t.addLogModal}</h3>
              <button onClick={() => setShowLogModal(false)} className="text-gray-400 dark:text-gray-500 hover:text-gray-600 dark:hover:text-gray-300">
                <X className="w-5 h-5" />
              </button>
            </div>
            <div className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                  {t.message}
                </label>
                <textarea
                  value={logMessage}
                  onChange={(e) => setLogMessage(e.target.value)}
                  rows={4}
                  className="input w-full"
                  placeholder={`${t.message}...`}
                />
              </div>
              <div className="flex justify-end space-x-2">
                <button
                  onClick={() => setShowLogModal(false)}
                  className="btn btn-secondary"
                >
                  {t.cancel}
                </button>
                <button
                  onClick={() => addLogMutation.mutate(logMessage)}
                  disabled={addLogMutation.isPending || !logMessage.trim()}
                  className="btn btn-primary"
                >
                  {addLogMutation.isPending ? t.adding : t.addLog}
                </button>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Unassign Confirmation Modal */}
      {showUnassignModal && selectedAssignment && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white dark:bg-gray-800 rounded-lg p-6 w-full max-w-md border border-gray-200 dark:border-gray-700">
            <div className="flex items-center justify-between mb-4">
              <h3 className="text-lg font-semibold text-gray-900 dark:text-gray-100">{t.unassignResource}</h3>
              <button
                onClick={() => {
                  setShowUnassignModal(false)
                  setSelectedAssignment(null)
                }}
                className="text-gray-400 dark:text-gray-500 hover:text-gray-600 dark:hover:text-gray-300"
              >
                <X className="w-5 h-5" />
              </button>
            </div>
            <div className="mb-6">
              <p className="text-gray-700 dark:text-gray-300">
                {t.confirmUnassignResource}
              </p>
              <div className="mt-3 p-3 bg-gray-50 dark:bg-gray-700 rounded-lg">
                <div className="flex items-center">
                  {selectedAssignment.resourceType === 0 ? (
                    <Truck className="w-5 h-5 text-gray-600 dark:text-gray-400 mr-3" />
                  ) : (
                    <Users className="w-5 h-5 text-gray-600 dark:text-gray-400 mr-3" />
                  )}
                  <span className="font-medium text-gray-900 dark:text-gray-100">
                    {getResourceName(selectedAssignment.resourceType, selectedAssignment.resourceId)}
                  </span>
                </div>
              </div>
            </div>
            <div className="flex justify-end space-x-2">
              <button
                onClick={() => {
                  setShowUnassignModal(false)
                  setSelectedAssignment(null)
                }}
                className="btn btn-secondary"
              >
                {t.cancel}
              </button>
              <button
                onClick={() => {
                  unassignResourceMutation.mutate(selectedAssignment.id)
                  setShowUnassignModal(false)
                  setSelectedAssignment(null)
                }}
                disabled={unassignResourceMutation.isPending}
                className="btn btn-danger"
              >
                {unassignResourceMutation.isPending ? t.loading : t.unassign}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Close Incident Modal */}
      {showCloseModal && !showCloseConfirmation && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white dark:bg-gray-800 rounded-lg p-6 w-full max-w-md">
            <div className="flex justify-between items-center mb-4">
              <h3 className="text-lg font-semibold text-gray-900 dark:text-gray-100">
                {t.closeIncident || 'Close Incident'}
              </h3>
              <button
                onClick={() => setShowCloseModal(false)}
                className="text-gray-400 hover:text-gray-600 dark:hover:text-gray-300"
              >
                <X className="w-5 h-5" />
              </button>
            </div>
            <div className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                  {t.closureReason || 'Closure Reason'}
                </label>
                <select
                  value={selectedClosureReason}
                  onChange={(e) => setSelectedClosureReason(Number(e.target.value) as IncidentClosureReason)}
                  className="input w-full"
                >
                  <option value={IncidentClosureReason.Action}>
                    {t.action || 'Action'}
                  </option>
                  <option value={IncidentClosureReason.WithoutAction}>
                    {t.withoutAction || 'Without Action'}
                  </option>
                  <option value={IncidentClosureReason.PreArrival}>
                    {t.preArrival || 'Pre-Arrival'}
                  </option>
                  <option value={IncidentClosureReason.Cancelled}>
                    {t.cancelled || 'Cancelled'}
                  </option>
                  <option value={IncidentClosureReason.FalseAlarm}>
                    {t.falseAlarm || 'False Alarm'}
                  </option>
                </select>
              </div>
            </div>
            <div className="flex justify-end space-x-2 mt-6">
              <button
                onClick={() => setShowCloseModal(false)}
                className="btn btn-secondary"
              >
                {t.cancel}
              </button>
              <button
                onClick={() => setShowCloseConfirmation(true)}
                className="btn btn-danger"
              >
                {t.continue || 'Continue'}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Close Confirmation Modal */}
      {showCloseConfirmation && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white dark:bg-gray-800 rounded-lg p-6 w-full max-w-md">
            <div className="flex justify-between items-center mb-4">
              <h3 className="text-lg font-semibold text-gray-900 dark:text-gray-100">
                {t.confirmCloseIncident || 'Confirm Close Incident'}
              </h3>
              <button
                onClick={() => {
                  setShowCloseConfirmation(false)
                  setShowCloseModal(false)
                }}
                className="text-gray-400 hover:text-gray-600 dark:hover:text-gray-300"
              >
                <X className="w-5 h-5" />
              </button>
            </div>
            <div className="mb-6">
              <div className="flex items-center mb-4">
                <AlertTriangle className="w-6 h-6 text-yellow-500 mr-3" />
                <p className="text-gray-700 dark:text-gray-300">
                  {t.confirmCloseIncidentMessage || 'Are you sure you want to close this incident? This action will set the status to "Fully Controlled" and prevent further status changes.'}
                </p>
              </div>

              {/* Show vehicle auto-update warning if there are assigned vehicles */}
              {incident?.assignments?.filter(a => a.resourceType === ResourceType.Vehicle && a.status !== 'Finished').length > 0 && (
                <div className="bg-blue-50 dark:bg-blue-900/20 p-3 rounded-lg mb-4 border-l-4 border-blue-500">
                  <div className="flex items-start">
                    <Info className="w-4 h-4 text-blue-500 mr-2 mt-0.5 flex-shrink-0" />
                    <p className="text-sm text-blue-700 dark:text-blue-300">
                      <strong>Note:</strong> All assigned vehicles will automatically be set to "Finished" status when this incident is closed.
                    </p>
                  </div>
                </div>
              )}

              <div className="bg-gray-50 dark:bg-gray-700 p-3 rounded-lg">
                <p className="text-sm text-gray-600 dark:text-gray-400">
                  <strong>{t.closureReason || 'Closure Reason'}:</strong> {
                    selectedClosureReason === IncidentClosureReason.Action ? (t.action || 'Action') :
                    selectedClosureReason === IncidentClosureReason.WithoutAction ? (t.withoutAction || 'Without Action') :
                    selectedClosureReason === IncidentClosureReason.PreArrival ? (t.preArrival || 'Pre-Arrival') :
                    selectedClosureReason === IncidentClosureReason.Cancelled ? (t.cancelled || 'Cancelled') :
                    (t.falseAlarm || 'False Alarm')
                  }
                </p>
              </div>
            </div>
            <div className="flex justify-end space-x-2">
              <button
                onClick={() => setShowCloseConfirmation(false)}
                className="btn btn-secondary"
              >
                {t.goBack || 'Go Back'}
              </button>
              <button
                onClick={() => closeIncidentMutation.mutate(selectedClosureReason)}
                disabled={closeIncidentMutation.isPending}
                className="btn btn-danger"
              >
                {closeIncidentMutation.isPending ? (t.closing || 'Closing...') : (t.closeIncident || 'Close Incident')}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Vehicle Status Modal */}
      {showVehicleStatusModal && selectedVehicleAssignment && (
        <div className="fixed inset-0 bg-black/60 backdrop-blur-sm flex items-center justify-center z-50 p-4">
          <div className="bg-white dark:bg-gray-800 rounded-xl shadow-2xl p-8 w-full max-w-md border border-gray-200 dark:border-gray-700 transform transition-all">
            <div className="flex items-center justify-between mb-6 pb-4 border-b border-gray-200 dark:border-gray-700">
              <div className="flex items-center">
                <Truck className="w-6 h-6 text-blue-600 dark:text-blue-400 mr-3" />
                <h3 className="text-xl font-bold text-gray-900 dark:text-gray-100">{t.updateVehicleStatus || 'Update Vehicle Status'}</h3>
              </div>
              <button
                onClick={() => {
                  setShowVehicleStatusModal(false)
                  setSelectedVehicleAssignment(null)
                }}
                className="p-2 text-gray-400 dark:text-gray-500 hover:text-gray-600 dark:hover:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-700 rounded-lg transition-colors"
              >
                <X className="w-5 h-5" />
              </button>
            </div>
            <div className="space-y-6">
              <div className="bg-gray-50 dark:bg-gray-700 p-3 rounded-lg">
                <p className="text-sm font-medium text-gray-900 dark:text-gray-100">
                  {getResourceName(selectedVehicleAssignment.resourceType, selectedVehicleAssignment.resourceId)}
                </p>
                <p className="text-xs text-gray-500 dark:text-gray-400 mt-1">
                  Current Status: <span className="font-semibold">{getVehicleStatusTranslation(selectedVehicleAssignment.status, t)}</span>
                </p>
              </div>
              <div>
                <label className="block text-sm font-bold text-gray-700 dark:text-gray-300 mb-3">
                  {t.newStatus}
                </label>
                <select
                  value={selectedVehicleAssignment.status}
                  onChange={(e) => setSelectedVehicleAssignment({...selectedVehicleAssignment, status: e.target.value})}
                  className="w-full px-4 py-3 bg-gray-50 dark:bg-gray-700 border border-gray-300 dark:border-gray-600 rounded-lg text-gray-900 dark:text-gray-100 font-medium focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all"
                >
                  {getVehicleStatusOptions().map(({ value, label }) => (
                    <option key={value} value={value}>
                      {label}
                    </option>
                  ))}
                </select>
              </div>
              <div className="flex justify-end space-x-3 pt-4">
                <button
                  onClick={() => {
                    setShowVehicleStatusModal(false)
                    setSelectedVehicleAssignment(null)
                  }}
                  className="px-6 py-3 bg-gray-100 dark:bg-gray-700 text-gray-700 dark:text-gray-300 font-semibold rounded-lg border border-gray-300 dark:border-gray-600 hover:bg-gray-200 dark:hover:bg-gray-600 transition-colors focus:outline-none focus:ring-2 focus:ring-gray-500 focus:ring-offset-2 dark:focus:ring-offset-gray-800"
                >
                  {t.cancel}
                </button>
                <button
                  onClick={() => updateAssignmentStatusMutation.mutate({
                    assignmentId: selectedVehicleAssignment.id,
                    status: selectedVehicleAssignment.status
                  })}
                  disabled={updateAssignmentStatusMutation.isPending}
                  className="px-6 py-3 bg-blue-600 hover:bg-blue-700 disabled:bg-blue-400 text-white font-semibold rounded-lg shadow-sm transition-all focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 dark:focus:ring-offset-gray-800 disabled:cursor-not-allowed"
                >
                  {updateAssignmentStatusMutation.isPending ? t.updating : t.update}
                </button>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Personnel Status Modal */}
      {showPersonnelStatusModal && selectedPersonnelAssignment && (
        <div className="fixed inset-0 bg-black/60 backdrop-blur-sm flex items-center justify-center z-50 p-4">
          <div className="bg-white dark:bg-gray-800 rounded-xl shadow-2xl p-8 w-full max-w-md border border-gray-200 dark:border-gray-700 transform transition-all">
            <div className="flex items-center justify-between mb-6 pb-4 border-b border-gray-200 dark:border-gray-700">
              <div className="flex items-center">
                <Users className="w-6 h-6 text-green-600 dark:text-green-400 mr-3" />
                <h3 className="text-xl font-bold text-gray-900 dark:text-gray-100">{t.updatePersonnelStatus || 'Update Personnel Status'}</h3>
              </div>
              <button
                onClick={() => {
                  setShowPersonnelStatusModal(false)
                  setSelectedPersonnelAssignment(null)
                }}
                className="p-2 text-gray-400 dark:text-gray-500 hover:text-gray-600 dark:hover:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-700 rounded-lg transition-colors"
              >
                <X className="w-5 h-5" />
              </button>
            </div>
            <div className="space-y-6">
              <div className="bg-gray-50 dark:bg-gray-700 p-3 rounded-lg">
                <p className="text-sm font-medium text-gray-900 dark:text-gray-100">
                  {getResourceName(selectedPersonnelAssignment.resourceType, selectedPersonnelAssignment.resourceId)}
                </p>
                <p className="text-xs text-gray-500 dark:text-gray-400 mt-1">
                  Current Status: <span className="font-semibold">{getAssignmentStatusTranslation(selectedPersonnelAssignment.status, t)}</span>
                </p>
              </div>
              <div>
                <label className="block text-sm font-bold text-gray-700 dark:text-gray-300 mb-3">
                  {t.newStatus}
                </label>
                <select
                  value={selectedPersonnelAssignment.status}
                  onChange={(e) => setSelectedPersonnelAssignment({...selectedPersonnelAssignment, status: e.target.value})}
                  className="w-full px-4 py-3 bg-gray-50 dark:bg-gray-700 border border-gray-300 dark:border-gray-600 rounded-lg text-gray-900 dark:text-gray-100 font-medium focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all"
                >
                  {getPersonnelStatusOptions().map(({ value, label }) => (
                    <option key={value} value={value}>
                      {label}
                    </option>
                  ))}
                </select>
              </div>
              <div className="flex justify-end space-x-3 pt-4">
                <button
                  onClick={() => {
                    setShowPersonnelStatusModal(false)
                    setSelectedPersonnelAssignment(null)
                  }}
                  className="px-6 py-3 bg-gray-100 dark:bg-gray-700 text-gray-700 dark:text-gray-300 font-semibold rounded-lg border border-gray-300 dark:border-gray-600 hover:bg-gray-200 dark:hover:bg-gray-600 transition-colors focus:outline-none focus:ring-2 focus:ring-gray-500 focus:ring-offset-2 dark:focus:ring-offset-gray-800"
                >
                  {t.cancel}
                </button>
                <button
                  onClick={() => updateAssignmentStatusMutation.mutate({
                    assignmentId: selectedPersonnelAssignment.id,
                    status: selectedPersonnelAssignment.status
                  })}
                  disabled={updateAssignmentStatusMutation.isPending}
                  className="px-6 py-3 bg-green-600 hover:bg-green-700 disabled:bg-green-400 text-white font-semibold rounded-lg shadow-sm transition-all focus:outline-none focus:ring-2 focus:ring-green-500 focus:ring-offset-2 dark:focus:ring-offset-gray-800 disabled:cursor-not-allowed"
                >
                  {updateAssignmentStatusMutation.isPending ? t.updating : t.update}
                </button>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Reopen Incident Modal */}
      {showReopenModal && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white dark:bg-gray-800 rounded-lg p-6 w-full max-w-md">
            <div className="flex justify-between items-center mb-4">
              <h3 className="text-lg font-semibold text-gray-900 dark:text-gray-100">
                {t.confirmReopenIncident || 'Confirm Reopen Incident'}
              </h3>
              <button
                onClick={() => setShowReopenModal(false)}
                className="text-gray-400 hover:text-gray-600 dark:hover:text-gray-300"
              >
                <X className="w-5 h-5" />
              </button>
            </div>
            <div className="mb-6">
              <div className="flex items-center mb-4">
                <AlertTriangle className="w-6 h-6 text-blue-500 mr-3" />
                <p className="text-gray-700 dark:text-gray-300">
                  {t.confirmReopenIncidentMessage || 'Are you sure you want to reopen this incident? This will allow status changes and resource assignments again.'}
                </p>
              </div>
            </div>
            <div className="flex justify-end space-x-2">
              <button
                onClick={() => setShowReopenModal(false)}
                className="btn btn-secondary"
              >
                {t.cancel}
              </button>
              <button
                onClick={() => reopenIncidentMutation.mutate()}
                disabled={reopenIncidentMutation.isPending}
                className="btn btn-primary"
              >
                {reopenIncidentMutation.isPending ? (t.reopening || 'Reopening...') : (t.reopenIncident || 'Reopen Incident')}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Callers Modal */}
      {showCallersModal && (
        <div className="fixed inset-0 bg-black/60 backdrop-blur-sm flex items-center justify-center z-50 p-4">
          <div className="bg-white dark:bg-gray-800 rounded-xl shadow-2xl p-8 w-full max-w-2xl border border-gray-200 dark:border-gray-700 transform transition-all max-h-[90vh] overflow-y-auto">
            <div className="flex items-center justify-between mb-6 pb-4 border-b border-gray-200 dark:border-gray-700">
              <div className="flex items-center">
                <Phone className="w-6 h-6 text-indigo-600 dark:text-indigo-400 mr-3" />
                <h3 className="text-xl font-bold text-gray-900 dark:text-gray-100">{t.callers || 'Callers'}</h3>
              </div>
              <button
                onClick={() => setShowCallersModal(false)}
                className="p-2 text-gray-400 dark:text-gray-500 hover:text-gray-600 dark:hover:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-700 rounded-lg transition-colors"
              >
                <X className="w-5 h-5" />
              </button>
            </div>
            
            <div className="space-y-4">
              {incident.callers && incident.callers.length > 0 ? (
                incident.callers.map((caller, index) => (
                  <div key={caller.id || index} className="bg-gray-50 dark:bg-gray-700 border border-gray-200 dark:border-gray-600 rounded-lg p-4 hover:shadow-md transition-shadow">
                    <div className="flex items-start justify-between">
                      <div className="flex-1">
                        <div className="flex items-center mb-2">
                          <div className="p-2 bg-indigo-100 dark:bg-indigo-900/30 rounded-lg mr-3">
                            <Phone className="w-5 h-5 text-indigo-600 dark:text-indigo-400" />
                          </div>
                          <div>
                            <h4 className="font-bold text-lg text-gray-900 dark:text-gray-100">
                              {caller.name || t.unknownCaller || 'Unknown Caller'}
                            </h4>
                            <p className="text-sm text-gray-600 dark:text-gray-400">
                              <span className="font-medium">{t.phoneNumber || 'Phone'}:</span> {caller.phoneNumber}
                            </p>
                          </div>
                        </div>
                        
                        {caller.calledAt && (
                          <div className="flex items-center text-sm text-gray-600 dark:text-gray-400 mb-2">
                            <Clock className="w-4 h-4 mr-1" />
                            <span className="font-medium">{t.calledAt || 'Called at'}:</span>
                            <span className="ml-1 font-mono">{formatInLocalTimezone(caller.calledAt, 'MMM d, yyyy HH:mm:ss')}</span>
                          </div>
                        )}
                        
                        {caller.notes && (
                          <div className="mt-3">
                            <p className="text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">{t.notes || 'Notes'}:</p>
                            <p className="text-sm text-gray-600 dark:text-gray-400 bg-white dark:bg-gray-800 p-2 rounded border border-gray-200 dark:border-gray-600">
                              {caller.notes}
                            </p>
                          </div>
                        )}
                      </div>
                    </div>
                  </div>
                ))
              ) : (
                <div className="text-center py-8">
                  <div className="w-16 h-16 mx-auto mb-4 bg-gray-100 dark:bg-gray-700 rounded-full flex items-center justify-center">
                    <Phone className="w-8 h-8 text-gray-400" />
                  </div>
                  <p className="text-gray-500 dark:text-gray-400 font-medium">{t.noCallersRecorded || 'No callers recorded for this incident'}</p>
                </div>
              )}
            </div>
            
            <div className="flex justify-end pt-6 mt-6 border-t border-gray-200 dark:border-gray-700">
              <button
                onClick={() => setShowCallersModal(false)}
                className="px-6 py-3 bg-gray-100 dark:bg-gray-700 text-gray-700 dark:text-gray-300 font-semibold rounded-lg border border-gray-300 dark:border-gray-600 hover:bg-gray-200 dark:hover:bg-gray-600 transition-colors focus:outline-none focus:ring-2 focus:ring-gray-500 focus:ring-offset-2 dark:focus:ring-offset-gray-800"
              >
                {t.close || 'Close'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}