import { useUserStore } from '../lib/userStore'
import { UserRole } from '../types'

export type AgencyType = 'fire' | 'police' | 'coastguard' | 'ekab'

/**
 * Determines agency type from user agency name (primary method)
 */
export const getAgencyTypeFromUser = (user?: { role?: UserRole; agencyName?: string }): AgencyType => {
  if (!user) return 'fire'
  
  // Use agency name as the primary method (like the rest of the system)
  return getAgencyTypeFromName(user.agencyName)
}

/**
 * Determines agency type from agency name
 */
export const getAgencyTypeFromName = (agencyName?: string): AgencyType => {
  if (!agencyName) return 'fire'
  
  // Match the exact agency names used in the system
  switch (agencyName) {
    case 'Hellenic Fire Service':
      return 'fire'
    case 'Hellenic Coast Guard':
      return 'coastguard'
    case 'Hellenic Police':
      return 'police'
    case 'EKAB':
      return 'ekab'
    default:
      // Fallback for partial matches
      const lowerAgency = agencyName.toLowerCase()
      if (lowerAgency.includes('fire')) return 'fire'
      if (lowerAgency.includes('coast')) return 'coastguard'
      if (lowerAgency.includes('police')) return 'police'
      if (lowerAgency.includes('ekab')) return 'ekab'
      return 'fire' // default fallback
  }
}

/**
 * Hook to get current user's agency type
 */
export const useUserAgencyType = (): AgencyType => {
  const { user } = useUserStore()
  return getAgencyTypeFromUser(user ?? undefined)
}

/**
 * Get personnel type label based on agency
 */
export const getPersonnelTypeLabel = (agencyType: AgencyType, type: 'firefighter' | 'civilian', t: any) => {
  if (type === 'civilian') return t.civilians
  
  switch (agencyType) {
    case 'fire':
      return t.firePersonnel || 'Firefighters'
    case 'police':
      return t.policeOfficer || 'Police Officers'
    case 'coastguard':
      return t.coastGuardMembers || 'Coast Guard Members'
    case 'ekab':
      return t.EKABMembers || 'EKAB Members'
    default:
      return t.firePersonnel || 'Firefighters'
  }
}

/**
 * Get vehicle type label based on agency
 */
export const getVehicleTypeLabel = (agencyType: AgencyType, t: any) => {
  switch (agencyType) {
    case 'fire':
      return t.fireTrucksNumber || 'Fire Trucks'
    case 'police':
      return t.policeVehicles || 'Police Cars'
    case 'coastguard':
      return t.coastGuardVehicles || 'Coast Guard Resources'
    case 'ekab':
      return t.EKABVehicles || 'EKAB Vehicles'
    default:
      return t.fireTrucksNumber || 'Fire Trucks'
  }
}

/**
 * Get personnel label based on agency
 */
export const getPersonnelLabel = (agencyType: AgencyType, t: any) => {
  switch (agencyType) {
    case 'fire':
      return t.firePersonnel || 'Fire Personnel'
    case 'police':
      return t.policeOfficer || 'Police Personnel'
    case 'coastguard':
      return t.coastGuardMembers || 'Coast Guard Personnel'
    case 'ekab':
      return t.EKABMembers || 'EKAB Personnel'
    default:
      return t.firePersonnel || 'Fire Personnel'
  }
}
