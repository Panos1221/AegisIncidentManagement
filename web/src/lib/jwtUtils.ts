import { User, UserRole } from '../types'

interface JWTPayload {
  nameid: string
  email: string
  name: string
  role: string
  AgencyId: string
  StationId: string
  exp: number
}

export const decodeJWT = (token: string): JWTPayload | null => {
  try {
    const base64Url = token.split('.')[1]
    const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/')
    const jsonPayload = decodeURIComponent(
      atob(base64)
        .split('')
        .map(c => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
        .join('')
    )
    return JSON.parse(jsonPayload)
  } catch (error) {
    console.error('Error decoding JWT:', error)
    return null
  }
}

export const isTokenExpired = (token: string): boolean => {
  const payload = decodeJWT(token)
  if (!payload) return true
  
  const currentTime = Date.now() / 1000
  return payload.exp < currentTime
}

export const getUserFromToken = (token: string): User | null => {
  const payload = decodeJWT(token)
  if (!payload || isTokenExpired(token)) return null

  // Map string role to UserRole enum
  let role: UserRole
  switch (payload.role) {
    case 'Dispatcher':
      role = UserRole.Dispatcher
      break
    case 'FireDispatcher':
      role = UserRole.FireDispatcher
      break
    case 'Firefighter':
      role = UserRole.Firefighter
      break
    case 'CoastGuardDispatcher':
      role = UserRole.CoastGuardDispatcher
      break
    case 'CoastGuardMember':
      role = UserRole.CoastGuardMember
      break
    case 'EKABDispatcher':
      role = UserRole.EKABDispatcher
      break
    case 'EKABMember':
      role = UserRole.EKABMember
      break
    default:
      console.error('Unknown role:', payload.role)
      return null
  }

  return {
    id: parseInt(payload.nameid),
    supabaseUserId: payload.nameid,
    email: payload.email,
    name: payload.name,
    role: role,
    agencyId: parseInt(payload.AgencyId),
    agencyName: '', // Will be fetched separately if needed
    stationId: payload.StationId ? parseInt(payload.StationId) : undefined,
    stationName: '', // Will be fetched separately if needed
    isActive: true,
    createdAt: new Date().toISOString(),
  }
}