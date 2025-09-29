import { useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Search, Plus, X, User } from 'lucide-react'
import { Incident, CreateIncidentCommander, Personnel } from '../../types'
import { incidentsApi, personnelApi } from '../../lib/api'
import { useUserStore } from '../../lib/userStore'
import { useTranslation } from '../../hooks/useTranslation'
import { getRanksByAgency, translateRank } from '../../utils/rankUtils'

interface IncidentCommandersTabProps {
  incident: Incident
}

export default function IncidentCommandersTab({ incident }: IncidentCommandersTabProps) {
  const queryClient = useQueryClient()
  const { user } = useUserStore()
  const t = useTranslation()
  const [searchTerm, setSearchTerm] = useState('')
  const [selectedPersonnel, setSelectedPersonnel] = useState<Personnel | null>(null)
  const [observations, setObservations] = useState('')

  // Fetch personnel from user's agency
  const { data: personnel = [], isLoading: isLoadingPersonnel } = useQuery({
    queryKey: ['personnel', user?.agencyId],
    queryFn: () => personnelApi.getAll({ isActive: true }).then((res: any) => res.data),
    enabled: !!user?.agencyId,
  })

  const addCommanderMutation = useMutation({
    mutationFn: (data: CreateIncidentCommander) =>
      incidentsApi.addCommander(incident.id, data),
    onSuccess: () => {
      // Invalidate both the incidents list and the specific incident
      queryClient.invalidateQueries({ queryKey: ['incidents'] })
      queryClient.invalidateQueries({ queryKey: ['incident'] })
      setSelectedPersonnel(null)
      setObservations('')
      setSearchTerm('')
    },
  })

  const removeCommanderMutation = useMutation({
    mutationFn: (commanderId: number) =>
      incidentsApi.removeCommander(incident.id, commanderId),
    onSuccess: () => {
      // Invalidate both the incidents list and the specific incident
      queryClient.invalidateQueries({ queryKey: ['incidents'] })
      queryClient.invalidateQueries({ queryKey: ['incident'] })
    },
  })

  const updateCommanderMutation = useMutation({
    mutationFn: ({ commanderId, observations }: { commanderId: number; observations: string }) =>
      incidentsApi.updateCommander(incident.id, commanderId, { observations }),
    onSuccess: () => {
      // Invalidate both the incidents list and the specific incident
      queryClient.invalidateQueries({ queryKey: ['incidents'] })
      queryClient.invalidateQueries({ queryKey: ['incident'] })
    },
  })

  const handleAddCommander = () => {
    if (selectedPersonnel && user) {
      addCommanderMutation.mutate({
        personnelId: selectedPersonnel.id,
        observations,
        assignedByUserId: user.id,
      })
    }
  }

  // Function to get rank order for sorting (lower index = higher rank)
  const getRankOrder = (rank: string, agencyName?: string) => {
    try {
      const rankGroups = getRanksByAgency(agencyName || '', t)
      // Flatten all rank options from all groups
      const allRanks = rankGroups.flatMap(group => group.options)
      const rankIndex = allRanks.findIndex(r => r.value === rank || r.label === rank)
      return rankIndex === -1 ? 999 : rankIndex // Unknown ranks go to the end
    } catch (error) {
      return 999
    }
  }

  // Sort commanders by rank when more than 2 (highest rank first)
  const sortedCommanders = incident.commanders.length > 2 
    ? [...incident.commanders].sort((a, b) => {
        const aRankOrder = getRankOrder(a.personnelRank, user?.agencyName)
        const bRankOrder = getRankOrder(b.personnelRank, user?.agencyName)
        return aRankOrder - bRankOrder // Lower index = higher rank = comes first
      })
    : incident.commanders

  // Filter personnel - show all ranks
  const filteredPersonnel = personnel
    .filter((p: Personnel) => {
      // Only active personnel
      if (!p.isActive) return false
      
      // Search filter - only apply if there's a search term
      if (!searchTerm.trim()) return true
      
      const searchLower = searchTerm.toLowerCase().trim()
      const nameMatch = p.name && p.name.toLowerCase().includes(searchLower)
      const badgeMatch = p.badgeNumber && p.badgeNumber.toLowerCase().includes(searchLower)
      const rankMatch = p.rank && p.rank.toLowerCase().includes(searchLower)
      
      return nameMatch || badgeMatch || rankMatch
    })
    // Filter out already assigned commanders
    .filter((p: Personnel) => !incident.commanders.some(c => c.personnelId === p.id))

  return (
    <div className="space-y-6">
      {/* Search for Incident Manager */}
      <div className="bg-gray-50 dark:bg-gray-900 rounded-lg p-4">
        <h3 className="text-lg font-semibold text-gray-900 dark:text-gray-100 mb-4">
          {t.searchForCommander}
        </h3>

        <div className="space-y-4">
          {/* Search Input */}
          <div className="relative">
            <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 h-4 w-4" />
            <input
              type="text"
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              placeholder={`${t.search} ${t.badgeNumber.toLowerCase()} ${t.or} ${t.name.toLowerCase()}...`}
              className="w-full pl-10 pr-4 py-2 border border-gray-300 dark:border-gray-600 rounded-md bg-white dark:bg-gray-800 text-gray-900 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>

          {/* Available Personnel */}
          {searchTerm && (
            <div className="max-h-60 overflow-y-auto space-y-2">
              {isLoadingPersonnel ? (
                <div className="p-3 text-center text-gray-500 dark:text-gray-400">
                  Φόρτωση προσωπικού...
                </div>
              ) : filteredPersonnel.length > 0 ? (
              filteredPersonnel.map((person: Personnel) => (
                <div
                  key={person.id}
                  onClick={() => setSelectedPersonnel(person)}
                  className={`p-3 rounded-md cursor-pointer border-2 transition-colors ${
                    selectedPersonnel?.id === person.id
                      ? 'border-blue-500 bg-blue-50 dark:bg-blue-900/20'
                      : 'border-gray-200 dark:border-gray-700 hover:border-gray-300 dark:hover:border-gray-600'
                  }`}
                >
                  <div className="flex items-center justify-between">
                    <div>
                      <div className="font-medium text-gray-900 dark:text-gray-100">
                        {person.name}
                      </div>
                      <div className="text-sm text-gray-600 dark:text-gray-400">
                        {person.rank} - {t.signal}: {person.badgeNumber || 'Χωρίς αριθμό'}
                      </div>
                      {person.station && (
                        <div className="text-xs text-gray-500 dark:text-gray-500">
                          {person.station.name}
                        </div>
                      )}
                    </div>
                  </div>
                </div>
              ))
            ) : (
                <div className="p-3 text-center text-gray-500 dark:text-gray-400">
                  {personnel.length === 0 
                    ? 'Δεν βρέθηκε προσωπικό στην υπηρεσία σας'
                    : 'Δεν βρέθηκε προσωπικό με αυτά τα κριτήρια'
                  }
                </div>
              )}
            </div>
          )}

          {/* Add Commander Form */}
          {selectedPersonnel && (
            <div className="bg-white dark:bg-gray-800 rounded-md p-4 border border-gray-200 dark:border-gray-700">
              <h4 className="font-medium text-gray-900 dark:text-gray-100 mb-3">
                {t.searchForCommander}: {selectedPersonnel.name}
              </h4>
              <div className="space-y-3">
                <div>
                  <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                    {t.observations}
                  </label>
                  <textarea
                    value={observations}
                    onChange={(e) => setObservations(e.target.value)}
                    rows={3}
                    className="w-full rounded-md border border-gray-300 dark:border-gray-600 px-3 py-2 text-sm bg-white dark:bg-gray-800 text-gray-900 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-blue-500"
                    placeholder={t.optionalObservations}
                  />
                </div>
                <div className="flex space-x-2">
                  <button
                    onClick={handleAddCommander}
                    disabled={addCommanderMutation.isPending}
                    className="flex items-center px-3 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500 disabled:opacity-50"
                  >
                    <Plus className="h-4 w-4 mr-1" />
                    {t.add}
                  </button>
                  <button
                    onClick={() => {
                      setSelectedPersonnel(null)
                      setObservations('')
                    }}
                    className="px-3 py-2 bg-gray-300 dark:bg-gray-600 text-gray-700 dark:text-gray-300 rounded-md hover:bg-gray-400 dark:hover:bg-gray-500"
                  >
                    {t.cancel}
                  </button>
                </div>
              </div>
            </div>
          )}
        </div>
      </div>

      {/* Current Commanders */}
      <div className="bg-gray-50 dark:bg-gray-900 rounded-lg p-4">
        <h3 className="text-lg font-semibold text-gray-900 dark:text-gray-100 mb-4">
          {t.commanders}
        </h3>

        {sortedCommanders.length === 0 ? (
          <div className="text-center py-8 text-gray-500 dark:text-gray-400">
            <User className="h-12 w-12 mx-auto mb-4 text-gray-300 dark:text-gray-600" />
            <p>{t.noCommandersAssigned}</p>
          </div>
        ) : (
          <div className="space-y-4">
            {sortedCommanders.map((commander) => (
              <div key={commander.id} className="bg-white dark:bg-gray-800 rounded-md p-4 border border-gray-200 dark:border-gray-700">
                <div className="flex items-start justify-between mb-3">
                  <div>
                    <div className="font-medium text-gray-900 dark:text-gray-100">
                      {commander.personnelName}
                    </div>
                    <div className="text-sm text-gray-600 dark:text-gray-400">
                      {translateRank(commander.personnelRank,  t)} - {t.badgeNumber}: {commander.personnelBadgeNumber}
                    </div>
                  </div>
                  <button
                    onClick={() => removeCommanderMutation.mutate(commander.id)}
                    disabled={removeCommanderMutation.isPending}
                    className="text-red-600 hover:text-red-800 p-1"
                  >
                    <X className="h-4 w-4" />
                  </button>
                </div>

                {/* Observations */}
                <div>
                  <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                    {t.observations}
                  </label>
                  <textarea
                    defaultValue={commander.observations || ''}
                    onBlur={(e) => {
                      if (e.target.value !== commander.observations) {
                        updateCommanderMutation.mutate({
                          commanderId: commander.id,
                          observations: e.target.value,
                        })
                      }
                    }}
                    rows={2}
                    className="w-full rounded-md border border-gray-300 dark:border-gray-600 px-3 py-2 text-sm bg-white dark:bg-gray-800 text-gray-900 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-blue-500"
                    placeholder={t.observationsForCommander}
                  />
                </div>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  )
}