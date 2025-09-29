import { IncidentStatus, IncidentPriority, VehicleStatus } from '../types'

/**
 * Get background color classes for incident cards based on status
 * These colors work for both light and dark themes
 */
export const getIncidentCardBackgroundColor = (status: IncidentStatus): string => {
  switch (status) {
    case IncidentStatus.Created:
      return 'bg-blue-400 dark:bg-gray-700 border-l-blue-700 dark:border-l-gray-500';
    case IncidentStatus.OnGoing:
      return 'bg-red-500 dark:bg-red-700/70 border-l-red-800 dark:border-l-red-400';
    case IncidentStatus.PartialControl:
      return 'bg-orange-400 dark:bg-orange-800/80 border-l-orange-800 dark:border-l-orange-400';
    case IncidentStatus.Controlled:
      return 'bg-yellow-300 dark:bg-yellow-600/60 border-l-yellow-700 dark:border-l-yellow-400';
    case IncidentStatus.FullyControlled:
      return 'bg-green-400 dark:bg-green-800/50 border-l-green-800 dark:border-l-green-400';
    case IncidentStatus.Closed:
      return 'bg-gray-400 dark:bg-gray-800 border-l-gray-700 dark:border-l-gray-600';
    default:
      return 'bg-gray-400 dark:bg-gray-800 border-l-gray-700 dark:border-l-gray-600';
  }
};

/**
 * Get text color classes for incident cards based on status
 * Ensures proper contrast on colored backgrounds
 */
export const getIncidentCardTextColor = (status: IncidentStatus): string => {
  switch (status) {
    case IncidentStatus.Created:
      return 'text-black dark:text-white';
    case IncidentStatus.OnGoing:
      return 'text-black dark:text-white';
    case IncidentStatus.PartialControl:
      return 'text-black dark:text-white';
    case IncidentStatus.Controlled:
      return 'text-black dark:text-white';
    case IncidentStatus.FullyControlled:
      return 'text-black dark:text-white';
    case IncidentStatus.Closed:
      return 'text-black dark:text-white';
    default:
      return 'text-black dark:text-white';
  }
};

/**
 * Get secondary text color classes for incident cards based on status
 * For smaller text elements like location, time, etc.
 * 0 black, 100 white
 */
export const getIncidentCardSecondaryTextColor = (status: IncidentStatus): string => {
  switch (status) {
    case IncidentStatus.Created:
      return 'text-blue-100 dark:text-gray-300';
    case IncidentStatus.OnGoing:
      return 'text-white-100 dark:text-gray-300';
    case IncidentStatus.PartialControl:
      return 'text-white-100 dark:text-gray-300';
    case IncidentStatus.Controlled:
      return 'text-yellow-800 dark:text-gray-300';
    case IncidentStatus.FullyControlled:
      return 'text-green-100 dark:text-gray-300';
    case IncidentStatus.Closed:
      return 'text-gray-100 dark:text-gray-300';
    default:
      return 'text-gray-100 dark:text-gray-300';
  }
}


/**
 * Get badge color classes for incident status badges
 * Enhanced with subtle colors for better readability and professional appearance
 */
export const getIncidentStatusBadgeColor = (status: IncidentStatus): string => {
  switch (status) {
    case IncidentStatus.Created:
      return 'bg-slate-100 text-slate-700 border border-slate-200 dark:bg-slate-800 dark:text-slate-300 dark:border-slate-600';
    case IncidentStatus.OnGoing:
      return 'bg-rose-100 text-rose-700 border border-rose-200 dark:bg-rose-900/30 dark:text-rose-300 dark:border-rose-700';
    case IncidentStatus.PartialControl:
      return 'bg-amber-100 text-amber-700 border border-amber-200 dark:bg-amber-800/30 dark:text-amber-300 dark:border-amber-700';
    case IncidentStatus.Controlled:
      return 'bg-yellow-100 text-yellow-700 border border-yellow-200 dark:bg-yellow-500/20 dark:text-yellow-200 dark:border-yellow-500';
    case IncidentStatus.FullyControlled:
      return 'bg-emerald-100 text-emerald-700 border border-emerald-200 dark:bg-emerald-900/30 dark:text-emerald-300 dark:border-emerald-700';
    case IncidentStatus.Closed:
      return 'bg-gray-100 text-gray-700 border border-gray-200 dark:bg-gray-800 dark:text-gray-300 dark:border-gray-600';
    default:
      return 'bg-gray-100 text-gray-600 border border-gray-200 dark:bg-gray-800 dark:text-gray-400 dark:border-gray-600';
  }
};

/**
 * Get vibrant priority color classes for incident priority badges
 * Enhanced with more vibrant colors for better dark theme visibility, similar to button styling
 */
