import React from 'react'
import { Navigate } from 'react-router-dom'
import { useUserStore } from '../lib/userStore'

interface RoleProtectedRouteProps {
  children: React.ReactNode
  requiredPermission: () => boolean
  fallbackPath?: string
}

const RoleProtectedRoute: React.FC<RoleProtectedRouteProps> = ({ 
  children, 
  requiredPermission, 
  fallbackPath = '/dashboard' 
}) => {
  const { isAuthenticated } = useUserStore()

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />
  }

  if (!requiredPermission()) {
    return <Navigate to={fallbackPath} replace />
  }

  return <>{children}</>
}

export default RoleProtectedRoute