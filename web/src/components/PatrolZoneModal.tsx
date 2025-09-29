import { useState, useEffect } from 'react'
import { createPortal } from 'react-dom'
import { useMutation, useQueryClient, useQuery } from '@tanstack/react-query'
import { patrolZonesApi, fireStationsApi, coastGuardStationsApi, policeStationsApi, hospitalsApi } from '../lib/api'
import { CreatePatrolZone, UpdatePatrolZone, PatrolZone } from '../types'
import { useTranslation } from '../hooks/useTranslation'
import { useUserStore } from '../lib/userStore'
import { X, Shield, MapPin, Palette, AlertCircle, Users, CheckCircle } from 'lucide-react'
import LoadingSpinner from './LoadingSpinner'

interface PatrolZoneModalProps {
  isOpen: boolean
  onClose: () => void
  pendingZone?: CreatePatrolZone | null
  editingZone?: PatrolZone | null
  mode: 'create' | 'edit'
  onStartDrawing?: () => void
}

const COLOR_OPTIONS = [
  '#ef4444', // Red
  '#f59e0b', // Orange
  '#10b981', // Green
  '#3b82f6', // Blue
  '#8b5cf6', // Purple
  '#ec4899', // Pink
  '#06b6d4', // Cyan
  '#84cc16'  // Lime
]

// Map user agency names to API expected strings
const mapAgencyNameToApiString = (agencyName?: string): string => {
  if (!agencyName) return 'fire' // default fallback

  const lowerAgency = agencyName.toLowerCase()

  if (lowerAgency.includes('fire') || agencyName === 'Hellenic Fire Service' || agencyName === 'Fire Department' || agencyName === 'FireDepartment') {
    return 'fire'
  } else if (lowerAgency.includes('coast') || agencyName === 'Hellenic Coast Guard' || agencyName === 'Coast Guard' || agencyName === 'CoastGuard') {
    return 'coastguard'
  } else if (lowerAgency.includes('police') || agencyName === 'Hellenic Police' || agencyName === 'Police') {
    return 'police'
  } else if (lowerAgency.includes('ekab') || agencyName === 'EKAB') {
    return 'hospital'
  }

  return 'fire' // default fallback
}

// Get stations based on agency type
const getStationsByAgency = async (agencyName?: string) => {
  const agencyType = mapAgencyNameToApiString(agencyName)

  switch (agencyType) {
    case 'fire':
      return await fireStationsApi.getStations()
    case 'coastguard':
      return await coastGuardStationsApi.getAll()
    case 'police':
      return await policeStationsApi.getStations()
    case 'hospital':
      return await hospitalsApi.getAll()
    default:
      return await fireStationsApi.getStations()
  }
}

