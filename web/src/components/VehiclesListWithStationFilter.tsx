import React, { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { vehiclesApi, fireStationsApi } from '../lib/api'
import { VehicleStatus, FireStation } from '../types'
import { Truck, Filter, Fuel, Droplets, Battery, Gauge, Plus, X, Save } from 'lucide-react'
import StationFilter from './StationFilter'
import { useTranslation } from '../hooks/useTranslation'
import { getVehicleStatusTranslation } from '../utils/incidentUtils'
import { useUserStore } from '../lib/userStore'

/**
 * Enhanced VehiclesList component demonstrating StationFilter integration
 * This shows how the StationFilter component would be used in practice
 */
export default function VehiclesListWithStationFilter() {
  const t = useTranslation()
  const { user } = useUserStore()
  const queryClient = useQueryClient()
  const [stationFilter, setStationFilter] = useState<number | undefined>(undefined)
  const [statusFilter, setStatusFilter] = useState<number | ''>('')
  const [showAddModal, setShowAddModal] = useState(false)
  const [formData, setFormData] = useState({
    stationId: 0,
    callsign: '',
    type: '',
    plateNumber: '',
    waterCapacityLiters: 0
  })

  // Fetch agency-specific stations for name mapping
  const { data: stations = [] } = useQuery({
    queryKey: ['stations', user?.agencyId],
    queryFn: () => fireStationsApi.getStations(),
    staleTime: 5 * 60 * 1000, // 5 minutes
    enabled: !!user?.agencyId,
  })

  // Helper function to get station name by ID
  const getStationName = (stationId: number): string => {
    const station = stations.find((s: FireStation) => s.id === stationId)
    return station?.name || `Station ${stationId}`
  }

  const { data: vehicles, isLoading } = useQuery({
    queryKey: ['vehicles', statusFilter, stationFilter],
    queryFn: () => vehiclesApi.getAll({ 
      status: statusFilter === '' ? undefined : statusFilter,
      stationId: stationFilter
    }).then(res => res.data),
  })

  const createVehicleMutation = useMutation({
    mutationFn: (data: typeof formData) => vehiclesApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['vehicles'] })
      setShowAddModal(false)
      setFormData({ callsign: '', type: '', plateNumber: '', stationId: 1, waterCapacityLiters: 0 })
    }
  })

  const getStatusColor = (status: VehicleStatus) => {
    switch (status) {
      case VehicleStatus.Available:
        return 'bg-green-500 text-white dark:bg-green-500 dark:text-white'
      case VehicleStatus.Notified:
        return 'bg-blue-500 text-white dark:bg-blue-500 dark:text-white'
      case VehicleStatus.EnRoute:
        return 'bg-yellow-500 text-yellow-900 dark:bg-yellow-500 dark:text-yellow-900'
      case VehicleStatus.OnScene:
        return 'bg-orange-500 text-white dark:bg-orange-500 dark:text-white'
      case VehicleStatus.Busy:
        return 'bg-red-500 text-white dark:bg-red-500 dark:text-white'
      case VehicleStatus.Maintenance:
        return 'bg-purple-500 text-white dark:bg-purple-500 dark:text-white'
      case VehicleStatus.Offline:
        return 'bg-gray-500 text-white dark:bg-gray-500 dark:text-white'
      default:
        return 'bg-gray-500 text-white dark:bg-gray-500 dark:text-white'
    }
  }

  const getWaterPercentage = (vehicle: any) => {
    if (!vehicle.waterLevelLiters || !vehicle.waterCapacityLiters) return null
    return Math.round((vehicle.waterLevelLiters / vehicle.waterCapacityLiters) * 100)
  }

  // Group vehicles by station when no station filter is applied
  const groupedVehicles = React.useMemo(() => {
    if (!vehicles) return {}
    if (stationFilter) return { [stationFilter]: vehicles }
    
    return vehicles.reduce((acc, vehicle) => {
      const stationId = vehicle.stationId
      if (!acc[stationId]) acc[stationId] = []
      acc[stationId].push(vehicle)
      return acc
    }, {} as Record<number, typeof vehicles>)
  }, [vehicles, stationFilter])

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
          <h1 className="text-3xl font-bold text-gray-900">Vehicles</h1>
          <p className="mt-2 text-gray-600">
            Monitor vehicle status and telemetry
          </p>
        </div>
        <button 
          onClick={() => setShowAddModal(true)}
          className="btn btn-primary flex items-center"
        >
          <Plus className="w-4 h-4 mr-2" />
          Add Vehicle
        </button>
      </div>

      {/* Enhanced Filters with Station Filter */}
      <div className="card p-4 mb-6">
        <div className="flex items-center space-x-6">
          {/* Status Filter */}
          <div className="flex items-center space-x-4">
            <Filter className="w-5 h-5 text-gray-400 dark:text-gray-500" />
            <select
              value={statusFilter}
              onChange={(e) => setStatusFilter(e.target.value as VehicleStatus | '')}
              className="input"
            >
              <option value="">All Statuses</option>
              {Object.entries(VehicleStatus)
                .filter(([key]) => isNaN(Number(key)))
                .map(([key, value]) => (
                  <option key={key} value={value}>
                    {key}
                  </option>
                ))}
            </select>
          </div>

          {/* Station Filter */}
          <StationFilter
            selectedStationId={stationFilter}
            onStationChange={setStationFilter}
            placeholder={t.allStations}
          />

          {/* Clear Filters */}
          {(statusFilter || stationFilter) && (
            <button
              onClick={() => {
                setStatusFilter('')
                setStationFilter(undefined)
              }}
              className="btn btn-secondary text-sm"
            >
              Clear Filters
            </button>
          )}
        </div>
      </div>

      {/* Vehicles Display - Grouped by Station */}
      {Object.entries(groupedVehicles).map(([stationId, stationVehicles]) => (
        <div key={stationId} className="mb-8">
          <h2 className="text-xl font-semibold text-gray-900 mb-4">
            {getStationName(parseInt(stationId))} ({stationVehicles.length} vehicle{stationVehicles.length !== 1 ? 's' : ''})
          </h2>
          
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            {stationVehicles.map((vehicle) => (
              <div key={vehicle.id} className="card p-6">
                <div className="flex items-start justify-between mb-4">
                  <div className="flex items-center">
                    <div className="p-2 bg-blue-100 rounded-lg">
                      <Truck className="w-6 h-6 text-blue-600" />
                    </div>
                    <div className="ml-3">
                      <h3 className="text-lg font-semibold text-gray-900">
                        {vehicle.callsign}
                      </h3>
                      <p className="text-sm text-gray-600">{vehicle.type}</p>
                    </div>
                  </div>
                  <span className={`px-2 py-1 text-xs font-medium rounded-full ${getStatusColor(vehicle.status)}`}>
                    {getVehicleStatusTranslation(vehicle.status, t)}
                  </span>
                </div>

                <div className="space-y-3">
                  <div className="flex justify-between text-sm">
                    <span className="text-gray-600">Plate Number</span>
                    <span className="font-medium text-gray-900">{vehicle.plateNumber}</span>
                  </div>

                  {/* Telemetry section remains the same */}
                  <div className="pt-3 border-t border-gray-200 dark:border-gray-700">
                    <h4 className="text-sm font-medium text-gray-900 dark:text-gray-100 mb-3">Telemetry</h4>
                    <div className="grid grid-cols-2 gap-3">
                      {/* Fuel Level */}
                      {vehicle.fuelLevelPercent !== null && (
                        <div className="flex items-center">
                          <Fuel className="w-4 h-4 text-gray-400 mr-2" />
                          <div className="flex-1">
                            <div className="flex justify-between text-xs mb-1">
                              <span>Fuel</span>
                              <span>{vehicle.fuelLevelPercent}%</span>
                            </div>
                            <div className="w-full bg-gray-200 rounded-full h-2">
                              <div
                                className={`h-2 rounded-full ${
                                  (vehicle.fuelLevelPercent ?? 0) > 50
                                    ? 'bg-green-500'
                                    : (vehicle.fuelLevelPercent ?? 0) > 25
                                    ? 'bg-yellow-500'
                                    : 'bg-red-500'
                                }`}
                                style={{ width: `${vehicle.fuelLevelPercent}%` }}
                              />
                            </div>
                          </div>
                        </div>
                      )}

                      {/* Water Level */}
                      {getWaterPercentage(vehicle) !== null && (
                        <div className="flex items-center">
                          <Droplets className="w-4 h-4 text-blue-400 mr-2" />
                          <div className="flex-1">
                            <div className="flex justify-between text-xs mb-1">
                              <span>Water</span>
                              <span>{getWaterPercentage(vehicle)}%</span>
                            </div>
                            <div className="w-full bg-gray-200 rounded-full h-2">
                              <div
                                className="h-2 rounded-full bg-blue-500"
                                style={{ width: `${getWaterPercentage(vehicle)}%` }}
                              />
                            </div>
                          </div>
                        </div>
                      )}

                      {/* Battery Voltage */}
                      {vehicle.batteryVoltage !== null && (
                        <div className="flex items-center">
                          <Battery className="w-4 h-4 text-gray-400 mr-2" />
                          <div className="flex-1">
                            <div className="text-xs">
                              <span>Battery: {vehicle.batteryVoltage}V</span>
                            </div>
                          </div>
                        </div>
                      )}

                      {/* Pump Pressure */}
                      {vehicle.pumpPressureKPa !== null && (
                        <div className="flex items-center">
                          <Gauge className="w-4 h-4 text-gray-400 mr-2" />
                          <div className="flex-1">
                            <div className="text-xs">
                              <span>Pump: {vehicle.pumpPressureKPa} kPa</span>
                            </div>
                          </div>
                        </div>
                      )}
                    </div>
                  </div>

                  {vehicle.lastTelemetryAt && (
                    <div className="text-xs text-gray-500 dark:text-gray-400 pt-2 border-t border-gray-200 dark:border-gray-700">
                      Last update: {new Date(vehicle.lastTelemetryAt).toLocaleString()}
                    </div>
                  )}
                </div>
              </div>
            ))}
          </div>

          {stationVehicles.length === 0 && (
            <div className="text-center py-8 text-gray-500">
              No vehicles found for {getStationName(parseInt(stationId))}
            </div>
          )}
        </div>
      ))}

      {Object.keys(groupedVehicles).length === 0 && (
        <div className="text-center py-12">
          <Truck className="w-12 h-12 text-gray-400 mx-auto mb-4" />
          <h3 className="text-lg font-medium text-gray-900 mb-2">No vehicles found</h3>
          <p className="text-gray-600">
            {statusFilter || stationFilter ? 'Try adjusting your filters' : 'No vehicles are currently registered'}
          </p>
        </div>
      )}

      {/* Add Vehicle Modal - Same as original */}
      {showAddModal && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white dark:bg-gray-800 rounded-lg p-6 w-full max-w-md">
            <div className="flex items-center justify-between mb-4">
              <h3 className="text-lg font-semibold text-gray-900 dark:text-gray-100">Add Vehicle</h3>
              <button onClick={() => setShowAddModal(false)}>
                <X className="w-5 h-5" />
              </button>
            </div>
            <form onSubmit={(e) => {
              e.preventDefault()
              if (!formData.callsign.trim() || !formData.type.trim() || !formData.plateNumber.trim()) {
                alert('Please fill in all required fields')
                return
              }
              createVehicleMutation.mutate(formData)
            }} className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                  Callsign *
                </label>
                <input
                  type="text"
                  value={formData.callsign}
                  onChange={(e) => setFormData(prev => ({ ...prev, callsign: e.target.value }))}
                  className="input w-full"
                  placeholder="e.g., Engine 1, Truck 2"
                  required
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                  Type *
                </label>
                <select
                  value={formData.type}
                  onChange={(e) => setFormData(prev => ({ ...prev, type: e.target.value }))}
                  className="input w-full"
                  required
                >
                  <option value="">Select vehicle type</option>
                  <option value="Fire Engine">Fire Engine</option>
                  <option value="Ladder Truck">Ladder Truck</option>
                  <option value="Ambulance">Ambulance</option>
                  <option value="Rescue Vehicle">Rescue Vehicle</option>
                  <option value="Tanker">Tanker</option>
                  <option value="Command Vehicle">Command Vehicle</option>
                </select>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                  Plate Number *
                </label>
                <input
                  type="text"
                  value={formData.plateNumber}
                  onChange={(e) => setFormData(prev => ({ ...prev, plateNumber: e.target.value }))}
                  className="input w-full"
                  placeholder="License plate number"
                  required
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                  Station
                </label>
                <StationFilter
                  selectedStationId={formData.stationId}
                  onStationChange={(stationId) => setFormData(prev => ({ ...prev, stationId: stationId || 1 }))}
                  placeholder="Select Station"
                  required
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                  Water Capacity (Liters)
                </label>
                <input
                  type="number"
                  value={formData.waterCapacityLiters}
                  onChange={(e) => setFormData(prev => ({ ...prev, waterCapacityLiters: parseInt(e.target.value) || 0 }))}
                  className="input w-full"
                  placeholder="0"
                  min="0"
                />
              </div>
              <div className="flex justify-end space-x-2">
                <button
                  type="button"
                  onClick={() => setShowAddModal(false)}
                  className="btn btn-secondary"
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  disabled={createVehicleMutation.isPending}
                  className="btn btn-primary flex items-center"
                >
                  <Save className="w-4 h-4 mr-2" />
                  {createVehicleMutation.isPending ? 'Adding...' : 'Add Vehicle'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  )
}