import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { Polygon, Popup } from 'react-leaflet'
import { patrolZonesApi } from '../lib/api'
import { PatrolZone } from '../types'
import { useTranslation } from '../hooks/useTranslation'
import { useUserStore } from '../lib/userStore'
import { useToast } from './ToastContainer'
import { Shield, Users, Clock, Edit, Plus, Minus, Trash2 } from 'lucide-react'

interface PatrolZoneLayerProps {
  onPatrolZoneClick?: (zone: PatrolZone) => void
  onAssignVehicle?: (zone: PatrolZone) => void
}

export default function PatrolZoneLayer({ onPatrolZoneClick, onAssignVehicle }: PatrolZoneLayerProps) {
  const t = useTranslation()
  const { user } = useUserStore()
  const queryClient = useQueryClient()
  const { showSuccess, showError, showConfirmation } = useToast()
  
  const { data: patrolZones, isLoading } = useQuery({
    queryKey: ['patrolZones', user?.stationId],
    queryFn: () => patrolZonesApi.getAll(user?.stationId ? { stationId: user.stationId } : {}).then(res => res.data),
    enabled: !!user, // Enable when user exists, regardless of stationId
    staleTime: 5 * 60 * 1000, // Cache for 5 minutes
  })

  const unassignVehicleMutation = useMutation({
    mutationFn: (assignmentId: number) => {
      return patrolZonesApi.unassignVehicle(assignmentId)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['patrolZones'] })
      queryClient.invalidateQueries({ queryKey: ['vehicles'] })
      showSuccess(t.resourceUnassignedSuccessfully)
    },
    onError: (error: any) => {
      showError('Error unassigning vehicle', error.response?.data?.message)
    }
  })

  const deletePatrolZoneMutation = useMutation({
    mutationFn: (patrolZoneId: number) => {
      return patrolZonesApi.delete(patrolZoneId)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['patrolZones'] })
      showSuccess(t.patrolZoneDeleted)
    },
    onError: (error: any) => {
      showError('Error deleting patrol zone', error.response?.data?.message)
    }
  })

  const handleUnassign = (assignmentId: number, vehicleCallsign: string) => {
    const confirmMessage = `${t.confirmUnassignResource || 'Are you sure you want to unassign'} ${vehicleCallsign}?`
    showConfirmation({
      message: confirmMessage,
      onConfirm: () => unassignVehicleMutation.mutate(assignmentId),
      onCancel: () => {}, // No action needed on cancel
      confirmText: t.unassign || 'Unassign',
      cancelText: t.cancel || 'Cancel',
      type: 'warning'
    })
  }

  const handleDeletePatrolZone = (zone: PatrolZone) => {
    // Check if there are active vehicle assignments
    const activeAssignments = zone.vehicleAssignments?.filter(a => a.isActive).length || 0
    
    let confirmMessage = t.confirmDeletePatrolZone
    if (activeAssignments > 0) {
      confirmMessage += ` This zone has ${activeAssignments} active vehicle assignment(s) that will be removed.`
    }
    
    showConfirmation({
      message: confirmMessage,
      onConfirm: () => deletePatrolZoneMutation.mutate(zone.id),
      onCancel: () => {}, // No action needed on cancel
      confirmText: t.delete || 'Delete',
      cancelText: t.cancel || 'Cancel',
      type: 'danger'
    })
  }

  if (isLoading || !patrolZones) {
    return null
  }

  const getPriorityText = (priority: number) => {
    switch (priority) {
      case 1: return t.highPriorityPatrol
      case 2: return t.mediumPriorityPatrol
      case 3: return t.lowPriorityPatrol
      default: return t.unknownPriority
    }
  }

  const getPriorityColor = (priority: number) => {
    switch (priority) {
      case 1: return '#dc2626' // High - Red
      case 2: return '#d97706' // Medium - Yellow/Orange
      case 3: return '#16a34a' // Low - Green
      default: return '#6b7280' // Unknown - Gray
    }
  }

  return (
    <>
      {patrolZones.map((zone) => {
        try {
          const geoJson = JSON.parse(zone.boundaryCoordinates)
          const coordinates = geoJson.coordinates[0].map(([lng, lat]: [number, number]) => [lat, lng])
          const color = zone.color || getPriorityColor(zone.priority)
          
          return (
            <Polygon
              key={`patrol-zone-${zone.id}`}
              positions={coordinates}
              pathOptions={{
                color: color,
                weight: 2,
                opacity: 0.8,
                fillOpacity: zone.isActive ? 0.2 : 0.1,
                dashArray: zone.isActive ? undefined : '5, 5'
              }}
              eventHandlers={{}}
            >
              <Popup>
                <div className="p-3 min-w-[250px]">
                  <div className="flex items-center justify-between mb-2">
                    <h3 className="font-semibold text-gray-900 dark:text-gray-100 flex items-center">
                      <Shield className="w-4 h-4 mr-2" style={{ color }} />
                      {zone.name}
                    </h3>
                    <span className={`
                      px-2.5 py-1 text-xs font-medium rounded-full border shadow-sm
                      ${zone.isActive 
                        ? 'bg-green-50 dark:bg-green-900/40 text-green-700 dark:text-green-200 border-green-200 dark:border-green-600 shadow-green-100 dark:shadow-green-900/20' 
                        : 'bg-gray-50 dark:bg-gray-800/60 text-gray-600 dark:text-gray-300 border-gray-200 dark:border-gray-600 shadow-gray-100 dark:shadow-gray-800/20'
                      }
                    `}>
                      {zone.isActive ? t.activeStatus : t.inactiveStatus}
                    </span>
                  </div>
                  
                  {zone.description && (
                    <p className="text-sm text-gray-600 dark:text-gray-400 mb-2">{zone.description}</p>
                  )}
                  
                  <div className="space-y-2 text-xs">
                    <div className="flex items-center">
                      <span className="font-medium text-gray-700 dark:text-gray-300">{t.patrolZonePriority}:</span>
                      <span className={`ml-2 px-2 py-0.5 rounded-md text-xs font-medium border shadow-sm`}
                            style={{ 
                              backgroundColor: `${getPriorityColor(zone.priority)}15`, 
                              color: getPriorityColor(zone.priority),
                              borderColor: `${getPriorityColor(zone.priority)}40`
                            }}>
                        {getPriorityText(zone.priority)}
                      </span>
                    </div>
                    
                    <div className="flex items-center">
                      <span className="font-medium text-gray-700 dark:text-gray-300">{t.station}:</span>
                      <span className="ml-2 text-gray-600 dark:text-gray-400">{zone.stationName}</span>
                    </div>
                    
                    <div className="flex items-center">
                      <Users className="w-3 h-3 mr-1 text-gray-500 dark:text-gray-400" />
                      <span className="font-medium text-gray-700 dark:text-gray-300">{t.assignedVehicles}:</span>
                      <span className="ml-2 text-gray-600 dark:text-gray-400 font-medium">
                        {zone.vehicleAssignments?.filter(a => a.isActive).length || 0}
                      </span>
                    </div>
                    
                    <div className="flex items-center">
                      <Clock className="w-3 h-3 mr-1 text-gray-500 dark:text-gray-400" />
                      <span className="font-medium text-gray-700 dark:text-gray-300">{t.createdDate}:</span>
                      <span className="ml-2 text-gray-600 dark:text-gray-400">
                        {new Date(zone.createdAt).toLocaleDateString()}
                      </span>
                    </div>
                    
                    <div className="text-xs text-gray-500 dark:text-gray-400 mt-2 pt-1 border-t border-gray-200 dark:border-gray-600">
                      <span className="font-medium">{t.createdBy}:</span> <span className="text-gray-600 dark:text-gray-400">{zone.createdByUserName}</span>
                    </div>
                  </div>
                  
                  {zone.vehicleAssignments?.filter(a => a.isActive).length > 0 && (
                    <div className="mt-3 pt-2 border-t border-gray-200 dark:border-gray-600">
                      <div className="text-xs font-medium text-gray-700 dark:text-gray-300 mb-2">{t.activeAssignments}:</div>
                      <div className="space-y-2">
                        {zone.vehicleAssignments
                          .filter(a => a.isActive)
                          .map(assignment => (
                            <div key={assignment.id} className="flex items-center justify-between p-2 bg-green-50 dark:bg-green-900/20 border border-green-200 dark:border-green-700 rounded">
                              <div className="text-xs text-gray-700 dark:text-gray-300">
                                <div className="font-medium">{assignment.vehicleCallsign}</div>
                                <div className="text-gray-500 dark:text-gray-400">{assignment.vehicleType}</div>
                              </div>
                              <button
                                onClick={() => handleUnassign(assignment.id, assignment.vehicleCallsign)}
                                disabled={unassignVehicleMutation.isPending}
                                className="p-1 text-rose-600 dark:text-rose-400 hover:text-rose-700 dark:hover:text-rose-300 hover:bg-rose-50 dark:hover:bg-rose-900/20 rounded transition-all duration-200 disabled:opacity-50 border border-transparent hover:border-rose-200 dark:hover:border-rose-700"
                                title={t.unassignVehicle}
                              >
                                <Minus className="w-3 h-3" />
                              </button>
                            </div>
                          ))
                        }
                      </div>
                    </div>
                  )}
                  
                  <div className="mt-3 pt-2 border-t border-gray-200 dark:border-gray-600">
                    <div className="space-y-2">
                      <button
                        onClick={() => onPatrolZoneClick?.(zone)}
                        className="w-full bg-slate-100 dark:bg-slate-700 hover:bg-slate-200 dark:hover:bg-slate-600 text-slate-700 dark:text-slate-200 border border-slate-200 dark:border-slate-600 px-3 py-2 rounded-md text-sm font-medium transition-all duration-200 flex items-center justify-center gap-2 shadow-sm hover:shadow-md"
                      >
                        <Edit className="w-4 h-4" />
                        {t.edit}
                      </button>
                      <button
                        onClick={() => onAssignVehicle?.(zone)}
                        className="w-full bg-emerald-50 dark:bg-emerald-900/30 hover:bg-emerald-100 dark:hover:bg-emerald-900/50 text-emerald-700 dark:text-emerald-300 border border-emerald-200 dark:border-emerald-600 px-3 py-2 rounded-md text-sm font-medium transition-all duration-200 flex items-center justify-center gap-2 shadow-sm hover:shadow-md"
                      >
                        <Plus className="w-4 h-4" />
                        {t.assignVehicle}
                      </button>
                      <button
                        onClick={() => handleDeletePatrolZone(zone)}
                        disabled={deletePatrolZoneMutation.isPending}
                        className="w-full bg-rose-50 dark:bg-rose-900/30 hover:bg-rose-100 dark:hover:bg-rose-900/50 text-rose-700 dark:text-rose-300 border border-rose-200 dark:border-rose-600 px-3 py-2 rounded-md text-sm font-medium transition-all duration-200 flex items-center justify-center gap-2 shadow-sm hover:shadow-md disabled:opacity-50 disabled:cursor-not-allowed"
                      >
                        <Trash2 className="w-4 h-4" />
                        {deletePatrolZoneMutation.isPending ? t.loading : t.deletePatrolZone}
                      </button>
                    </div>
                  </div>
                </div>
              </Popup>
            </Polygon>
          )
        } catch (error) {
          console.error('Error parsing patrol zone coordinates:', error)
          return null
        }
      })}
    </>
  )
}