import { useState } from 'react'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { Incident, CreateIncidentFire, CreateIncidentDamage, Injury, Death } from '../../types'
import { incidentsApi } from '../../lib/api'
import { useTranslation } from '../../hooks/useTranslation'
import { useUserAgencyType, getPersonnelTypeLabel } from '../../utils/agencyUtils'
import AddInjuryModal from '../modals/AddInjuryModal'
import AddDeathModal from '../modals/AddDeathModal'

interface IncidentCasualtiesTabProps {
  incident: Incident
}

export default function IncidentCasualtiesTab({ incident }: IncidentCasualtiesTabProps) {
  const queryClient = useQueryClient()
  const t = useTranslation()
  const agencyType = useUserAgencyType()

  // Modal states
  const [isInjuryModalOpen, setIsInjuryModalOpen] = useState(false)
  const [isDeathModalOpen, setIsDeathModalOpen] = useState(false)

  // Casualty state
  const [injuries, setInjuries] = useState<Injury[]>(incident.injuries || [])
  const [deaths, setDeaths] = useState<Death[]>(incident.deaths || [])

  // Fire state
  const [fireData, setFireData] = useState<CreateIncidentFire>({
    burnedArea: incident.fire?.burnedArea || '',
    burnedItems: incident.fire?.burnedItems || '',
  })

  // Damage state
  const [damageData, setDamageData] = useState<CreateIncidentDamage>({
    ownerName: incident.damage?.ownerName || '',
    tenantName: incident.damage?.tenantName || '',
    damageAmount: incident.damage?.damageAmount || undefined,
    savedProperty: incident.damage?.savedProperty || undefined,
    incidentCause: incident.damage?.incidentCause || '',
  })


  const updateCasualtiesMutation = useMutation({
    mutationFn: (data: { injuries: Injury[], deaths: Death[] }) =>
      incidentsApi.updateCasualties(incident.id, data),
    onSuccess: () => {
      // Invalidate both the incidents list and the specific incident
      queryClient.invalidateQueries({ queryKey: ['incidents'] })
      queryClient.invalidateQueries({ queryKey: ['incident'] })
    },
  })

  const updateFireMutation = useMutation({
    mutationFn: (data: CreateIncidentFire) =>
      incidentsApi.updateFire(incident.id, data),
    onSuccess: () => {
      // Invalidate both the incidents list and the specific incident
      queryClient.invalidateQueries({ queryKey: ['incidents'] })
      queryClient.invalidateQueries({ queryKey: ['incident'] })
    },
  })

  const updateDamageMutation = useMutation({
    mutationFn: (data: CreateIncidentDamage) =>
      incidentsApi.updateDamage(incident.id, data),
    onSuccess: () => {
      // Invalidate both the incidents list and the specific incident
      queryClient.invalidateQueries({ queryKey: ['incidents'] })
      queryClient.invalidateQueries({ queryKey: ['incident'] })
    },
  })

  const handleCasualtySubmit = (e: React.FormEvent) => {
    e.preventDefault()
    updateCasualtiesMutation.mutate({ injuries, deaths })
  }

  const handleFireSubmit = (e: React.FormEvent) => {
    e.preventDefault()
    updateFireMutation.mutate(fireData)
  }

  const handleDamageSubmit = (e: React.FormEvent) => {
    e.preventDefault()
    updateDamageMutation.mutate(damageData)
  }

  const handleAddInjury = (injury: Injury) => {
    setInjuries(prev => [...prev, injury])
  }

  const handleRemoveInjury = (index: number) => {
    setInjuries(prev => prev.filter((_, i) => i !== index))
  }

  const handleAddDeath = (death: Death) => {
    setDeaths(prev => [...prev, death])
  }

  const handleRemoveDeath = (index: number) => {
    setDeaths(prev => prev.filter((_, i) => i !== index))
  }

  return (
    <div className="space-y-6">
      {/* Casualties Section */}
      <form onSubmit={handleCasualtySubmit} className="bg-gray-50 dark:bg-gray-900 rounded-lg p-4">
        <h3 className="text-lg font-semibold text-gray-900 dark:text-gray-100 mb-4">
          {agencyType === 'fire' ? t.accidents : `${t.accidents} & ${t.damages}`}
        </h3>

        {/* Injuries */}
        <div className="mb-6">
          <div className="flex justify-between items-center mb-3">
            <h4 className="font-medium text-gray-900 dark:text-gray-100">{t.injuries}</h4>
            <button
              type="button"
              onClick={() => setIsInjuryModalOpen(true)}
              className="px-3 py-1 bg-green-600 text-white text-sm rounded-md hover:bg-green-700 focus:outline-none focus:ring-2 focus:ring-green-500"
            >
              + {t.add || 'Add'} {t.injury || 'Injury'}
            </button>
          </div>

          {injuries && injuries.length > 0 ? (
            <div className="space-y-2">
              {injuries.map((injury, index) => (
                <div key={index} className="flex items-center justify-between bg-yellow-50 dark:bg-yellow-900/20 border border-yellow-200 dark:border-yellow-800 rounded-md p-3">
                  <div className="flex-1">
                    <div className="flex items-center gap-2">
                      <span className="font-medium text-gray-900 dark:text-gray-100">{injury.name}</span>
                      <span className="px-2 py-1 text-xs rounded-full bg-blue-100 dark:bg-blue-900 text-blue-800 dark:text-blue-200">
                        {getPersonnelTypeLabel(agencyType, injury.type, t)}
                      </span>
                    </div>
                    {injury.description && (
                      <p className="text-sm text-gray-600 dark:text-gray-400 mt-1">{injury.description}</p>
                    )}
                  </div>
                  <button
                    type="button"
                    onClick={() => handleRemoveInjury(index)}
                    className="ml-2 p-1 text-red-600 hover:text-red-800 dark:text-red-400 dark:hover:text-red-300"
                  >
                    <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                    </svg>
                  </button>
                </div>
              ))}
            </div>
          ) : (
            <p className="text-gray-500 dark:text-gray-400 text-sm italic">{t.noInjuries || 'No injuries recorded'}</p>
          )}
        </div>

        {/* Deaths */}
        <div className="mb-6">
          <div className="flex justify-between items-center mb-3">
            <h4 className="font-medium text-gray-900 dark:text-gray-100">{t.deaths}</h4>
            <button
              type="button"
              onClick={() => setIsDeathModalOpen(true)}
              className="px-3 py-1 bg-red-600 text-white text-sm rounded-md hover:bg-red-700 focus:outline-none focus:ring-2 focus:ring-red-500"
            >
              + {t.add || 'Add'} {t.death || 'Death'}
            </button>
          </div>

          {deaths && deaths.length > 0 ? (
            <div className="space-y-2">
              {deaths.map((death, index) => (
                <div key={index} className="flex items-center justify-between bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-md p-3">
                  <div className="flex-1">
                    <div className="flex items-center gap-2">
                      <span className="font-medium text-gray-900 dark:text-gray-100">{death.name}</span>
                      <span className="px-2 py-1 text-xs rounded-full bg-blue-100 dark:bg-blue-900 text-blue-800 dark:text-blue-200">
                        {getPersonnelTypeLabel(agencyType, death.type, t)}
                      </span>
                    </div>
                    {death.description && (
                      <p className="text-sm text-gray-600 dark:text-gray-400 mt-1">{death.description}</p>
                    )}
                  </div>
                  <button
                    type="button"
                    onClick={() => handleRemoveDeath(index)}
                    className="ml-2 p-1 text-red-600 hover:text-red-800 dark:text-red-400 dark:hover:text-red-300"
                  >
                    <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                    </svg>
                  </button>
                </div>
              ))}
            </div>
          ) : (
            <p className="text-gray-500 dark:text-gray-400 text-sm italic">{t.noDeaths || 'No deaths recorded'}</p>
          )}
        </div>

        <div className="flex justify-end">
          <button
            type="submit"
            disabled={updateCasualtiesMutation.isPending}
            className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500 disabled:opacity-50"
          >
            {updateCasualtiesMutation.isPending ? `${t.save}...` : `${t.save}`}
          </button>
        </div>
      </form>

      {/* Fire Section - Only show for Fire Department */}
      {agencyType === 'fire' && (
      <form onSubmit={handleFireSubmit} className="bg-gray-50 dark:bg-gray-900 rounded-lg p-4">
        <h3 className="text-lg font-semibold text-gray-900 dark:text-gray-100 mb-4">
          {t.burned}
        </h3>

        <div className="grid grid-cols-2 gap-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
              {t.burnedArea}
            </label>
            <textarea
              value={fireData.burnedArea}
              onChange={(e) => setFireData(prev => ({ ...prev, burnedArea: e.target.value }))}
              rows={3}
              className="w-full rounded-md border border-gray-300 dark:border-gray-600 px-3 py-2 text-sm bg-white dark:bg-gray-800 text-gray-900 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-blue-500"
              placeholder={`${t.incidentDescription} ${t.burnedArea.toLowerCase()}...`}
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
              {t.burnedItems}
            </label>
            <textarea
              value={fireData.burnedItems}
              onChange={(e) => setFireData(prev => ({ ...prev, burnedItems: e.target.value }))}
              rows={3}
              className="w-full rounded-md border border-gray-300 dark:border-gray-600 px-3 py-2 text-sm bg-white dark:bg-gray-800 text-gray-900 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-blue-500"
              placeholder={`${t.burnedItems}...`}
            />
          </div>
        </div>

        <div className="flex justify-end mt-4">
          <button
            type="submit"
            disabled={updateFireMutation.isPending}
            className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500 disabled:opacity-50"
          >
            {updateFireMutation.isPending ? `${t.save}...` : `${t.save}`}
          </button>
        </div>
      </form>
      )}

      {/* Damage Section - Show for Fire, Police, and Coast Guard */}
      {(agencyType === 'fire' || agencyType === 'police' || agencyType === 'coastguard') && (
      <form onSubmit={handleDamageSubmit} className="bg-gray-50 dark:bg-gray-900 rounded-lg p-4">
        <h3 className="text-lg font-semibold text-gray-900 dark:text-gray-100 mb-4">
          {t.damages}
        </h3>

        <div className="grid grid-cols-2 gap-4 mb-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
              {t.ownerName}
            </label>
            <input
              type="text"
              value={damageData.ownerName}
              onChange={(e) => setDamageData(prev => ({ ...prev, ownerName: e.target.value }))}
              className="w-full rounded-md border border-gray-300 dark:border-gray-600 px-3 py-2 text-sm bg-white dark:bg-gray-800 text-gray-900 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-blue-500"
              placeholder={t.ownerName}
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
              {t.tenantName}
            </label>
            <input
              type="text"
              value={damageData.tenantName}
              onChange={(e) => setDamageData(prev => ({ ...prev, tenantName: e.target.value }))}
              className="w-full rounded-md border border-gray-300 dark:border-gray-600 px-3 py-2 text-sm bg-white dark:bg-gray-800 text-gray-900 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-blue-500"
              placeholder={t.tenantName}
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
              {t.damageAmount} (€)
            </label>
            <input
              type="number"
              min="0"
              step="0.01"
              value={damageData.damageAmount || ''}
              onChange={(e) => setDamageData(prev => ({
                ...prev,
                damageAmount: e.target.value ? parseFloat(e.target.value) : undefined
              }))}
              className="w-full rounded-md border border-gray-300 dark:border-gray-600 px-3 py-2 text-sm bg-white dark:bg-gray-800 text-gray-900 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-blue-500"
              placeholder="0.00"
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
              {t.savedProperty} (€)
            </label>
            <input
              type="number"
              min="0"
              step="0.01"
              value={damageData.savedProperty || ''}
              onChange={(e) => setDamageData(prev => ({
                ...prev,
                savedProperty: e.target.value ? parseFloat(e.target.value) : undefined
              }))}
              className="w-full rounded-md border border-gray-300 dark:border-gray-600 px-3 py-2 text-sm bg-white dark:bg-gray-800 text-gray-900 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-blue-500"
              placeholder="0.00"
            />
          </div>
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
            {t.incidentCause}
          </label>
          <textarea
            value={damageData.incidentCause}
            onChange={(e) => setDamageData(prev => ({ ...prev, incidentCause: e.target.value }))}
            rows={3}
            className="w-full rounded-md border border-gray-300 dark:border-gray-600 px-3 py-2 text-sm bg-white dark:bg-gray-800 text-gray-900 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-blue-500"
            placeholder={`${t.incidentDescription} ${t.incidentCause.toLowerCase()}...`}
          />
        </div>

        <div className="flex justify-end mt-4">
          <button
            type="submit"
            disabled={updateDamageMutation.isPending}
            className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500 disabled:opacity-50"
          >
            {updateDamageMutation.isPending ? `${t.save}...` : `${t.save}`}
          </button>
        </div>
      </form>
      )}

      {/* Modals */}
      <AddInjuryModal
        isOpen={isInjuryModalOpen}
        onClose={() => setIsInjuryModalOpen(false)}
        onSave={handleAddInjury}
      />

      <AddDeathModal
        isOpen={isDeathModalOpen}
        onClose={() => setIsDeathModalOpen(false)}
        onSave={handleAddDeath}
      />
    </div>
  )
}