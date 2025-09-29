import { useState, useEffect } from 'react'
import { useQuery, useQueryClient } from '@tanstack/react-query'
import { useSearchParams, useNavigate } from 'react-router-dom'
import { MapContainer, TileLayer, Marker, Popup, useMap } from 'react-leaflet'
import { incidentsApi, vehiclesApi, fireStationsApi, fireHydrantsApi, shipsApi, coastGuardStationsApi, policeStationsApi, hospitalsApi, ApiError } from '../lib/api'
import { IncidentStatus, VehicleStatus, ResourceType, CreatePatrolZone, PatrolZone } from '../types'
import MapErrorBoundary from '../components/MapErrorBoundary'
import LoadingSpinner from '../components/LoadingSpinner'
import RetryButton from '../components/RetryButton'
import OptimizedFireStationBoundaries from '../components/OptimizedFireStationBoundaries'
import PatrolZoneLayer from '../components/PatrolZoneLayer'
import PatrolZoneDrawer from '../components/PatrolZoneDrawer'
import PatrolZoneModal from '../components/PatrolZoneModal'
import { PatrolZoneVehicleAssignmentModal } from '../components/PatrolZoneVehicleAssignmentModal'
import MapFiltersModal, { MapFilters } from '../components/MapFiltersModal'
import { useTranslation } from '../hooks/useTranslation'
import { useTheme } from '../lib/themeContext'
import { useUserStore } from '../lib/userStore'
import { useSignalR } from '../hooks/useSignalR'
import { getIncidentStatusTranslation, getVehicleStatusTranslation, getIncidentStatusBadgeColor, getVehicleStatusBadgeColor } from '../utils/incidentUtils'
import { getFireDepartmentIncidentIcon, getEKABIncidentIcon } from '../lib/incidentIcons'
import { AlertTriangle, Map, Satellite, Eye, Plus, Truck, Filter } from 'lucide-react'
import 'leaflet/dist/leaflet.css'
import '../styles/fire-station-map.css'
import L from 'leaflet'

// Fix for default markers in react-leaflet
delete (L.Icon.Default.prototype as any)._getIconUrl
L.Icon.Default.mergeOptions({
  iconRetinaUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.7.1/images/marker-icon-2x.png',
  iconUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.7.1/images/marker-icon.png',
  shadowUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.7.1/images/marker-shadow.png',
})

// Custom icons
const incidentIcon = new L.Icon({
  iconUrl: 'https://raw.githubusercontent.com/pointhi/leaflet-color-markers/master/img/marker-icon-red.png',
  shadowUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.7.1/images/marker-shadow.png',
  iconSize: [25, 41],
  iconAnchor: [12, 41],
  popupAnchor: [1, -34],
  shadowSize: [41, 41]
})

const vehicleIcon = new L.Icon({
  iconUrl: 'https://raw.githubusercontent.com/pointhi/leaflet-color-markers/master/img/marker-icon-blue.png',
  shadowUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.7.1/images/marker-shadow.png',
  iconSize: [25, 41],
  iconAnchor: [12, 41],
  popupAnchor: [1, -34],
  shadowSize: [41, 41]
})


const fireHydrantIcon = new L.Icon({
  iconUrl: '/src/icons/General/fire-hydrant-icon.png',
  shadowUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.7.1/images/marker-shadow.png',
  iconSize: [30, 30],
  iconAnchor: [10, 33],
  popupAnchor: [1, -28],
  shadowSize: [33, 33]
})

const shipIcon = new L.Icon({
  iconUrl: 'https://raw.githubusercontent.com/pointhi/leaflet-color-markers/master/img/marker-icon-violet.png',
  shadowUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.7.1/images/marker-shadow.png',
  iconSize: [25, 41],
  iconAnchor: [12, 41],
  popupAnchor: [1, -34],
  shadowSize: [41, 41]
})

const coastGuardStationIcon = new L.Icon({
  iconUrl: '/src/icons/Stations/port-station.png',
  shadowUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.7.1/images/marker-shadow.png',
  iconSize: [36, 36],
  iconAnchor: [15, 30],
  popupAnchor: [1, -28],
  shadowSize: [0, 0]
})

const policeStationIcon = new L.Icon({
  iconUrl: '/src/icons/Stations/policeStation.png',
  shadowUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.7.1/images/marker-shadow.png',
  iconSize: [32, 32],
  iconAnchor: [16, 32],
  popupAnchor: [0, -32],
  shadowSize: [0, 0]
})

const hospitalIcon = new L.Icon({
  iconUrl: '/src/icons/Stations/hospital.png',
  shadowUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.7.1/images/marker-shadow.png',
  iconSize: [32, 32],
  iconAnchor: [16, 32],
  popupAnchor: [0, -32],
  shadowSize: [0, 0]
})

const fireStationIcon = new L.Icon({
  iconUrl: '/src/icons/Stations/fireStation.png',
  shadowUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.7.1/images/marker-shadow.png',
  iconSize: [32, 32],
  iconAnchor: [16, 32],
  popupAnchor: [0, -32],
  shadowSize: [0, 0]
})

// Component to handle map centering from URL params
function MapController({ incidents }: { incidents?: any[] }) {
  const [searchParams] = useSearchParams()
  const map = useMap()
  
  useEffect(() => {
    const lat = searchParams.get('lat')
    const lng = searchParams.get('lng')
    const incidentId = searchParams.get('incident')
    
    if (lat && lng) {
      const latitude = parseFloat(lat)
      const longitude = parseFloat(lng)
      if (!isNaN(latitude) && !isNaN(longitude)) {
        map.setView([latitude, longitude], 16)
        
        // If incident ID is provided, try to open its popup after a short delay
        if (incidentId && incidents) {
          setTimeout(() => {
            const incident = incidents.find(inc => inc.id.toString() === incidentId)
            if (incident) {
              // Find the marker and open its popup
              map.eachLayer((layer: any) => {
                if (layer.options && layer.options.incidentId === incident.id) {
                  layer.openPopup()
                }
              })
            }
          }, 500)
        }
      }
    }
  }, [searchParams, map, incidents])
  
  return null
}

