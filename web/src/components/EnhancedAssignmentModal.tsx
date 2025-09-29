import React, { useState, useMemo, useRef, useEffect } from 'react'
import { useQuery } from '@tanstack/react-query'
import { vehiclesApi, personnelApi, fireStationsApi } from '../lib/api'
import { Vehicle, Personnel, FireStation, VehicleStatus, ResourceType, Incident, Assignment } from '../types'
import { 
  X, 
  Search, 
  Filter, 
  Truck, 
  Users, 
  ChevronDown, 
  ChevronRight, 
  MapPin,
  Clock,
  AlertCircle,
  CheckCircle,
  XCircle,
  Fuel,
  Droplets,
  Battery,
  Gauge
} from 'lucide-react'
import { useTranslation } from '../hooks/useTranslation'
import { getVehicleStatusTranslation } from '../utils/incidentUtils'
import { useUserStore } from '../lib/userStore'

interface EnhancedAssignmentModalProps {
  isOpen: boolean
  onClose: () => void
  incident: Incident
  allIncidents: Incident[]
  onAssign: (resourceType: ResourceType, resourceId: number) => void
  isAssigning: boolean
}

interface FilterState {
  searchTerm: string
  selectedStations: number[]
  vehicleStatusFilter: VehicleStatus | 'all'
  availabilityFilter: 'all' | 'available' | 'assigned' | 'unavailable'
  showOnlyMyStation: boolean
}

