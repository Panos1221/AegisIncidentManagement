import axios, { AxiosError } from 'axios'
import {
  Incident,
  Vehicle,
  CreateIncident,
  CloseIncidentDto,
  IncidentStatus,
  Personnel,
  CreatePersonnel,
  User,
  CreateUser,
  Notification,
  VehicleAssignment,
  CreateVehicleAssignment,
  FireStation,
  FireStationBoundary,
  LocationDto,
  UpdateVehicleDto,
  Ship,
  PatrolZone,
  CreatePatrolZone,
  UpdatePatrolZone,
  PatrolZoneAssignment,
  CreatePatrolZoneAssignment,
  IncidentTypesByAgency,
  FireHydrant,
  CoastGuardStation,
  PoliceStation,
  Hospital,
  StationAssignmentRequestDto,
  StationAssignmentResponseDto,
  CreateIncidentInvolvement,
  CreateIncidentCommander,
  IncidentCommander,
  CreateIncidentFire,
  CreateIncidentDamage,
  Injury,
  Death
} from '../types'

// Custom error class for API errors
export class ApiError extends Error {
  constructor(
    message: string,
    public status?: number,
    public code?: string,
    public originalError?: AxiosError
  ) {
    super(message)
    this.name = 'ApiError'
  }
}

// Helper function to create user-friendly error messages
const createErrorMessage = (error: AxiosError): string => {
  if (!error.response) {
    return 'Network error. Please check your internet connection and try again.'
  }

  const status = error.response.status
  const data = error.response.data as any

  switch (status) {
    case 400:
      return data?.message || 'Invalid request. Please check your input and try again.'
    case 401:
      return 'Authentication required. Please log in and try again.'
    case 403:
      return 'You do not have permission to perform this action.'
    case 404:
      return 'The requested resource was not found.'
    case 409:
      return data?.message || 'A conflict occurred. The resource may already exist.'
    case 422:
      return data?.message || 'Validation failed. Please check your input.'
    case 500:
      return 'Server error. Please try again later.'
    case 502:
    case 503:
    case 504:
      return 'Service temporarily unavailable. Please try again in a few moments.'
    default:
      return data?.message || `An error occurred (${status}). Please try again.`
  }
}

// Axios instance with error handling
const api = axios.create({
  baseURL: '/api',
  headers: {
    'Content-Type': 'application/json',
  },
  timeout: 30000, // 30 second timeout
})

// Request interceptor to add auth token
api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('authToken')
    if (token) {
      config.headers.Authorization = `Bearer ${token}`
    }
    return config
  },
  (error) => {
    return Promise.reject(error)
  }
)

// Response interceptor for error handling
api.interceptors.response.use(
  (response) => response,
  (error: AxiosError) => {
    const message = createErrorMessage(error)
    const apiError = new ApiError(
      message,
      error.response?.status,
      error.code,
      error
    )
    
    console.error('API Error:', {
      message,
      status: error.response?.status,
      url: error.config?.url,
      method: error.config?.method,
      originalError: error
    })
    
    return Promise.reject(apiError)
  }
)

// Retry helper function
const withRetry = async <T>(
  operation: () => Promise<T>,
  maxRetries: number = 2,
  delay: number = 1000
): Promise<T> => {
  let lastError: Error

  for (let attempt = 0; attempt <= maxRetries; attempt++) {
    try {
      return await operation()
    } catch (error) {
      lastError = error as Error
      
      // Don't retry on client errors (4xx) except for 408 (timeout) and 429 (rate limit)
      if (error instanceof ApiError && error.status) {
        const shouldRetry = error.status >= 500 || error.status === 408 || error.status === 429
        if (!shouldRetry || attempt === maxRetries) {
          throw error
        }
      } else if (attempt === maxRetries) {
        throw error
      }

      // Wait before retrying (exponential backoff)
      if (attempt < maxRetries) {
        await new Promise(resolve => setTimeout(resolve, delay * Math.pow(2, attempt)))
      }
    }
  }

  throw lastError!
}

