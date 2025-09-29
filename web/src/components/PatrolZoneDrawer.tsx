import { useState, useEffect } from 'react'
import { useMap } from 'react-leaflet'
import L from 'leaflet'
import 'leaflet-draw/dist/leaflet.draw.css'
import 'leaflet-draw'
import { CreatePatrolZone } from '../types'
import { stationAssignmentApi } from '../lib/api'
// import { useTranslation } from '../hooks/useTranslation'
import { useUserStore } from '../lib/userStore'

// Map user agency names to API expected strings
const mapAgencyNameToApiString = (agencyName?: string): string => {
  if (!agencyName) return 'fire' // default fallback

  const lowerAgency = agencyName.toLowerCase()

  if (lowerAgency.includes('fire') || agencyName === 'Hellenic Fire Service' || agencyName === 'Fire Department' || agencyName === 'FireDepartment') {
    return 'fire'
  } else if (lowerAgency.includes('coast') || agencyName === 'Hellenic Coast Guard' || agencyName === 'Coast Guard' || agencyName === 'CoastGuard') {
    return 'coastguard'
  } else if (lowerAgency.includes('police') || agencyName === 'Hellenic Police' || agencyName === 'Police') {
    return 'police'
  } else if (lowerAgency.includes('ekab') || agencyName === 'EKAB') {
    return 'hospital'
  }

  return 'fire' // default fallback
}

interface PatrolZoneDrawerProps {
  onPatrolZoneCreated: (zone: CreatePatrolZone) => void
  onCancel: () => void
  startDrawing?: boolean
}

export default function PatrolZoneDrawer({ onPatrolZoneCreated, startDrawing }: PatrolZoneDrawerProps) {
  const map = useMap()
  // const t = useTranslation()
  const { user } = useUserStore()
  const [drawControl, setDrawControl] = useState<L.Control.Draw | null>(null)
  // const [drawnItems, setDrawnItems] = useState<L.FeatureGroup | null>(null)

  useEffect(() => {
    if (!map) return

    // Create a feature group to store drawn items
    const drawnItemsLayer = new L.FeatureGroup()
    map.addLayer(drawnItemsLayer)
    // setDrawnItems(drawnItemsLayer)

    // Create draw control
    const drawControlInstance = new L.Control.Draw({
      position: 'topright',
      draw: {
        polygon: {
          allowIntersection: false,
          drawError: {
            color: '#e1e100',
            message: '<strong>Error:</strong> Shape edges cannot cross!'
          },
          shapeOptions: {
            color: '#3b82f6',
            weight: 3,
            opacity: 0.8,
            fillOpacity: 0.2
          }
        },
        polyline: false,
        rectangle: false,
        circle: false,
        marker: false,
        circlemarker: false
      },
      edit: {
        featureGroup: drawnItemsLayer,
        remove: true
      }
    })

    setDrawControl(drawControlInstance)

    // Handle draw events
    const onDrawCreated = async (e: any) => {
      const layer = e.layer
      const coordinates = layer.getLatLngs()[0].map((latlng: L.LatLng) => [latlng.lng, latlng.lat])

      // Close the polygon by adding the first point at the end
      coordinates.push(coordinates[0])

      // Create GeoJSON polygon
      const geoJsonPolygon = {
        type: 'Polygon',
        coordinates: [coordinates]
      }

      // Calculate center point
      const bounds = layer.getBounds()
      const center = bounds.getCenter()

      console.log('User object in PatrolZoneDrawer:', user)
      console.log('User stationId:', user?.stationId)

      // Try to find nearest station automatically
      let autoAssignedStationId = 0
      try {
        if (center.lat && center.lng && user?.agencyName) {
          const agencyType = mapAgencyNameToApiString(user.agencyName)
          console.log('Looking up nearest station for patrol zone center:', center.lat, center.lng, 'Agency:', agencyType)

          const station = await stationAssignmentApi.findByLocation({
            latitude: center.lat,
            longitude: center.lng,
            agencyType
          })

          if (station) {
            autoAssignedStationId = station.stationId
            console.log('Auto-assigned station for patrol zone:', station.stationName, 'ID:', station.stationId)
          }
        }
      } catch (error) {
        console.error('Failed to find nearest station for patrol zone:', error)
        // Continue with stationId: 0 for manual selection
      }

      // Create patrol zone data
      const patrolZoneData: CreatePatrolZone = {
        name: `Patrol Zone ${Date.now()}`, // Temporary name, will be updated in modal
        description: '',
        stationId: autoAssignedStationId, // Auto-assigned if found, otherwise 0 for manual selection
        boundaryCoordinates: JSON.stringify(geoJsonPolygon),
        centerLatitude: center.lat,
        centerLongitude: center.lng,
        priority: 2, // Medium priority by default
        color: '#3b82f6' // Blue by default
      }

      console.log('Created patrol zone data:', patrolZoneData)

      drawnItemsLayer.addLayer(layer)
      onPatrolZoneCreated(patrolZoneData)
    }

    const onDrawStart = () => {
      // Drawing started
    }

    const onDrawStop = () => {
      // Drawing stopped
    }

    map.on(L.Draw.Event.CREATED, onDrawCreated)
    map.on(L.Draw.Event.DRAWSTART, onDrawStart)
    map.on(L.Draw.Event.DRAWSTOP, onDrawStop)

    return () => {
      map.off(L.Draw.Event.CREATED, onDrawCreated)
      map.off(L.Draw.Event.DRAWSTART, onDrawStart)
      map.off(L.Draw.Event.DRAWSTOP, onDrawStop)
      if (drawControlInstance) {
        map.removeControl(drawControlInstance)
      }
      map.removeLayer(drawnItemsLayer)
    }
  }, [map, onPatrolZoneCreated, user?.stationId])

  useEffect(() => {
    if (!map || !drawControl) return

    // Add the draw control when component mounts
    map.addControl(drawControl)

    return () => {
      try {
        if (drawControl) {
          map.removeControl(drawControl)
        }
      } catch (e) {
        // Control might already be removed
      }
    }
  }, [map, drawControl])

  // Effect to automatically start drawing when startDrawing prop is true
  useEffect(() => {
    if (!map || !drawControl || !startDrawing) return

    // Find the polygon draw button and trigger it
    const polygonButton = document.querySelector('.leaflet-draw-draw-polygon')
    if (polygonButton) {
      (polygonButton as HTMLElement).click()
    }
  }, [map, drawControl, startDrawing])

  return null // This component doesn't render anything visible
}