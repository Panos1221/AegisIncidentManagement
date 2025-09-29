import { useState, useMemo } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { vehiclesApi, fireStationsApi, ApiError } from '../lib/api'
import { VehicleStatus, Vehicle, UpdateVehicleDto, FireStation } from '../types'
import { Truck, Filter, Fuel, Droplets, Battery, Gauge, Plus, X, Save, Edit2, CheckCircle, AlertTriangle } from 'lucide-react'
import { StationFilter, VehicleEditModal, ErrorBoundary, LoadingSpinner, RetryButton } from '../components'
import { useTranslation } from '../hooks/useTranslation'
import { useUserStore } from '../lib/userStore'
import { getVehicleStatusTranslation } from '../utils/incidentUtils'

export default function VehiclesList() {
  const queryClient = useQueryClient()
  const t = useTranslation()
  const { canEditVehicle, canManageStation, user, isDispatcher } = useUserStore()

  // Fetch agency-specific stations for name mapping
  const { data: stations = [] } = useQuery({
    queryKey: ['fireStations', user?.agencyId],
    queryFn: () => fireStationsApi.getStations(),
    staleTime: 5 * 60 * 1000, // 5 minutes
    enabled: !!user?.agencyId,
  })

  // Helper function to get station name by ID
  const getStationName = (stationId: number): string => {
    const station = stations.find((s: FireStation) => s.id === stationId)
    return station?.name || `Station ${stationId}`
  }

  // Helper function to render vehicle types based on user's agency
  const renderVehicleTypeOptions = () => {
    const agencyName = user?.agencyName
    console.log('User agency name:', agencyName, 'User:', user)

    // Check for Fire Department (multiple possible names)
    if (agencyName === 'Hellenic Fire Service' || agencyName === 'Fire Department' || agencyName === 'FireDepartment' || agencyName?.toLowerCase().includes('fire')) {
      return (
        <optgroup label="Fire Department">
          <option value="Fire Engine">{t.fireEngine}</option>
          <option value="Ladder">{t.ladder}</option>
          <option value="Rescue Vehicle">{t.rescueVehicle}</option>
          <option value="Command">{t.command}</option>
          <option value="Tanker">{t.tanker}</option>
          <option value="Fire Boat">{t.fireBoat}</option>
          <option value="Hazmat Truck">{t.hazmatTruck}</option>
          <option value="Support">{t.support}</option>
          <option value="Foodtruck">{t.foodtruck}</option>
          <option value="FC Bus">{t.fcbus}</option>
          <option value="Petrol Truck">{t.petroltruck}</option>
        </optgroup>
      )
    } else if (agencyName === 'Hellenic Coast Guard' || agencyName === 'Coast Guard' || agencyName === 'CoastGuard' || agencyName?.toLowerCase().includes('coast')) {
      return (
        <optgroup label="Coast Guard">
          <option value="Patrol Boat">{t.patrolBoat}</option>
          <option value="Offshore Patrol Vessel">{t.offshorePatrolVessel}</option>
          <option value="Search Rescue Boat">{t.searchRescueBoat}</option>
          <option value="Rigid Inflatable">{t.rigidInflatable}</option>
          <option value="Pollution Control">{t.pollutionControl}</option>
          <option value="CG Helicopter">{t.cghelicopter}</option>
          <option value="CG Airplane">{t.cgairplane}</option>
          <option value="Patrol Vehicle">{t.patrolVehicle}</option>
          <option value="CG Bus">{t.cgbus}</option>
        </optgroup>
      )
    } else if (agencyName === 'Hellenic Police' || agencyName === 'Police' || agencyName?.toLowerCase().includes('police')) {
      return (
        <optgroup label="Police">
          <option value="Police Patrol Car">{t.policePatrolCar}</option>
          <option value="Police Motorcycle">{t.policeMotorcycle}</option>
          <option value="Police Van">{t.policeVan}</option>
          <option value="Police Bus">{t.policeBus}</option>
          <option value="Police Helicopter">{t.policeHelicopter}</option>
          <option value="Police Boat">{t.policeBoat}</option>
          <option value="Police Command Vehicle">{t.policeCommandVehicle}</option>
          <option value="Special Operations Vehicle">{t.policeSpecialOperations}</option>
          <option value="Traffic Enforcement Vehicle">{t.policeTrafficEnforcement}</option>
          <option value="K-9 Unit Vehicle">{t.policeK9Unit}</option>
          <option value="Bomb Squad Vehicle">{t.policeBombSquad}</option>
          <option value="Forensics Vehicle">{t.policeForensics}</option>
        </optgroup>
      )
    } else if (agencyName === 'EKAB' || agencyName?.toLowerCase().includes('ekab')) {
      return (
        <optgroup label="EKAB">
          <option value="Basic Ambulance">{t.basicAmbulance}</option>
          <option value="Advanced Ambulance">{t.advancedAmbulance}</option>
          <option value="Intensive Care Ambulance">{t.intensiveCareAmbulance}</option>
          <option value="Neonatal Ambulance">{t.neonatalAmbulance}</option>
          <option value="EKAB Motorcycle">{t.ekabMotorcycle}</option>
          <option value="EKAB Helicopter">{t.ekabHelicopter}</option>
          <option value="EKAB Command Vehicle">{t.ekabCommandVehicle}</option>
          <option value="Mobile ICU">{t.ekabMobileICU}</option>
          <option value="EKAB Rescue Vehicle">{t.ekabRescueVehicle}</option>
          <option value="EKAB Supply Vehicle">{t.ekabSupplyVehicle}</option>
        </optgroup>
      )
    }

    // Fallback - show all if agency not recognized
    return (
      <>
        <optgroup label="Fire Department">
          <option value="Fire Engine">{t.fireEngine}</option>
          <option value="Ladder">{t.ladder}</option>
          <option value="Rescue Vehicle">{t.rescueVehicle}</option>
          <option value="Command">{t.command}</option>
          <option value="Tanker">{t.tanker}</option>
          <option value="Fire Boat">{t.fireBoat}</option>
          <option value="Hazmat Truck">{t.hazmatTruck}</option>
          <option value="Support">{t.support}</option>
          <option value="Foodtruck">{t.foodtruck}</option>
          <option value="FC Bus">{t.fcbus}</option>
          <option value="Petrol Truck">{t.petroltruck}</option>
        </optgroup>
        <optgroup label="Coast Guard">
          <option value="Patrol Boat">{t.patrolBoat}</option>
          <option value="Offshore Patrol Vessel">{t.offshorePatrolVessel}</option>
          <option value="Search Rescue Boat">{t.searchRescueBoat}</option>
          <option value="Rigid Inflatable">{t.rigidInflatable}</option>
          <option value="Pollution Control">{t.pollutionControl}</option>
          <option value="CG Helicopter">{t.cghelicopter}</option>
          <option value="CG Airplane">{t.cgairplane}</option>
          <option value="Patrol Vehicle">{t.patrolVehicle}</option>
          <option value="CG Bus">{t.cgbus}</option>
        </optgroup>
        <optgroup label="Police">
          <option value="Police Patrol Car">{t.policePatrolCar}</option>
          <option value="Police Motorcycle">{t.policeMotorcycle}</option>
          <option value="Police Van">{t.policeVan}</option>
          <option value="Police Bus">{t.policeBus}</option>
          <option value="Police Helicopter">{t.policeHelicopter}</option>
          <option value="Police Boat">{t.policeBoat}</option>
          <option value="Police Command Vehicle">{t.policeCommandVehicle}</option>
          <option value="Special Operations Vehicle">{t.policeSpecialOperations}</option>
          <option value="Traffic Enforcement Vehicle">{t.policeTrafficEnforcement}</option>
          <option value="K-9 Unit Vehicle">{t.policeK9Unit}</option>
          <option value="Bomb Squad Vehicle">{t.policeBombSquad}</option>
          <option value="Forensics Vehicle">{t.policeForensics}</option>
        </optgroup>
        <optgroup label="EKAB">
          <option value="Basic Ambulance">{t.basicAmbulance}</option>
          <option value="Advanced Ambulance">{t.advancedAmbulance}</option>
          <option value="Intensive Care Ambulance">{t.intensiveCareAmbulance}</option>
          <option value="Neonatal Ambulance">{t.neonatalAmbulance}</option>
          <option value="EKAB Motorcycle">{t.ekabMotorcycle}</option>
          <option value="EKAB Helicopter">{t.ekabHelicopter}</option>
          <option value="EKAB Command Vehicle">{t.ekabCommandVehicle}</option>
          <option value="Mobile ICU">{t.ekabMobileICU}</option>
          <option value="EKAB Rescue Vehicle">{t.ekabRescueVehicle}</option>
          <option value="EKAB Supply Vehicle">{t.ekabSupplyVehicle}</option>
        </optgroup>
      </>
    )
  }
  const [statusFilter, setStatusFilter] = useState<VehicleStatus | ''>('')
  // For members, automatically set to their station and don't allow changes
  const [stationFilter, setStationFilter] = useState<number | undefined>(
    !isDispatcher() && user?.stationId ? user.stationId : undefined
  )
  const [showAddModal, setShowAddModal] = useState(false)
  const [editingVehicle, setEditingVehicle] = useState<Vehicle | null>(null)
  const [successMessage, setSuccessMessage] = useState<string | null>(null)
  const [formData, setFormData] = useState({
    callsign: '',
    type: '',
    plateNumber: '',
    stationId: 1,
    waterCapacityLiters: 0
  })

  const {
    data: vehicles,
    isLoading,
    error: vehiclesError,
    refetch: refetchVehicles
  } = useQuery({
    queryKey: ['vehicles', statusFilter, stationFilter],
    queryFn: () => vehiclesApi.getAll({
      status: statusFilter === '' ? undefined : statusFilter,
      stationId: stationFilter
    }).then(res => res.data),
    retry: (failureCount, error) => {
      if (error instanceof ApiError && error.status && error.status < 500) {
        return false
      }
      return failureCount < 2
    },
    staleTime: 30000, // Consider data stale after 30 seconds
  })

  const createVehicleMutation = useMutation({
    mutationFn: (data: typeof formData) => vehiclesApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['vehicles'] })
      setShowAddModal(false)
      setFormData({ callsign: '', type: '', plateNumber: '', stationId: 1, waterCapacityLiters: 0 })
      setSuccessMessage(t.vehicleCreatedSuccessfully)
      setTimeout(() => setSuccessMessage(null), 3000)
    },
    onError: (error) => {
      console.error('Failed to create vehicle:', error)
    }
  })

  const updateVehicleMutation = useMutation({
    mutationFn: ({ id, data }: { id: number; data: UpdateVehicleDto }) =>
      vehiclesApi.updateVehicle(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['vehicles'] })
      setEditingVehicle(null)
      setSuccessMessage(t.vehicleUpdatedSuccessfully)
      setTimeout(() => setSuccessMessage(null), 3000)
    },
    onError: (error) => {
      console.error('Failed to update vehicle:', error)
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
  const groupedVehicles = useMemo(() => {
    if (!vehicles) return {}
    if (stationFilter) return { [stationFilter]: vehicles }

    return vehicles.reduce((acc, vehicle) => {
      const stationId = vehicle.stationId
      if (!acc[stationId]) acc[stationId] = []
      acc[stationId].push(vehicle)
      return acc
    }, {} as Record<number, typeof vehicles>)
  }, [vehicles, stationFilter])

  const handleEditVehicle = (vehicle: Vehicle) => {
    const { canEditVehicle } = useUserStore.getState()
    if (canEditVehicle(vehicle.stationId)) {
      setEditingVehicle(vehicle)
    }
  }

  const handleSaveVehicle = (data: UpdateVehicleDto) => {
    if (editingVehicle) {
      updateVehicleMutation.mutate({ id: editingVehicle.id, data })
    }
  }

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-64">
        <LoadingSpinner size="lg" text={t.loadingData} />
      </div>
    )
  }

  return (
    <ErrorBoundary>
      <div>
        <div className="flex items-center justify-between mb-8">
          <div>
            <h1 className="text-3xl font-bold text-gray-900 dark:text-gray-100">{t.vehicles}</h1>
            <p className="mt-2 text-gray-600 dark:text-gray-400">
              {t.monitorVehicleStatus}
            </p>
          </div>
          {canManageStation() && (
            <button
              onClick={() => setShowAddModal(true)}
              className="btn btn-primary flex items-center"
            >
              <Plus className="w-4 h-4 mr-2" />
              {t.addVehicle}
            </button>
          )}
        </div>

        {/* Success Message */}
        {successMessage && (
          <div className="mb-4 p-3 bg-green-50 dark:bg-green-900/20 border border-green-200 dark:border-green-800 rounded-md flex items-center">
            <CheckCircle className="w-4 h-4 text-green-500 dark:text-green-400 mr-2" />
            <span className="text-green-700 dark:text-green-300 text-sm">{successMessage}</span>
          </div>
        )}

        {/* Error Message */}
        {vehiclesError && (
          <div className="mb-4 p-4 bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-lg">
            <div className="flex items-center justify-between">
              <div className="flex items-center">
                <AlertTriangle className="w-5 h-5 text-red-500 mr-3" />
                <div>
                  <h3 className="text-sm font-medium text-red-800 dark:text-red-200">{t.failedToLoadVehicles}</h3>
                  <p className="text-sm text-red-700 dark:text-red-300 mt-1">{vehiclesError.message}</p>
                </div>
              </div>
              <RetryButton onRetry={() => refetchVehicles()} />
            </div>
          </div>
        )}

        {/* Enhanced Filters */}
        <div className="card p-4 mb-6">
          <div className="flex items-center space-x-6 flex-wrap gap-y-2">
            {/* Status Filter */}
            <div className="flex items-center space-x-4">
              <Filter className="w-5 h-5 text-gray-400 dark:text-gray-500" />
              <select
                value={statusFilter}
                onChange={(e) => setStatusFilter(e.target.value as VehicleStatus | '')}
                className="input"
              >
                <option value="">{t.allStatuses}</option>
                <option value={VehicleStatus.Available}>{t.available}</option>
                <option value={VehicleStatus.Notified}>{t.notified}</option>
                <option value={VehicleStatus.EnRoute}>{t.enRoute}</option>
                <option value={VehicleStatus.OnScene}>{t.onScene}</option>
                <option value={VehicleStatus.Busy}>{t.busy}</option>
                <option value={VehicleStatus.Maintenance}>{t.maintenance}</option>
                <option value={VehicleStatus.Offline}>{t.offline}</option>
              </select>
            </div>

            {/* Station Filter - Only show for dispatchers */}
            {isDispatcher() && (
              <StationFilter
                selectedStationId={stationFilter}
                onStationChange={setStationFilter}
                placeholder={t.allStations}
              />
            )}

            {/* Clear Filters */}
            {(statusFilter || (isDispatcher() && stationFilter)) && (
              <button
                onClick={() => {
                  setStatusFilter('')
                  if (isDispatcher()) {
                    setStationFilter(undefined)
                  }
                }}
                className="btn btn-secondary text-sm"
              >
                {t.clearFilters}
              </button>
            )}
          </div>
        </div>

        {/* Vehicles Display - Grouped by Station */}
        {Object.entries(groupedVehicles).map(([stationId, stationVehicles]) => (
          <div key={stationId} className="mb-8">
            <h2 className="text-xl font-semibold text-gray-900 dark:text-gray-100 mb-4">
              {getStationName(parseInt(stationId))} ({stationVehicles.length} {stationVehicles.length === 1 ? t.vehicleCount : t.vehiclesCount})
            </h2>

            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
              {stationVehicles.map((vehicle) => (
                <div key={vehicle.id} className="card p-6">
                  <div className="flex items-start justify-between mb-4">
                    <div className="flex items-center">
                      <div className="p-2 bg-blue-100 dark:bg-blue-900/50 rounded-lg">
                        <Truck className="w-6 h-6 text-blue-600 dark:text-blue-400" />
                      </div>
                      <div className="ml-3">
                        <h3 className="text-lg font-semibold text-gray-900 dark:text-gray-100">
                          {vehicle.callsign}
                        </h3>
                        <p className="text-sm text-gray-600 dark:text-gray-400">{vehicle.type}</p>
                      </div>
                    </div>
                    <div className="flex items-center space-x-2">
                      {canEditVehicle(vehicle.stationId) && (
                        <button
                          onClick={() => handleEditVehicle(vehicle)}
                          className="p-1 text-gray-400 dark:text-gray-500 hover:text-blue-600 dark:hover:text-blue-400 transition-colors"
                          title={t.editVehicleTitle}
                        >
                          <Edit2 className="w-4 h-4" />
                        </button>
                      )}
                      <span className={`px-2 py-1 text-xs font-medium rounded-full ${getStatusColor(vehicle.status)}`}>
                        {getVehicleStatusTranslation(vehicle.status, t)}
                      </span>
                    </div>
                  </div>

                  <div className="space-y-3">
                    <div className="flex justify-between text-sm">
                      <span className="text-gray-600 dark:text-gray-400">{t.plateNumber}</span>
                      <span className="font-medium text-gray-900 dark:text-gray-100">{vehicle.plateNumber}</span>
                    </div>

                    {/* Telemetry */}
                    <div className="pt-3 border-t border-gray-200 dark:border-gray-700">
                      <h4 className="text-sm font-medium text-gray-900 dark:text-gray-100 mb-3">{t.telemetry}</h4>
                      <div className="grid grid-cols-2 gap-3">
                        {/* Fuel Level */}
                        {vehicle.fuelLevelPercent !== null && (
                          <div className="flex items-center">
                            <Fuel className="w-4 h-4 text-gray-400 dark:text-gray-500 mr-2" />
                            <div className="flex-1">
                              <div className="flex justify-between text-xs mb-1">
                                <span className="text-gray-600 dark:text-gray-400">{t.fuel}</span>
                                <span className="text-gray-900 dark:text-gray-100">{vehicle.fuelLevelPercent}%</span>
                              </div>
                              <div className="w-full bg-gray-200 dark:bg-gray-700 rounded-full h-2">
                                <div
                                  className={`h-2 rounded-full ${(vehicle.fuelLevelPercent ?? 0) > 50
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
                                <span className="text-gray-600 dark:text-gray-400">{t.water}</span>
                                <span className="text-gray-900 dark:text-gray-100">{getWaterPercentage(vehicle)}%</span>
                              </div>
                              <div className="w-full bg-gray-200 dark:bg-gray-700 rounded-full h-2">
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
                            <Battery className="w-4 h-4 text-gray-400 dark:text-gray-500 mr-2" />
                            <div className="flex-1">
                              <div className="text-xs text-gray-600 dark:text-gray-400">
                                <span>{t.battery}: {vehicle.batteryVoltage}V</span>
                              </div>
                            </div>
                          </div>
                        )}

                        {/* Pump Pressure */}
                        {vehicle.pumpPressureKPa !== null && (
                          <div className="flex items-center">
                            <Gauge className="w-4 h-4 text-gray-400 dark:text-gray-500 mr-2" />
                            <div className="flex-1">
                              <div className="text-xs text-gray-600 dark:text-gray-400">
                                <span>{t.pump}: {vehicle.pumpPressureKPa} kPa</span>
                              </div>
                            </div>
                          </div>
                        )}
                      </div>
                    </div>

                    {vehicle.lastTelemetryAt && (
                      <div className="text-xs text-gray-500 dark:text-gray-400 pt-2 border-t border-gray-200 dark:border-gray-700">
                        {t.lastUpdate}: {new Date(vehicle.lastTelemetryAt).toLocaleString()}
                      </div>
                    )}
                  </div>
                </div>
              ))}
            </div>

            {stationVehicles.length === 0 && (
              <div className="text-center py-8 text-gray-500 dark:text-gray-400">
                {t.noVehiclesForStation} {getStationName(parseInt(stationId))}
              </div>
            )}
          </div>
        ))}

        {Object.keys(groupedVehicles).length === 0 && (
          <div className="text-center py-12">
            <Truck className="w-12 h-12 text-gray-400 dark:text-gray-500 mx-auto mb-4" />
            <h3 className="text-lg font-medium text-gray-900 dark:text-gray-100 mb-2">{t.noVehiclesFound}</h3>
            <p className="text-gray-600 dark:text-gray-400">
              {statusFilter || stationFilter ? t.tryAdjustingFilters : t.noVehiclesRegistered}
            </p>
          </div>
        )}

        {/* Add Vehicle Modal */}
        {showAddModal && (
          <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
            <div className="bg-white dark:bg-gray-800 rounded-lg p-6 w-full max-w-md border border-gray-200 dark:border-gray-700">
              <div className="flex items-center justify-between mb-4">
                <h3 className="text-lg font-semibold text-gray-900 dark:text-gray-100">{t.addVehicle}</h3>
                <button
                  onClick={() => setShowAddModal(false)}
                  className="text-gray-400 dark:text-gray-500 hover:text-gray-600 dark:hover:text-gray-300"
                >
                  <X className="w-5 h-5" />
                </button>
              </div>
              <form onSubmit={(e) => {
                e.preventDefault()
                if (!formData.callsign.trim() || !formData.type.trim() || !formData.plateNumber.trim()) {
                  alert(t.fillRequiredFields)
                  return
                }
                createVehicleMutation.mutate(formData)
              }} className="space-y-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                    {t.callsign} *
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
                    {t.type} *
                  </label>
                  <select
                    value={formData.type}
                    onChange={(e) => setFormData(prev => ({ ...prev, type: e.target.value }))}
                    className="input w-full"
                    required
                  >
                    <option value="">{t.selectVehicleType}</option>
                    {renderVehicleTypeOptions()}
                  </select>
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                    {t.plateNumber} *
                  </label>
                  <input
                    type="text"
                    value={formData.plateNumber}
                    onChange={(e) => setFormData(prev => ({ ...prev, plateNumber: e.target.value }))}
                    className="input w-full"
                    placeholder={t.licensePlateNumber}
                    required
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                    {t.station}
                  </label>
                  {isDispatcher() ? (
                    <StationFilter
                      selectedStationId={formData.stationId}
                      onStationChange={(stationId) => setFormData(prev => ({ ...prev, stationId: stationId || 1 }))}
                      placeholder={t.selectStation}
                      required
                    />
                  ) : (
                    <input
                      type="text"
                      value={user?.stationName || getStationName(user?.stationId || 0)}
                      className="input w-full bg-gray-100 dark:bg-gray-700"
                      disabled
                    />
                  )}
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                    {t.waterCapacityLiters}
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
                    {t.cancel}
                  </button>
                  <button
                    type="submit"
                    disabled={createVehicleMutation.isPending}
                    className="btn btn-primary flex items-center disabled:opacity-50 disabled:cursor-not-allowed"
                  >
                    {createVehicleMutation.isPending ? (
                      <LoadingSpinner size="sm" className="mr-2" />
                    ) : (
                      <Save className="w-4 h-4 mr-2" />
                    )}
                    {createVehicleMutation.isPending ? t.addingVehicle : t.addVehicle}
                  </button>
                </div>
              </form>
            </div>
          </div>
        )}

        {/* Vehicle Edit Modal */}
        {editingVehicle && (
          <VehicleEditModal
            vehicle={editingVehicle}
            isOpen={!!editingVehicle}
            onClose={() => setEditingVehicle(null)}
            onSave={handleSaveVehicle}
            isLoading={updateVehicleMutation.isPending}
            error={updateVehicleMutation.error?.message || null}
          />
        )}

        {/* Add Vehicle Modal Error */}
        {createVehicleMutation.error && showAddModal && (
          <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
            <div className="bg-white dark:bg-gray-800 rounded-lg p-6 w-full max-w-md border border-gray-200 dark:border-gray-700">
              <div className="flex items-center justify-between mb-4">
                <h3 className="text-lg font-semibold text-red-800 dark:text-red-200">{t.errorCreatingVehicle}</h3>
                <button
                  onClick={() => createVehicleMutation.reset()}
                  className="text-gray-400 dark:text-gray-500 hover:text-gray-600 dark:hover:text-gray-300"
                >
                  <X className="w-5 h-5" />
                </button>
              </div>
              <div className="flex items-center mb-4">
                <AlertTriangle className="w-5 h-5 text-red-500 mr-3" />
                <p className="text-red-700 dark:text-red-300">{createVehicleMutation.error.message}</p>
              </div>
              <div className="flex justify-end space-x-2">
                <button
                  onClick={() => createVehicleMutation.reset()}
                  className="btn btn-secondary"
                >
                  {t.close}
                </button>
                <RetryButton
                  onRetry={() => {
                    createVehicleMutation.reset()
                    createVehicleMutation.mutate(formData)
                  }}
                />
              </div>
            </div>
          </div>
        )}
      </div>
    </ErrorBoundary>
  )
}