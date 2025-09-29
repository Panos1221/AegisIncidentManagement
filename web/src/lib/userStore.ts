import { create } from 'zustand'
import { persist, createJSONStorage } from 'zustand/middleware'
import { User, UserRole } from '../types'

interface UserState {
  user: User | null
  isAuthenticated: boolean
  setUser: (user: User | null) => void
  logout: () => void
  isDispatcher: () => boolean
  isFirefighter: () => boolean
  canCreateIncidents: () => boolean
  canAssignResources: () => boolean
  canManageStation: () => boolean
  canAccessAgency: (agencyId: number) => boolean
  canAccessStation: (stationId: number) => boolean
  canViewIncidentsList: () => boolean
  canViewVehiclesList: () => boolean
  canViewPersonnelRoster: () => boolean
  canEditVehicle: (vehicleStationId?: number) => boolean
  canEditPersonnel: (personnelStationId?: number) => boolean
  getPermissionLevel: () => string
  canViewIncident: (incidentStationId?: number) => boolean
  canAddIncidentNotes: (incidentStationId?: number) => boolean
  canModifyIncident: () => boolean
  getUserAgencyId: () => number | undefined
  getUserStationId: () => number | undefined
}

export const useUserStore = create<UserState>()(
  persist(
    (set, get) => ({
      user: null,
      isAuthenticated: false,
      
      setUser: (user) => set({ user, isAuthenticated: !!user }),
      
      logout: () => set({ user: null, isAuthenticated: false }),
      
      isDispatcher: () => {
        const { user } = get()
        return user?.role === UserRole.Dispatcher || 
               user?.role === UserRole.FireDispatcher ||
               user?.role === UserRole.CoastGuardDispatcher ||
               user?.role === UserRole.EKABDispatcher
      },
      
      isFirefighter: () => {
        const { user } = get()
        return user?.role === UserRole.Firefighter ||
               user?.role === UserRole.CoastGuardMember ||
               user?.role === UserRole.EKABMember
      },
      
      canCreateIncidents: () => {
        const { user } = get()
        return user?.role === UserRole.Dispatcher || 
               user?.role === UserRole.FireDispatcher ||
               user?.role === UserRole.CoastGuardDispatcher ||
               user?.role === UserRole.EKABDispatcher
      },
      
      canAssignResources: () => {
        const { user } = get()
        return user?.role === UserRole.Dispatcher || 
               user?.role === UserRole.FireDispatcher ||
               user?.role === UserRole.CoastGuardDispatcher ||
               user?.role === UserRole.EKABDispatcher
      },
      
      canManageStation: () => {
        const { user } = get()
        return user?.role === UserRole.Firefighter ||
               user?.role === UserRole.CoastGuardMember ||
               user?.role === UserRole.EKABMember
      },
  
  // Agency-based filtering functions
  canAccessAgency: (agencyId: number) => {
    const { user } = get()
    return user?.agencyId === agencyId
  },
  
  canAccessStation: (stationId: number) => {
    const { user } = get()
    // Dispatchers can access all stations in their agency (API validates agency)
    if (get().isDispatcher()) {
      return true // Agency filtering handled at API level
    }
    // Regular members can only access their assigned station
    return user?.stationId === stationId
  },

  // Role-specific permission checks
  canViewIncidentsList: () => {
    // All authenticated users can view incidents (filtered by their permissions)
    return true
  },

  canViewVehiclesList: () => {
    // All authenticated users can view vehicles (filtered by their permissions)
    return true
  },

  canViewPersonnelRoster: () => {
    // Only dispatchers can view personnel roster
    return get().isDispatcher()
  },

  canEditVehicle: (vehicleStationId?: number) => {
    const { user } = get()
    if (!user) return false
    
    // Dispatchers cannot edit vehicle details, only assign them
    if (get().isDispatcher()) return false
    
    // Regular members can edit vehicles at their station
    return user.stationId === vehicleStationId
  },

  canEditPersonnel: (personnelStationId?: number) => {
    const { user } = get()
    if (!user) return false
    
    // Dispatchers cannot edit personnel details
    if (get().isDispatcher()) return false
    
    // Regular members can edit personnel at their station
    return user.stationId === personnelStationId
  },

  getPermissionLevel: () => {
    const { user } = get()
    if (!user) return 'none'
    
    if (get().isDispatcher()) return 'dispatcher'
    if (get().isFirefighter()) return 'member'
    
    return 'none'
  },

  // Incident-specific permissions
  canViewIncident: (incidentStationId?: number) => {
    const { user } = get()
    if (!user) return false
    
    // Dispatchers can view all incidents in their agency
    if (get().isDispatcher()) return true
    
    // Agency members can only view incidents at their station
    return user.stationId === incidentStationId
  },

  canAddIncidentNotes: (incidentStationId?: number) => {
    const { user } = get()
    if (!user) return false
    
    // Dispatchers can add notes to any incident in their agency
    if (get().isDispatcher()) return true
    
    // Agency members can add notes to incidents at their station
    return user.stationId === incidentStationId
  },

  canModifyIncident: () => {
    // Only dispatchers can modify incident status, assign resources, etc.
    return get().isDispatcher()
  },
  
  getUserAgencyId: () => {
    const { user } = get()
    return user?.agencyId
  },
  
  getUserStationId: () => {
    const { user } = get()
    return user?.stationId
  },
    }),
    {
      name: 'user-storage',
      storage: createJSONStorage(() => localStorage),
    }
  )
)