export const getIncidentPriorityBadgeColor = (priority: IncidentPriority): string => {
  switch (priority) {
    case IncidentPriority.Critical:
      return 'bg-red-500 text-white dark:bg-red-500 dark:text-white';
    case IncidentPriority.High:
      return 'bg-orange-500 text-white dark:bg-orange-500 dark:text-white';
    case IncidentPriority.Normal:
      return 'bg-yellow-500 text-yellow-900 dark:bg-yellow-500 dark:text-yellow-900';
    case IncidentPriority.Low:
      return 'bg-green-500 text-white dark:bg-green-500 dark:text-white';
    default:
      return 'bg-gray-500 text-white dark:bg-gray-500 dark:text-white';
  }
};

/**
 * Get vibrant status color classes for general status indicators (vehicles, patrol areas, etc.)
 * Enhanced with more vibrant colors for better dark theme visibility
 */
export const getStatusBadgeColor = (status: string): string => {
  switch (status.toLowerCase()) {
    case 'available':
    case 'active':
    case 'covered':
      return 'bg-green-500 text-white dark:bg-green-500 dark:text-white';
    case 'busy':
    case 'limited':
    case 'dispatched':
      return 'bg-yellow-500 text-yellow-900 dark:bg-yellow-500 dark:text-yellow-900';
    case 'maintenance':
    case 'critical':
    case 'uncovered':
      return 'bg-red-500 text-white dark:bg-red-500 dark:text-white';
    case 'offline':
    case 'outofservice':
      return 'bg-gray-500 text-white dark:bg-gray-500 dark:text-white';
    default:
      return 'bg-blue-500 text-white dark:bg-blue-500 dark:text-white';
  }
};


/**
 * Get translated status name
 */
export const getIncidentStatusTranslation = (status: IncidentStatus, t: any): string => {
  switch (status) {
    case IncidentStatus.Created:
      return t.created
    case IncidentStatus.OnGoing:
      return t.onGoing
    case IncidentStatus.PartialControl:
      return t.partialControl
    case IncidentStatus.Controlled:
      return t.controlled
    case IncidentStatus.FullyControlled:
      return t.fullyControlled
    case IncidentStatus.Closed:
      return t.closed
    default:
      return IncidentStatus[status] || t.unknown
  }
}

/**
 * Get all incident status options for dropdowns with translations
 */
export interface IncidentStatusOption {
  value: IncidentStatus;
  label: string;
  key: string;
}

export const getIncidentStatusOptions = (t: any): IncidentStatusOption[] => {
  return Object.entries(IncidentStatus)
    .filter(([key]) => isNaN(Number(key)))
    .map(([key, value]) => ({
      value: value as IncidentStatus,
      label: getIncidentStatusTranslation(value as IncidentStatus, t),
      key
    }))
}

/**
 * Get incident status options for status updates (excludes Created and Closed status)
 */
export const getIncidentStatusUpdateOptions = (t: any): IncidentStatusOption[] => {
  return Object.entries(IncidentStatus)
    .filter(([key, value]) => isNaN(Number(key)) && value !== IncidentStatus.Created && value !== IncidentStatus.Closed)
    .map(([key, value]) => ({
      value: value as IncidentStatus,
      label: getIncidentStatusTranslation(value as IncidentStatus, t),
      key
    }))
}

/**
 * Get badge color classes for vehicle status badges
 * Enhanced with consistent colors for better readability and professional appearance
 */
export const getVehicleStatusBadgeColor = (status: VehicleStatus): string => {
  switch (status) {
    case VehicleStatus.Available:
      return 'bg-green-100 text-green-800 border border-green-200 dark:bg-green-900/30 dark:text-green-300 dark:border-green-700';
    case VehicleStatus.Notified:
      return 'bg-blue-100 text-blue-800 border border-blue-200 dark:bg-blue-900/30 dark:text-blue-300 dark:border-blue-700';
    case VehicleStatus.EnRoute:
      return 'bg-yellow-100 text-yellow-800 border border-yellow-200 dark:bg-yellow-900/30 dark:text-yellow-300 dark:border-yellow-700';
    case VehicleStatus.OnScene:
      return 'bg-orange-100 text-orange-800 border border-orange-200 dark:bg-orange-900/30 dark:text-orange-300 dark:border-orange-700';
    case VehicleStatus.Busy:
      return 'bg-red-100 text-red-800 border border-red-200 dark:bg-red-900/30 dark:text-red-300 dark:border-red-700';
    case VehicleStatus.Maintenance:
      return 'bg-purple-100 text-purple-800 border border-purple-200 dark:bg-purple-900/30 dark:text-purple-300 dark:border-purple-700';
    case VehicleStatus.Offline:
      return 'bg-gray-100 text-gray-800 border border-gray-200 dark:bg-gray-800 dark:text-gray-300 dark:border-gray-600';
    default:
      return 'bg-gray-100 text-gray-600 border border-gray-200 dark:bg-gray-800 dark:text-gray-400 dark:border-gray-600';
  }
};

/**
 * Get translated vehicle status name
 */
export const getVehicleStatusTranslation = (status: VehicleStatus, t: any): string => {
  switch (status) {
    case VehicleStatus.Available:
      return t.available
    case VehicleStatus.Notified:
      return t.notified
    case VehicleStatus.EnRoute:
      return t.enRoute
    case VehicleStatus.OnScene:
      return t.onScene
    case VehicleStatus.Busy:
      return t.busy
    case VehicleStatus.Maintenance:
      return t.maintenance
    case VehicleStatus.Offline:
      return t.offline
    default:
      return VehicleStatus[status] || t.unknown
  }
}

