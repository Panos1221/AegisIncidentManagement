import { User, UserRole } from '../types'
import { useUserStore } from './userStore'

interface LoginRequest {
  email: string
  password: string
}

interface LoginResponse {
  id: number
  email: string
  name: string
  role: number
  agencyId: number
  agencyName: string
  stationId?: number
  stationName?: string
  isActive: boolean
  token: string
}

class AuthService {
  private readonly API_BASE_URL = 'http://localhost:5000/api'

  async login(credentials: LoginRequest): Promise<User> {
    const response = await fetch(`${this.API_BASE_URL}/auth/login`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(credentials),
    })

    if (!response.ok) {
      const errorData = await response.json().catch(() => ({ message: 'Login failed' }))
      throw new Error(errorData.message || 'Invalid credentials')
    }

    const data: LoginResponse = await response.json()

    // Store the JWT token
    localStorage.setItem('authToken', data.token)

    // Convert the response to match our User interface
    const user: User = {
      id: data.id,
      supabaseUserId: data.id.toString(),
      email: data.email,
      name: data.name,
      role: data.role as UserRole,
      agencyId: data.agencyId,
      agencyName: data.agencyName,
      stationId: data.stationId,
      stationName: data.stationName,
      isActive: data.isActive,
      createdAt: new Date().toISOString(),
    }

    // Update the user store
    useUserStore.getState().setUser(user)

    return user
  }

  logout(): void {
    localStorage.removeItem('authToken')
    useUserStore.getState().logout()
  }

  getToken(): string | null {
    return localStorage.getItem('authToken')
  }

  isAuthenticated(): boolean {
    const token = this.getToken()
    if (!token) return false

    // Check if token is expired (basic check)
    try {
      const payload = JSON.parse(atob(token.split('.')[1]))
      const currentTime = Date.now() / 1000
      return payload.exp > currentTime
    } catch {
      return false
    }
  }

  async validateToken(): Promise<boolean> {
    const token = this.getToken()
    if (!token) return false

    try {
      const response = await fetch(`${this.API_BASE_URL}/auth/validate`, {
        method: 'GET',
        headers: {
          'Authorization': `Bearer ${token}`,
        },
      })
      return response.ok
    } catch {
      return false
    }
  }
}

export const authService = new AuthService()
export default authService