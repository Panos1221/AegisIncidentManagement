import { X, Filter, Eye, EyeOff, Building2, Shield, Anchor, Truck, AlertTriangle } from 'lucide-react'
import { useTranslation } from '../hooks/useTranslation'

export interface MapFilters {
  // Common filters for all agencies
  incidents: boolean
  vehicles: boolean
  
  // Fire Service specific
  fireStations: boolean
  fireStationBoundaries: boolean
  fireHydrants: boolean
  
  // Police specific
  policeStations: boolean
  patrolZones: boolean
  
  // Coast Guard specific
  coastGuardStations: boolean
  ships: boolean
  
  // EKAB specific
  ambulances: boolean
  hospitals: boolean
}

export interface MapFiltersModalProps {
  isOpen: boolean
  onClose: () => void
  filters: MapFilters
  onFiltersChange: (filters: MapFilters) => void
  userAgency?: string
}

export default function MapFiltersModal({
  isOpen,
  onClose,
  filters,
  onFiltersChange,
  userAgency
}: MapFiltersModalProps) {
  const t = useTranslation()

  if (!isOpen) return null

  const handleFilterToggle = (filterKey: keyof MapFilters) => {
    onFiltersChange({
      ...filters,
      [filterKey]: !filters[filterKey]
    })
  }

  const getFilterIcon = (filterType: string) => {
    switch (filterType) {
      case 'incidents':
        return <AlertTriangle className="w-4 h-4" />
      case 'vehicles':
      case 'ambulances':
        return <Truck className="w-4 h-4" />
      case 'fireStations':
      case 'policeStations':
      case 'coastGuardStations':
      case 'hospitals':
        return <Building2 className="w-4 h-4" />
      case 'patrolZones':
        return <Shield className="w-4 h-4" />
      case 'ships':
        return <Anchor className="w-4 h-4" />
      default:
        return <div className="w-4 h-4 rounded-full bg-current" />
    }
  }

  const getFilterColor = (filterType: string) => {
    switch (filterType) {
      case 'incidents':
        return 'text-red-500'
      case 'vehicles':
      case 'ambulances':
        return 'text-blue-500'
      case 'fireStations':
        return 'text-green-500'
      case 'fireStationBoundaries':
        return 'text-purple-500'
      case 'fireHydrants':
        return 'text-orange-500'
      case 'policeStations':
        return 'text-blue-500'
      case 'hospitals':
        return 'text-red-500'
      case 'patrolZones':
        return 'text-orange-500'
      case 'coastGuardStations':
        return 'text-blue-500'
      case 'ships':
        return 'text-violet-500'
      default:
        return 'text-gray-500'
    }
  }

  const renderFilterToggle = (
    filterKey: keyof MapFilters,
    label: string,
    description?: string
  ) => {
    const isActive = filters[filterKey]
    const colorClass = getFilterColor(filterKey)
    
    return (
      <div
        key={filterKey}
        className={`
          flex items-center justify-between p-3 rounded-lg border transition-all duration-200 cursor-pointer
          ${isActive 
            ? 'bg-blue-50 dark:bg-blue-900/20 border-blue-200 dark:border-blue-800' 
            : 'bg-white dark:bg-gray-800 border-gray-200 dark:border-gray-700 hover:bg-gray-50 dark:hover:bg-gray-700'
          }
        `}
        onClick={() => handleFilterToggle(filterKey)}
      >
        <div className="flex items-center gap-3">
          <div className={`${colorClass}`}>
            {getFilterIcon(filterKey)}
          </div>
          <div>
            <div className="font-medium text-gray-900 dark:text-gray-100">
              {label}
            </div>
            {description && (
              <div className="text-sm text-gray-500 dark:text-gray-400">
                {description}
              </div>
            )}
          </div>
        </div>
        <div className="flex items-center gap-2">
          {isActive ? (
            <Eye className="w-4 h-4 text-blue-500" />
          ) : (
            <EyeOff className="w-4 h-4 text-gray-400" />
          )}
        </div>
      </div>
    )
  }

  const getAgencyFilters = () => {
    const commonFilters = [
      { key: 'incidents' as keyof MapFilters, label: t.activeIncidents, description: t.incidentsFilterDescription },
      { key: 'vehicles' as keyof MapFilters, label: t.vehicles, description: t.vehiclesFilterDescription }
    ]

    switch (userAgency) {
      case 'Hellenic Fire Service':
        return [
          ...commonFilters,
          { key: 'fireStations' as keyof MapFilters, label: t.fireStations, description: t.fireStationsFilterDescription },
          { key: 'fireStationBoundaries' as keyof MapFilters, label: t.fireStationDistricts, description: t.fireStationBoundariesFilterDescription },
          { key: 'fireHydrants' as keyof MapFilters, label: t.hydrants, description: t.fireHydrantsFilterDescription }
        ]
      
      case 'Hellenic Police':
        return [
          ...commonFilters,
          { key: 'policeStations' as keyof MapFilters, label: t.policeStations, description: t.policeStationsFilterDescription },
          { key: 'patrolZones' as keyof MapFilters, label: t.patrolZone, description: t.patrolZoneDescription }
        ]
      
      case 'Hellenic Coast Guard':
        return [
          ...commonFilters,
          { key: 'coastGuardStations' as keyof MapFilters, label: t.coastGuardStations, description: t.coastGuardStationsFilterDescription },
          { key: 'ships' as keyof MapFilters, label: t.vessels, description: t.shipsFilterDescription },
          { key: 'patrolZones' as keyof MapFilters, label: t.patrolZone, description: t.patrolZoneDescription }
        ]
      
      case 'EKAB':
        return [
          ...commonFilters,
          { key: 'ambulances' as keyof MapFilters, label: t.ambulance, description: t.ambulancesFilterDescription },
          { key: 'hospitals' as keyof MapFilters, label: t.hospitals, description: t.hospitalsFilterDescription }
        ]
      
      default:
        return commonFilters
    }
  }

  return (
    <div 
      className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-[9999] p-4"
      onClick={onClose}
    >
      <div 
        className={`
          w-full max-w-md bg-white dark:bg-gray-800 rounded-lg shadow-xl border border-gray-200 dark:border-gray-700
          max-h-[90vh] overflow-hidden flex flex-col
        `}
        onClick={(e) => e.stopPropagation()}
      >
        {/* Header */}
        <div className="flex items-center justify-between p-4 border-b border-gray-200 dark:border-gray-700">
          <div className="flex items-center gap-2">
            <Filter className="w-5 h-5 text-blue-500" />
            <h2 className="text-lg font-semibold text-gray-900 dark:text-gray-100">
              {t.mapFilters}
            </h2>
          </div>
          <button
            onClick={onClose}
            className="p-1 text-gray-400 hover:text-gray-600 dark:hover:text-gray-300 transition-colors"
          >
            <X className="w-5 h-5" />
          </button>
        </div>

        {/* Content */}
        <div className="flex-1 overflow-y-auto p-4">
          <div className="space-y-3">
            <div className="mb-4">
              <h3 className="text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                {t.availableFilters}
              </h3>
              <p className="text-xs text-gray-500 dark:text-gray-400">
                {t.filtersDescription}
              </p>
            </div>
            
            {getAgencyFilters().map(filter => 
              renderFilterToggle(filter.key, filter.label, filter.description)
            )}
          </div>
        </div>

        {/* Footer */}
        <div className="p-4 border-t border-gray-200 dark:border-gray-700">
          <div className="flex gap-2">
            <button
              onClick={() => {
                // Reset all filters to true (show all)
                const resetFilters = Object.keys(filters).reduce((acc, key) => {
                  acc[key as keyof MapFilters] = true
                  return acc
                }, {} as MapFilters)
                onFiltersChange(resetFilters)
              }}
              className="flex-1 px-3 py-2 text-sm font-medium text-gray-700 dark:text-gray-300 bg-gray-100 dark:bg-gray-700 hover:bg-gray-200 dark:hover:bg-gray-600 rounded-lg transition-colors"
            >
              {t.showAll}
            </button>
            <button
              onClick={() => {
                // Hide all filters except incidents (keep incidents visible for safety)
                const hideAllFilters = Object.keys(filters).reduce((acc, key) => {
                  acc[key as keyof MapFilters] = key === 'incidents'
                  return acc
                }, {} as MapFilters)
                onFiltersChange(hideAllFilters)
              }}
              className="flex-1 px-3 py-2 text-sm font-medium text-gray-700 dark:text-gray-300 bg-gray-100 dark:bg-gray-700 hover:bg-gray-200 dark:hover:bg-gray-600 rounded-lg transition-colors"
            >
              {t.hideAll}
            </button>
            <button
              onClick={onClose}
              className="flex-1 px-3 py-2 text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 dark:bg-blue-700 dark:hover:bg-blue-600 rounded-lg transition-colors"
            >
              {t.apply}
            </button>
          </div>
        </div>
      </div>
    </div>
  )
}