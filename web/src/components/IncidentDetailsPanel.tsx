import { useState } from 'react'
import { AlertTriangle, Calendar, Phone, MapPin, Users, Shield, FileText } from 'lucide-react'
import { useQuery } from '@tanstack/react-query'
import { Incident, FireStation } from '../types'
import { useTranslation } from '../hooks/useTranslation'
import { useUserStore } from '../lib/userStore'
import { useUserAgencyType } from '../utils/agencyUtils'
import { formatInLocalTimezone } from '../utils/dateUtils'
import { getIncidentStatusBadgeColor, getIncidentStatusTranslation, getIncidentPriorityTranslation } from '../utils/incidentUtils'
import { fireStationsApi } from '../lib/api'
import IncidentInvolvementTab from './incident-details/IncidentInvolvementTab'
import IncidentCommandersTab from './incident-details/IncidentCommandersTab'
import IncidentCasualtiesTab from './incident-details/IncidentCasualtiesTab'

interface IncidentDetailsPanelProps {
  incident: Incident
}

type TabType = 'general' | 'involvement' | 'commanders' | 'casualties'

export default function IncidentDetailsPanel({ incident }: IncidentDetailsPanelProps) {
  const t = useTranslation()
  const { user } = useUserStore()
  const agencyType = useUserAgencyType()
  const [activeTab, setActiveTab] = useState<TabType>('general')

  // Fetch stations for name mapping
  const { data: stations = [] } = useQuery({
    queryKey: ['stations', user?.agencyId],
    queryFn: () => fireStationsApi.getStations(),
    staleTime: 5 * 60 * 1000, // 5 minutes
    enabled: !!user?.agencyId,
  })

  // Helper function to get station name by ID
  const getStationName = (stationId: number): string => {
    const station = stations.find((s: FireStation) => s.id === stationId)
    return station?.name || `${t.station || 'Station'} #${stationId}`
  }

  const getCasualtiesTabLabel = () => {
    if (agencyType === 'fire') {
      return t.casualtiesAccidentsBurns || 'Accidents, Damages, and Burns'
    }
    return `${t.accidents || 'Accidents'} & ${t.damages || 'Damages'}`
  }

  const tabs = [
    { id: 'general' as TabType, label: t.generalInfo || 'General Info', icon: FileText },
    { id: 'involvement' as TabType, label: t.involvement || 'Involvement', icon: Users },
    { id: 'commanders' as TabType, label: t.commanders || 'Commanders', icon: Shield },
    { id: 'casualties' as TabType, label: getCasualtiesTabLabel(), icon: AlertTriangle },
  ]

  return (
    <div className="h-full flex flex-col">
      {/* Header */}
      <div className="p-4 border-b border-gray-200 dark:border-gray-700">
        <h2 className="text-lg font-semibold text-gray-900 dark:text-gray-100 flex items-center">
          <AlertTriangle className="h-5 w-5 mr-2 text-blue-500" />
          {t.incidentDetails || 'Incident Details'}
        </h2>
      </div>

      {/* Tabs */}
      <div className="border-b border-gray-200 dark:border-gray-700">
        <nav className="flex space-x-8 px-4" aria-label="Tabs">
          {tabs.map((tab) => {
            const Icon = tab.icon
            return (
              <button
                key={tab.id}
                onClick={() => setActiveTab(tab.id)}
                className={`${
                  activeTab === tab.id
                    ? 'border-blue-500 text-blue-600 dark:text-blue-400'
                    : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300 dark:text-gray-400 dark:hover:text-gray-300'
                } whitespace-nowrap py-2 px-1 border-b-2 font-medium text-sm flex items-center`}
              >
                <Icon className="h-4 w-4 mr-2" />
                {tab.label}
              </button>
            )
          })}
        </nav>
      </div>

      {/* Tab Content */}
      <div className="flex-1 overflow-y-auto p-4">
        {activeTab === 'general' && (
          <div className="space-y-3">
            {/* Incident Header with ID in top right */}
            <div className="bg-gray-50 dark:bg-gray-900 rounded-lg p-3">
              <div className="flex items-start justify-between mb-2">
                <div className="flex-1">
                  <h3 className="text-lg font-bold text-gray-900 dark:text-gray-100">
                    {incident.mainCategory}
                    {incident.participationType === 'Reinforcement' && (
                      <span className="ml-2 inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-200">
                        {t.reinforcement || 'Reinforcement'}
                      </span>
                    )}
                  </h3>
                  {/* Sub Category directly below main category */}
                  <div className="text-gray-600 dark:text-gray-400 font-medium text-sm mt-1">
                    {incident.subCategory}
                  </div>
                </div>
                <div className="flex flex-col items-end gap-1">
                  <span className="text-xs text-gray-500 dark:text-gray-400 font-mono">
                    #{incident.id}
                  </span>
                  <span className={`inline-flex items-center px-2 py-1 rounded-full text-xs font-medium ${getIncidentStatusBadgeColor(incident.status)}`}>
                    {getIncidentStatusTranslation(incident.status, t)}
                  </span>
                  <span className={`inline-flex items-center px-2 py-1 rounded-full text-xs font-medium ${
                    incident.priority === 1 ? 'bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-200' :
                    incident.priority === 2 ? 'bg-orange-100 text-orange-800 dark:bg-orange-900 dark:text-orange-200' :
                    incident.priority === 3 ? 'bg-yellow-100 text-yellow-800 dark:bg-yellow-900 dark:text-yellow-200' :
                    'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200'
                  }`}>
                    {getIncidentPriorityTranslation(incident.priority, t)}
                  </span>
                </div>
              </div>

              {/* Priority with Responsible Station */}
              <div className="grid grid-cols-2 gap-4 text-sm mb-3">
                <div>
                  <span className="font-bold text-gray-700 dark:text-gray-300">
                    {t.priority || 'Priority'}:
                  </span>
                  <div className="text-gray-900 dark:text-gray-100 font-semibold">
                    {getIncidentPriorityTranslation(incident.priority, t)}
                  </div>
                </div>
                <div>
                  <span className="font-bold text-gray-700 dark:text-gray-300">
                    {t.incidentResponsibleStation || 'Responsible Station'}:
                  </span>
                  <div className="text-gray-900 dark:text-gray-100 font-semibold">
                    {getStationName(incident.stationId)}
                  </div>
                </div>
              </div>

              {/* Basic Information Grid */}
              <div className="grid grid-cols-2 gap-4 text-sm">
                <div>
                  <div className="flex items-center mb-1">
                    <Calendar className="h-4 w-4 mr-2 text-blue-500" />
                    <span className="font-bold text-gray-700 dark:text-gray-300">
                      {t.createdAt || 'Created'}:
                    </span>
                  </div>
                  <div className="text-gray-900 dark:text-gray-100 font-semibold">
                    {formatInLocalTimezone(incident.createdAt, 'dd/MM/yyyy HH:mm')}
                  </div>
                </div>
                <div>
                  <div className="flex items-center mb-1">
                    <Phone className="h-4 w-4 mr-2 text-blue-500" />
                    <span className="font-bold text-gray-700 dark:text-gray-300">
                      {t.caller || 'Caller'}:
                    </span>
                  </div>
                  {incident.callers && incident.callers.length > 0 && (
                    <div className="text-gray-900 dark:text-gray-100 font-semibold">
                      {incident.callers.map(caller => caller.phoneNumber).join(', ')}
                    </div>
                  )}
                </div>
              </div>
            </div>

            {/* Location Information */}
            <div className="bg-gray-50 dark:bg-gray-900 rounded-lg p-4">
              <h4 className="font-bold text-gray-900 dark:text-gray-100 mb-3 flex items-center text-base">
                <MapPin className="h-5 w-5 mr-2 text-green-500" />
                {t.location || 'Location'}
              </h4>

              {/* Address Grid */}
              <div className="grid grid-cols-3 gap-4 text-sm">
                <div>
                  <span className="font-bold text-gray-700 dark:text-gray-300">
                    {t.street || 'Street'}:
                  </span>
                  <div className="text-gray-900 dark:text-gray-100 font-semibold">
                    {incident.street || 'N/A'}
                  </div>
                </div>
                <div>
                  <span className="font-bold text-gray-700 dark:text-gray-300">
                    {t.streetNumber || 'Number'}:
                  </span>
                  <div className="text-gray-900 dark:text-gray-100 font-semibold">
                    {incident.streetNumber || 'N/A'}
                  </div>
                </div>
                <div>
                  <span className="font-bold text-gray-700 dark:text-gray-300">
                    {t.city || 'City'}:
                  </span>
                  <div className="text-gray-900 dark:text-gray-100 font-semibold">
                    {incident.city || 'N/A'}
                  </div>
                </div>
                <div>
                  <span className="font-bold text-gray-700 dark:text-gray-300">
                    {t.postalCode || 'Postal'}:
                  </span>
                  <div className="text-gray-900 dark:text-gray-100 font-semibold">
                    {incident.postalCode || 'N/A'}
                  </div>
                </div>
                <div>
                  <span className="font-bold text-gray-700 dark:text-gray-300">
                    {t.region || 'Region'}:
                  </span>
                  <div className="text-gray-900 dark:text-gray-100 font-semibold">
                    {incident.region || 'N/A'}
                  </div>
                </div>
                <div>
                  <span className="font-bold text-gray-700 dark:text-gray-300">
                    {t.coordinates || 'Coords'}:
                  </span>
                  <div className="text-gray-900 dark:text-gray-100 font-mono font-semibold">
                    {incident.latitude.toFixed(4)}, {incident.longitude.toFixed(4)}
                  </div>
                </div>
              </div>

              {/* Full Address Fallback */}
              {incident.address && !(incident.street || incident.city) && (
                <div className="border-t border-gray-200 dark:border-gray-700 pt-3 mt-3">
                  <span className="font-bold text-gray-700 dark:text-gray-300 text-sm">
                    {t.fullAddress || 'Full Address'}:
                  </span>
                  <div className="text-gray-900 dark:text-gray-100 text-sm font-semibold">
                    {incident.address}
                  </div>
                </div>
              )}
            </div>

            {/* Notes */}
            {incident.notes && (
              <div className="bg-gray-50 dark:bg-gray-900 rounded-lg p-4">
                <h4 className="font-bold text-gray-900 dark:text-gray-100 mb-3 flex items-center text-base">
                  <AlertTriangle className="h-5 w-5 mr-2 text-yellow-500" />
                  {t.notes || 'Notes'}
                </h4>
                <p className="text-gray-700 dark:text-gray-300 text-sm leading-relaxed font-medium">
                  {incident.notes}
                </p>
              </div>
            )}
          </div>
        )}

        {activeTab === 'involvement' && (
          <IncidentInvolvementTab incident={incident} />
        )}

        {activeTab === 'commanders' && (
          <IncidentCommandersTab incident={incident} />
        )}

        {activeTab === 'casualties' && (
          <IncidentCasualtiesTab incident={incident} />
        )}
      </div>
    </div>
  )
}