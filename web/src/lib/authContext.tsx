import React, { createContext, useContext, useEffect, ReactNode } from 'react'
import { useUserStore } from './userStore'
import { getUserFromToken, isTokenExpired } from './jwtUtils'
import { authService } from './authService'

interface AuthContextType {
  isAuthenticated: boolean
  user: any
  logout: () => void
}

const AuthContext = createContext<AuthContextType | undefined>(undefined)

export const useAuth = () => {
  const context = useContext(AuthContext)
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider')
  }
  return context
}

interface AuthProviderProps {
  children: ReactNode
}

export const AuthProvider: React.FC<AuthProviderProps> = ({ children }) => {
  const { user, isAuthenticated, logout: storeLogout } = useUserStore()

  const logout = () => {
    authService.logout()
  }

  // Check for existing token on app load
  useEffect(() => {
    const token = authService.getToken()
    if (token && !isAuthenticated) {
      // Check if token is expired
      if (isTokenExpired(token)) {
        authService.logout()
        return
      }
      
      // Try to restore user from token
      const userFromToken = getUserFromToken(token)
      if (userFromToken) {
        useUserStore.getState().setUser(userFromToken)
      } else {
        authService.logout()
      }
    }
  }, [isAuthenticated])

  const value = {
    isAuthenticated,
    user,
    logout,
  }

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}