export const incidentsApi = {
  getAll: (params?: {
    stationId?: number
    status?: IncidentStatus
    from?: string
    to?: string
    userId?: number
  }) => api.get<Incident[]>('/incidents', { params }),

  getById: (id: number) => api.get<Incident>(`/incidents/${id}`),

  create: (data: CreateIncident) => api.post<Incident>('/incidents', data),

  updateStatus: (id: number, status: IncidentStatus) =>
    api.patch(`/incidents/${id}/status`, { status }),

  close: (id: number, data: CloseIncidentDto) =>
    api.post(`/incidents/${id}/close`, data),

  reopen: (id: number) =>
    api.post(`/incidents/${id}/reopen`),

  assign: (id: number, resourceType: number, resourceId: number) =>
    api.post(`/incidents/${id}/assign`, { resourceType, resourceId }),

  unassign: (id: number, assignmentId: number) =>
    api.delete(`/incidents/${id}/assignments/${assignmentId}`),

  updateAssignmentStatus: (id: number, assignmentId: number, status: string) =>
    api.put(`/incidents/${id}/assignments/${assignmentId}/status`, { status }),

  addLog: (id: number, message: string, by?: string) =>
    api.post(`/incidents/${id}/logs`, { message, by }),

  getStatistics: (params?: { stationId?: number; userId?: number }) =>
    api.get('/incidents/statistics', { params }),

  // Incident detail methods
  updateInvolvement: (id: number, data: CreateIncidentInvolvement) =>
    api.put(`/incidents/${id}/involvement`, data),

  addCommander: (id: number, data: CreateIncidentCommander) =>
    api.post<IncidentCommander>(`/incidents/${id}/commanders`, data),

  updateCommander: (id: number, commanderId: number, data: { observations?: string }) =>
    api.put(`/incidents/${id}/commanders/${commanderId}`, data),

  removeCommander: (id: number, commanderId: number) =>
    api.delete(`/incidents/${id}/commanders/${commanderId}`),

  updateCasualties: (id: number, data: { injuries: Injury[], deaths: Death[] }) =>
    api.put(`/incidents/${id}/casualties`, data),

  updateFire: (id: number, data: CreateIncidentFire) =>
    api.put(`/incidents/${id}/fire`, data),

  updateDamage: (id: number, data: CreateIncidentDamage) =>
    api.put(`/incidents/${id}/damage`, data),
}

export const vehiclesApi = {
  getAll: (params?: { stationId?: number; status?: number }) =>
    withRetry(() => api.get<Vehicle[]>('/vehicles', { params })),

  getById: (id: number) => 
    withRetry(() => api.get<Vehicle>(`/vehicles/${id}`)),

  create: (data: { stationId: number; callsign: string; type: string; plateNumber: string; waterCapacityLiters?: number }) =>
    api.post<Vehicle>('/vehicles', data),

  update: (id: number, data: Partial<Vehicle>) =>
    api.patch(`/vehicles/${id}`, data),

  updateVehicle: (id: number, data: UpdateVehicleDto) =>
    api.put<Vehicle>(`/vehicles/${id}`, data),

  addTelemetry: (id: number, data: Partial<Vehicle>) =>
    api.post(`/vehicles/${id}/telemetry`, data),
}

export const personnelApi = {
  getAll: (params?: { stationId?: number; isActive?: boolean }) =>
    api.get<Personnel[]>('/personnel', { params }),

  getById: (id: number) => api.get<Personnel>(`/personnel/${id}`),

  create: (data: CreatePersonnel) => api.post<Personnel>('/personnel', data),

  update: (id: number, data: Partial<Personnel>) =>
    api.patch(`/personnel/${id}`, data),

  delete: (id: number) => api.delete(`/personnel/${id}`),

  getGroupedByStation: (params?: { isActive?: boolean }) =>
    api.get<Record<number, Personnel[]>>('/personnel/grouped-by-station', { params }),
}

export default api

export const usersApi = {
  getAll: (params?: { stationId?: number }) =>
    api.get<User[]>('/users', { params }),

  getById: (id: number) => api.get<User>(`/users/${id}`),

  getBySupabaseId: (supabaseUserId: string) =>
    api.get<User>(`/users/by-supabase/${supabaseUserId}`),

  create: (data: CreateUser) => api.post<User>('/users', data),

  update: (id: number, data: Partial<User>) =>
    api.patch(`/users/${id}`, data),
}

export const notificationsApi = {
  getAll: (params: { userId: number; isRead?: boolean }) =>
    api.get<Notification[]>('/notifications', { params }),

  markAsRead: (id: number) => api.patch(`/notifications/${id}/read`),

  markAllAsRead: (userId: number) => api.patch(`/notifications/user/${userId}/read-all`),
}

export const vehicleAssignmentsApi = {
  getAll: (params?: { vehicleId?: number; stationId?: number; activeOnly?: boolean }) =>
    api.get<VehicleAssignment[]>('/vehicleassignments', { params }),

  create: (data: CreateVehicleAssignment) =>
    api.post<VehicleAssignment>('/vehicleassignments', data),

  remove: (id: number) => api.delete(`/vehicleassignments/${id}`),
}

export const fireStationsApi = {
  getBoundaries: (options?: {
    limit?: number
    simplify?: boolean
    tolerance?: number
  }) => withRetry(async () => {
    const params = new URLSearchParams()
    if (options?.limit) params.append('limit', options.limit.toString())
    if (options?.simplify) params.append('simplify', 'true')
    if (options?.tolerance) params.append('tolerance', options.tolerance.toString())

    const url = `/firestations/boundaries${params.toString() ? `?${params.toString()}` : ''}`
    console.log('Making API call to:', url)

    const response = await api.get<FireStationBoundary[]>(url)
    console.log('API response:', {
      status: response.status,
      dataLength: response.data?.length,
      firstItem: response.data?.[0]
    })

    return response.data
  }),

  getAll: () => withRetry(async () => {
    const response = await api.get<FireStation[]>('/firestations')
    return response.data
  }),

  getById: (id: number) => withRetry(async () => {
    const response = await api.get<FireStation>(`/firestations/${id}`)
    return response.data
  }),

  getStations: () => withRetry(async () => {
    const response = await api.get<FireStation[]>('/firestations/stations')
    return response.data
  }),

  findByLocation: async (location: LocationDto) => {
    try {
      const response = await api.post<FireStation>('/firestations/find-by-location', location)
      return response.data
    } catch (error) {
      if (error instanceof ApiError && error.status === 404) {
        return null // No station found for this location
      }
      throw error
    }
  },

  getFireDistricts: () => withRetry(async () => {
    const response = await api.get('/fire-districts')
    return response.data
  }),
}

