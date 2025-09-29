import { useState, useEffect } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { personnelApi, vehiclesApi, vehicleAssignmentsApi, fireStationsApi } from '../lib/api'
import { useUserStore } from '../lib/userStore'
import { useTranslation } from '../hooks/useTranslation'
import { useSignalR } from '../hooks/useSignalR'
import { Users, Truck, Plus, X, UserPlus, Settings } from 'lucide-react'
import { RankSelect } from '../components/RankSelect'
import { translateRank } from '../utils/rankUtils'
import { FireStation } from '../types'

export default function StationManagement() {
  const { user, canManageStation } = useUserStore()
  const queryClient = useQueryClient()
  const t = useTranslation()
  const signalR = useSignalR()
  
  const [showAddPersonnelModal, setShowAddPersonnelModal] = useState(false)
  const [showAddVehicleModal, setShowAddVehicleModal] = useState(false)
  const [showAssignVehicleModal, setShowAssignVehicleModal] = useState(false)
  const [selectedVehicle, setSelectedVehicle] = useState<number | null>(null)
  
  const [newPersonnel, setNewPersonnel] = useState({
    name: '',
    rank: '',
    badgeNumber: '',
  })
  
  const [newVehicle, setNewVehicle] = useState({
    callsign: '',
    type: 'FireTruck',
    plateNumber: '',
    waterCapacityLiters: 0,
  })

  const { data: personnel } = useQuery({
    queryKey: ['personnel', user?.stationId],
    queryFn: () => personnelApi.getAll({ stationId: user?.stationId }).then(res => res.data),
    enabled: !!user?.stationId,
  })

  const { data: vehicles } = useQuery({
    queryKey: ['vehicles', user?.stationId],
    queryFn: () => vehiclesApi.getAll({ stationId: user?.stationId }).then(res => res.data),
    enabled: !!user?.stationId,
  })

  const { data: vehicleAssignments } = useQuery({
    queryKey: ['vehicle-assignments', user?.stationId],
    queryFn: () => vehicleAssignmentsApi.getAll({ stationId: user?.stationId }).then(res => res.data),
    enabled: !!user?.stationId,
  })

  const { data: stations = [] } = useQuery({
    queryKey: ['stations'],
    queryFn: () => fireStationsApi.getStations(),
  })

  const getStationName = (stationId: number) => {
    const station = stations.find((s: FireStation) => s.id === stationId)
    return station?.name || `Station ${stationId}`
  }

  // Set up SignalR event handlers for real-time updates
  useEffect(() => {
    if (!signalR || !user?.stationId) return

    // Personnel real-time updates
    const cleanupPersonnelCreated = signalR.addPersonnelCreatedHandler(() => {
      queryClient.invalidateQueries({ queryKey: ['personnel', user.stationId] })
    })

    const cleanupPersonnelUpdated = signalR.addPersonnelUpdatedHandler(() => {
      queryClient.invalidateQueries({ queryKey: ['personnel', user.stationId] })
    })

    const cleanupPersonnelDeleted = signalR.addPersonnelDeletedHandler(() => {
      queryClient.invalidateQueries({ queryKey: ['personnel', user.stationId] })
    })

    // Vehicle real-time updates
    const cleanupVehicleCreated = signalR.addVehicleCreatedHandler(() => {
      queryClient.invalidateQueries({ queryKey: ['vehicles', user.stationId] })
    })

    const cleanupVehicleUpdated = signalR.addVehicleUpdatedHandler(() => {
      queryClient.invalidateQueries({ queryKey: ['vehicles', user.stationId] })
    })

    const cleanupVehicleDeleted = signalR.addVehicleDeletedHandler(() => {
      queryClient.invalidateQueries({ queryKey: ['vehicles', user.stationId] })
    })

    // Vehicle assignment changes
    const cleanupVehicleAssignmentChanged = signalR.addVehicleAssignmentChangedHandler(() => {
      queryClient.invalidateQueries({ queryKey: ['vehicle-assignments', user.stationId] })
    })

    return () => {
      cleanupPersonnelCreated()
      cleanupPersonnelUpdated()
      cleanupPersonnelDeleted()
      cleanupVehicleCreated()
      cleanupVehicleUpdated()
      cleanupVehicleDeleted()
      cleanupVehicleAssignmentChanged()
    }
  }, [signalR, queryClient, user?.stationId])

  const createPersonnelMutation = useMutation({
    mutationFn: (data: typeof newPersonnel) => 
      personnelApi.create({ 
        ...data, 
        stationId: user!.stationId!, 
        agencyId: user!.agencyId! 
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['personnel'] })
      setShowAddPersonnelModal(false)
      setNewPersonnel({ name: '', rank: '', badgeNumber: '' })
    }
  })

  const createVehicleMutation = useMutation({
    mutationFn: (data: typeof newVehicle) => 
      vehiclesApi.create({ ...data, stationId: user!.stationId! }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['vehicles'] })
      setShowAddVehicleModal(false)
      setNewVehicle({ callsign: '', type: 'FireTruck', plateNumber: '', waterCapacityLiters: 0 })
    }
  })

  const assignVehicleMutation = useMutation({
    mutationFn: ({ vehicleId, personnelId }: { vehicleId: number; personnelId: number }) =>
      vehicleAssignmentsApi.create({ vehicleId, personnelId }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['vehicle-assignments'] })
      setShowAssignVehicleModal(false)
      setSelectedVehicle(null)
    }
  })

  const removeAssignmentMutation = useMutation({
    mutationFn: (assignmentId: number) => vehicleAssignmentsApi.remove(assignmentId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['vehicle-assignments'] })
    }
  })

  if (!canManageStation()) {
    return (
      <div className="text-center py-12">
        <h3 className="text-lg font-medium text-gray-900 dark:text-gray-100 mb-2">{t.accessDenied}</h3>
        <p className="text-gray-600 dark:text-gray-400">{t.youDontHavePermission}</p>
      </div>
    )
  }

  return (
    <div>
      <div className="mb-8">
        <h1 className="text-3xl font-bold text-gray-900 dark:text-gray-100">{t.stationManagementTitle}</h1>
        <p className="mt-2 text-gray-600 dark:text-gray-400">
          {t.managePersonnelAndVehicles} <strong>{getStationName(user?.stationId || 0)}</strong>
        </p>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
        {/* Personnel Management */}
        <div className="card">
          <div className="p-6 border-b border-gray-200 dark:border-gray-700 flex items-center justify-between">
            <h2 className="text-lg font-semibold text-gray-900 dark:text-gray-100 flex items-center">
              <Users className="w-5 h-5 mr-2" />
              {t.personnel} ({personnel?.length || 0})
            </h2>
            <button
              onClick={() => setShowAddPersonnelModal(true)}
              className="btn btn-primary flex items-center text-sm"
            >
              <UserPlus className="w-4 h-4 mr-1" />
              {t.addPersonnel}
            </button>
          </div>
          <div className="p-6">
            {personnel?.length === 0 ? (
              <p className="text-gray-500 dark:text-gray-400 text-center py-4">{t.noPersonnelFound}</p>
            ) : (
              <div className="space-y-3">
                {personnel?.map((person) => (
                  <div key={person.id} className="flex items-center justify-between p-3 bg-gray-50 dark:bg-gray-700 rounded-lg">
                    <div>
                      <p className="font-medium text-gray-900 dark:text-gray-100">{person.name}</p>
                      <p className="text-sm text-gray-600 dark:text-gray-400">{translateRank(person.rank, t)}</p>
                      {person.badgeNumber && (
                        <p className="text-xs text-gray-500 dark:text-gray-500">{t.badgeNumber}: {person.badgeNumber}</p>
                      )}
                    </div>
                    <span className={`px-2 py-1 text-xs font-medium rounded-full ${
                      person.isActive ? 'bg-green-100 dark:bg-green-900/50 text-green-800 dark:text-green-200' : 'bg-gray-100 dark:bg-gray-600 text-gray-800 dark:text-gray-200'
                    }`}>
                      {person.isActive ? t.active : t.inactive}
                    </span>
                  </div>
                ))}
              </div>
            )}
          </div>
        </div>

        {/* Vehicle Management */}
        <div className="card">
          <div className="p-6 border-b border-gray-200 dark:border-gray-700 flex items-center justify-between">
            <h2 className="text-lg font-semibold text-gray-900 dark:text-gray-100 flex items-center">
              <Truck className="w-5 h-5 mr-2" />
              {t.vehicles} ({vehicles?.length || 0})
            </h2>
            <button
              onClick={() => setShowAddVehicleModal(true)}
              className="btn btn-primary flex items-center text-sm"
            >
              <Plus className="w-4 h-4 mr-1" />
              {t.addVehicle}
            </button>
          </div>
          <div className="p-6">
            {vehicles?.length === 0 ? (
              <p className="text-gray-500 dark:text-gray-400 text-center py-4">{t.noVehiclesFound}</p>
            ) : (
              <div className="space-y-3">
                {vehicles?.map((vehicle) => (
                  <div key={vehicle.id} className="p-3 bg-gray-50 dark:bg-gray-700 rounded-lg">
                    <div className="flex items-center justify-between mb-2">
                      <div>
                        <p className="font-medium text-gray-900 dark:text-gray-100">{vehicle.callsign}</p>
                        <p className="text-sm text-gray-600 dark:text-gray-400">{vehicle.type}</p>
                      </div>
                      <button
                        onClick={() => {
                          setSelectedVehicle(vehicle.id)
                          setShowAssignVehicleModal(true)
                        }}
                        className="btn btn-secondary text-xs flex items-center"
                      >
                        <Settings className="w-3 h-3 mr-1" />
                        {t.assign}
                      </button>
                    </div>
                    
                    {/* Current assignments */}
                    <div className="mt-2">
                      <p className="text-xs font-medium text-gray-700 dark:text-gray-300 mb-1">{t.assignedPersonnel}:</p>
                      {vehicleAssignments?.filter(va => va.vehicleId === vehicle.id && va.isActive).length === 0 ? (
                        <p className="text-xs text-gray-500 dark:text-gray-400">{t.noPersonnelAssigned}</p>
                      ) : (
                        <div className="space-y-1">
                          {vehicleAssignments
                            ?.filter(va => va.vehicleId === vehicle.id && va.isActive)
                            .map((assignment) => (
                              <div key={assignment.id} className="flex items-center justify-between text-xs">
                                <span>{assignment.personnelName} ({translateRank(assignment.personnelRank, t)})</span>
                                <button
                                  onClick={() => removeAssignmentMutation.mutate(assignment.id)}
                                  className="text-red-600 hover:text-red-800 dark:text-red-400 dark:hover:text-red-300"
                                >
                                  <X className="w-3 h-3" />
                                </button>
                              </div>
                            ))}
                        </div>
                      )}
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>
        </div>
      </div>

      {/* Add Personnel Modal */}
      {showAddPersonnelModal && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white dark:bg-gray-800 rounded-lg p-6 w-full max-w-md border border-gray-200 dark:border-gray-700">
            <div className="flex items-center justify-between mb-4">
              <h3 className="text-lg font-semibold text-gray-900 dark:text-gray-100">{t.addPersonnel}</h3>
              <button 
                onClick={() => setShowAddPersonnelModal(false)}
                className="text-gray-400 hover:text-gray-600 dark:text-gray-500 dark:hover:text-gray-300"
              >
                <X className="w-5 h-5" />
              </button>
            </div>
            <form onSubmit={(e) => {
              e.preventDefault()
              createPersonnelMutation.mutate(newPersonnel)
            }}>
              <div className="space-y-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">{t.name} *</label>
                  <input
                    type="text"
                    value={newPersonnel.name}
                    onChange={(e) => setNewPersonnel(prev => ({ ...prev, name: e.target.value }))}
                    className="input w-full"
                    required
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">{t.rank} *</label>
                  <RankSelect
                    value={newPersonnel.rank}
                    onChange={(rank) => setNewPersonnel(prev => ({ ...prev, rank }))}
                    agencyName={user?.agencyName}
                    required
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">{t.badgeNumber}</label>
                  <input
                    type="text"
                    value={newPersonnel.badgeNumber}
                    onChange={(e) => setNewPersonnel(prev => ({ ...prev, badgeNumber: e.target.value }))}
                    className="input w-full"
                  />
                </div>
              </div>
              <div className="flex justify-end space-x-2 mt-6">
                <button
                  type="button"
                  onClick={() => setShowAddPersonnelModal(false)}
                  className="btn btn-secondary"
                >
                  {t.cancel}
                </button>
                <button
                  type="submit"
                  disabled={createPersonnelMutation.isPending}
                  className="btn btn-primary"
                >
                  {createPersonnelMutation.isPending ? t.addingVehicle : t.addPersonnel}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Add Vehicle Modal */}
      {showAddVehicleModal && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white dark:bg-gray-800 rounded-lg p-6 w-full max-w-md border border-gray-200 dark:border-gray-700">
            <div className="flex items-center justify-between mb-4">
              <h3 className="text-lg font-semibold text-gray-900 dark:text-gray-100">{t.addVehicle}</h3>
              <button 
                onClick={() => setShowAddVehicleModal(false)}
                className="text-gray-400 hover:text-gray-600 dark:text-gray-500 dark:hover:text-gray-300"
              >
                <X className="w-5 h-5" />
              </button>
            </div>
            <form onSubmit={(e) => {
              e.preventDefault()
              createVehicleMutation.mutate(newVehicle)
            }}>
              <div className="space-y-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">{t.callsign} *</label>
                  <input
                    type="text"
                    value={newVehicle.callsign}
                    onChange={(e) => setNewVehicle(prev => ({ ...prev, callsign: e.target.value }))}
                    className="input w-full"
                    placeholder="e.g., Engine 1"
                    required
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">{t.type} *</label>
                  <select
                    value={newVehicle.type}
                    onChange={(e) => setNewVehicle(prev => ({ ...prev, type: e.target.value }))}
                    className="input w-full"
                    required
                  >
                      <option value="">{t.selectVehicleType}</option>
                      <option value="Fire Engine">{t.fireEngine}</option>
                      <option value="Ladder">{t.ladder}</option>
                      <option value="Rescue Vehicle">{t.rescueVehicle}</option>
                      <option value="Ambulance">{t.ambulance}</option>
                      <option value="Command">{t.command}</option>
                      <option value="Tanker">{t.tanker}</option>
                      <option value="Fire Boat">{t.fireBoat}</option>
                      <option value="Hazmat Truck">{t.hazmatTruck}</option>
                      <option value="Support">{t.support}</option>
                      <option value="Foodtruck">{t.foodtruck}</option>
                      <option value="FC Bus">{t.fcbus}</option>
                      <option value="Petrol Truck">{t.petroltruck}</option> 
                  </select>
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">{t.plateNumber}</label>
                  <input
                    type="text"
                    value={newVehicle.plateNumber}
                    onChange={(e) => setNewVehicle(prev => ({ ...prev, plateNumber: e.target.value }))}
                    className="input w-full"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">{t.waterCapacityLiters}</label>
                  <input
                    type="number"
                    value={newVehicle.waterCapacityLiters}
                    onChange={(e) => setNewVehicle(prev => ({ ...prev, waterCapacityLiters: parseInt(e.target.value) || 0 }))}
                    className="input w-full"
                  />
                </div>
              </div>
              <div className="flex justify-end space-x-2 mt-6">
                <button
                  type="button"
                  onClick={() => setShowAddVehicleModal(false)}
                  className="btn btn-secondary"
                >
                  {t.cancel}
                </button>
                <button
                  type="submit"
                  disabled={createVehicleMutation.isPending}
                  className="btn btn-primary"
                >
                  {createVehicleMutation.isPending ? t.addingVehicle : t.addVehicle}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Assign Vehicle Modal */}
      {showAssignVehicleModal && selectedVehicle && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white dark:bg-gray-800 rounded-lg p-6 w-full max-w-md border border-gray-200 dark:border-gray-700">
            <div className="flex items-center justify-between mb-4">
              <h3 className="text-lg font-semibold text-gray-900 dark:text-gray-100">{t.assignPersonnelToVehicle}</h3>
              <button 
                onClick={() => setShowAssignVehicleModal(false)}
                className="text-gray-400 hover:text-gray-600 dark:text-gray-500 dark:hover:text-gray-300"
              >
                <X className="w-5 h-5" />
              </button>
            </div>
            <div className="space-y-3">
              <p className="text-sm text-gray-600 dark:text-gray-400 mb-4">
                {t.selectPersonnelToAssign} {vehicles?.find(v => v.id === selectedVehicle)?.callsign}:
              </p>
              {personnel?.filter(p => p.isActive).map((person) => {
                const isAssigned = vehicleAssignments?.some(va => 
                  va.personnelId === person.id && va.isActive
                )
                return (
                  <button
                    key={person.id}
                    onClick={() => assignVehicleMutation.mutate({
                      vehicleId: selectedVehicle,
                      personnelId: person.id
                    })}
                    disabled={isAssigned || assignVehicleMutation.isPending}
                    className={`w-full text-left p-3 border rounded-lg transition-colors ${
                      isAssigned 
                        ? 'bg-gray-100 dark:bg-gray-600 text-gray-500 dark:text-gray-400 cursor-not-allowed'
                        : 'hover:bg-gray-50 dark:hover:bg-gray-700 border-gray-300 dark:border-gray-600 text-gray-900 dark:text-gray-100'
                    }`}
                  >
                    <p className="font-medium">{person.name}</p>
                    <p className="text-sm text-gray-600 dark:text-gray-400">{translateRank(person.rank, t)}</p>
                    {isAssigned && (
                      <p className="text-xs text-gray-500 dark:text-gray-400">{t.alreadyAssignedToVehicle}</p>
                    )}
                  </button>
                )
              })}
              <div className="flex justify-end mt-4">
                <button
                  onClick={() => setShowAssignVehicleModal(false)}
                  className="btn btn-secondary"
                >
                  {t.close}
                </button>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}