export default function EnhancedAssignmentModal({
  isOpen,
  onClose,
  incident,
  allIncidents,
  onAssign,
  isAssigning
}: EnhancedAssignmentModalProps) {
  const t = useTranslation()
  const { user, isDispatcher } = useUserStore()
  const modalRef = useRef<HTMLDivElement>(null)
  
  const [activeTab, setActiveTab] = useState<'vehicles' | 'personnel'>('vehicles')
  const [expandedStations, setExpandedStations] = useState<Set<number>>(new Set())
  const [stationDropdownOpen, setStationDropdownOpen] = useState(false)
  const [filters, setFilters] = useState<FilterState>({
    searchTerm: '',
    selectedStations: [],
    vehicleStatusFilter: 'all',
    availabilityFilter: 'all',
    showOnlyMyStation: true
  })

  // Handle click outside to close modal
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (modalRef.current && !modalRef.current.contains(event.target as Node)) {
        onClose()
      }
    }

    if (isOpen) {
      document.addEventListener('mousedown', handleClickOutside)
      return () => document.removeEventListener('mousedown', handleClickOutside)
    }
  }, [isOpen, onClose])

  // Close dropdown when clicking outside
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      const target = event.target as Element
      if (!target.closest('.station-dropdown')) {
        setStationDropdownOpen(false)
      }
    }

    if (stationDropdownOpen) {
      document.addEventListener('mousedown', handleClickOutside)
      return () => document.removeEventListener('mousedown', handleClickOutside)
    }
  }, [stationDropdownOpen])

  // Fetch data
  const { data: vehicles = [] } = useQuery({
    queryKey: ['vehicles'],
    queryFn: () => vehiclesApi.getAll().then(res => res.data),
    enabled: isOpen
  })

  const { data: personnel = [] } = useQuery({
    queryKey: ['personnel'],
    queryFn: () => personnelApi.getAll().then(res => res.data),
    enabled: isOpen
  })

  const { data: stations = [] } = useQuery({
    queryKey: ['stations'],
    queryFn: () => fireStationsApi.getStations(),
    enabled: isOpen
  })

  // Helper functions
  const getStationName = (stationId: number): string => {
    const station = stations.find(s => s.id === stationId)
    return station?.name || `Station ${stationId}`
  }

  const getVehicleAssignmentInfo = (vehicleId: number) => {
    const currentAssignment = incident.assignments.find(a => 
      a.resourceType === ResourceType.Vehicle && 
      a.resourceId === vehicleId &&
      a.status !== 'Finished'
    )
    
    const otherAssignment = allIncidents.find(inc => 
      inc.id !== incident.id &&
      inc.assignments.some(a => 
        a.resourceType === ResourceType.Vehicle && 
        a.resourceId === vehicleId &&
        a.status !== 'Finished'
      )
    )

    return {
      isCurrentIncident: !!currentAssignment,
      isAssigned: !!currentAssignment || !!otherAssignment,
      incidentId: otherAssignment?.id
    }
  }

  const getPersonnelAssignmentInfo = (personnelId: number) => {
    const currentAssignment = incident.assignments.find(a => 
      a.resourceType === ResourceType.Personnel && 
      a.resourceId === personnelId &&
      a.status !== 'Finished'
    )
    
    const otherAssignment = allIncidents.find(inc => 
      inc.id !== incident.id &&
      inc.assignments.some(a => 
        a.resourceType === ResourceType.Personnel && 
        a.resourceId === personnelId &&
        a.status !== 'Finished'
      )
    )

    return {
      isCurrentIncident: !!currentAssignment,
      isAssigned: !!currentAssignment || !!otherAssignment,
      incidentId: otherAssignment?.id
    }
  }

  // Filter and group vehicles
  const filteredAndGroupedVehicles = useMemo(() => {
    let filtered = vehicles.filter(vehicle => {
      // Search filter
      if (filters.searchTerm) {
        const searchLower = filters.searchTerm.toLowerCase()
        if (!vehicle.callsign.toLowerCase().includes(searchLower) &&
            !vehicle.type.toLowerCase().includes(searchLower)) {
          return false
        }
      }

      // Station filter - prioritize showOnlyMyStation
      if (filters.showOnlyMyStation) {
        // Show only vehicles from the incident's responsible station
        if (vehicle.stationId !== incident.stationId) {
          return false
        }
      } else if (filters.selectedStations.length > 0) {
        // Only apply selectedStations filter if showOnlyMyStation is false
        if (!filters.selectedStations.includes(vehicle.stationId)) {
          return false
        }
      }

      // Status filter
      if (filters.vehicleStatusFilter !== 'all') {
        if (vehicle.status !== filters.vehicleStatusFilter) return false
      }

      // Availability filter
      if (filters.availabilityFilter !== 'all') {
        const assignmentInfo = getVehicleAssignmentInfo(vehicle.id)
        const isAvailable = vehicle.status === VehicleStatus.Available && !assignmentInfo.isAssigned
        
        switch (filters.availabilityFilter) {
          case 'available':
            if (!isAvailable) return false
            break
          case 'assigned':
            if (!assignmentInfo.isCurrentIncident) return false
            break
          case 'unavailable':
            if (isAvailable || assignmentInfo.isCurrentIncident) return false
            break
        }
      }

      return true
    })

    // Group by station
    const grouped = filtered.reduce((acc, vehicle) => {
      if (!acc[vehicle.stationId]) {
        acc[vehicle.stationId] = []
      }
      acc[vehicle.stationId].push(vehicle)
      return acc
    }, {} as Record<number, Vehicle[]>)

    return grouped
  }, [vehicles, filters, user?.stationId, incident, allIncidents])

  // Filter and group personnel
  const filteredAndGroupedPersonnel = useMemo(() => {
    let filtered = personnel.filter(person => {
      // Search filter
      if (filters.searchTerm) {
        const searchLower = filters.searchTerm.toLowerCase()
        if (!person.name.toLowerCase().includes(searchLower) &&
            !person.rank.toLowerCase().includes(searchLower)) {
          return false
        }
      }

      // Station filter - prioritize showOnlyMyStation
      if (filters.showOnlyMyStation) {
        // Show only personnel from the incident's responsible station
        if (person.stationId !== incident.stationId) {
          return false
        }
      } else if (filters.selectedStations.length > 0) {
        // Only apply selectedStations filter if showOnlyMyStation is false
        if (!filters.selectedStations.includes(person.stationId)) {
          return false
        }
      }

      // Availability filter
      if (filters.availabilityFilter !== 'all') {
        const assignmentInfo = getPersonnelAssignmentInfo(person.id)
        const isAvailable = !assignmentInfo.isAssigned
        
        switch (filters.availabilityFilter) {
          case 'available':
            if (!isAvailable) return false
            break
          case 'assigned':
            if (!assignmentInfo.isCurrentIncident) return false
            break
          case 'unavailable':
            if (isAvailable || assignmentInfo.isCurrentIncident) return false
            break
        }
      }

      return true
    })

    // Group by station
    const grouped = filtered.reduce((acc, person) => {
      if (!acc[person.stationId]) {
        acc[person.stationId] = []
      }
      acc[person.stationId].push(person)
      return acc
    }, {} as Record<number, Personnel[]>)

    return grouped
  }, [personnel, filters, user?.stationId, incident, allIncidents])

  const toggleStationExpansion = (stationId: number) => {
    const newExpanded = new Set(expandedStations)
    if (newExpanded.has(stationId)) {
      newExpanded.delete(stationId)
    } else {
      newExpanded.add(stationId)
    }
    setExpandedStations(newExpanded)
  }

  const handleStationFilterChange = (stationId: number, checked: boolean) => {
    setFilters(prev => ({
      ...prev,
      selectedStations: checked 
        ? [...prev.selectedStations, stationId]
        : prev.selectedStations.filter(id => id !== stationId)
    }))
  }

  const clearFilters = () => {
    setFilters({
      searchTerm: '',
      selectedStations: [],
      vehicleStatusFilter: 'all',
      availabilityFilter: 'all',
      showOnlyMyStation: !isDispatcher()
    })
  }

  const renderVehicleCard = (vehicle: Vehicle) => {
    const assignmentInfo = getVehicleAssignmentInfo(vehicle.id)
    const isAvailable = vehicle.status === VehicleStatus.Available && !assignmentInfo.isAssigned
    const isAssignedToThis = assignmentInfo.isCurrentIncident

    return (
      <div
        key={vehicle.id}
        className={`p-3 border rounded-lg transition-all hover:shadow-md aspect-square flex flex-col ${
          isAvailable 
            ? 'border-green-300 bg-green-50 dark:border-green-600 dark:bg-green-900/20' 
            : isAssignedToThis
            ? 'border-blue-300 bg-blue-50 dark:border-blue-600 dark:bg-blue-900/20'
            : 'border-red-300 bg-red-50 dark:border-red-600 dark:bg-red-900/20'
        }`}
      >
        {/* Header with icon and callsign */}
        <div className="flex items-center justify-between mb-2">
          <div className={`p-1.5 rounded-lg ${
            isAvailable ? 'bg-green-100 dark:bg-green-800' :
            isAssignedToThis ? 'bg-blue-100 dark:bg-blue-800' :
            'bg-red-100 dark:bg-red-800'
          }`}>
            <Truck className={`w-4 h-4 ${
              isAvailable ? 'text-green-600 dark:text-green-300' :
              isAssignedToThis ? 'text-blue-600 dark:text-blue-300' :
              'text-red-600 dark:text-red-300'
            }`} />
          </div>
          <span className="text-xs text-gray-600 dark:text-gray-400 truncate ml-1">
            {vehicle.type}
          </span>
        </div>

        {/* Vehicle info */}
        <div className="flex-1 flex flex-col">
          <h4 className="font-semibold text-sm text-gray-900 dark:text-gray-100 truncate mb-1">
            {vehicle.callsign}
          </h4>
          
          {vehicle.plateNumber && (
            <div className="text-xs text-gray-600 dark:text-gray-400 mb-2 truncate">
              {vehicle.plateNumber}
            </div>
          )}

          {/* Status indicators - compact */}
          <div className="flex flex-wrap gap-1 text-xs mb-2">
            {vehicle.fuelLevelPercent !== undefined && (
              <div className="flex items-center space-x-1">
                <Fuel className="w-3 h-3" />
                <span>{vehicle.fuelLevelPercent}%</span>
              </div>
            )}
            {vehicle.waterLevelLiters !== undefined && vehicle.waterCapacityLiters && (
              <div className="flex items-center space-x-1">
                <Droplets className="w-3 h-3" />
                <span>{Math.round((vehicle.waterLevelLiters / vehicle.waterCapacityLiters) * 100)}%</span>
              </div>
            )}
          </div>

          {/* Assignment status */}
          <div className="mt-auto">
            {isAvailable && (
              <span className="inline-flex items-center px-1.5 py-0.5 text-xs font-medium bg-green-100 text-green-800 dark:bg-green-800 dark:text-green-100 rounded-full">
                <CheckCircle className="w-3 h-3 mr-1" />
                Available
              </span>
            )}
            {isAssignedToThis && (
              <span className="inline-flex items-center px-1.5 py-0.5 text-xs font-medium bg-blue-100 text-blue-800 dark:bg-blue-800 dark:text-blue-100 rounded-full">
                <AlertCircle className="w-3 h-3 mr-1" />
                Assigned
              </span>
            )}
            {assignmentInfo.isAssigned && !isAssignedToThis && (
              <span className="inline-flex items-center px-1.5 py-0.5 text-xs font-medium bg-red-100 text-red-800 dark:bg-red-800 dark:text-red-100 rounded-full">
                <XCircle className="w-3 h-3 mr-1" />
                Busy
              </span>
            )}
            {vehicle.status !== VehicleStatus.Available && !assignmentInfo.isAssigned && (
              <span className="inline-flex items-center px-1.5 py-0.5 text-xs font-medium bg-gray-100 text-gray-800 dark:bg-gray-800 dark:text-gray-100 rounded-full">
                <XCircle className="w-3 h-3 mr-1" />
                {getVehicleStatusTranslation(vehicle.status, t)}
              </span>
            )}
          </div>
        </div>

        {/* Action button */}
        {isAvailable && (
          <button
            onClick={() => onAssign(ResourceType.Vehicle, vehicle.id)}
            disabled={isAssigning}
            className="mt-2 w-full px-2 py-1.5 bg-green-600 hover:bg-green-700 disabled:bg-green-400 text-white text-xs font-medium rounded-lg transition-colors focus:outline-none focus:ring-2 focus:ring-green-500 focus:ring-offset-1"
          >
            {isAssigning ? t.assigning : t.assign}
          </button>
        )}
      </div>
    )
  }

  const renderPersonnelCard = (person: Personnel) => {
    const assignmentInfo = getPersonnelAssignmentInfo(person.id)
    const isAvailable = !assignmentInfo.isAssigned
    const isAssignedToThis = assignmentInfo.isCurrentIncident

    return (
      <div
        key={person.id}
        className={`p-4 border rounded-lg transition-all hover:shadow-md ${
          isAvailable 
            ? 'border-green-300 bg-green-50 dark:border-green-600 dark:bg-green-900/20' 
            : isAssignedToThis
            ? 'border-blue-300 bg-blue-50 dark:border-blue-600 dark:bg-blue-900/20'
            : 'border-red-300 bg-red-50 dark:border-red-600 dark:bg-red-900/20'
        }`}
      >
        <div className="flex items-start justify-between">
          <div className="flex items-start space-x-3 flex-1">
            <div className={`p-2 rounded-lg ${
              isAvailable ? 'bg-green-100 dark:bg-green-800' :
              isAssignedToThis ? 'bg-blue-100 dark:bg-blue-800' :
              'bg-red-100 dark:bg-red-800'
            }`}>
              <Users className={`w-5 h-5 ${
                isAvailable ? 'text-green-600 dark:text-green-300' :
                isAssignedToThis ? 'text-blue-600 dark:text-blue-300' :
                'text-red-600 dark:text-red-300'
              }`} />
            </div>
            
            <div className="flex-1 min-w-0">
              <div className="flex items-center space-x-2 mb-1">
                <h4 className="font-semibold text-gray-900 dark:text-gray-100 truncate">
                  {person.name}
                </h4>
                <span className="text-sm text-gray-600 dark:text-gray-400">
                  {person.rank}
                </span>
              </div>
              
              {person.badgeNumber && (
                <div className="text-sm text-gray-600 dark:text-gray-400 mb-2">
                  Badge: {person.badgeNumber}
                </div>
              )}

              {/* Assignment status */}
              <div className="mt-2">
                {isAvailable && (
                  <span className="inline-flex items-center px-2 py-1 text-xs font-medium bg-green-100 text-green-800 dark:bg-green-800 dark:text-green-100 rounded-full">
                    <CheckCircle className="w-3 h-3 mr-1" />
                    {t.available}
                  </span>
                )}
                {isAssignedToThis && (
                  <span className="inline-flex items-center px-2 py-1 text-xs font-medium bg-blue-100 text-blue-800 dark:bg-blue-800 dark:text-blue-100 rounded-full">
                    <AlertCircle className="w-3 h-3 mr-1" />
                    {t.assignedToThisIncident}
                  </span>
                )}
                {assignmentInfo.isAssigned && !isAssignedToThis && (
                  <span className="inline-flex items-center px-2 py-1 text-xs font-medium bg-red-100 text-red-800 dark:bg-red-800 dark:text-red-100 rounded-full">
                    <XCircle className="w-3 h-3 mr-1" />
                    {t.assignedToIncident} #{assignmentInfo.incidentId}
                  </span>
                )}
              </div>
            </div>
          </div>

          {/* Action button */}
          <div className="ml-4">
            {isAvailable && (
              <button
                onClick={() => onAssign(ResourceType.Personnel, person.id)}
                disabled={isAssigning}
                className="px-4 py-2 bg-green-600 hover:bg-green-700 disabled:bg-green-400 text-white text-sm font-medium rounded-lg transition-colors focus:outline-none focus:ring-2 focus:ring-green-500 focus:ring-offset-2"
              >
                {isAssigning ? t.assigning : t.assign}
              </button>
            )}
          </div>
        </div>
      </div>
    )
  }

  if (!isOpen) return null

  const currentData = activeTab === 'vehicles' ? filteredAndGroupedVehicles : filteredAndGroupedPersonnel
  const stationIds = Object.keys(currentData).map(Number).sort()

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
      <div 
        ref={modalRef}
        className="bg-white dark:bg-gray-800 rounded-xl shadow-2xl w-full max-w-6xl max-h-[90vh] flex flex-col border border-gray-200 dark:border-gray-700"
      >
        {/* Header */}
        <div className="flex items-center justify-between p-6 border-b border-gray-200 dark:border-gray-700">
          <div>
            <h2 className="text-2xl font-bold text-gray-900 dark:text-gray-100">
              {t.assignResourceModal}
            </h2>
            <p className="text-sm text-gray-600 dark:text-gray-400 mt-1">
              Incident #{incident.id} - {incident.address}
            </p>
          </div>
          <button
            onClick={onClose}
            className="p-2 text-gray-400 hover:text-gray-600 dark:hover:text-gray-300 rounded-lg hover:bg-gray-100 dark:hover:bg-gray-700 transition-colors"
          >
            <X className="w-6 h-6" />
          </button>
        </div>

        {/* Tabs */}
        <div className="flex border-b border-gray-200 dark:border-gray-700">
          <button
            onClick={() => setActiveTab('vehicles')}
            className={`px-6 py-3 text-sm font-medium border-b-2 transition-colors ${
              activeTab === 'vehicles'
                ? 'border-blue-500 text-blue-600 dark:text-blue-400'
                : 'border-transparent text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-300'
            }`}
          >
            <Truck className="w-4 h-4 inline mr-2" />
            {t.vehiclesSection} ({Object.values(filteredAndGroupedVehicles).flat().length})
          </button>
          <button
            onClick={() => setActiveTab('personnel')}
            className={`px-6 py-3 text-sm font-medium border-b-2 transition-colors ${
              activeTab === 'personnel'
                ? 'border-blue-500 text-blue-600 dark:text-blue-400'
                : 'border-transparent text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-300'
            }`}
          >
            <Users className="w-4 h-4 inline mr-2" />
            {t.personnel} ({Object.values(filteredAndGroupedPersonnel).flat().length})
          </button>
        </div>

        {/* Filters */}
        <div className="p-6 border-b border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-900/50">
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
            {/* Search */}
            <div className="relative">
              <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 w-4 h-4" />
              <input
                type="text"
                placeholder={t.search}
                value={filters.searchTerm}
                onChange={(e) => setFilters(prev => ({ ...prev, searchTerm: e.target.value }))}
                className="w-full pl-10 pr-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg bg-white dark:bg-gray-800 text-gray-900 dark:text-gray-100 focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              />
            </div>

            {/* Availability Filter */}
            <select
              value={filters.availabilityFilter}
              onChange={(e) => setFilters(prev => ({ ...prev, availabilityFilter: e.target.value as any }))}
              className="px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg bg-white dark:bg-gray-800 text-gray-900 dark:text-gray-100 focus:ring-2 focus:ring-blue-500 focus:border-transparent"
            >
              <option value="all">{t.allStatuses}</option>
              <option value="available">{t.available}</option>
              <option value="assigned">{t.assignedToThisIncident}</option>
              <option value="unavailable">{t.unavailable}</option>
            </select>

            {/* Vehicle Status Filter (only for vehicles tab) */}
            {activeTab === 'vehicles' && (
              <select
                value={filters.vehicleStatusFilter}
                onChange={(e) => setFilters(prev => ({ ...prev, vehicleStatusFilter: e.target.value as any }))}
                className="px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg bg-white dark:bg-gray-800 text-gray-900 dark:text-gray-100 focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              >
                <option value="all">{t.allStatuses}</option>
                <option value={VehicleStatus.Available}>{t.available}</option>
                <option value={VehicleStatus.Notified}>{t.notified}</option>
                <option value={VehicleStatus.EnRoute}>{t.enRoute}</option>
                <option value={VehicleStatus.OnScene}>{t.onScene}</option>
                <option value={VehicleStatus.Busy}>{t.busy}</option>
                <option value={VehicleStatus.Maintenance}>{t.maintenance}</option>
                <option value={VehicleStatus.Offline}>{t.offline}</option>
              </select>
            )}

            {/* Clear Filters */}
            <button
              onClick={clearFilters}
              className="px-4 py-2 text-sm font-medium text-gray-700 dark:text-gray-300 bg-white dark:bg-gray-800 border border-gray-300 dark:border-gray-600 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-700 transition-colors"
            >
              {t.clear}
            </button>
          </div>

          {/* Station Filters */}
          <div className="mt-4">
            {isDispatcher() ? (
              <>
                <div className="flex items-center space-x-4 mb-2">
                  <label className="flex items-center">
                    <input
                      type="checkbox"
                      checked={filters.showOnlyMyStation}
                      onChange={(e) => setFilters(prev => ({ 
                        ...prev, 
                        showOnlyMyStation: e.target.checked,
                        selectedStations: e.target.checked ? [] : prev.selectedStations
                      }))}
                      className="mr-2"
                    />
                    <span className="text-sm text-gray-700 dark:text-gray-300">
                      {t.showResponsibleStationOnly} ({getStationName(incident.stationId)})
                    </span>
                  </label>
                </div>
                
                {!filters.showOnlyMyStation && (
                  <div className="relative station-dropdown">
                    <button
                      onClick={() => setStationDropdownOpen(!stationDropdownOpen)}
                      className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg bg-white dark:bg-gray-800 text-gray-900 dark:text-gray-100 focus:ring-2 focus:ring-blue-500 focus:border-transparent text-left flex items-center justify-between"
                    >
                      <span>
                        {filters.selectedStations.length === 0 
                          ? t.selectStations 
                          : `${filters.selectedStations.length} ${t.stationsSelected}`
                        }
                      </span>
                      <ChevronDown className={`w-4 h-4 transition-transform ${stationDropdownOpen ? 'rotate-180' : ''}`} />
                    </button>
                    
                    {stationDropdownOpen && (
                      <div className="absolute z-10 w-full mt-1 bg-white dark:bg-gray-800 border border-gray-300 dark:border-gray-600 rounded-lg shadow-lg max-h-48 overflow-y-auto">
                        {stations.map(station => (
                          <label key={station.id} className="flex items-center px-3 py-2 hover:bg-gray-50 dark:hover:bg-gray-700 cursor-pointer">
                            <input
                              type="checkbox"
                              checked={filters.selectedStations.includes(station.id)}
                              onChange={(e) => handleStationFilterChange(station.id, e.target.checked)}
                              className="mr-2"
                            />
                            <span className="text-sm text-gray-700 dark:text-gray-300">
                              {station.name}
                            </span>
                          </label>
                        ))}
                      </div>
                    )}
                  </div>
                )}
              </>
            ) : (
              <div className="text-sm text-gray-600 dark:text-gray-400">
                Showing resources from {user?.stationId ? getStationName(user.stationId) : 'your station'} only
              </div>
            )}
          </div>
        </div>

        {/* Content */}
        <div className="flex-1 overflow-y-auto p-6">
          {stationIds.length === 0 ? (
            <div className="text-center py-12">
              <div className="w-16 h-16 mx-auto mb-4 bg-gray-100 dark:bg-gray-700 rounded-full flex items-center justify-center">
                {activeTab === 'vehicles' ? (
                  <Truck className="w-8 h-8 text-gray-400" />
                ) : (
                  <Users className="w-8 h-8 text-gray-400" />
                )}
              </div>
              <h3 className="text-lg font-medium text-gray-900 dark:text-gray-100 mb-2">
                No {activeTab} found
              </h3>
              <p className="text-gray-600 dark:text-gray-400">
                Try adjusting your filters to see more results
              </p>
            </div>
          ) : (
            <div className="space-y-6">
              {stationIds.map(stationId => {
                const stationData = currentData[stationId]
                const isExpanded = expandedStations.has(stationId)
                
                return (
                  <div key={stationId} className="border border-gray-200 dark:border-gray-700 rounded-lg">
                    <button
                      onClick={() => toggleStationExpansion(stationId)}
                      className="w-full px-4 py-3 flex items-center justify-between bg-gray-50 dark:bg-gray-800 hover:bg-gray-100 dark:hover:bg-gray-700 rounded-t-lg transition-colors"
                    >
                      <div className="flex items-center space-x-3">
                        <MapPin className="w-5 h-5 text-gray-500" />
                        <h3 className="text-lg font-semibold text-gray-900 dark:text-gray-100">
                          {getStationName(stationId)}
                        </h3>
                        <span className="px-2 py-1 text-xs font-medium bg-blue-100 text-blue-800 dark:bg-blue-800 dark:text-blue-100 rounded-full">
                          {stationData.length} {activeTab}
                        </span>
                      </div>
                      {isExpanded ? (
                        <ChevronDown className="w-5 h-5 text-gray-500" />
                      ) : (
                        <ChevronRight className="w-5 h-5 text-gray-500" />
                      )}
                    </button>
                    
                    {isExpanded && (
                      <div className="p-4 bg-white dark:bg-gray-800">
                        {activeTab === 'vehicles' ? (
                          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4">
                            {stationData.map(vehicle => renderVehicleCard(vehicle as Vehicle))}
                          </div>
                        ) : (
                          <div className="space-y-3">
                            {stationData.map(person => renderPersonnelCard(person as Personnel))}
                          </div>
                        )}
                      </div>
                    )}
                  </div>
                )
              })}
            </div>
          )}
        </div>

        {/* Footer */}
        <div className="flex justify-end p-6 border-t border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-900/50">
          <button
            onClick={onClose}
            className="px-6 py-2 text-sm font-medium text-gray-700 dark:text-gray-300 bg-white dark:bg-gray-800 border border-gray-300 dark:border-gray-600 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-700 transition-colors"
          >
            {t.cancel}
          </button>
        </div>
      </div>
    </div>
  )
}