import React, { useState, useEffect } from 'react'
import { useQuery } from '@tanstack/react-query'
import { Vehicle, UpdateVehicleDto } from '../types'
import { fireStationsApi, ApiError } from '../lib/api'
import { X, Save, AlertCircle, AlertTriangle } from 'lucide-react'
import LoadingSpinner from './LoadingSpinner'
import RetryButton from './RetryButton'

interface VehicleEditModalProps {
  vehicle: Vehicle
  isOpen: boolean
  onClose: () => void
  onSave: (data: UpdateVehicleDto) => void
  isLoading?: boolean
  error?: string | null
}

const VehicleEditModal: React.FC<VehicleEditModalProps> = ({
  vehicle,
  isOpen,
  onClose,
  onSave,
  isLoading = false,
  error = null
}) => {
  const [formData, setFormData] = useState<UpdateVehicleDto>({
    callsign: vehicle.callsign,
    type: vehicle.type,
    plateNumber: vehicle.plateNumber,
    stationId: vehicle.stationId,
    waterCapacityLiters: vehicle.waterCapacityLiters || 0
  })

  const [validationErrors, setValidationErrors] = useState<Record<string, string>>({})

  // Fetch fire stations for the dropdown (filtered by user's agency)
  const { 
    data: stations, 
    isLoading: stationsLoading, 
    error: stationsError,
    refetch: refetchStations 
  } = useQuery({
    queryKey: ['fire-stations'],
    queryFn: () => fireStationsApi.getStations(),
    enabled: isOpen,
    staleTime: 5 * 60 * 1000, // Cache for 5 minutes
    retry: (failureCount, error) => {
      if (error instanceof ApiError && error.status && error.status < 500) {
        return false
      }
      return failureCount < 2
    },
  })

  // Reset form data when vehicle changes
  useEffect(() => {
    setFormData({
      callsign: vehicle.callsign,
      type: vehicle.type,
      plateNumber: vehicle.plateNumber,
      stationId: vehicle.stationId,
      waterCapacityLiters: vehicle.waterCapacityLiters || 0
    })
    setValidationErrors({})
  }, [vehicle])

  const validateForm = (): boolean => {
    const errors: Record<string, string> = {}

    if (!formData.callsign?.trim()) {
      errors.callsign = 'Callsign is required'
    }

    if (!formData.type?.trim()) {
      errors.type = 'Vehicle type is required'
    }

    if (!formData.plateNumber?.trim()) {
      errors.plateNumber = 'Plate number is required'
    }

    if (!formData.stationId) {
      errors.stationId = 'Station assignment is required'
    }

    if (formData.waterCapacityLiters && formData.waterCapacityLiters < 0) {
      errors.waterCapacityLiters = 'Water capacity cannot be negative'
    }

    setValidationErrors(errors)
    return Object.keys(errors).length === 0
  }

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault()
    
    if (!validateForm()) {
      return
    }

    onSave(formData)
  }

  const handleInputChange = (field: keyof UpdateVehicleDto, value: string | number) => {
    setFormData(prev => ({ ...prev, [field]: value }))
    
    // Clear validation error for this field
    if (validationErrors[field]) {
      setValidationErrors(prev => {
        const newErrors = { ...prev }
        delete newErrors[field]
        return newErrors
      })
    }
  }

  if (!isOpen) return null

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-[9999]">
      <div className="bg-white dark:bg-gray-800 rounded-lg p-6 w-full max-w-md max-h-[90vh] overflow-y-auto border border-gray-200 dark:border-gray-700">
        <div className="flex items-center justify-between mb-4">
          <h3 className="text-lg font-semibold text-gray-900 dark:text-gray-100">Edit Vehicle</h3>
          <button 
            onClick={onClose}
            disabled={isLoading}
            className="text-gray-400 dark:text-gray-500 hover:text-gray-600 dark:hover:text-gray-300"
          >
            <X className="w-5 h-5" />
          </button>
        </div>

        {error && (
          <div className="mb-4 p-3 bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-md flex items-center">
            <AlertCircle className="w-4 h-4 text-red-500 mr-2" />
            <span className="text-red-700 dark:text-red-300 text-sm">{error}</span>
          </div>
        )}

        {stationsError && (
          <div className="mb-4 p-3 bg-yellow-50 dark:bg-yellow-900/20 border border-yellow-200 dark:border-yellow-800 rounded-md">
            <div className="flex items-center justify-between">
              <div className="flex items-center">
                <AlertTriangle className="w-4 h-4 text-yellow-500 mr-2" />
                <span className="text-yellow-700 dark:text-yellow-300 text-sm">
                  Failed to load stations: {stationsError.message}
                </span>
              </div>
              <RetryButton onRetry={() => refetchStations()} size="sm" />
            </div>
          </div>
        )}

        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              Callsign *
            </label>
            <input
              type="text"
              value={formData.callsign || ''}
              onChange={(e) => handleInputChange('callsign', e.target.value)}
              className={`w-full px-3 py-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-primary-500 bg-white dark:bg-gray-800 text-gray-900 dark:text-gray-100 ${
                validationErrors.callsign ? 'border-red-300 dark:border-red-600' : 'border-gray-300 dark:border-gray-600'
              }`}
              placeholder="e.g., Engine 1, Truck 2"
              disabled={isLoading}
            />
            {validationErrors.callsign && (
              <p className="mt-1 text-sm text-red-600 dark:text-red-400">{validationErrors.callsign}</p>
            )}
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              Type *
            </label>
            <select
              value={formData.type || ''}
              onChange={(e) => handleInputChange('type', e.target.value)}
              className={`w-full px-3 py-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-primary-500 bg-white dark:bg-gray-800 text-gray-900 dark:text-gray-100 ${
                validationErrors.type ? 'border-red-300 dark:border-red-600' : 'border-gray-300 dark:border-gray-600'
              }`}
              disabled={isLoading}
            >
              <option value="">Select vehicle type</option>
              <option value="Fire Engine">Fire Engine</option>
              <option value="Ladder Truck">Ladder Truck</option>
              <option value="Ambulance">Ambulance</option>
              <option value="Rescue Vehicle">Rescue Vehicle</option>
              <option value="Tanker">Tanker</option>
              <option value="Command Vehicle">Command Vehicle</option>
            </select>
            {validationErrors.type && (
              <p className="mt-1 text-sm text-red-600 dark:text-red-400">{validationErrors.type}</p>
            )}
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              Plate Number *
            </label>
            <input
              type="text"
              value={formData.plateNumber || ''}
              onChange={(e) => handleInputChange('plateNumber', e.target.value)}
              className={`w-full px-3 py-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-primary-500 bg-white dark:bg-gray-800 text-gray-900 dark:text-gray-100 ${
                validationErrors.plateNumber ? 'border-red-300 dark:border-red-600' : 'border-gray-300 dark:border-gray-600'
              }`}
              placeholder="License plate number"
              disabled={isLoading}
            />
            {validationErrors.plateNumber && (
              <p className="mt-1 text-sm text-red-600 dark:text-red-400">{validationErrors.plateNumber}</p>
            )}
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              Station *
            </label>
            {stationsLoading ? (
              <div className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md bg-gray-50 dark:bg-gray-700">
                <LoadingSpinner size="sm" text="Loading stations..." />
              </div>
            ) : (
              <select
                value={formData.stationId || ''}
                onChange={(e) => handleInputChange('stationId', parseInt(e.target.value))}
                className={`w-full px-3 py-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-primary-500 bg-white dark:bg-gray-800 text-gray-900 dark:text-gray-100 ${
                  validationErrors.stationId ? 'border-red-300 dark:border-red-600' : 'border-gray-300 dark:border-gray-600'
                }`}
                disabled={isLoading || !!stationsError}
              >
                <option value="">
                  {stationsError ? 'Failed to load stations' : 'Select station'}
                </option>
                {stations?.map(station => (
                  <option key={station.id} value={station.id}>
                    {station.name}
                  </option>
                ))}
              </select>
            )}
            {validationErrors.stationId && (
              <p className="mt-1 text-sm text-red-600 dark:text-red-400">{validationErrors.stationId}</p>
            )}
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              Water Capacity (Liters)
            </label>
            <input
              type="number"
              value={formData.waterCapacityLiters || 0}
              onChange={(e) => handleInputChange('waterCapacityLiters', parseInt(e.target.value) || 0)}
              className={`w-full px-3 py-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-primary-500 bg-white dark:bg-gray-800 text-gray-900 dark:text-gray-100 ${
                validationErrors.waterCapacityLiters ? 'border-red-300 dark:border-red-600' : 'border-gray-300 dark:border-gray-600'
              }`}
              placeholder="0"
              min="0"
              disabled={isLoading}
            />
            {validationErrors.waterCapacityLiters && (
              <p className="mt-1 text-sm text-red-600 dark:text-red-400">{validationErrors.waterCapacityLiters}</p>
            )}
          </div>

          <div className="flex justify-end space-x-2 pt-4">
            <button
              type="button"
              onClick={onClose}
              disabled={isLoading}
              className="btn btn-secondary"
            >
              Cancel
            </button>
            <button
              type="submit"
              disabled={isLoading || !!stationsError}
              className="btn btn-primary flex items-center disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {isLoading ? (
                <LoadingSpinner size="sm" className="mr-2" />
              ) : (
                <Save className="w-4 h-4 mr-2" />
              )}
              {isLoading ? 'Saving...' : 'Save Changes'}
            </button>
          </div>
        </form>
      </div>
    </div>
  )
}

export default VehicleEditModal