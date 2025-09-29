import { useState, useMemo, useEffect } from 'react'
import { useQuery, useQueryClient } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { incidentsApi, fireStationsApi } from '../lib/api'
import { IncidentStatus, FireStation, Incident } from '../types'
import { Plus, Filter, MapPin, Clock, AlertTriangle, RotateCcw } from 'lucide-react'
import { format } from 'date-fns'
import { useTranslation } from '../hooks/useTranslation'
import { useUserStore } from '../lib/userStore'
import { useSignalR } from '../hooks/useSignalR'
import { useIncidentNotification } from '../lib/incidentNotificationContext'
import { getIncidentCardBackgroundColor, getIncidentStatusBadgeColor, getIncidentStatusTranslation, getIncidentCardTextColor, getIncidentCardSecondaryTextColor } from '../utils/incidentUtils'

export default function IncidentsList() {
  const t  = useTranslation()
  const queryClient = useQueryClient()
  const [statusFilter, setStatusFilter] = useState<IncidentStatus | 'all'>('all')
  const [participationFilter, setParticipationFilter] = useState<'all' | 'primary' | 'reinforcement'>('all')
  const [sortBy, setSortBy] = useState<'status' | 'resources' | 'date'>('status')
  const [sortOrder, setSortOrder] = useState<'asc' | 'desc'>('asc')
  const { user, canCreateIncidents } = useUserStore()
  const signalR = useSignalR()
  const { isIncidentFlashing } = useIncidentNotification()

  const { data: incidents, isLoading } = useQuery({
    queryKey: ['incidents', statusFilter],
    queryFn: () => incidentsApi.getAll({ 
      status: statusFilter === 'all' ? undefined : statusFilter 
    }).then(res => res.data),
  })

  const { data: stations = [] } = useQuery({
    queryKey: ['stations', user?.agencyId],
    queryFn: () => fireStationsApi.getStations(),
    enabled: !!user?.agencyId,
  })

  // Set up SignalR event handlers to invalidate queries when incidents change
  useEffect(() => {
    if (!signalR) return

    // Invalidate incidents query when status changes
    const cleanupStatusChanged = signalR.addIncidentStatusChangedHandler(() => {
      queryClient.invalidateQueries({ queryKey: ['incidents'] })
    })

    // Invalidate incidents query when new incidents are created
    const cleanupCreated = signalR.addIncidentCreatedHandler(() => {
      queryClient.invalidateQueries({ queryKey: ['incidents'] })
    })

    // Invalidate incidents query when incidents are updated
    const cleanupUpdate = signalR.addIncidentUpdateHandler(() => {
      queryClient.invalidateQueries({ queryKey: ['incidents'] })
    })

    // Invalidate incidents query when resources are assigned
    const cleanupResourceAssigned = signalR.addResourceAssignedHandler(() => {
      queryClient.invalidateQueries({ queryKey: ['incidents'] })
    })

    return () => {
      cleanupStatusChanged();
      cleanupCreated();
      cleanupUpdate();
      cleanupResourceAssigned();
    };
  }, [signalR, queryClient])

  const getStationName = (stationId: number) => {
    const station = stations.find((s: FireStation) => s.id === stationId)
    return station?.name || `Station ${stationId}`
  }

  const resetFilters = () => {
    setStatusFilter('all')
    setParticipationFilter('all')
    setSortBy('status')
    setSortOrder('asc')
  }

  // Enhanced sorting logic with multiple options
  const sortedIncidents = useMemo(() => {
    if (!incidents) return []

    // First filter by participation type
    let filteredIncidents = incidents
    if (participationFilter === 'primary') {
      filteredIncidents = incidents.filter(incident =>
        incident.participationType !== 'Reinforcement'
      )
    } else if (participationFilter === 'reinforcement') {
      filteredIncidents = incidents.filter(incident =>
        incident.participationType === 'Reinforcement'
      )
    }

    const incidentsCopy = [...filteredIncidents]
    
    return incidentsCopy.sort((a, b) => {
      let comparison = 0
      
      switch (sortBy) {
        case 'status':
          // Sort by status priority: OnGoing → PartialControl → Controlled → FullyControlled → Closed
          const statusPriority = {
            [IncidentStatus.OnGoing]: 1,
            [IncidentStatus.PartialControl]: 2,
            [IncidentStatus.Controlled]: 3,
            [IncidentStatus.FullyControlled]: 4,
            [IncidentStatus.Closed]: 5,
            [IncidentStatus.Created]: 6 // Created status last if it exists
          }
          const aPriority = statusPriority[a.status] || 999
          const bPriority = statusPriority[b.status] || 999
          comparison = aPriority - bPriority
          break
          
        case 'resources':
          // Sort by number of assigned resources
          const aResourceCount = a.assignments?.length || 0
          const bResourceCount = b.assignments?.length || 0
          comparison = aResourceCount - bResourceCount
          break
          
        case 'date':
          // Sort by creation date
          comparison = new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime()
          break
      }
      
      // Apply sort order (asc/desc)
      return sortOrder === 'desc' ? -comparison : comparison
    })
  }, [incidents, participationFilter, sortBy, sortOrder])

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600"></div>
      </div>
    )
  }

  return (
    <div>
      <div className="flex items-center justify-between mb-8">
        <div>
          <h1 className="text-3xl font-bold text-gray-900 dark:text-gray-100">{t.incidents}</h1>
          <p className="mt-2 text-gray-600 dark:text-gray-400">
            {t.manageAndTrackIncidents}
          </p>
        </div>
        {canCreateIncidents() && (
          <Link to="/incidents/new" className="btn btn-primary flex items-center">
            <Plus className="w-4 h-4 mr-2" />
            {t.newIncidentButton}
          </Link>
        )}
      </div>

      {/* Filters and Sorting */}
      <div className="card p-4 mb-6">
        <div className="flex items-center space-x-2 mb-4">
          <Filter className="w-5 h-5 text-gray-400 dark:text-gray-500" />
          <span className="text-sm font-medium text-gray-700 dark:text-gray-300">{t.filtersAndSorting}</span>
        </div>
        
        <div className="grid grid-cols-1 sm:grid-cols-4 gap-4">
          {/* Status Filter */}
          <div>
            <label htmlFor="status-filter" className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              {t.filter} {t.status}
            </label>
            <select
              id="status-filter"
              value={statusFilter}
              onChange={(e) => setStatusFilter(e.target.value as IncidentStatus | 'all')}
              className="w-full p-2 border border-gray-300 dark:border-gray-600 rounded-md bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
            >
              <option value="all">{t.allStatuses}</option>
              <option value={IncidentStatus.OnGoing}>{t.onGoing}</option>
              <option value={IncidentStatus.PartialControl}>{t.partialControl}</option>
              <option value={IncidentStatus.Controlled}>{t.controlled}</option>
              <option value={IncidentStatus.FullyControlled}>{t.fullyControlled}</option>
              <option value={IncidentStatus.Closed}>{t.closed}</option>
            </select>
          </div>

          {/* Participation Filter */}
          <div>
            <label htmlFor="participation-filter" className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              {t.participationType || 'Participation Type'}
            </label>
            <select
              id="participation-filter"
              value={participationFilter}
              onChange={(e) => setParticipationFilter(e.target.value as 'all' | 'primary' | 'reinforcement')}
              className="w-full p-2 border border-gray-300 dark:border-gray-600 rounded-md bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
            >
              <option value="all">{t.allTypes || 'All Types'}</option>
              <option value="primary">{t.primaryIncidents || 'Primary Incidents'}</option>
              <option value="reinforcement">{t.reinforcementIncidents || 'Reinforcement'}</option>
            </select>
          </div>

          {/* Sort By */}
          <div>
            <label htmlFor="sort-by" className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              {t.sortBy}
            </label>
            <select
              id="sort-by"
              value={sortBy}
              onChange={(e) => setSortBy(e.target.value as 'status' | 'resources' | 'date')}
              className="w-full p-2 border border-gray-300 dark:border-gray-600 rounded-md bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
            >
              <option value="status">{t.sortByStatus}</option>
              <option value="resources">{t.sortByResources}</option>
              <option value="date">{t.sortByDate}</option>
            </select>
          </div>

          {/* Sort Order */}
          {/* <div>
            <label htmlFor="sort-order" className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              {t.sortOrder}
            </label>
            <select
              id="sort-order"
              value={sortOrder}
              onChange={(e) => setSortOrder(e.target.value as 'asc' | 'desc')}
              className="w-full p-2 border border-gray-300 dark:border-gray-600 rounded-md bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
            >
              <option value="asc">
                {sortBy === 'status' ? 'Most Urgent First' : 
                 sortBy === 'resources' ? t.lowToHigh : t.oldestFirst}
              </option>
              <option value="desc">
                {sortBy === 'status' ? 'Least Urgent First' : 
                 sortBy === 'resources' ? t.highToLow : t.newestFirst}
              </option>
            </select>
          </div> */}
        </div>
        
        {/* Reset Filters Button */}
        <div className="mt-4 flex justify-end">
          <button
            onClick={resetFilters}
            className="flex items-center space-x-2 px-4 py-2 text-sm font-medium text-gray-700 dark:text-gray-300 bg-gray-100 dark:bg-gray-700 border border-gray-300 dark:border-gray-600 rounded-md hover:bg-gray-200 dark:hover:bg-gray-600 transition-colors"
          >
            <RotateCcw className="w-4 h-4" />
            <span>{t.clearFilters || 'Clear Filters'}</span>
          </button>
        </div>
      </div>

      {/* Incidents List */}
      <div className="space-y-4">
        {sortedIncidents?.map((incident: Incident) => {
          const isFlashing = isIncidentFlashing(incident.id);
          return (
          <Link
            key={incident.id}
            to={`/incidents/${incident.id}`}
            className={`p-6 hover:shadow-lg transition-shadow block border-l-4 rounded-lg border border-gray-200 dark:border-gray-600 ${getIncidentCardBackgroundColor(incident.status)} ${isFlashing ? 'incident-flash' : ''}`}
          >
            <div className="flex items-start justify-between">
              <div className="flex-1">
                <div className="flex items-center space-x-3 mb-2">
                  <div className="flex-1">
                    <div className="flex flex-col">
                      <h3 className={`text-lg font-semibold ${getIncidentCardTextColor(incident.status)}`}>
                        {incident.mainCategory}
                        {incident.participationType === 'Reinforcement' && (
                          <span className="ml-2 inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-200">
                            {t.reinforcement || 'Reinforcement'}
                          </span>
                        )}
                      </h3>
                      <h4 className={`text-sm font-medium ${getIncidentCardTextColor(incident.status)}`}>
                        {incident.subCategory}
                      </h4>
                    </div>
                  </div>
                  <span className={`px-2 py-1 text-xs font-medium rounded-full ${getIncidentStatusBadgeColor(incident.status)}`}>
                    {getIncidentStatusTranslation(incident.status, t)}
                  </span>
                </div>
                
                {incident.notes && (
                  <p className={`${getIncidentCardSecondaryTextColor(incident.status)} mb-3`}>{incident.notes}</p>
                )}
                
                <div className={`flex items-center space-x-4 text-sm ${getIncidentCardSecondaryTextColor(incident.status)}`}>
                  <div className="flex items-center">
                    <MapPin className="w-4 h-4 mr-1" />
                    {/* {incident.latitude.toFixed(4)}, {incident.longitude.toFixed(4)} */}
                    {incident.address}
                  </div>
                  <div className="flex items-center">
                    <Clock className="w-4 h-4 mr-1" />
                    {format(new Date(incident.createdAt), 'MMM d, yyyy HH:mm')}
                  </div>
                  <div>
                    {incident.assignments.length} {incident.assignments.length === 1 ? t.resource : t.resources} assigned
                  </div>
                </div>
              </div>
              
              <div className="ml-4">
                <div className="text-right">
                  <div className="text-sm font-medium text-gray-900 dark:text-gray-100">
                    {t.incidentId}: {incident.id}
                  </div>
                  <div className="text-sm text-white-500 dark:text-gray-400">
                    {getStationName(incident.stationId)}
                  </div>
                </div>
              </div>
            </div>
          </Link>
          );
        })}
        
        {sortedIncidents?.length === 0 && (
          <div className="text-center py-12">
            <AlertTriangle className="w-12 h-12 text-gray-400 dark:text-gray-500 mx-auto mb-4" />
            <h3 className="text-lg font-medium text-gray-900 dark:text-gray-100 mb-2">{t.noIncidentsFound}</h3>
            <p className="text-gray-600 dark:text-gray-400">
              {statusFilter ? t.tryAdjustingFilters : t.createFirstIncident}
            </p>
          </div>
        )}
      </div>
    </div>
  )
}