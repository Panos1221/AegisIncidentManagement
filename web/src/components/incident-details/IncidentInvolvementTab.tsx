import { useState } from 'react'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { Incident, CreateIncidentInvolvement } from '../../types'
import { incidentsApi } from '../../lib/api'
import { useTranslation } from '../../hooks/useTranslation'
import { useUserAgencyType, getVehicleTypeLabel, getPersonnelLabel } from '../../utils/agencyUtils'

interface IncidentInvolvementTabProps {
  incident: Incident
}

export default function IncidentInvolvementTab({ incident }: IncidentInvolvementTabProps) {
  const queryClient = useQueryClient()
  const t = useTranslation()
  const agencyType = useUserAgencyType()
  const [formData, setFormData] = useState<CreateIncidentInvolvement>({
    fireTrucksNumber: incident.involvement?.fireTrucksNumber || undefined,
    firePersonnel: incident.involvement?.firePersonnel || undefined,
    otherAgencies: incident.involvement?.otherAgencies || '',
    serviceActions: incident.involvement?.serviceActions || '',
    rescuedPeople: incident.involvement?.rescuedPeople || undefined,
    rescueInformation: incident.involvement?.rescueInformation || '',
  })


  const updateMutation = useMutation({
    mutationFn: (data: CreateIncidentInvolvement) =>
      incidentsApi.updateInvolvement(incident.id, data),
    onSuccess: () => {
      // Invalidate both the incidents list and the specific incident
      queryClient.invalidateQueries({ queryKey: ['incidents'] })
      queryClient.invalidateQueries({ queryKey: ['incident'] })
    },
  })

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault()
    updateMutation.mutate(formData)
  }

  const handleInputChange = (field: keyof CreateIncidentInvolvement, value: any) => {
    setFormData(prev => ({ ...prev, [field]: value }))
  }

  return (
    <form onSubmit={handleSubmit} className="space-y-6">
      {/* Personnel Section */}
      <div className="bg-gray-50 dark:bg-gray-900 rounded-lg p-4">
        <h3 className="text-lg font-semibold text-gray-900 dark:text-gray-100 mb-4">
          {t.personnel}
        </h3>

        <div className="space-y-4">
          {/* Vehicles */}
          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              {getVehicleTypeLabel(agencyType, t)}
            </label>
            <input
              type="number"
              min="0"
              value={formData.fireTrucksNumber || ''}
              onChange={(e) =>
                handleInputChange(
                  'fireTrucksNumber',
                  e.target.value ? parseInt(e.target.value) : undefined
                )
              }
              className="w-16 rounded-md border border-gray-300 dark:border-gray-600 px-2 py-1 text-sm bg-white dark:bg-gray-800 text-gray-900 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-blue-500"
              placeholder="0"
            />
          </div>

          {/* Personnel */}
          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              {getPersonnelLabel(agencyType, t)}
            </label>
            <input
              type="number"
              min="0"
              value={formData.firePersonnel || ''}
              onChange={(e) =>
                handleInputChange(
                  'firePersonnel',
                  e.target.value ? parseInt(e.target.value) : undefined
                )
              }
              className="w-16 rounded-md border border-gray-300 dark:border-gray-600 px-2 py-1 text-sm bg-white dark:bg-gray-800 text-gray-900 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-blue-500"
              placeholder="0"
            />
          </div>
        </div>
      </div>

      {/* Other Agencies */}
      <div className="bg-gray-50 dark:bg-gray-900 rounded-lg p-4">
        <h3 className="text-lg font-semibold text-gray-900 dark:text-gray-100 mb-4">
          {t.otherAgencies}
        </h3>
        <textarea
          value={formData.otherAgencies}
          onChange={(e) => handleInputChange('otherAgencies', e.target.value)}
          rows={3}
          className="w-full rounded-md border border-gray-300 dark:border-gray-600 px-3 py-2 text-sm bg-white dark:bg-gray-800 text-gray-900 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-blue-500"
          placeholder={t.describeOtherAgencies}
        />
      </div>

      {/* Service Actions */}
      <div className="bg-gray-50 dark:bg-gray-900 rounded-lg p-4">
        <h3 className="text-lg font-semibold text-gray-900 dark:text-gray-100 mb-4">
          {t.serviceActions}
        </h3>
        <textarea
          value={formData.serviceActions}
          onChange={(e) => handleInputChange('serviceActions', e.target.value)}
          rows={3}
          className="w-full rounded-md border border-gray-300 dark:border-gray-600 px-3 py-2 text-sm bg-white dark:bg-gray-800 text-gray-900 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-blue-500"
          placeholder={t.describeServiceActions}
        />
      </div>

      {/* Rescues Section - Only show for Fire and Coast Guard */}
      {(agencyType === 'fire' || agencyType === 'coastguard') && (
      <div className="bg-gray-50 dark:bg-gray-900 rounded-lg p-4">
        <h3 className="text-lg font-semibold text-gray-900 dark:text-gray-100 mb-4">
          {t.rescues}
        </h3>

        <div className="grid grid-cols-2 gap-0 mb-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              {t.rescuedPeople}
            </label>
            <input
              type="number"
              min="0"
              value={formData.rescuedPeople || ''}
              onChange={(e) => handleInputChange('rescuedPeople', e.target.value ? parseInt(e.target.value) : undefined)}
              className="w-16 rounded-md border border-gray-300 dark:border-gray-600 px-3 py-2 text-sm bg-white dark:bg-gray-800 text-gray-900 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-blue-500"
              placeholder="0"
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              {t.information}
            </label>
            <textarea
                value={formData.rescueInformation}
                onChange={(e) => handleInputChange('rescueInformation', e.target.value)}
                rows={3}
                className="w-full rounded-md border border-gray-300 dark:border-gray-600 px-3 py-2 text-sm bg-white dark:bg-gray-800 text-gray-900 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-blue-500"
                placeholder={t.rescueDetails}
              />
          </div>
        </div>
      </div>
      )}

      {/* Submit Button */}
      <div className="flex justify-end">
        <button
          type="submit"
          disabled={updateMutation.isPending}
          className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500 disabled:opacity-50 disabled:cursor-not-allowed"
        >
          {updateMutation.isPending ? t.save + '...' : t.save}
        </button>
      </div>
    </form>
  )
}