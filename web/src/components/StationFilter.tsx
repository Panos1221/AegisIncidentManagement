import React from 'react'
import { useQuery } from '@tanstack/react-query'
import { fireStationsApi, ApiError } from '../lib/api'
import { Filter, AlertTriangle } from 'lucide-react'
import LoadingSpinner from './LoadingSpinner'
import RetryButton from './RetryButton'
import { useTranslation } from '../hooks/useTranslation'
import { useUserStore } from '../lib/userStore'

interface StationFilterProps {
  selectedStationId?: number
  onStationChange: (stationId?: number) => void
  className?: string
  placeholder?: string
  required?: boolean
}

const StationFilter: React.FC<StationFilterProps> = ({
  selectedStationId,
  onStationChange,
  className = '',
  placeholder = '',
  required = false
}) => {
  const t = useTranslation(); 
  const { user } = useUserStore()
  placeholder = t.allStations;
  const { data: stations, isLoading, error, refetch } = useQuery({
    queryKey: ['fire-stations', user?.agencyId],
    queryFn: () => fireStationsApi.getStations(),
    staleTime: 5 * 60 * 1000, // Cache for 5 minutes
    enabled: !!user?.agencyId, // Only fetch when user has agency
    retry: (failureCount, error) => {
      // Don't retry on client errors
      if (error instanceof ApiError && error.status && error.status < 500) {
        return false
      }
      return failureCount < 2
    },
  })

  const handleChange = (event: React.ChangeEvent<HTMLSelectElement>) => {
    const value = event.target.value
    onStationChange(value ? parseInt(value, 10) : undefined)
  }

  if (error) {
    return (
      <div className={`flex items-center space-x-2 ${className}`}>
        <div className="flex items-center text-red-600 text-sm">
          <AlertTriangle className="w-4 h-4 mr-2" />
          <span>Failed to load stations</span>
        </div>
        <RetryButton onRetry={() => refetch()} size="sm" />
      </div>
    )
  }

  return (
    <div className={`flex items-center ${className}`}>
      {!required && <Filter className="w-5 h-5 text-gray-400 mr-3" />}
      {isLoading ? (
        <div className={`flex items-center ${required ? 'w-full' : 'min-w-48'}`}>
          <LoadingSpinner size="sm" text="Loading stations..." />
        </div>
      ) : (
        <select
          value={selectedStationId || ''}
          onChange={handleChange}
          className={required ? "input w-full" : "input min-w-48"}
          aria-label="Filter by fire station"
          required={required}
        >
          {!required && <option value="">{placeholder}</option>}
          {stations?.map((station) => (
            <option key={station.id} value={station.id}>
              {station.name}
            </option>
          ))}
        </select>
      )}
    </div>
  )
}

export default StationFilter