export const shipsApi = {
  getAll: () => withRetry(async () => {
    const response = await api.get<Ship[]>('/ships')
    return response
  }),
}

export const patrolZonesApi = {
  getAll: (params?: { stationId?: number }) =>
    withRetry(() => api.get<PatrolZone[]>('/patrolzones', { params })),

  getById: (id: number) =>
    withRetry(() => api.get<PatrolZone>(`/patrolzones/${id}`)),

  create: (data: CreatePatrolZone) =>
    api.post<PatrolZone>('/patrolzones', data),

  update: (id: number, data: UpdatePatrolZone) =>
    api.put<PatrolZone>(`/patrolzones/${id}`, data),

  delete: (id: number) =>
    api.delete(`/patrolzones/${id}`),

  assignVehicle: (id: number, data: CreatePatrolZoneAssignment) =>
    api.post<PatrolZoneAssignment>(`/patrolzones/${id}/assignments`, data),

  unassignVehicle: (assignmentId: number) =>
    api.delete(`/patrolzones/assignments/${assignmentId}`),

  getAssignmentHistory: (vehicleId: number) =>
    api.get<PatrolZoneAssignment[]>(`/patrolzones/vehicles/${vehicleId}/assignment-history`),
}

export const incidentTypesApi = {
  getAll: () =>
    withRetry(() => api.get<IncidentTypesByAgency[]>('/incidenttypes')),

  getByAgency: (agencyName: string) =>
    withRetry(() => api.get<IncidentTypesByAgency>(`/incidenttypes/agency/${encodeURIComponent(agencyName)}`)),
}

export const fireHydrantsApi = {
  getAll: () => withRetry(async () => {
    const response = await api.get<FireHydrant[]>('/firehydrants')
    return response.data
  }),

  getById: (id: number) => withRetry(async () => {
    const response = await api.get<FireHydrant>(`/firehydrants/${id}`)
    return response.data
  }),

  getCount: () => withRetry(async () => {
    const response = await api.get<{ fireHydrantCount: number }>('/firehydrants/count')
    return response.data
  }),
}

export const coastGuardStationsApi = {
  getAll: () => withRetry(async () => {
    const response = await api.get<CoastGuardStation[]>('/CoastGuardStations')
    return response.data
  }),

  getById: (id: number) => withRetry(async () => {
    const response = await api.get<CoastGuardStation>(`/CoastGuardStations/${id}`)
    return response.data
  }),
}

export const policeStationsApi = {
  getAll: () => withRetry(async () => {
    const response = await api.get<PoliceStation[]>('/PoliceStations')
    return response.data
  }),

  getById: (id: number) => withRetry(async () => {
    const response = await api.get<PoliceStation>(`/PoliceStations/${id}`)
    return response.data
  }),

  getStations: () => withRetry(async () => {
    const response = await api.get<PoliceStation[]>('/PoliceStations/stations')
    return response.data
  }),
}

export const hospitalsApi = {
  getAll: () => withRetry(async () => {
    const response = await api.get<Hospital[]>('/hospitals')
    return response.data
  }),

  getById: (id: number) => withRetry(async () => {
    const response = await api.get<Hospital>(`/hospitals/${id}`)
    return response.data
  }),

  getCount: () => withRetry(async () => {
    const response = await api.get<{ totalCount: number; ekabCount: number; statusMessage: string }>('/hospitals/count')
    return response.data
  }),

  getInBounds: (minLat: number, maxLat: number, minLng: number, maxLng: number, agencyCode?: string) => withRetry(async () => {
    const params = new URLSearchParams({
      minLat: minLat.toString(),
      maxLat: maxLat.toString(),
      minLng: minLng.toString(),
      maxLng: maxLng.toString(),
    })
    
    if (agencyCode) {
      params.append('agencyCode', agencyCode)
    }
    
    const response = await api.get(`/hospitals/in-bounds?${params.toString()}`)
    return response.data
  }),
}

export const stationAssignmentApi = {
  findByLocation: async (request: StationAssignmentRequestDto) => {
    try {
      const response = await api.post<StationAssignmentResponseDto>('/stationassignment/find-by-location', request)
      return response.data
    } catch (error) {
      if (error instanceof ApiError && error.status === 404) {
        return null // No station found for this location
      }
      throw error
    }
  },
}

