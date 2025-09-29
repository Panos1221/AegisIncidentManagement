// Import Fire Department specific icons
import fireLocationControlled from '../icons/Incidents/FireDept/fire-location-controlled.png'
import fireLocationEnded from '../icons/Incidents/FireDept/fire-location-ended.png'
import fireLocationOngoing from '../icons/Incidents/FireDept/fire-location-ongoing.png'
import fireLocationPartial from '../icons/Incidents/FireDept/fire-location-partial.png'
import wildfireControlled from '../icons/Incidents/FireDept/wildfire-controlled.png'
import wildfireEnded from '../icons/Incidents/FireDept/wildfire-ended.png'
import wildfireOngoing from '../icons/Incidents/FireDept/wildfire-ongoing.png'
import wildfirePartial from '../icons/Incidents/FireDept/wildfire-partial.png'

// Import Universal assistance icons
import helpLocationControlled from '../icons/Incidents/Universal/help-location-controlled.png'
import helpLocationEnded from '../icons/Incidents/Universal/help-location-ended.png'
import helpLocationOngoing from '../icons/Incidents/Universal/help-location-ongoing.png'
import helpLocationPartial from '../icons/Incidents/Universal/help-location-partial.png'

// Import Leaflet for icon creation
import L from 'leaflet'
import { IncidentStatus } from '../types'

// Function to get the correct icon path based on category and status
export const getIncidentIconPath = (mainCategory: string, status: IncidentStatus): string => {
  const statusKey = getStatusKey(status)

  // Map main categories to icon types
  switch (mainCategory) {
    case 'ΔΑΣΙΚΕΣ ΠΥΡΚΑΓΙΕΣ':
    case 'Forest Fires':
      return getWildfireIcon(statusKey)

    case 'ΑΣΤΙΚΕΣ ΠΥΡΚΑΓΙΕΣ':
    case 'Urban Fires':
      return getFireLocationIcon(statusKey)

    case 'ΠΑΡΟΧΕΣ ΒΟΗΘΕΙΑΣ':
    case 'Assistance Callouts':
    case 'ΕΓΚΛΩΒΙΣΜΟΣ':
    case 'Entrapment (Elevator Call)':
    case 'ΔΡΑΣΤΗΡΙΟΤΗΤΑ':
    case 'Activities':
      return getHelpLocationIcon(statusKey)

    default:
      return getFireLocationIcon(statusKey) // Default to fire location icons
  }
}

// Helper function to convert IncidentStatus to icon status key
const getStatusKey = (status: IncidentStatus): string => {
  switch (status) {
    case IncidentStatus.OnGoing:
      return 'ongoing'
    case IncidentStatus.PartialControl:
      return 'partial'
    case IncidentStatus.Controlled:
      return 'controlled'
    case IncidentStatus.FullyControlled:
    case IncidentStatus.Closed:
      return 'ended'
    default:
      return 'ongoing'
  }
}

// Icon getter functions
const getWildfireIcon = (statusKey: string): string => {
  switch (statusKey) {
    case 'ongoing': return wildfireOngoing
    case 'partial': return wildfirePartial
    case 'controlled': return wildfireControlled
    case 'ended': return wildfireEnded
    default: return wildfireOngoing
  }
}

const getFireLocationIcon = (statusKey: string): string => {
  switch (statusKey) {
    case 'ongoing': return fireLocationOngoing
    case 'partial': return fireLocationPartial
    case 'controlled': return fireLocationControlled
    case 'ended': return fireLocationEnded
    default: return fireLocationOngoing
  }
}

const getHelpLocationIcon = (statusKey: string): string => {
  switch (statusKey) {
    case 'ongoing': return helpLocationOngoing
    case 'partial': return helpLocationPartial
    case 'controlled': return helpLocationControlled
    case 'ended': return helpLocationEnded
    default: return helpLocationOngoing
  }
}

// Create Leaflet icon object from icon path
export const createIncidentIcon = (iconPath: string): L.Icon => {
  return new L.Icon({
    iconUrl: iconPath,
    shadowUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.7.1/images/marker-shadow.png',
    iconSize: [42, 42],
    iconAnchor: [16, 32],
    popupAnchor: [0, -32],
    shadowSize: [41, 41]
  })
}

// Main function to get Leaflet icon for Fire Department incidents
export const getFireDepartmentIncidentIcon = (mainCategory: string, status: IncidentStatus): L.Icon => {
  const iconPath = getIncidentIconPath(mainCategory, status)
  return createIncidentIcon(iconPath)
}

// Main function to get Leaflet icon for EKAB incidents (always uses helpLocation icons)
export const getEKABIncidentIcon = (status: IncidentStatus): L.Icon => {
  const statusKey = getStatusKey(status)
  const iconPath = getHelpLocationIcon(statusKey)
  return createIncidentIcon(iconPath)
}