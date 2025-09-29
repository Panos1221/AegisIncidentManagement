import { useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { X, Car, Filter, CheckCircle, AlertTriangle, XCircle } from 'lucide-react'
import { vehiclesApi, patrolZonesApi, fireStationsApi } from '../lib/api'
import { PatrolZone } from '../types'
import { useTranslation } from '../hooks/useTranslation'
import { useUserStore } from '../lib/userStore'
import { useToast } from './ToastContainer'
import { getVehicleStatusTranslation } from '../utils/incidentUtils'

interface PatrolZoneVehicleAssignmentModalProps {
  isOpen: boolean
  onClose: () => void
  patrolZone: PatrolZone | null
}

export function PatrolZoneVehicleAssignmentModal({
  isOpen,
  onClose,
  patrolZone
}: PatrolZoneVehicleAssignmentModalProps) {
  const t  = useTranslation()
  const queryClient = useQueryClient()
  const { user } = useUserStore()
  const { showSuccess, showError } = useToast()
  const [selectedVehicleId, setSelectedVehicleId] = useState<number | null>(null)
  const [showAllStations, setShowAllStations] = useState(false)

  // Get all stations for filtering
  const { data: stations = [] } = useQuery({
    queryKey: ['stations'],
    queryFn: () => fireStationsApi.getAll(),
    enabled: isOpen
  })

  // Get available vehicles (filtered by station or all)
  const { data: vehicles = [], isLoading: vehiclesLoading } = useQuery({
    queryKey: ['vehicles', showAllStations ? 'all' : user?.stationId],
    queryFn: () => vehiclesApi.getAll({ 
      stationId: showAllStations ? undefined : user?.stationId 
    }).then(res => res.data),
    enabled: isOpen && !!user
  })

  // Get all patrol zones to check for assignments
  const { data: allPatrolZones = [] } = useQuery({
    queryKey: ['patrolZones', user?.stationId],
    queryFn: () => patrolZonesApi.getAll(user?.stationId ? { stationId: user.stationId } : {}).then(res => res.data),
    enabled: isOpen && !!user
  })

  // Helper function to get station name
  const getStationName = (stationId: number) => {
    const station = stations.find(s => s.id === stationId)
    return station?.name || `Station ${stationId}`
  }

  // Helper function to get vehicle assignment info
  const getVehicleAssignmentInfo = (vehicleId: number) => {
    if (!patrolZone) {
      return { isAssigned: false, patrolZoneName: '', isCurrentZone: false }
    }

    // Check if vehicle is assigned to current patrol zone
    const currentAssignment = patrolZone.vehicleAssignments?.find(assignment => 
      assignment.vehicleId === vehicleId && assignment.isActive
    )
    if (currentAssignment) {
      return { isAssigned: true, patrolZoneName: patrolZone.name, isCurrentZone: true }
    }

    // Check if vehicle is assigned to other patrol zones
    const otherAssignment = allPatrolZones.find(zone => 
      zone.id !== patrolZone.id &&
      zone.vehicleAssignments?.some(assignment => 
        assignment.vehicleId === vehicleId && assignment.isActive
      )
    )
    
    if (otherAssignment) {
      return { isAssigned: true, patrolZoneName: otherAssignment.name, isCurrentZone: false }
    }

    return { isAssigned: false, patrolZoneName: null, isCurrentZone: false }
  }

  // Filter out vehicles that are already assigned to this patrol zone
  const availableVehicles = vehicles.filter(vehicle => 
    !patrolZone?.vehicleAssignments?.some(assignment => 
      assignment.vehicleId === vehicle.id && assignment.isActive
    )
  )

  const assignVehicleMutation = useMutation({
    mutationFn: (vehicleId: number) => {
      if (!patrolZone) throw new Error('No patrol zone selected')
      return patrolZonesApi.assignVehicle(patrolZone.id, { vehicleId, patrolZoneId: patrolZone.id })
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['patrolZones'] })
      queryClient.invalidateQueries({ queryKey: ['vehicles'] })
      showSuccess(t.vehicleAssignedSuccessfully)
      onClose()
    },
    onError: (error: any) => {
      showError(t.errorAssigningVehicle, error.response?.data?.message)
    }
  })

  const handleClose = () => {
    setSelectedVehicleId(null)
    setShowAllStations(false)
    onClose()
  }

  const handleAssign = () => {
    if (selectedVehicleId) {
      assignVehicleMutation.mutate(selectedVehicleId)
    }
  }

  if (!isOpen || !patrolZone) return null

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-[9999]">
      <div className="bg-white dark:bg-gray-800 rounded-lg shadow-xl max-w-2xl w-full mx-4 max-h-[90vh] flex flex-col">
        <div className="flex items-center justify-between p-6 border-b border-gray-200 dark:border-gray-700">
          <h2 className="text-xl font-semibold text-gray-900 dark:text-gray-100">
            {t.assignVehicle}
          </h2>
          <button
            onClick={handleClose}
            className="text-gray-400 hover:text-gray-600 dark:hover:text-gray-300 transition-colors"
          >
            <X className="w-6 h-6" />
          </button>
        </div>

        <div className="p-6 flex-1 overflow-hidden flex flex-col">
          <div className="mb-4">
            <h3 className="text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              {t.patrolZone}: {patrolZone.name}
            </h3>
          </div>

          {/* Station Filter Toggle */}
          <div className="mb-4 p-4 bg-gray-50 dark:bg-gray-700 rounded-lg">
            <div className="flex items-center justify-between">
              <div className="flex items-center">
                <Filter className="w-4 h-4 mr-2 text-gray-600 dark:text-gray-400" />
                <span className="text-sm font-medium text-gray-700 dark:text-gray-300">
                  {t.vehicleFilter || 'Vehicle Filter'}
                </span>
              </div>
              <label className="flex items-center cursor-pointer">
                <input
                  type="checkbox"
                  checked={showAllStations}
                  onChange={(e) => setShowAllStations(e.target.checked)}
                  className="sr-only"
                />
                <div className={`relative inline-flex h-6 w-11 items-center rounded-full transition-colors ${
                  showAllStations ? 'bg-blue-600' : 'bg-gray-300 dark:bg-gray-600'
                }`}>
                  <span className={`inline-block h-4 w-4 transform rounded-full bg-white transition-transform ${
                    showAllStations ? 'translate-x-6' : 'translate-x-1'
                  }`} />
                </div>
                <span className="ml-3 text-sm text-gray-700 dark:text-gray-300">
                  {showAllStations ? (t.showAllStations || 'All Stations') : (t.responsibleStationOnly || 'My Station Only')}
                </span>
              </label>
            </div>
            <div className="mt-2 text-xs text-gray-500 dark:text-gray-400">
              {showAllStations 
                ? (t.showingVehiclesFromAllStations || 'Showing vehicles from all stations as additional resources')
                : (t.showingVehiclesFromMyStation || `Showing vehicles from your station only`)
              }
            </div>
          </div>

          <div className="flex-1 flex flex-col min-h-0">
            <h4 className="text-sm font-medium text-gray-700 dark:text-gray-300 mb-3">
              {t.selectVehicleToAssign}
            </h4>
            {vehiclesLoading ? (
              <div className="flex items-center justify-center py-8">
                <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
              </div>
            ) : availableVehicles.length === 0 ? (
              <div className="flex items-center justify-center py-8 text-gray-500 dark:text-gray-400">
                <AlertTriangle className="w-5 h-5 mr-2" />
                {t.noAvailableVehicles}
              </div>
            ) : (
              <div className="flex-1 overflow-y-auto">
                <div className="space-y-3 pr-2">
                  {availableVehicles.map((vehicle) => {
                    const assignmentInfo = getVehicleAssignmentInfo(vehicle.id)
                    const isAvailable = vehicle.status === 0 && !assignmentInfo.isAssigned
                    const isFromDifferentStation = showAllStations && vehicle.stationId !== patrolZone?.stationId
                    
                    return (
                      <label
                        key={vehicle.id}
                        className={`flex items-start p-4 border rounded-lg cursor-pointer transition-colors ${
                          selectedVehicleId === vehicle.id
                            ? 'border-blue-500 bg-blue-50 dark:bg-blue-900/30 dark:border-blue-400'
                            : isAvailable
                            ? 'border-green-200 dark:border-green-700 bg-green-50 dark:bg-green-900/20 hover:border-green-300 dark:hover:border-green-600'
                            : assignmentInfo.isAssigned
                            ? 'border-orange-200 dark:border-orange-700 bg-orange-50 dark:bg-orange-900/20 hover:border-orange-300 dark:hover:border-orange-600'
                            : 'border-gray-200 dark:border-gray-600 hover:border-gray-300 dark:hover:border-gray-500'
                        }`}
                      >
                        <input
                          type="radio"
                          name="vehicle"
                          value={vehicle.id}
                          checked={selectedVehicleId === vehicle.id}
                          onChange={() => setSelectedVehicleId(vehicle.id)}
                          className="sr-only"
                        />
                        <Car className={`w-5 h-5 mr-3 mt-0.5 ${
                          isAvailable 
                            ? 'text-green-600' 
                            : assignmentInfo.isAssigned 
                            ? 'text-orange-600' 
                            : 'text-gray-400'
                        }`} />
                        <div className="flex-1 min-w-0">
                          <div className="flex items-center justify-between">
                            <div className="font-medium text-gray-900 dark:text-gray-100">
                              {vehicle.callsign}
                            </div>
                            {selectedVehicleId === vehicle.id && (
                              <div className="w-4 h-4 bg-blue-500 rounded-full flex items-center justify-center ml-2">
                                <div className="w-2 h-2 bg-white rounded-full"></div>
                              </div>
                            )}
                          </div>
                          <div className="text-sm text-gray-500 dark:text-gray-400 mt-1">
                            {vehicle.type} • {vehicle.plateNumber}
                          </div>
                          {showAllStations && (
                            <div className="text-sm text-gray-600 dark:text-gray-400 mt-1">
                              <span className={`inline-flex items-center px-2 py-1 rounded-full text-xs font-medium ${
                                isFromDifferentStation 
                                  ? 'bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-200'
                                  : 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200'
                              }`}>
                                {getStationName(vehicle.stationId)}
                                {isFromDifferentStation && ` (${t.additionalResource})`}
                              </span>
                            </div>
                          )}
                          <div className="flex flex-wrap gap-2 mt-2">
                            {isAvailable && (
                              <span className="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200">
                                <CheckCircle className="w-3 h-3 mr-1" />
                                {t.available}
                              </span>
                            )}
                            {assignmentInfo.isAssigned && (
                              <span className="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-orange-100 text-orange-800 dark:bg-orange-900 dark:text-orange-200">
                                <AlertTriangle className="w-3 h-3 mr-1" />
                                {t.assignedToPatrolZone}: {assignmentInfo.patrolZoneName}
                              </span>
                            )}
                            {vehicle.status !== 0 && (
                              <span className="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-200">
                                <XCircle className="w-3 h-3 mr-1" />
                                {getVehicleStatusTranslation(vehicle.status, t)}
                              </span>
                            )}
                          </div>
                        </div>
                      </label>
                    )
                  })}
                </div>
              </div>
            )}
          </div>
        </div>

        <div className="flex justify-end gap-3 p-6 border-t border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-700/50 flex-shrink-0">
          <button
            onClick={handleClose}
            className="px-4 py-2 text-sm font-medium text-gray-700 dark:text-gray-300 bg-white dark:bg-gray-700 border border-gray-300 dark:border-gray-600 rounded-md hover:bg-gray-50 dark:hover:bg-gray-600 transition-colors"
          >
            {t.cancel}
          </button>
          <button
            onClick={handleAssign}
            disabled={!selectedVehicleId || assignVehicleMutation.isPending}
            className="px-4 py-2 text-sm font-medium text-white bg-green-600 border border-transparent rounded-md hover:bg-green-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
          >
            {assignVehicleMutation.isPending ? t.assigning : t.assign}
          </button>
        </div>
      </div>
    </div>
  )
}