export default function PatrolZoneModal({ 
  isOpen, 
  onClose, 
  pendingZone, 
  editingZone, 
  mode,
  onStartDrawing
}: PatrolZoneModalProps) {
  const t = useTranslation()
  const { user } = useUserStore()
  const queryClient = useQueryClient()
  
  const [formData, setFormData] = useState({
    name: '',
    description: '',
    stationId: 0,
    priority: 2,
    color: '#3b82f6',
    isActive: true
  })

  const [errors, setErrors] = useState<Record<string, string>>({})
  const [isStationAutoAssigned, setIsStationAutoAssigned] = useState(false)

  // Load available stations for selection (filtered by user's agency)
  const { data: stations = [] } = useQuery({
    queryKey: ['stations', user?.agencyName],
    queryFn: () => getStationsByAgency(user?.agencyName),
    staleTime: 5 * 60 * 1000, // Cache for 5 minutes
    enabled: !!user?.agencyName, // Only fetch when user has agency
  })

  useEffect(() => {
    console.log('PatrolZoneModal initial form setup effect triggered:', {
      mode,
      pendingZone: pendingZone ? { stationId: pendingZone.stationId, name: pendingZone.name } : null,
      editingZone: editingZone ? { stationId: editingZone.stationId, name: editingZone.name } : null,
      isOpen
    })
    
    if (mode === 'create' && pendingZone) {
      const wasAutoAssigned = Boolean(pendingZone.stationId && pendingZone.stationId > 0)
      console.log('Setting up create mode form data:', {
        wasAutoAssigned,
        pendingStationId: pendingZone.stationId
      })
      setIsStationAutoAssigned(wasAutoAssigned)

      const initialFormData = {
        name: pendingZone.name,
        description: pendingZone.description || '',
        stationId: pendingZone.stationId || 0,
        priority: pendingZone.priority || 2,
        color: pendingZone.color || '#3b82f6',
        isActive: true
      }
      setFormData(initialFormData)
    } else if (mode === 'edit' && editingZone) {
      setIsStationAutoAssigned(false) // Editing mode doesn't show auto-assignment

      setFormData({
        name: editingZone.name,
        description: editingZone.description || '',
        stationId: editingZone.stationId,
        priority: editingZone.priority,
        color: editingZone.color || '#3b82f6',
        isActive: editingZone.isActive
      })
    }
    setErrors({})
  }, [mode, pendingZone, editingZone, isOpen])

  // Separate effect to update stationId when stations data loads and there's an auto-assigned station
  useEffect(() => {
    if (mode === 'create' && pendingZone && stations.length > 0) {
      // If there's an auto-assigned station, make sure it's properly set in the form
      if (pendingZone.stationId && pendingZone.stationId > 0) {
        // Verify the station exists in the loaded stations list
        const stationExists = stations.some((station: any) => station.id === pendingZone.stationId)
        
        if (stationExists) {
          setFormData(prev => ({
            ...prev,
            stationId: pendingZone.stationId
          }))
        }
      }
    }
  }, [mode, pendingZone, stations])

  const createMutation = useMutation({
    mutationFn: (data: CreatePatrolZone) => patrolZonesApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['patrolZones'] })
      onClose()
    },
    onError: (error: any) => {
      console.error('Error creating patrol zone:', error)
      setErrors({ submit: error.response?.data?.message || 'Failed to create patrol zone' })
    }
  })

  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: number, data: UpdatePatrolZone }) => 
      patrolZonesApi.update(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['patrolZones'] })
      onClose()
    },
    onError: (error: any) => {
      console.error('Error updating patrol zone:', error)
      setErrors({ submit: error.response?.data?.message || 'Failed to update patrol zone' })
    }
  })

  const validateForm = () => {
    const newErrors: Record<string, string> = {}
    
    if (!formData.name.trim()) {
      newErrors.name = 'Name is required'
    }
    
    if (formData.name.length > 100) {
      newErrors.name = 'Name must be less than 100 characters'
    }
    
    if (formData.description.length > 500) {
      newErrors.description = 'Description must be less than 500 characters'
    }
    
    if (!formData.stationId || formData.stationId === 0) {
      newErrors.stationId = 'Station is required'
    }
    
    setErrors(newErrors)
    return Object.keys(newErrors).length === 0
  }

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault()
    
    if (!validateForm()) return
    
    if (mode === 'create' && pendingZone) {
      const newZone: CreatePatrolZone = {
        ...pendingZone,
        name: formData.name,
        description: formData.description,
        stationId: formData.stationId,
        priority: formData.priority,
        color: formData.color
      }
      console.log('Sending patrol zone data to API:', newZone)
      createMutation.mutate(newZone)
    } else if (mode === 'edit' && editingZone) {
      const updateData: UpdatePatrolZone = {
        name: formData.name,
        description: formData.description,
        stationId: formData.stationId,
        priority: formData.priority,
        color: formData.color,
        isActive: formData.isActive
      }
      updateMutation.mutate({ id: editingZone.id, data: updateData })
    }
  }

  const handleStationChange = (stationId: number) => {
    setFormData({ ...formData, stationId })
    // Clear auto-assignment state when manually selecting station
    setIsStationAutoAssigned(false)
  }

  const isLoading = createMutation.isPending || updateMutation.isPending

  if (!isOpen) return null

  return createPortal(
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center" style={{ zIndex: 10000 }}>
      <div className="bg-white dark:bg-gray-800 rounded-lg shadow-xl w-full max-w-md mx-4 max-h-[90vh] overflow-y-auto">
        <div className="flex items-center justify-between p-6 border-b border-gray-200 dark:border-gray-700">
          <h2 className="text-xl font-semibold text-gray-900 dark:text-gray-100 flex items-center">
            <Shield className="w-5 h-5 mr-2" />
            {mode === 'create' ? t.createPatrolZone : t.editPatrolZone}
          </h2>
          <button
            onClick={onClose}
            className="text-gray-400 hover:text-gray-600 dark:hover:text-gray-300"
            disabled={isLoading}
          >
            <X className="w-6 h-6" />
          </button>
        </div>

        <form onSubmit={handleSubmit} className="p-6 space-y-4">
          {errors.submit && (
            <div className="p-3 bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-lg">
              <div className="flex items-center">
                <AlertCircle className="w-4 h-4 text-red-500 mr-2" />
                <span className="text-sm text-red-700 dark:text-red-300">{errors.submit}</span>
              </div>
            </div>
          )}

          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
              {t.patrolZoneName} *
            </label>
            <input
              type="text"
              value={formData.name}
              onChange={(e) => setFormData({ ...formData, name: e.target.value })}
              className={`w-full px-3 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:border-gray-600 dark:text-gray-100 ${
                errors.name ? 'border-red-500' : 'border-gray-300'
              }`}
              placeholder={t.patrolZoneNamePlaceholder}
              disabled={isLoading}
            />
            {errors.name && (
              <p className="mt-1 text-sm text-red-600 dark:text-red-400">{t.patrolZoneNameRequired}</p>
            )}
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
              Station *
            </label>
            <select
              value={formData.stationId}
              onChange={(e) => handleStationChange(parseInt(e.target.value))}
              className={`w-full px-3 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:border-gray-600 dark:text-gray-100 ${
                isStationAutoAssigned
                  ? 'border-green-300 dark:border-green-600 bg-green-50 dark:bg-green-900'
                  : errors.stationId ? 'border-red-500' : 'border-gray-300'
              }`}
              disabled={isLoading}
            >
              <option value={0}>Select a station...</option>
              {stations?.map((station: any) => (
                    <option key={station.id} value={station.id}>
                      {station.name}
                    </option>
                  ))}
            </select>

            {/* Auto-assignment status */}
            {isStationAutoAssigned && formData.stationId > 0 && (
              <div className="mt-2 flex items-center text-sm text-green-600 dark:text-green-400">
                <CheckCircle className="w-4 h-4 mr-1" />
                Automatically assigned {stations?.find((s: any) => s.id === formData.stationId)?.name || 'selected station'} based on patrol zone center
              </div>
            )}

            {errors.stationId && (
              <p className="mt-1 text-sm text-red-600 dark:text-red-400">{errors.stationId}</p>
            )}
          </div>

          <div>
              <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                {t.patrolZoneDescription}
              </label>
              <textarea
                value={formData.description}
                onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                rows={3}
                className={`w-full px-3 py-2 border rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:text-gray-100 ${
                  errors.description ? 'border-red-500' : 'border-gray-300'
                }`}
                placeholder={t.patrolZoneDescriptionPlaceholder}
                disabled={isLoading}
              />
            {errors.description && (
              <p className="mt-1 text-sm text-red-600 dark:text-red-400">{t.patrolZoneDescriptionError}</p>
            )}
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
              {t.patrolZonePriority} *
            </label>
            <select
              value={formData.priority}
              onChange={(e) => setFormData({ ...formData, priority: parseInt(e.target.value) })}
              className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:text-gray-100"
              disabled={isLoading}
            >
              <option value={1}>{t.highPriorityPatrol}</option>
              <option value={2}>{t.mediumPriorityPatrol}</option>
              <option value={3}>{t.lowPriorityPatrol}</option>
            </select>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              <Palette className="w-4 h-4 inline mr-1" />
              Color
            </label>
            <div className="flex flex-wrap gap-2">
              {COLOR_OPTIONS.map(color => (
                <button
                  key={color}
                  type="button"
                  onClick={() => setFormData({ ...formData, color })}
                  className={`w-8 h-8 rounded-full border-2 ${
                    formData.color === color 
                      ? 'border-gray-900 dark:border-gray-100' 
                      : 'border-gray-300 dark:border-gray-600'
                  }`}
                  style={{ backgroundColor: color }}
                  disabled={isLoading}
                />
              ))}
            </div>
          </div>

          {mode === 'edit' && (
            <div className="flex items-center">
              <input
                type="checkbox"
                id="isActive"
                checked={formData.isActive}
                onChange={(e) => setFormData({ ...formData, isActive: e.target.checked })}
                className="mr-2 rounded border-gray-300 text-blue-600 focus:ring-blue-500"
                disabled={isLoading}
              />
              <label htmlFor="isActive" className="text-sm text-gray-700 dark:text-gray-300">
                Active
              </label>
            </div>
          )}

          {mode === 'edit' && editingZone && (
            <div className="pt-4 border-t border-gray-200 dark:border-gray-700">
              <div className="flex items-center text-sm text-gray-600 dark:text-gray-400 mb-2">
                <Users className="w-4 h-4 mr-1" />
                <span>Vehicle Assignments: {editingZone.vehicleAssignments?.filter(a => a.isActive).length || 0}</span>
              </div>
              <div className="flex items-center text-sm text-gray-600 dark:text-gray-400">
                <MapPin className="w-4 h-4 mr-1" />
                <span>Station: {editingZone.stationName}</span>
              </div>
            </div>
          )}

          <div className="flex justify-end space-x-3 pt-4 border-t border-gray-200 dark:border-gray-600">
            <button
              type="button"
              onClick={onClose}
              className="px-4 py-2 text-sm font-medium text-gray-700 dark:text-gray-300 bg-white dark:bg-gray-700 border border-gray-300 dark:border-gray-600 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-600 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
              disabled={isLoading}
            >
              {t.cancel}
            </button>
            <button
              type="submit"
              onClick={() => {
                if (mode === 'create' && onStartDrawing) {
                  onStartDrawing();
                }
              }}
              className="px-4 py-2 text-sm font-medium text-white bg-blue-600 border border-transparent rounded-lg hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 disabled:opacity-50 disabled:cursor-not-allowed flex items-center"
              disabled={isLoading}
            >
              {isLoading && <LoadingSpinner size="sm" className="mr-2" />}
              {mode === 'create' ? t.createZone : t.updateZone}
            </button>
          </div>
        </form>
      </div>
    </div>,
    document.body
  )
}