export default function MapView() {
  const [searchParams] = useSearchParams()
  
  // Filter states - these will be managed by the filters modal
  const [filters, setFilters] = useState<MapFilters>({
    incidents: true,
    vehicles: true,
    fireStations: true,
    fireStationBoundaries: false,
    fireHydrants: false,
    policeStations: true,
    patrolZones: true,
    coastGuardStations: true,
    ships: true,
    ambulances: true,
    hospitals: true
  })

  const handleFiltersChange = (newFilters: MapFilters) => {
    setFilters(newFilters)
  }
  
  // These are now managed by the filters state
  const showFireStationBoundaries = filters.fireStationBoundaries
  const showFireStations = filters.fireStations
  const showFireHydrants = filters.fireHydrants
  const showPatrolZones = filters.patrolZones
  const [isDrawingPatrolZone, setIsDrawingPatrolZone] = useState(false)
  const [patrolZoneModalOpen, setPatrolZoneModalOpen] = useState(false)
  const [patrolZoneModalMode, setPatrolZoneModalMode] = useState<'create' | 'edit'>('create')
  const [pendingPatrolZone, setPendingPatrolZone] = useState<CreatePatrolZone | null>(null)
  const [editingPatrolZone, setEditingPatrolZone] = useState<PatrolZone | null>(null)
  const [vehicleAssignmentModalOpen, setVehicleAssignmentModalOpen] = useState(false)
  const [assigningPatrolZone, setAssigningPatrolZone] = useState<PatrolZone | null>(null)
  const [mapViewType, setMapViewType] = useState<'street' | 'satellite'>('street')
  const [showFiltersModal, setShowFiltersModal] = useState(false)
  
  const t = useTranslation()
  const { theme } = useTheme()
  const { user } = useUserStore()
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const signalR = useSignalR()
  
  // Check if user's agency should show station controls based on their agency
  const shouldShowFireStationControls = user?.agencyName === 'Hellenic Fire Service'
  const shouldShowPoliceStations = user?.agencyName === 'Hellenic Police'
  const shouldShowCoastGuardStations = user?.agencyName === 'Hellenic Coast Guard'
  const shouldShowEKABStations = user?.agencyName === 'EKAB'
  const shouldShowPatrolZoneControls = shouldShowPoliceStations || shouldShowCoastGuardStations

  const { 
    data: incidents, 
    isLoading: incidentsLoading, 
    error: incidentsError,
    refetch: refetchIncidents 
  } = useQuery({
    queryKey: ['incidents'],
    queryFn: () => incidentsApi.getAll().then(res => res.data),
    retry: (failureCount, error) => {
      // Don't retry on client errors
      if (error instanceof ApiError && error.status && error.status < 500) {
        return false
      }
      return failureCount < 2
    },
    staleTime: 30000, // Consider data stale after 30 seconds
  })

  const { 
    data: vehicles, 
    isLoading: vehiclesLoading, 
    error: vehiclesError,
    refetch: refetchVehicles 
  } = useQuery({
    queryKey: ['vehicles'],
    queryFn: () => vehiclesApi.getAll().then(res => res.data),
    retry: (failureCount, error) => {
      if (error instanceof ApiError && error.status && error.status < 500) {
        return false
      }
      return failureCount < 2
    },
    staleTime: 30000,
  })



  const { 
    data: fireHydrants, 
    refetch: refetchFireHydrants 
  } = useQuery({
    queryKey: ['fireHydrants'],
    queryFn: () => fireHydrantsApi.getAll(),
    retry: (failureCount, error) => {
      if (error instanceof ApiError && error.status && error.status < 500) {
        return false
      }
      return failureCount < 2
    },
    staleTime: 10 * 60 * 1000, // Cache for 10 minutes (fire hydrants change less frequently)
    enabled: showFireHydrants && shouldShowFireStationControls, // Only for Fire Service members
  })

  const { 
    data: ships, 
    isLoading: shipsLoading, 
    error: shipsError,
    refetch: refetchShips 
  } = useQuery({
    queryKey: ['ships'],
    queryFn: () => shipsApi.getAll().then(res => res.data),
    retry: (failureCount, error) => {
      if (error instanceof ApiError && error.status && error.status < 500) {
        return false
      }
      return failureCount < 2
    },
    staleTime: 30000, // Consider data stale after 30 seconds
    enabled: shouldShowCoastGuardStations, // Only fetch if user is from Coast Guard
  })

  const { 
    data: coastGuardStations, 
    refetch: refetchCoastGuardStations 
  } = useQuery({
    queryKey: ['coastGuardStations'],
    queryFn: () => coastGuardStationsApi.getAll(),
    retry: (failureCount, error) => {
      if (error instanceof ApiError && error.status && error.status < 500) {
        return false
      }
      return failureCount < 2
    },
    staleTime: 10 * 60 * 1000, // Cache for 10 minutes
    enabled: shouldShowCoastGuardStations, // Only fetch if user is from Coast Guard
  })

  const { 
    data: policeStations, 
    refetch: refetchPoliceStations 
  } = useQuery({
    queryKey: ['policeStations'],
    queryFn: () => policeStationsApi.getAll(),
    retry: (failureCount, error) => {
      if (error instanceof ApiError && error.status && error.status < 500) {
        return false
      }
      return failureCount < 2
    },
    staleTime: 10 * 60 * 1000, // Cache for 10 minutes
    enabled: shouldShowPoliceStations, // Only fetch if user is from Police
  })

  const {
    data: hospitals,
    refetch: refetchHospitals
  } = useQuery({
    queryKey: ['hospitals'],
    queryFn: () => hospitalsApi.getAll(),
    retry: (failureCount, error) => {
      if (error instanceof ApiError && error.status && error.status < 500) {
        return false
      }
      return failureCount < 2
    },
    staleTime: 10 * 60 * 1000, // Cache for 10 minutes
    enabled: shouldShowEKABStations, // Only fetch if user is from EKAB
  })

  const {
    data: fireStation,
    refetch: refetchfireStation
  } = useQuery({
    queryKey: ['fireStation'],
    queryFn: () => fireStationsApi.getStations(),
    retry: (failureCount, error) => {
      if (error instanceof ApiError && error.status && error.status < 500) {
        return false
      }
      return failureCount < 2
    },
    staleTime: 10 * 60 * 1000, // Cache for 10 minutes
    enabled: showFireStations && shouldShowFireStationControls, // Only fetch if user is from Fire Department and stations are toggled on
  })

  // Athens center coordinates (or from URL params)
  const lat = searchParams.get('lat')
  const lng = searchParams.get('lng')
  const center: [number, number] = lat && lng ? [parseFloat(lat), parseFloat(lng)] : [37.9755, 23.7348]
  const zoom = lat && lng ? 16 : 12

  // Color generation and styling now handled by OptimizedFireStationBoundaries

  // Handle retry for all data
  const handleRetryAll = () => {
    refetchIncidents()
    refetchVehicles()
    refetchFireHydrants()
    refetchShips()
    refetchCoastGuardStations()
    refetchPoliceStations()
    refetchHospitals()
    refetchfireStation()
    // Boundary retries are now handled by OptimizedFireStationBoundaries
  }

  // Set up SignalR event handlers to invalidate queries when incidents change
  useEffect(() => {
    if (!signalR) return

    // Invalidate incidents query when status changes
    const cleanupStatusChanged = signalR.addIncidentStatusChangedHandler(() => {
      queryClient.invalidateQueries({ queryKey: ['incidents'], exact: false })
    })

    // Invalidate incidents query when new incidents are created
    const cleanupCreated = signalR.addIncidentCreatedHandler(() => {
      queryClient.invalidateQueries({ queryKey: ['incidents'], exact: false })
    })

    // Invalidate incidents query when incidents are updated
    const cleanupUpdate = signalR.addIncidentUpdateHandler(() => {
      queryClient.invalidateQueries({ queryKey: ['incidents'], exact: false })
    })

    // Invalidate incidents query when resources are assigned
    const cleanupResourceAssigned = signalR.addResourceAssignedHandler(() => {
      queryClient.invalidateQueries({ queryKey: ['incidents'], exact: false })
    })

    // Invalidate incidents query when assignment status changes
    const cleanupAssignmentStatusChanged = signalR.addAssignmentStatusChangedHandler(() => {
      queryClient.invalidateQueries({ queryKey: ['incidents'], exact: false })
    })

    // Invalidate vehicles query when vehicle status changes
    const cleanupVehicleStatusChanged = signalR.addVehicleStatusChangedHandler(() => {
      queryClient.invalidateQueries({ queryKey: ['vehicles'] })
    })

    return () => {
      cleanupStatusChanged();
      cleanupCreated();
      cleanupUpdate();
      cleanupResourceAssigned();
      cleanupAssignmentStatusChanged();
      cleanupVehicleStatusChanged();
    };
  }, [signalR, queryClient])

  // Patrol zone event handlers
  const handlePatrolZoneCreated = (zone: CreatePatrolZone) => {
    setPendingPatrolZone(zone)
    setPatrolZoneModalOpen(true)
    setPatrolZoneModalMode('create')
  }

  const handlePatrolZoneClick = (zone: PatrolZone) => {
    setEditingPatrolZone(zone)
    setPatrolZoneModalOpen(true)
    setPatrolZoneModalMode('edit')
  }

  const handleAssignVehicle = (zone: PatrolZone) => {
    setAssigningPatrolZone(zone)
    setVehicleAssignmentModalOpen(true)
  }

  const handleModalClose = () => {
    setPatrolZoneModalOpen(false)
    setPendingPatrolZone(null)
    setEditingPatrolZone(null)
    setIsDrawingPatrolZone(false)
  }

  // Check if there are any critical errors that prevent map display
  const hasCriticalErrors = incidentsError || vehiclesError  || shipsError
  const hasDataLoadingIssues = incidentsLoading || vehiclesLoading || shipsLoading

  return (
    <div>
      <div className="mb-6">
        <div className="flex items-center justify-between">
          <div>
            <h1 className="text-3xl font-bold text-gray-900 dark:text-gray-100">{t.mapViewTitle}</h1>
            <p className="mt-2 text-gray-600 dark:text-gray-400">
              {t.realTimeView}
            </p>
          </div>
        </div>
      </div>

      {/* Error Alert */}
      {hasCriticalErrors && (
        <div className="mb-6 p-4 bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-lg">
          <div className="flex items-center">
            <AlertTriangle className="w-5 h-5 text-red-500 mr-3" />
            <div className="flex-1">
              <h3 className="text-sm font-medium text-red-800 dark:text-red-200">{t.dataLoadingIssues}</h3>
              <div className="mt-1 text-sm text-red-700 dark:text-red-300">
                {incidentsError && <div>• {t.incidents}: {incidentsError.message}</div>}
                {vehiclesError && <div>• {t.vehicles}: {vehiclesError.message}</div>}
                {shipsError && <div>• {t.vessels}: {shipsError.message}</div>}
              </div>
            </div>
            <RetryButton onRetry={handleRetryAll} size="sm" />
          </div>
        </div>
      )}

      {/* Legend and Controls */}
      <div className="card p-4 mb-6">
        <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
          <div>
            <h2 className="text-lg font-semibold text-gray-900 dark:text-gray-100 mb-3">{t.legend}</h2>
            <div className="flex flex-wrap gap-6">
              <div className="flex items-center">
                <div className="w-4 h-4 bg-red-500 rounded-full mr-2"></div>
                <span className="text-sm text-gray-700 dark:text-gray-300">{t.activeIncidents}</span>
              </div>
              <div className="flex items-center">
                <div className="w-4 h-4 bg-blue-500 rounded-full mr-2"></div>
                <span className="text-sm text-gray-700 dark:text-gray-300">{t.vehicles}</span>
              </div>
              {shouldShowCoastGuardStations && (
                <div className="flex items-center">
                  <div className="w-4 h-4 bg-violet-500 rounded-full mr-2"></div>
                  <span className="text-sm text-gray-700 dark:text-gray-300">{t.vessels}</span>
                </div>
              )}
              {shouldShowPatrolZoneControls && showPatrolZones && (
                <div className="flex items-center">
                  <div className="w-4 h-4 bg-orange-500 rounded-full mr-2"></div>
                  <span className="text-sm text-gray-700 dark:text-gray-300">Patrol Zones</span>
                </div>
              )}
              {shouldShowFireStationControls && showFireStations && (
                <div className="flex items-center">
                  <div className="w-4 h-4 bg-green-500 rounded-full mr-2"></div>
                  <span className="text-sm text-gray-700 dark:text-gray-300">{t.fireStations}</span>
                </div>
              )}
              {shouldShowFireStationControls && showFireHydrants && (
                <div className="flex items-center">
                  <div className="w-3 h-3 bg-orange-500 rounded-full mr-2"></div>
                  <span className="text-sm text-gray-700 dark:text-gray-300">Fire Hydrants</span>
                </div>
              )}
              {(shouldShowPoliceStations || shouldShowCoastGuardStations || shouldShowEKABStations) && showFireStations && (
                <div className="flex items-center">
                  <div className="w-4 h-4 bg-blue-500 rounded-full mr-2"></div>
                  <span className="text-sm text-gray-700 dark:text-gray-300">{user?.agencyName} Stations</span>
                </div>
              )}
              {shouldShowFireStationControls && showFireStationBoundaries && (
                <div className="flex items-center">
                  <div className="w-4 h-4 border-2 border-purple-500 bg-purple-200 dark:bg-purple-800 mr-2"></div>
                  <span className="text-sm text-gray-700 dark:text-gray-300">{t.stationDistricts}</span>
                </div>
              )}
            </div>
          </div>   

          <div className="flex w-full flex-wrap items-start justify-end gap-3">
            {/* Map View Toggle */}
            <button
              onClick={() => setMapViewType(mapViewType === "street" ? "satellite" : "street")}
              className={`px-3 py-2 rounded-lg text-sm font-medium transition-all duration-200 flex items-center gap-2 ${
                theme === "dark"
                  ? "bg-gray-700 hover:bg-gray-600 text-white border border-gray-600"
                  : "bg-white hover:bg-gray-50 text-gray-700 border border-gray-300 shadow-sm"
              }`}
              title={`Switch to ${mapViewType === "street" ? "Satellite" : "Street"} View`}
            >
              {mapViewType === "street" ? (
                <>
                  <Satellite className="w-4 h-4" />
                  <span>Satellite</span>
                </>
              ) : (
                <>
                  <Map className="w-4 h-4" />
                  <span>Street</span>
                </>
              )}
            </button>

            {/* Filters Button */}
            <button
              onClick={() => setShowFiltersModal(true)}
              className={`px-3 py-2 rounded-lg text-sm font-medium transition-all duration-200 flex items-center gap-2 ${
                theme === "dark"
                  ? "bg-gray-700 hover:bg-gray-600 text-white border border-gray-600"
                  : "bg-white hover:bg-gray-50 text-gray-700 border border-gray-300 shadow-sm"
              }`}
              title={t.showFilters}
            >
              <Filter className="w-4 h-4" />
              <span>{t.mapFilters}</span>
            </button>



            {/* Create Patrol Zone button for Police and Coast Guard */}
            {shouldShowPatrolZoneControls && (
              <button
                onClick={() => {
                  setIsDrawingPatrolZone(true)
                  setPatrolZoneModalMode('create')
                }}
                className="flex items-center gap-2 px-4 py-2 rounded-lg font-medium text-sm transition-all duration-200 shadow-sm bg-blue-600 text-white hover:bg-blue-700 dark:bg-blue-700 dark:hover:bg-blue-600"
                disabled={isDrawingPatrolZone}
              >
                <Plus className="w-4 h-4" />
                {isDrawingPatrolZone ? 'Drawing...' : 'Create Zone'}
              </button>
            )}


          </div>

        </div>
      </div>

      {/* Map */}
      <div className="card overflow-hidden">
        <div style={{ height: '600px' }}>
          {hasDataLoadingIssues && (
            <div className="absolute inset-0 bg-white dark:bg-gray-800 bg-opacity-75 dark:bg-opacity-75 flex items-center justify-center z-10">
              <LoadingSpinner size="lg" text={t.loadingData} />
            </div>
          )}
          
          <MapErrorBoundary>
            <MapContainer
              id="main-map"
              center={center}
              zoom={zoom}
              style={{ height: '100%', width: '100%' }}
            >
            <MapController />
            {/* Base layer */}
            <TileLayer
              attribution={
                mapViewType === 'satellite'
                  ? 'Tiles &copy; Esri &mdash; Source: Esri, Maxar, Earthstar Geographics, and the GIS User Community'
                  : '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
              }
              url={
                mapViewType === 'satellite'
                  ? "https://server.arcgisonline.com/ArcGIS/rest/services/World_Imagery/MapServer/tile/{z}/{y}/{x}"
                  : "https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
              }
            />
            
            {/* Labels overlay for satellite view */}
            {mapViewType === 'satellite' && (
              <TileLayer
                url="https://server.arcgisonline.com/ArcGIS/rest/services/Reference/World_Boundaries_and_Places/MapServer/tile/{z}/{y}/{x}"
                opacity={0.9}
              />
            )}


            {/* Optimized Fire Station Boundaries - Only for Fire Department */}
            {shouldShowFireStationControls && (
              <OptimizedFireStationBoundaries
                visible={filters.fireStationBoundaries}
              />
            )}

            {/* Coast Guard Stations - Only for Coast Guard when stations toggle is on */}
            {filters.coastGuardStations && shouldShowCoastGuardStations && coastGuardStations?.map((station) => (
              <Marker
                key={`cg-station-${station.id}`}
                position={[station.latitude, station.longitude]}
                icon={coastGuardStationIcon}
              >
                <Popup>
                  <div className="p-2">
                    <h3 className="font-semibold text-gray-900">
                      {t.language === 'el' ? station.nameGr || station.name : station.name}
                    </h3>
                    <div className="space-y-1 mt-2">
                      {station.address && (
                        <div className="flex justify-between text-xs">
                          <span>{t.address}:</span>
                          <span className="text-right ml-2">{station.address}</span>
                        </div>
                      )}
                      {station.area && (
                        <div className="flex justify-between text-xs">
                          <span>{t.region}:</span>
                          <span>{station.area}</span>
                        </div>
                      )}
                      {station.telephone && (
                        <div className="flex justify-between text-xs">
                          <span>{t.telephone}:</span>
                          <span className="font-mono">{station.telephone}</span>
                        </div>
                      )}
                      {station.email && (
                        <div className="flex justify-between text-xs">
                          <span>{t.email}:</span>
                          <span className="font-mono text-blue-600">{station.email}</span>
                        </div>
                      )}
                      {station.type && (
                        <div className="flex justify-between text-xs">
                          <span>{t.type}:</span>
                          <span>{station.type}</span>
                        </div>
                      )}
                    </div>
                  </div>
                </Popup>
              </Marker>
            ))}

            {/* Police Stations - Only for Police */}
            {filters.policeStations && shouldShowPoliceStations && policeStations?.map((station) => (
              <Marker
                key={`police-station-${station.id}`}
                position={[station.latitude, station.longitude]}
                icon={policeStationIcon}
              >
                <Popup>
                  <div className="p-2">
                    <h3 className="font-semibold text-gray-900 text-sm">
                      {station.name}
                    </h3>
                    <div className="space-y-1 mt-2">
                      {station.address && (
                        <div className="flex justify-between text-xs">
                          <span>{t.address}:</span>
                          <span className="text-right ml-2">{station.address}</span>
                        </div>
                      )}
                      {station.sinoikia && (
                        <div className="flex justify-between text-xs">
                          <span>{t.district}:</span>
                          <span>{station.sinoikia}</span>
                        </div>
                      )}
                      {station.diam && (
                        <div className="flex justify-between text-xs">
                          <span>{t.department}:</span>
                          <span>{station.diam}</span>
                        </div>
                      )}
                    </div>
                  </div>
                </Popup>
              </Marker>
            ))}

            {/* Hospitals - Only for EKAB */}
            {filters.hospitals && shouldShowEKABStations && hospitals?.map((hospital) => (
              <Marker
                key={`hospital-${hospital.id}`}
                position={[hospital.latitude, hospital.longitude]}
                icon={hospitalIcon}
              >
                <Popup>
                  <div className="p-2">
                    <h3 className="font-semibold text-gray-900 text-sm">
                      {hospital.name}
                    </h3>
                    <div className="space-y-1 mt-2">
                      {hospital.address && (
                        <div className="flex justify-between text-xs">
                          <span>{t.address}:</span>
                          <span className="text-right ml-2">{hospital.address}</span>
                        </div>
                      )}
                      {hospital.city && (
                        <div className="flex justify-between text-xs">
                          <span>{t.city}:</span>
                          <span>{hospital.city}</span>
                        </div>
                      )}
                      {hospital.region && (
                        <div className="flex justify-between text-xs">
                          <span>{t.region}:</span>
                          <span>{hospital.region}</span>
                        </div>
                      )}
                    </div>
                  </div>
                </Popup>
              </Marker>
            ))}

            {/* Fire Station Locations - Only for Fire Department */}
            {filters.fireStations && showFireStations && shouldShowFireStationControls && fireStation?.map((fireStation) => (
              <Marker
                key={`fire-station-${fireStation.id}`}
                position={[fireStation.latitude, fireStation.longitude]}
                icon={fireStationIcon}
              >
                <Popup>
                  <div className="p-2">
                    <h3 className="font-semibold text-gray-900 text-sm">
                      {fireStation.name}
                    </h3>
                    <div className="space-y-1 mt-2">
                      {fireStation.address && (
                        <div className="flex justify-between text-xs">
                          <span>{t.address}:</span>
                          <span className="text-right ml-2">{fireStation.address}</span>
                        </div>
                      )}
                      {fireStation.city && (
                        <div className="flex justify-between text-xs">
                          <span>{t.city}:</span>
                          <span>{fireStation.city}</span>
                        </div>
                      )}
                      {fireStation.region && (
                        <div className="flex justify-between text-xs">
                          <span>{t.region}:</span>
                          <span>{fireStation.region}</span>
                        </div>
                      )}
                    </div>
                  </div>
                </Popup>
              </Marker>
            ))}

            {/* Fire Hydrants - Only for Fire Department */}
            {filters.fireHydrants && shouldShowFireStationControls && fireHydrants?.map((hydrant) => (
              <Marker
                key={`hydrant-${hydrant.id}`}
                position={[hydrant.latitude, hydrant.longitude]}
                icon={fireHydrantIcon}
              >
                <Popup>
                  <div className="p-2">
                    <h3 className="font-semibold text-gray-900">{t.hydrant}</h3>
                  </div>
                </Popup>
              </Marker>
            ))}

            {/* Active Incidents */}
            {filters.incidents && incidents
              ?.filter(incident =>
                incident.status !== IncidentStatus.FullyControlled &&
                incident.status !== IncidentStatus.Closed
              )
              .map((incident) => {
                // Get appropriate icon based on user's agency and incident details
                let markerIcon: L.Icon
                if (user?.agencyName === 'Hellenic Fire Service') {
                  markerIcon = getFireDepartmentIncidentIcon(incident.mainCategory, incident.status)
                } else if (user?.agencyName === 'EKAB') {
                  markerIcon = getEKABIncidentIcon(incident.status)
                } else {
                  markerIcon = incidentIcon
                }

                return (
                  <Marker
                    key={`incident-${incident.id}`}
                    position={[incident.latitude, incident.longitude]}
                    icon={markerIcon}
                  >
                  <Popup>
                    <div className="p-2 bg-white dark:bg-gray-800 text-gray-900 dark:text-gray-100">
                      <h3 className="font-semibold text-gray-900 dark:text-gray-100 mb-2">{incident.mainCategory} - {incident.subCategory}</h3>
                      <p className="text-sm text-gray-600 dark:text-gray-300 mb-2">
                        {incident.notes || t.noNotes}
                      </p>
                      
                      {/* Assigned Vehicles */}
                      {incident.assignments && incident.assignments.filter(a => a.resourceType === ResourceType.Vehicle).length > 0 && (
                        <div className="mb-2">
                          <div className="flex items-center mb-1">
                            <Truck className="w-3 h-3 mr-1 text-blue-600 dark:text-blue-400" />
                            <span className="text-xs font-medium text-gray-700 dark:text-gray-300">{t.assignedResources}:</span>
                          </div>
                          <div className="space-y-1">
                            {incident.assignments
                              .filter(a => a.resourceType === ResourceType.Vehicle)
                              .map((assignment) => {
                                const assignedVehicle = vehicles?.find(v => v.id === assignment.resourceId);
                                return assignedVehicle ? (
                                  <div key={assignment.id} className="text-xs text-gray-600 dark:text-gray-300 bg-blue-50 dark:bg-blue-900/30 px-2 py-1 rounded">
                                    {assignedVehicle.callsign} - {assignedVehicle.type}
                                  </div>
                                ) : null;
                              })}
                          </div>
                        </div>
                      )}
                      
                      <div className="flex items-center justify-between">
                        <span className={`
                          px-2 py-1 text-xs font-medium rounded-full
                          ${getIncidentStatusBadgeColor(incident.status)}
                        `}>
                          {getIncidentStatusTranslation(incident.status, t)}
                        </span>
                        <span className="text-xs text-gray-500 dark:text-gray-400">
                          {t.id}: {incident.id}
                        </span>
                      </div>
                      
                      {/* View Details Button */}
                      <div className="flex justify-end mt-2">
                        <button
                          onClick={() => navigate(`/incidents/${incident.id}`)}
                          className="p-1 text-blue-600 dark:text-blue-400 hover:text-blue-800 dark:hover:text-blue-300 hover:bg-blue-50 dark:hover:bg-blue-900/30 rounded transition-colors"
                          title="View incident details"
                        >
                          <Eye className="w-4 h-4" />
                        </button>
                      </div>
                    </div>
                  </Popup>
                </Marker>
                )
              })}

            {/* Vehicles with GPS coordinates */}
            {filters.vehicles && vehicles
              ?.filter(vehicle => vehicle.latitude && vehicle.longitude)
              .map((vehicle) => (
                <Marker
                  key={`vehicle-${vehicle.id}`}
                  position={[vehicle.latitude!, vehicle.longitude!]}
                  icon={vehicleIcon}
                >
                  <Popup>
                    <div className="p-2">
                      <h3 className="font-semibold text-gray-900">{vehicle.callsign}</h3>
                      <p className="text-sm text-gray-600 mb-2">{vehicle.type}</p>
                      <div className="space-y-1">
                        <div className="flex justify-between text-xs">
                          <span>{t.status}:</span>
                          <span className={`
                            px-1 py-0.5 rounded text-xs font-medium
                            ${getVehicleStatusBadgeColor(vehicle.status)}
                          `}>
                            {getVehicleStatusTranslation(vehicle.status, t)}
                          </span>
                        </div>
                        {vehicle.fuelLevelPercent && (
                          <div className="flex justify-between text-xs">
                            <span>{t.fuel}:</span>
                            <span>{vehicle.fuelLevelPercent}%</span>
                          </div>
                        )}
                        {vehicle.waterLevelLiters && vehicle.waterCapacityLiters && (
                          <div className="flex justify-between text-xs">
                            <span>{t.water}:</span>
                            <span>
                              {Math.round((vehicle.waterLevelLiters / vehicle.waterCapacityLiters) * 100)}%
                            </span>
                          </div>
                        )}
                      </div>
                    </div>
                  </Popup>
                </Marker>
              ))}

            {/* Ships - Only for Coast Guard */}
            {filters.ships && shouldShowCoastGuardStations && ships?.map((ship) => (
              <Marker
                key={`ship-${ship.mmsi}`}
                position={[ship.latitude, ship.longitude]}
                icon={shipIcon}
              >
                <Popup>
                  <div className="p-2">
                    <h3 className="font-semibold text-gray-900">
                      {ship.name || `MMSI: ${ship.mmsi}`}
                    </h3>
                    <div className="space-y-1 mt-2">
                      <div className="flex justify-between text-xs">
                        <span>MMSI:</span>
                        <span className="font-mono">{ship.mmsi}</span>
                      </div>
                      {ship.speed && (
                        <div className="flex justify-between text-xs">
                          <span>Speed:</span>
                          <span>{ship.speed.toFixed(1)} {t.knots}</span>
                        </div>
                      )}
                      <div className="flex justify-between text-xs">
                        <span>Last Update:</span>
                        <span>{new Date(ship.lastUpdate).toLocaleTimeString()}</span>
                      </div>
                    </div>
                  </div>
                </Popup>
              </Marker>
            ))}

            {/* Patrol Zone Layer - Only for Police and Coast Guard */}
            {shouldShowPatrolZoneControls && showPatrolZones && (
              <PatrolZoneLayer 
                  onPatrolZoneClick={handlePatrolZoneClick}
                  onAssignVehicle={handleAssignVehicle}
                />
            )}

            {/* Patrol Zone Drawer - Only when drawing */}
            {isDrawingPatrolZone && (
              <PatrolZoneDrawer
                onPatrolZoneCreated={handlePatrolZoneCreated}
                onCancel={() => setIsDrawingPatrolZone(false)}
                startDrawing={isDrawingPatrolZone}
              />
            )}
            </MapContainer>
          </MapErrorBoundary>
        </div>
      </div>

      {/* Patrol Zone Modal */}
      {patrolZoneModalOpen && (
        <PatrolZoneModal
          isOpen={patrolZoneModalOpen}
          onClose={handleModalClose}
          mode={patrolZoneModalMode}
          pendingZone={pendingPatrolZone}
          editingZone={editingPatrolZone}
          onStartDrawing={() => setIsDrawingPatrolZone(true)}
        />
      )}

      {/* Vehicle Assignment Modal */}
      <PatrolZoneVehicleAssignmentModal
        isOpen={vehicleAssignmentModalOpen}
        onClose={() => {
          setVehicleAssignmentModalOpen(false)
          setAssigningPatrolZone(null)
        }}
        patrolZone={assigningPatrolZone}
      />

      {/* Map Filters Modal */}
      <MapFiltersModal
        isOpen={showFiltersModal}
        onClose={() => setShowFiltersModal(false)}
        filters={filters}
        onFiltersChange={handleFiltersChange}
        userAgency={user?.agencyName}
      />

      {/* Statistics */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mt-6">
        <div className="card p-6">
          <h3 className="text-lg font-semibold text-gray-900 dark:text-gray-100 mb-2">{t.activeIncidents}</h3>
          <p className="text-3xl font-bold text-red-600 dark:text-red-400">
            {incidents?.filter(i => 
              i.status !== IncidentStatus.FullyControlled && i.status !== IncidentStatus.Closed
            ).length || 0}
          </p>
        </div>
        
        <div className="card p-6">
          <h3 className="text-lg font-semibold text-gray-900 dark:text-gray-100 mb-2">{t.availableVehiclesModal}</h3>
          <p className="text-3xl font-bold text-green-600 dark:text-green-400">
            {vehicles?.filter(v => v.status === VehicleStatus.Available).length || 0}
          </p>
        </div>
        
        <div className="card p-6">
          <h3 className="text-lg font-semibold text-gray-900 dark:text-gray-100 mb-2">{t.vehiclesOnMap}</h3>
          <p className="text-3xl font-bold text-blue-600 dark:text-blue-400">
            {vehicles?.filter(v => v.latitude && v.longitude).length || 0}
          </p>
        </div>
        
        {shouldShowCoastGuardStations && (
          <div className="card p-6">
            <h3 className="text-lg font-semibold text-gray-900 dark:text-gray-100 mb-2">{t.vessels}</h3>
            <p className="text-3xl font-bold text-violet-600 dark:text-violet-400">
              {ships?.length || 0}
            </p>
          </div>
        )}
      </div>
      <style>
        {`
          /* Dark theme filters for map tiles only - exclude popups */
          .dark #main-map .leaflet-layer {
            filter: ${theme === 'dark' && mapViewType !== 'satellite' 
              ? 'invert(100%) hue-rotate(180deg) brightness(95%) contrast(90%)' 
              : 'none'};
          }
          
          /* Dark theme for zoom controls only */
          .dark #main-map .leaflet-control-zoom-in,
          .dark #main-map .leaflet-control-zoom-out {
            filter: ${theme === 'dark' && mapViewType !== 'satellite' 
              ? 'invert(100%) hue-rotate(180deg) brightness(95%) contrast(90%)' 
              : 'none'};
          }
          
          /* Satellite view filtering */
          .dark #main-map .leaflet-tile {
            filter: ${mapViewType === 'satellite' ? 'brightness(0.9) contrast(1.05) saturate(1.1)' : 'none'};
          }
          
          /* Show attribution for CodePen style */
          .leaflet-control-attribution {
            display: none !important;
          }
          
          /* Dark theme map container */
          .dark #main-map {
            background-color: #000000 !important;
          }
          
          /* Enhanced popup styling for dark theme */
          .leaflet-popup-content-wrapper {
            background: ${theme === 'dark' ? '#0f172a' : '#ffffff'} !important;
            color: ${theme === 'dark' ? '#e2e8f0' : '#000000'} !important;
            border-radius: 12px !important;
            box-shadow: ${theme === 'dark' 
              ? '0 25px 50px -12px rgba(0, 0, 0, 0.8), 0 0 0 1px rgba(148, 163, 184, 0.1)'
              : '0 25px 50px rgba(0, 0, 0, 0.25), 0 12px 24px rgba(0, 0, 0, 0.15), 0 0 0 1px rgba(255, 255, 255, 0.8) inset'} !important;
            border: 2px solid ${theme === 'dark' ? '#334155' : '#e5e7eb'} !important;
            min-width: 280px !important;
            max-width: 350px !important;
            backdrop-filter: ${theme === 'dark' ? 'blur(12px) saturate(180%)' : 'blur(10px)'} !important;
            animation: popupFadeIn 0.3s ease-out !important;
          }
          
          @keyframes popupFadeIn {
            from {
              opacity: 0;
              transform: translateY(-10px) scale(0.95);
            }
            to {
              opacity: 1;
              transform: translateY(0) scale(1);
            }
          }
          
          .leaflet-popup-tip {
            background: ${theme === 'dark' ? '#0f172a' : '#ffffff'} !important;
            border: 2px solid ${theme === 'dark' ? '#334155' : '#e5e7eb'} !important;
            border-top: none !important;
            border-right: none !important;
            box-shadow: ${theme === 'dark' 
              ? '0 8px 16px -4px rgba(0, 0, 0, 0.6)' 
              : '0 4px 6px -1px rgba(0, 0, 0, 0.1)'} !important;
          }
          
          /* Enhanced popup content styling with better typography */
          .leaflet-popup-content {
            color: ${theme === 'dark' ? '#e2e8f0' : '#374151'} !important;
            margin: 16px 20px !important;
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif !important;
            line-height: 1.5 !important;
          }
          
          .leaflet-popup-content h3 {
            color: ${theme === 'dark' ? '#f1f5f9' : '#1f2937'} !important;
            margin: 0 0 12px 0 !important;
            font-weight: 700 !important;
            font-size: 16px !important;
            letter-spacing: -0.025em !important;
            border-bottom: 2px solid ${theme === 'dark' ? '#475569' : '#e5e7eb'} !important;
            padding-bottom: 8px !important;
          }
          
          .leaflet-popup-content .text-gray-900 {
            color: ${theme === 'dark' ? '#f8fafc' : '#1f2937'} !important;
            font-weight: 600 !important;
            font-size: 14px !important;
          }
          
          .leaflet-popup-content .text-gray-600 {
            color: ${theme === 'dark' ? '#cbd5e1' : '#4b5563'} !important;
            font-weight: 500 !important;
            font-size: 13px !important;
          }
          
          .leaflet-popup-content .text-xs {
            color: ${theme === 'dark' ? '#a1a1aa' : '#6b7280'} !important;
            font-weight: 400 !important;
            font-size: 12px !important;
          }
          
          /* Enhanced popup labels and values */
          .leaflet-popup-content p {
            margin: 8px 0 !important;
            display: flex !important;
            justify-content: space-between !important;
            align-items: center !important;
            padding: 4px 0 !important;
            border-bottom: 1px solid ${theme === 'dark' ? '#334155' : '#f3f4f6'} !important;
          }
          
          .leaflet-popup-content p:last-child {
            border-bottom: none !important;
            margin-bottom: 0 !important;
          }
          
          .leaflet-popup-content strong {
            color: ${theme === 'dark' ? '#f8fafc' : '#1f2937'} !important;
            font-weight: 600 !important;
            font-size: 13px !important;
            text-transform: uppercase !important;
            letter-spacing: 0.05em !important;
            flex-shrink: 0 !important;
            margin-right: 12px !important;
          }
          
          /* Enhanced button styling in popups */
          .leaflet-popup-content button {
            border: 1px solid transparent !important;
            border-radius: 6px !important;
            padding: 8px 16px !important;
            font-weight: 500 !important;
            font-size: 13px !important;
            margin: 8px 4px 4px 0 !important;
            cursor: pointer !important;
            transition: all 0.15s ease !important;
            box-shadow: none !important;
          }
          
          /* Green Edit button (first button) */
          .leaflet-popup-content button:first-of-type {
            background: ${theme === 'dark' ? '#059669' : '#22c55e'} !important;
            color: #ffffff !important;
            border-color: ${theme === 'dark' ? '#047857' : '#16a34a'} !important;
            box-shadow: ${theme === 'dark' ? '0 4px 12px rgba(5, 150, 105, 0.3)' : 'none'} !important;
          }
          
          .leaflet-popup-content button:first-of-type:hover {
            background: ${theme === 'dark' ? '#047857' : '#16a34a'} !important;
            transform: translateY(-1px) !important;
            box-shadow: ${theme === 'dark' ? '0 6px 16px rgba(5, 150, 105, 0.4)' : 'none'} !important;
          }
          
          /* Blue Assign Vehicle button (second button) */
          .leaflet-popup-content button:last-of-type {
            background: ${theme === 'dark' ? '#1d4ed8' : '#3b82f6'} !important;
            color: #ffffff !important;
            border-color: ${theme === 'dark' ? '#1e40af' : '#2563eb'} !important;
            box-shadow: ${theme === 'dark' ? '0 4px 12px rgba(29, 78, 216, 0.3)' : 'none'} !important;
          }
          
          .leaflet-popup-content button:last-of-type:hover {
            background: ${theme === 'dark' ? '#1e40af' : '#2563eb'} !important;
            transform: translateY(-1px) !important;
            box-shadow: ${theme === 'dark' ? '0 6px 16px rgba(29, 78, 216, 0.4)' : 'none'} !important;
          }
          
          /* Enhanced close button */
          .leaflet-popup-close-button {
            color: ${theme === 'dark' ? '#a1a1aa' : '#6b7280'} !important;
            font-size: 18px !important;
            font-weight: bold !important;
            padding: 4px 8px !important;
            border-radius: 6px !important;
            transition: all 0.2s ease !important;
            background: ${theme === 'dark' ? 'rgba(51, 65, 85, 0.3)' : 'transparent'} !important;
          }
          
          .leaflet-popup-close-button:hover {
            background: ${theme === 'dark' ? '#334155' : '#f3f4f6'} !important;
            color: ${theme === 'dark' ? '#f8fafc' : '#374151'} !important;
            transform: scale(1.1) !important;
          }
          
          /* Enhanced control styling for dark theme - let the filter handle the inversion */
          .leaflet-control-zoom a {
            background-color: #ffffff !important;
            color: #111827 !important;
            border: 1px solid #d1d5db !important;
            box-shadow: 0 1px 3px 0 rgba(0, 0, 0, 0.1) !important;
          }
          
          .leaflet-control-zoom a:hover {
            background-color: #f9fafb !important;
            border-color: #9ca3af !important;
          }
          
          .leaflet-control-zoom a:first-child {
            border-top-left-radius: 6px !important;
            border-top-right-radius: 6px !important;
          }
          
          .leaflet-control-zoom a:last-child {
            border-bottom-left-radius: 6px !important;
            border-bottom-right-radius: 6px !important;
          }
          
          /* Dark theme for other leaflet controls */
          .dark .leaflet-control {
            background-color: ${theme === 'dark' ? '#1e293b' : '#ffffff'} !important;
            border: 1px solid ${theme === 'dark' ? '#334155' : '#d1d5db'} !important;
            border-radius: 6px !important;
            box-shadow: ${theme === 'dark' ? '0 4px 6px -1px rgba(0, 0, 0, 0.3)' : '0 1px 3px 0 rgba(0, 0, 0, 0.1)'} !important;
          }
          
          /* Tooltip styling for dark theme */
          .dark .leaflet-tooltip {
            background-color: #1e293b !important;
            color: #f1f5f9 !important;
            border: 1px solid #334155 !important;
            box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.3) !important;
          }
          
          .dark .leaflet-tooltip:before {
            border-top-color: #1e293b !important;
          }
        `}
      </style>
    </div>
  )
}