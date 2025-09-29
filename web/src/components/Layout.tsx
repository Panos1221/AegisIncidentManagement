import { ReactNode } from 'react'
import { Link, useLocation } from 'react-router-dom'
import {
  Home,
  AlertTriangle,
  Truck,
  Map,
  Calendar,
  Users,
  LogOut,
  Cloud,
  Activity
} from 'lucide-react'
// Import agency logos
import aegisLogoEkab from '../icons/Logos/logo_ekab.png'
import logoCoastguard from '../icons/Logos/logo_coastguard.png'
import logoPolice from '../icons/Logos/logo_police.png'
import logoFire from '../icons/Logos/logo_fire.png'
import { useUserStore } from '../lib/userStore'
import { useAuth } from '../lib/authContext'
import { useQuery } from '@tanstack/react-query'
import { notificationsApi } from '../lib/api'
import { useTranslation } from '../hooks/useTranslation'
import SystemStatus from './SystemStatus'
import ThemeToggle from './ThemeToggle'
import LanguageSelector from './LanguageSelector'
import CADTopNav from './CADTopNav'
import { NotificationBell } from './NotificationBell'

interface LayoutProps {
  children: ReactNode
}

export default function Layout({ children }: LayoutProps) {
  const location = useLocation()
  const { user, canManageStation, canViewPersonnelRoster, isFirefighter } = useUserStore()
  const { logout } = useAuth()
  const t = useTranslation()
  
  // Check if current page is CAD
  const isCAD = location.pathname === '/cad'

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

  const getAgencyLogo = () => {
    switch (user?.agencyName) {
      case 'EKAB':
        return aegisLogoEkab
      case 'Hellenic Coast Guard':
        return logoCoastguard
      case 'Hellenic Police':
        return logoPolice
      case 'Hellenic Fire Service':
        return logoFire
      default:
        return undefined
    }
  }

  const { data: notifications } = useQuery({
    queryKey: ['notifications', user?.id],
    queryFn: () => notificationsApi.getAll({ userId: user!.id, isRead: false }).then(res => res.data),
    enabled: !!user,
  })

  const getNavigation = () => {
    const baseNavigation = [
      { name: t.dashboard, href: '/dashboard', icon: Home },
    ]

    // Add CAD for member roles (firefighters, coast guard members, EKAB members)
    if (isFirefighter()) {
      baseNavigation.push({ name: t.cad || 'Computer-aided dispatch (CAD)', href: '/cad', icon: Activity })
    }

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
        // Fallback to generic navigation
        baseNavigation.push(
          { name: t.incidents, href: '/incidents', icon: AlertTriangle },
          { name: t.vehicles, href: '/vehicles', icon: Truck },
          { name: t.map, href: '/map', icon: Map }
        )
    }

    // Add station management for users who can manage stations
    if (canManageStation()) {
      // Use agency-specific naming
      let stationName = t.stationNav; // Default fallback
      switch (user?.agencyName) {
        case 'Hellenic Fire Service':
          stationName = t.managePersonnel;
          break;
        case 'Hellenic Coast Guard':
          stationName = t.managePersonnel;
          break;
        case 'Hellenic Police':
          stationName = t.managePersonnel;
          break;         
        case 'EKAB':
          stationName = t.managePersonnel;
          break;
      }
      baseNavigation.push({ name: stationName, href: '/station', icon: Users })
    }

    // Add roster only for dispatchers
    if (canViewPersonnelRoster()) {
      baseNavigation.push({ name: t.roster, href: '/roster', icon: Calendar })
    }
    
    return baseNavigation
  }

  if (!user) {
    return (
      <div className="min-h-screen bg-gray-50 dark:bg-gray-900 flex items-center justify-center">
        <div className="text-center">
          <div className="w-16 h-16 bg-primary-600 rounded-lg flex items-center justify-center mx-auto mb-4">
            <AlertTriangle className="w-8 h-8 text-white" />
          </div>
          <h1 className="text-2xl font-bold text-gray-900 dark:text-gray-100 mb-2">{t.incidentManagementSystem}</h1>
          <p className="text-gray-600 dark:text-gray-400 mb-6">{t.pleaseLogIn}</p>
          <button className="btn btn-primary">
            {t.logInWithSupabase}
          </button>
          <div className="mt-6 flex justify-center space-x-2">
            <ThemeToggle />
            <LanguageSelector />
          </div>
        </div>
      </div>
    )
  }

  return (
    <div className="min-h-screen bg-gray-50 dark:bg-gray-900">
      {/* Sidebar - Hidden for CAD */}
      {!isCAD && (
        <div className="fixed inset-y-0 left-0 z-50 w-64 bg-white dark:bg-gray-800 shadow-lg">
        
      <div className="flex h-20 items-center px-6 border-b border-gray-200 dark:border-gray-700">
        <div className="flex items-center">
          {getAgencyLogo() && (
            <img
              src={getAgencyLogo()}
              alt={`${user?.agencyName} Logo`}
              className="h-12 w-auto object-contain"
            />
          )}
          <span
            className={`text-2xl font-bold tracking-tight ${getAgencyTextColor()} ml-5 -mt-1`}
          >
            Aegis
          </span>
        </div>
      </div>

        {/* User info */}
        <div className="px-6 py-4 border-b border-gray-200 dark:border-gray-700">
          <div className="flex items-center">
            <div className="w-8 h-8 bg-gray-300 dark:bg-gray-600 rounded-full flex items-center justify-center">
              <span className="text-sm font-medium text-gray-700 dark:text-gray-300">
                {user.name.charAt(0).toUpperCase()}
              </span>
            </div>
            <div className="ml-3 flex-1">
              <p className="text-sm font-medium text-gray-900 dark:text-gray-100">{user.name}</p>
              <p className={`text-xs ${getAgencyTextColor()}`}>
                {user.agencyName || 'Unknown Agency'}
              </p>
              {user.stationName && (
                <p className="text-xs text-gray-500 dark:text-gray-500">
                  {user.stationName}
                </p>
              )}
            </div>
          </div>


        </div>

        <nav className="mt-6 px-3 flex-1">
          <ul className="space-y-1">
            {getNavigation().map((item) => {
              const isActive = location.pathname === item.href
              return (
                <li key={item.name}>
                  <Link
                    to={item.href}
                    className={`
                      flex items-center px-3 py-2 text-sm font-medium rounded-md transition-colors relative
                      ${isActive
                        ? 'bg-primary-50 dark:bg-primary-900 text-primary-700 dark:text-primary-300 border-r-2 border-primary-600'
                        : 'text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-700 hover:text-gray-900 dark:hover:text-gray-100'
                      }
                    `}
                  >
                    <item.icon className="w-5 h-5 mr-3" />
                    {item.name}
                    {item.name === t.incidents && notifications && notifications.length > 0 && (
                      <span className="ml-auto bg-red-500 dark:bg-red-600 text-white text-xs rounded-full w-5 h-5 flex items-center justify-center">
                        {notifications.length}
                      </span>
                    )}
                  </Link>
                </li>
              )
            })}
          </ul>
        </nav>

        {/* System Status & Logout */}
        <div className="p-3 border-t border-gray-200 dark:border-gray-700 space-y-2">
          <div className="px-3 py-2">
            <SystemStatus />
          </div>
          <button
            onClick={logout}
            className="flex items-center w-full px-3 py-2 text-sm font-medium text-gray-700 dark:text-gray-300 rounded-md hover:bg-gray-50 dark:hover:bg-gray-700 hover:text-gray-900 dark:hover:text-gray-100 transition-colors"
          >
            <LogOut className="w-5 h-5 mr-3" />
            {t.logout}
          </button>
        </div>
        </div>
      )}

      {/* Main content */}
      <div className={isCAD ? '' : 'pl-64'}>
        {/* Top navigation for CAD */}
        {isCAD && <CADTopNav />}
        
        {/* Top bar with notifications - Hidden for CAD */}
        {!isCAD && (
          <div className="bg-white dark:bg-gray-800 shadow-sm border-b border-gray-200 dark:border-gray-700 px-6 py-4">
            <div className="flex items-center justify-end space-x-2">
              <NotificationBell />
              <ThemeToggle />
              <LanguageSelector />
            </div>
          </div>
        )}

        <main className={isCAD ? 'h-[calc(100vh-73px)]' : 'py-6'}>
          <div className={isCAD ? 'h-full' : 'mx-auto max-w-7xl px-6 lg:px-8'}>
            {children}
          </div>
        </main>
      </div>
    </div>
  )
}