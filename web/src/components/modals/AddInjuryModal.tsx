import { useState } from 'react'
import { Injury } from '../../types'
import { useTranslation } from '../../hooks/useTranslation'
import { useUserAgencyType, getPersonnelTypeLabel } from '../../utils/agencyUtils'

interface AddInjuryModalProps {
  isOpen: boolean
  onClose: () => void
  onSave: (injury: Injury) => void
}

export default function AddInjuryModal({ isOpen, onClose, onSave }: AddInjuryModalProps) {
  const t = useTranslation()
  const agencyType = useUserAgencyType()
  const [formData, setFormData] = useState<Injury>({
    name: '',
    type: 'civilian',
    description: ''
  })

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault()
    if (formData.name.trim()) {
      onSave(formData)
      setFormData({ name: '', type: 'civilian', description: '' })
      onClose()
    }
  }

  const handleClose = () => {
    setFormData({ name: '', type: 'civilian', description: '' })
    onClose()
  }

  if (!isOpen) return null

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
      <div className="bg-white dark:bg-gray-800 rounded-lg p-6 w-full max-w-md">
        <h2 className="text-lg font-semibold text-gray-900 dark:text-gray-100 mb-4">
          {t.add || 'Add'} {t.injury || 'Injury'}
        </h2>
        <form onSubmit={handleSubmit}>
          <div className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                {t.name}
              </label>
              <input
                type="text"
                value={formData.name}
                onChange={(e) => setFormData(prev => ({ ...prev, name: e.target.value }))}
                className="w-full rounded-md border border-gray-300 dark:border-gray-600 px-3 py-2 text-sm bg-white dark:bg-gray-700 text-gray-900 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-blue-500"
                placeholder={t.name}
                required
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                {t.type || 'Type'}
              </label>
              <select
                value={formData.type}
                onChange={(e) => setFormData(prev => ({ ...prev, type: e.target.value as 'firefighter' | 'civilian' }))}
                className="w-full rounded-md border border-gray-300 dark:border-gray-600 px-3 py-2 text-sm bg-white dark:bg-gray-700 text-gray-900 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-blue-500"
              >
                <option value="civilian">{t.civilians}</option>
                <option value="firefighter">{getPersonnelTypeLabel(agencyType, 'firefighter', t)}</option>
              </select>
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                {t.description || 'Description'} ({t.optional || 'Optional'})
              </label>
              <textarea
                value={formData.description}
                onChange={(e) => setFormData(prev => ({ ...prev, description: e.target.value }))}
                className="w-full rounded-md border border-gray-300 dark:border-gray-600 px-3 py-2 text-sm bg-white dark:bg-gray-700 text-gray-900 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-blue-500"
                rows={3}
                placeholder={t.description || 'Description of injury...'}
              />
            </div>
          </div>

          <div className="flex justify-end gap-2 mt-6">
            <button
              type="button"
              onClick={handleClose}
              className="px-4 py-2 text-gray-700 dark:text-gray-300 bg-gray-200 dark:bg-gray-600 rounded-md hover:bg-gray-300 dark:hover:bg-gray-500 focus:outline-none focus:ring-2 focus:ring-gray-500"
            >
              {t.cancel}
            </button>
            <button
              type="submit"
              className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500"
            >
              {t.add || 'Add'}
            </button>
          </div>
        </form>
      </div>
    </div>
  )
}