/**
 * Get translated assignment status name (for assignment status strings)
 */
export const getAssignmentStatusTranslation = (status: string, t: any): string => {
  switch (status.toLowerCase()) {
    case 'notified':
      return t.notified
    case 'assigned':
      return t.assigned
    case 'dispatched':
      return t.dispatched
    case 'en route':
    case 'enroute':
      return t.enRoute
    case 'on scene':
    case 'onscene':
      return t.onScene
    case 'finished':
    case 'completed':
      return t.completed
    default:
      return status
  }
}

/**
 * Get translated incident priority text
 */
export const getIncidentPriorityTranslation = (priority: IncidentPriority, t: any): string => {
  switch (priority) {
    case IncidentPriority.Critical:
      return t.criticalPriority
    case IncidentPriority.High:
      return t.highPriority
    case IncidentPriority.Normal:
      return t.normalPriority
    case IncidentPriority.Low:
      return t.lowPriority
    default:
      return t.unknownPriority
  }
}

/**
 * Get icon alt text for accessibility
 */
export const getIncidentIconAltText = (mainCategory: string, status: IncidentStatus, t: any): string => {
  const statusText = getIncidentStatusTranslation(status, t)

  switch (mainCategory) {
    case 'ΔΑΣΙΚΕΣ ΠΥΡΚΑΓΙΕΣ':
    case 'Forest Fires':
      return `Wildfire - ${statusText}`
    case 'ΑΣΤΙΚΕΣ ΠΥΡΚΑΓΙΕΣ':
    case 'Urban Fires':
      return `Fire - ${statusText}`
    case 'ΠΑΡΟΧΕΣ ΒΟΗΘΕΙΑΣ':
    case 'Assistance Callouts':
      return `Assistance - ${statusText}`
    default:
      return `Incident - ${statusText}`
  }
}

/**
 * Check if incident category should use Fire Department specific icons
 */
export const shouldUseFireDepartmentIcons = (agencyName?: string): boolean => {
  return agencyName === 'Hellenic Fire Service'
}

/**
 * Translate and improve activity log messages by replacing IDs with names and translating status messages
 */
export const translateLogMessage = (
  message: string, 
  vehicles: any[] = [], 
  personnel: any[] = [], 
  t: any
): string => {
  // Replace vehicle IDs with names
  const vehiclePattern = /vehicle (\d+)/gi
  let translatedMessage = message.replace(vehiclePattern, (match, id) => {
    const vehicle = vehicles?.find(v => v.id === parseInt(id))
    return vehicle ? `${t.vehicleSingle || 'Vehicle'} ${vehicle.callsign}` : match
  })

  // Replace personnel IDs with names
  const personnelPattern = /personnel (\d+)/gi
  translatedMessage = translatedMessage.replace(personnelPattern, (match, id) => {
    const person = personnel?.find(p => p.id === parseInt(id))
    return person ? `${t.personnel || 'Personnel'} ${person.name}` : match
  })

  // Translate common status messages
  const statusTranslations: { [key: string]: string } = {
    'Incident': t.incident || 'Incident',
    'assigned': t.assigned || 'assigned',
    'dispatched': t.dispatched || 'dispatched',
    'notified': t.notified || 'notified',
    'Notified': t.notified || 'Notified',
    'en route': t.enRoute || 'en route',
    'on scene': t.onScene || 'on scene',
    'On Scene': t.onScene || 'On Scene',
    'OnScene': t.onScene || 'OnScene',   
    'Created': t.created || 'Created',
    'OnGoing': t.onGoing || 'OnGoing',
    'completed': t.completed || 'completed',
    'Finished': t.completed || 'Finished',
    'available': t.available || 'available',
    'Unavailable': t.unavailable || 'Unavailable',
    'Personnel': t.personnel || 'Personnel',
    'Vehicle': t.vehicleSingle || 'Vehicle',
    'status changed': t.statusChanged || 'status changed',
    'priority changed': t.priorityChanged || 'priority changed',
    'incident created': t.incidentCreated || 'incident created',
    'incident updated': t.incidentUpdated || 'incident updated',
    'from': t.from || 'from',
    'Status': t.theStatus || 'Status',     
    'Priority: ': t.priority + ': ' || 'Priority:  ',
    'automatically': t.automatically || 'automatically',
    'changed': t.changed || 'changed',	
    'to': t.to || 'to',
    'High': t.highPriority || 'High',
    'Priority': t.lowPriority || 'Priority',
    'Critical': t.criticalPriority || 'Critical',
    'Normal': t.normalPriority || 'Normal',     
  }

  // Apply status translations
  Object.entries(statusTranslations).forEach(([english, translated]) => {
    const regex = new RegExp(`\\b${english}\\b`, 'gi')
    translatedMessage = translatedMessage.replace(regex, translated)
  })

  return translatedMessage
}