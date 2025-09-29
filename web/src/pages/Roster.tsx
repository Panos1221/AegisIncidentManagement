import React, { useState, useEffect } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { personnelApi, fireStationsApi, ApiError } from '../lib/api'
import { User, Plus, X, Save, Users, AlertTriangle, CheckCircle } from 'lucide-react'
import { StationFilter, ErrorBoundary, LoadingSpinner, RetryButton } from '../components'
import { RankSelect } from '../components/RankSelect'
import { useTranslation } from '../hooks/useTranslation'
import { useUserStore } from '../lib/userStore'
import { translateRank } from '../utils/rankUtils'

export default function Roster() {
  const t = useTranslation()
  const queryClient = useQueryClient()
  const { user } = useUserStore()
  
  const [showAddModal, setShowAddModal] = useState(false)
  const [selectedStationId, setSelectedStationId] = useState<number | undefined>(undefined)
  const [successMessage, setSuccessMessage] = useState<string | null>(null)
  const [formData, setFormData] = useState({
    name: '',
    rank: '',
    stationId: user?.stationId || 1,
    agencyId: user?.agencyId || 1,
    isActive: true
  })

  // Fetch personnel grouped by station
  const { 
    data: groupedPersonnel, 
    isLoading: personnelLoading,
    error: personnelError,
    refetch: refetchPersonnel 
  } = useQuery({
    queryKey: ['personnel-grouped'],
    queryFn: () => personnelApi.getGroupedByStation({ isActive: true }).then(res => {
      console.log('Personnel grouped data received:', res.data)
      return res.data
    }),
    retry: (failureCount, error) => {
      if (error instanceof ApiError && error.status && error.status < 500) {
        return false
      }
      return failureCount < 2
    },
    staleTime: 30000,
  })

  // Fetch fire stations for display names (filtered by user's agency)
  const { 
    data: stations,
    isLoading: stationsLoading,
    error: stationsError,
    refetch: refetchStations 
  } = useQuery({
    queryKey: ['fire-stations'],
    queryFn: () => fireStationsApi.getStations(),
    retry: (failureCount, error) => {
      if (error instanceof ApiError && error.status && error.status < 500) {
        return false
      }
      return failureCount < 2
    },
    staleTime: 5 * 60 * 1000,
  })

  // Update default station when stations are loaded
  useEffect(() => {
    if (stations && stations.length > 0 && (!user?.stationId || !stations.find(s => s.id === user.stationId))) {
      setFormData(prev => ({ 
        ...prev, 
        stationId: stations[0].id,
        agencyId: user?.agencyId || prev.agencyId
      }))
    }
  }, [stations, user?.stationId, user?.agencyId])

  const createPersonnelMutation = useMutation({
    mutationFn: (data: typeof formData) => {
      console.log('Creating personnel with data:', data)
      return personnelApi.create(data)
    },
    onSuccess: (response) => {
      console.log('Personnel created successfully:', response)
      queryClient.invalidateQueries({ queryKey: ['personnel-grouped'] })
      setShowAddModal(false)
      setFormData({ name: '', rank: '', stationId: user?.stationId || 1, agencyId: user?.agencyId || 1, isActive: true })
      setSuccessMessage(t.personnelAdded)
      setTimeout(() => setSuccessMessage(null), 3000)
    },
    onError: (error) => {
      console.error('Failed to create personnel:', error)
      // Don't hide the modal on error so user can see the error and retry
    }
  })

  // Helper function to get station name
  const getStationName = (stationId: number) => {
    const station = stations?.find(s => s.id === stationId)
    return station ? station.name : `Station ${stationId}`
  }

  // Filter personnel by selected station
  const getFilteredPersonnel = () => {
    if (!groupedPersonnel) return {}
    
    if (selectedStationId) {
      const stationPersonnel = groupedPersonnel[selectedStationId]
      return stationPersonnel ? { [selectedStationId]: stationPersonnel } : {}
    }
    
    return groupedPersonnel
  }

  const filteredPersonnel = getFilteredPersonnel()
  const totalPersonnel = Object.values(filteredPersonnel).flat().length

  const getStatusColor = (isActive: boolean) => {
    return isActive 
      ? 'bg-green-100 dark:bg-green-900/30 text-green-800 dark:text-green-300'
      : 'bg-gray-100 dark:bg-gray-700 text-gray-800 dark:text-gray-300'
  }

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault()
    if (!formData.name.trim() || !formData.rank.trim()) {
      alert(t.fillRequiredFields)
      return
    }
    
    // Ensure agencyId is set to current user's agency
    const personnelData = {
      ...formData,
      agencyId: user?.agencyId || formData.agencyId
    }
    
    createPersonnelMutation.mutate(personnelData)
  }

  if (personnelLoading) {
    return (
      <div className="flex items-center justify-center h-64">
        <LoadingSpinner size="lg" text={t.loadingData} />
      </div>
    )
  }

  return (
    <ErrorBoundary>
      <div>
      <div className="mb-8">
        <h1 className="text-3xl font-bold text-gray-900 dark:text-gray-100">{t.rosterPageTitle}</h1>
        <p className="mt-2 text-gray-600 dark:text-gray-400">
          {t.managePersonnelAndVehicles}
        </p>
      </div>

      {/* Success Message */}
      {successMessage && (
        <div className="mb-4 p-3 bg-green-50 dark:bg-green-900/20 border border-green-200 dark:border-green-800 rounded-md flex items-center">
          <CheckCircle className="w-4 h-4 text-green-500 mr-2" />
          <span className="text-green-700 dark:text-green-300 text-sm">{successMessage}</span>
        </div>
      )}

      {/* Error Messages */}
      {personnelError && (
        <div className="mb-4 p-4 bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-lg">
          <div className="flex items-center justify-between">
            <div className="flex items-center">
              <AlertTriangle className="w-5 h-5 text-red-500 mr-3" />
              <div>
                <h3 className="text-sm font-medium text-red-800 dark:text-red-200">{t.dataLoadError}</h3>
                <p className="text-sm text-red-700 dark:text-red-300 mt-1">{personnelError.message}</p>
              </div>
            </div>
            <RetryButton onRetry={() => refetchPersonnel()} />
          </div>
        </div>
      )}

      {stationsError && (
        <div className="mb-4 p-4 bg-yellow-50 dark:bg-yellow-900/20 border border-yellow-200 dark:border-yellow-800 rounded-lg">
          <div className="flex items-center justify-between">
            <div className="flex items-center">
              <AlertTriangle className="w-5 h-5 text-yellow-500 mr-3" />
              <div>
                <h3 className="text-sm font-medium text-yellow-800 dark:text-yellow-200">{t.dataLoadError}</h3>
                <p className="text-sm text-yellow-700 dark:text-yellow-300 mt-1">{stationsError.message}</p>
              </div>
            </div>
            <RetryButton onRetry={() => refetchStations()} />
          </div>
        </div>
      )}

      {/* Station Filter */}
      <div className="mb-6">
        <div className="flex items-center justify-between">
          <StationFilter
            selectedStationId={selectedStationId}
            onStationChange={setSelectedStationId}
            placeholder={t.allStations}
            className="flex-1 max-w-md"
          />
          <div className="text-sm text-gray-600 dark:text-gray-400">
            {totalPersonnel} {t.personnel} {selectedStationId ? t.station.toLowerCase() : t.active.toLowerCase()}
          </div>
        </div>
      </div>

      {/* Personnel by Station */}
      <div className="space-y-6">
        {Object.entries(filteredPersonnel).map(([stationId, stationPersonnel]) => (
          <div key={stationId} className="card">
            <div className="px-6 py-4 border-b border-gray-200 dark:border-gray-700">
              <div className="flex items-center justify-between">
                <div className="flex items-center">
                  <Users className="w-5 h-5 text-gray-400 dark:text-gray-500 mr-3" />
                  <div>
                    <h2 className="text-lg font-semibold text-gray-900 dark:text-gray-100">
                      {getStationName(parseInt(stationId))}
                    </h2>
                    <p className="text-sm text-gray-600 dark:text-gray-400">
                      {stationPersonnel.length} {t.personnel}
                    </p>
                  </div>
                </div>
                <button 
                  onClick={() => setShowAddModal(true)}
                  className="btn btn-primary flex items-center"
                  disabled={!!stationsError}
                >
                  <Plus className="w-4 h-4 mr-2" />
                  {t.addPersonnel}
                </button>
              </div>
            </div>

            <div className="overflow-x-auto">
              <table className="min-w-full divide-y divide-gray-200 dark:divide-gray-700">
                <thead className="bg-gray-50 dark:bg-gray-700">
                  <tr>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                      {t.personnel}
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                      {t.rank}
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                      {t.status}
                    </th>
                  </tr>
                </thead>
                <tbody className="bg-white dark:bg-gray-800 divide-y divide-gray-200 dark:divide-gray-700">
                  {stationPersonnel.map((person) => (
                    <tr key={person.id} className="hover:bg-gray-50 dark:hover:bg-gray-700">
                      <td className="px-6 py-4 whitespace-nowrap">
                        <div className="flex items-center">
                          <div className="w-10 h-10 bg-gradient-to-br from-blue-500 to-blue-600 rounded-full flex items-center justify-center">
                            <User className="w-5 h-5 text-white" />
                          </div>
                          <div className="ml-4">
                            <div className="text-sm font-medium text-gray-900 dark:text-gray-100">
                              {person.name}
                            </div>
                            <div className="text-xs text-gray-500 dark:text-gray-400">
                              {t.vehicleId}: {person.id}
                            </div>
                          </div>
                        </div>
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap">
                        <div className="text-sm font-medium text-gray-900 dark:text-gray-100">
                          {translateRank(person.rank, t)}
                        </div>
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap">
                        <span className={`px-3 py-1 text-xs font-medium rounded-full ${getStatusColor(person.isActive)}`}>
                          {person.isActive ? t.active : t.inactive}
                        </span>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>

            {stationPersonnel.length === 0 && (
              <div className="text-center py-12">
                <User className="w-12 h-12 text-gray-400 dark:text-gray-500 mx-auto mb-4" />
                <h3 className="text-lg font-medium text-gray-900 dark:text-gray-100 mb-2">{t.noPersonnelFound}</h3>
                <p className="text-gray-600 dark:text-gray-400">
                  {selectedStationId 
                    ? `${t.noPersonnelFound} ${getStationName(parseInt(stationId))}`
                    : t.addFirstPersonnel
                  }
                </p>
              </div>
            )}
          </div>
        ))}

        {Object.keys(filteredPersonnel).length === 0 && (
          <div className="card">
            <div className="text-center py-12">
              <Users className="w-12 h-12 text-gray-400 dark:text-gray-500 mx-auto mb-4" />
              <h3 className="text-lg font-medium text-gray-900 dark:text-gray-100 mb-2">
                {selectedStationId ? t.noPersonnelFound : t.noPersonnelFound}
              </h3>
              <p className="text-gray-600 dark:text-gray-400">
                {selectedStationId 
                  ? t.tryAdjustingFilters
                  : t.addFirstPersonnel
                }
              </p>
              <button 
                onClick={() => setShowAddModal(true)}
                className="btn btn-primary mt-4 flex items-center mx-auto"
              >
                <Plus className="w-4 h-4 mr-2" />
                {t.addPersonnel}
              </button>
            </div>
          </div>
        )}
      </div>

      {/* Add Personnel Modal */}
      {showAddModal && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white dark:bg-gray-800 rounded-lg p-6 w-full max-w-md border border-gray-200 dark:border-gray-700">
            <div className="flex items-center justify-between mb-4">
              <h3 className="text-lg font-semibold text-gray-900 dark:text-gray-100">{t.addPersonnel}</h3>
              <button 
                onClick={() => setShowAddModal(false)}
                className="text-gray-400 dark:text-gray-500 hover:text-gray-600 dark:hover:text-gray-300"
              >
                <X className="w-5 h-5" />
              </button>
            </div>
            <form onSubmit={handleSubmit} className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                  {t.name} *
                </label>
                <input
                  type="text"
                  value={formData.name}
                  onChange={(e) => setFormData(prev => ({ ...prev, name: e.target.value }))}
                  className="input w-full"
                  placeholder={t.name}
                  required
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                  {t.rank} *
                </label>
                <RankSelect
                  value={formData.rank}
                  onChange={(rank) => setFormData(prev => ({ ...prev, rank }))}
                  agencyName={user?.agencyName}
                  required
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                  {t.station}
                </label>
                {stationsLoading ? (
                  <div className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md bg-gray-50 dark:bg-gray-700">
                    <LoadingSpinner size="sm" text={t.loadingData} />
                  </div>
                ) : (
                  <select
                    value={formData.stationId}
                    onChange={(e) => setFormData(prev => ({ ...prev, stationId: parseInt(e.target.value) }))}
                    className="input w-full"
                    disabled={!!stationsError}
                  >
                    {stations?.map((station) => (
                      <option key={station.id} value={station.id}>
                        {station.name}
                      </option>
                    ))}
                  </select>
                )}
                {stationsError && (
                  <p className="mt-1 text-sm text-red-600 dark:text-red-400">
                    {t.dataLoadError}
                  </p>
                )}
              </div>
              <div className="flex items-center">
                <input
                  type="checkbox"
                  id="isActive"
                  checked={formData.isActive}
                  onChange={(e) => setFormData(prev => ({ ...prev, isActive: e.target.checked }))}
                  className="h-4 w-4 text-primary-600 focus:ring-primary-500 border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-800 rounded mr-2"
                />
                <label htmlFor="isActive" className="text-sm font-medium text-gray-700 dark:text-gray-300">
                  {t.active}
                </label>
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
                  disabled={createPersonnelMutation.isPending}
                  className="btn btn-primary flex items-center disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  {createPersonnelMutation.isPending ? (
                    <LoadingSpinner size="sm" className="mr-2" />
                  ) : (
                    <Save className="w-4 h-4 mr-2" />
                  )}
                  {createPersonnelMutation.isPending ? t.loading : t.addPersonnel}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Add Personnel Modal Error */}
      {createPersonnelMutation.error && showAddModal && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white dark:bg-gray-800 rounded-lg p-6 w-full max-w-md border border-gray-200 dark:border-gray-700">
            <div className="flex items-center justify-between mb-4">
              <h3 className="text-lg font-semibold text-red-800 dark:text-red-200">{t.error}</h3>
              <button 
                onClick={() => createPersonnelMutation.reset()}
                className="text-gray-400 dark:text-gray-500 hover:text-gray-600 dark:hover:text-gray-300"
              >
                <X className="w-5 h-5" />
              </button>
            </div>
            <div className="flex items-center mb-4">
              <AlertTriangle className="w-5 h-5 text-red-500 mr-3" />
              <p className="text-red-700 dark:text-red-300">{createPersonnelMutation.error.message}</p>
            </div>
            <div className="flex justify-end space-x-2">
              <button
                onClick={() => createPersonnelMutation.reset()}
                className="btn btn-secondary"
              >
                {t.close}
              </button>
              <RetryButton 
                onRetry={() => {
                  createPersonnelMutation.reset()
                  createPersonnelMutation.mutate(formData)
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