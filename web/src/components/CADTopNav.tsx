import { Link, useLocation } from 'react-router-dom'
import {
  Home,
  AlertTriangle,
  Truck,
  Map,
  Users,
  Cloud,
  Activity
} from 'lucide-react'
import { useUserStore } from '../lib/userStore'
import { useQuery } from '@tanstack/react-query'
import { notificationsApi } from '../lib/api'
import { useTranslation } from '../hooks/useTranslation'
import ThemeToggle from './ThemeToggle'
import LanguageSelector from './LanguageSelector'

export default function CADTopNav() {
  const location = useLocation()
  const { user, canManageStation } = useUserStore()
  const t = useTranslation()

  const { data: notifications } = useQuery({
    queryKey: ['notifications', user?.id],
    queryFn: () => notificationsApi.getAll({ userId: user!.id, isRead: false }).then(res => res.data),
    enabled: !!user,
  })

  const getAgencyTextColor = () => {
    switch (user?.agencyName) {
      case 'EKAB':
        return 'text-yellow-500'
      case 'Hellenic Coast Guard':
        return 'text-sky-400' // Light blue
      case 'Hellenic Police':
        return 'text-blue-900' // Dark blue
      case 'Hellenic Fire Service':
        return 'text-red-600'
      default:
        return 'text-primary-700 dark:text-primary-300' // fallback
    }
  }

  const getNavigation = () => {
    const baseNavigation = [
      { name: t.dashboard, href: '/dashboard', icon: Home },
      { name: t.cad || 'Computer-aided dispatch (CAD)', href: '/cad', icon: Activity },
    ]

    // Agency-specific navigation items
    switch (user?.agencyName) {
      case 'Hellenic Fire Service':
        baseNavigation.push(
          { name: t.incidents, href: '/incidents', icon: AlertTriangle },
          { name: t.fireTrucks, href: '/vehicles', icon: Truck },
          { name: t.map, href: '/map', icon: Map },
          { name: t.weatherForecast, href: '/weather', icon: Cloud }
        )
        break
      case 'Hellenic Coast Guard':
        baseNavigation.push(
          { name: t.maritimeIncidents, href: '/incidents', icon: AlertTriangle },
          { name: t.resourcesCapital, href: '/vehicles', icon: Truck },
          { name: t.maritimeMap, href: '/map', icon: Map },
          { name: t.weatherForecast, href: '/weather', icon: Cloud }
        )
        break
      case 'EKAB':
        baseNavigation.push(
          { name: t.incidentsPage, href: '/incidents', icon: AlertTriangle },
          { name: t.resourcesCapital, href: '/vehicles', icon: Truck },
          { name: t.coverageMap, href: '/map', icon: Map }
        )
        break
      default:
        baseNavigation.push(
          { name: t.incidents, href: '/incidents', icon: AlertTriangle },
          { name: t.vehicles, href: '/vehicles', icon: Truck },
          { name: t.map, href: '/map', icon: Map }
        )
    }

    // Add station management for users who can manage stations
    if (canManageStation()) {
      let stationName = t.stationNav
      switch (user?.agencyName) {
        case 'Hellenic Fire Service':
          stationName = 'Fire Stations'
          break
        case 'Hellenic Coast Guard':
          stationName = 'Ports & Bases'
          break
        case 'EKAB':
          stationName = 'Medical Centers'
          break
      }
      baseNavigation.push({ name: stationName, href: '/station', icon: Users })
    }

    return baseNavigation
  }

  return (
    <div className="bg-white dark:bg-gray-800 shadow-sm border-b border-gray-200 dark:border-gray-700 px-6 py-3">
      <div className="flex items-center justify-between">
        {/* Left side - Navigation icons */}
        <div className="flex items-center space-x-1">
          {getNavigation().map((item) => {
            const isActive = location.pathname === item.href
            return (
              <div key={item.name} className="relative group">
                <Link
                  to={item.href}
                  className={`
                    flex items-center justify-center w-10 h-10 rounded-lg transition-all duration-200 relative
                    ${
                      isActive
                        ? 'bg-primary-100 dark:bg-primary-900 text-primary-700 dark:text-primary-300'
                        : 'text-gray-600 dark:text-gray-400 hover:bg-gray-100 dark:hover:bg-gray-700 hover:text-gray-900 dark:hover:text-gray-100'
                    }
                  `}
                >
                  <item.icon className="w-5 h-5" />
                  {item.name === t.incidents && notifications && notifications.length > 0 && (
                    <span className="absolute -top-1 -right-1 bg-red-500 dark:bg-red-600 text-white text-xs rounded-full w-4 h-4 flex items-center justify-center">
                      {notifications.length}
                    </span>
                  )}
                </Link>
                {/* Hover tooltip */}
                <div className="absolute top-full left-1/2 transform -translate-x-1/2 mt-2 px-2 py-1 bg-gray-900 dark:bg-gray-700 text-white text-xs rounded opacity-0 group-hover:opacity-100 transition-opacity duration-200 pointer-events-none whitespace-nowrap z-50">
                  {item.name}
                  <div className="absolute bottom-full left-1/2 transform -translate-x-1/2 w-0 h-0 border-l-2 border-r-2 border-b-2 border-transparent border-b-gray-900 dark:border-b-gray-700"></div>
                </div>
              </div>
            )
          })}
        </div>

        {/* Right side - User info, controls, and app name */}
        <div className="flex items-center space-x-4">
          {/* User info */}
          <div className="flex items-center space-x-3">
            <div className="text-right">
              <p className="text-sm font-medium text-gray-900 dark:text-gray-100">{user?.name}</p>
              <p className="text-xs text-gray-500 dark:text-gray-400">{user?.agencyName}</p>
            </div>
            <div className="w-8 h-8 bg-gray-300 dark:bg-gray-600 rounded-full flex items-center justify-center">
              <span className="text-sm font-medium text-gray-700 dark:text-gray-300">
                {user?.name.charAt(0).toUpperCase()}
              </span>
            </div>
          </div>

          {/* Controls */}
          <div className="flex items-center space-x-0">
            <ThemeToggle />
            <LanguageSelector />
          </div>

          {/* App name */}
          <div className="flex items-center ml-6 space-x-10">
            <span className={`text-3xl font-bold tracking-tight ${getAgencyTextColor()}`}>
              Aegis
            </span>
          </div>
        </div>
      </div>
    </div>
  )
}