import { useState, useCallback, useRef, useEffect } from 'react'
import { MapContainer, TileLayer, Marker, useMapEvents } from 'react-leaflet'
import { Search, MapPin, Crosshair } from 'lucide-react'
import { useTheme } from '../lib/themeContext'
import L from 'leaflet'
import 'leaflet/dist/leaflet.css'

// Fix for default markers in react-leaflet
delete (L.Icon.Default.prototype as any)._getIconUrl
L.Icon.Default.mergeOptions({
  iconRetinaUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.7.1/images/marker-icon-2x.png',
  iconUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.7.1/images/marker-icon.png',
  shadowUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.7.1/images/marker-shadow.png',
})

interface AddressSuggestion {
  display_name: string
  lat: string
  lon: string
  address?: any
}

interface DetailedAddress {
  address?: string
  street?: string
  streetNumber?: string
  city?: string
  region?: string
  postalCode?: string
  country?: string
}

interface LocationPickerProps {
  latitude: number
  longitude: number
  address: string
  onLocationChange: (lat: number, lng: number, address?: string, detailedAddress?: DetailedAddress) => void
  onAddressChange: (address: string) => void
  onDetailedAddressChange?: (detailedAddress: DetailedAddress) => void
}

// Component to handle map clicks
function MapClickHandler({ onLocationSelect }: { onLocationSelect: (lat: number, lng: number) => void }) {
  useMapEvents({
    click: (e) => {
      onLocationSelect(e.latlng.lat, e.latlng.lng)
    },
  })
  return null
}

export default function LocationPicker({
  latitude,
  longitude,
  address,
  onLocationChange,
  onAddressChange,
  onDetailedAddressChange
}: LocationPickerProps) {
  const [searchQuery, setSearchQuery] = useState(address)
  const { theme } = useTheme()

  const [suggestions, setSuggestions] = useState<AddressSuggestion[]>([])
  const [showSuggestions, setShowSuggestions] = useState(false)
  const mapRef = useRef<L.Map>(null)
  const searchTimeoutRef = useRef<number | null>(null)

  // Default center (Athens, Greece)
  const defaultCenter: [number, number] = [37.9755, 23.7348]
  const currentPosition: [number, number] = latitude && longitude ? [latitude, longitude] : defaultCenter

  // Extract detailed address from Nominatim address object
  const extractDetailedAddress = (nominatimAddress: any): DetailedAddress => {
    if (!nominatimAddress) return {}

    const addr = nominatimAddress

    return {
      street: addr.road || addr.street || addr.pedestrian || undefined,
      streetNumber: addr.house_number || undefined,
      city: addr.city || addr.town || addr.municipality || addr.village || undefined,
      region: addr.state || addr.region || addr.county || addr.state_district || undefined,
      postalCode: addr.postcode || undefined,
      country: addr.country || 'Greece'
    }
  }

  // Search for address suggestions
  const searchSuggestions = async (query: string) => {
    if (!query.trim() || query.length < 3) {
      setSuggestions([])
      setShowSuggestions(false)
      return
    }

    try {
      // Enhance the query for better Greek address search
      let enhancedQuery = query.trim()

      // Add Greece context if not already present
      if (!enhancedQuery.toLowerCase().includes('greece') &&
        !enhancedQuery.toLowerCase().includes('ελλάδα') &&
        !enhancedQuery.toLowerCase().includes('athens') &&
        !enhancedQuery.toLowerCase().includes('αθήνα') &&
        !enhancedQuery.toLowerCase().includes('piraeus') &&
        !enhancedQuery.toLowerCase().includes('πειραιάς') &&
        !enhancedQuery.toLowerCase().includes('πειραιας')) {
        enhancedQuery += ', Greece'
      }

      // Extract street name and number if present
      const numberMatch = query.match(/(.+?)\s+(\d+)/)
      const streetName = numberMatch ? numberMatch[1].trim() : query
      const houseNumber = numberMatch ? numberMatch[2] : null

      // Try multiple search strategies for better results
      const searchStrategies = [
        // Strategy 1: Exact query with structured search
        `https://nominatim.openstreetmap.org/search?format=json&q=${encodeURIComponent(enhancedQuery)}&limit=10&addressdetails=1&countrycodes=gr&extratags=1`,
        // Strategy 2: If query has a number, try structured search
        ...(houseNumber ? [
          `https://nominatim.openstreetmap.org/search?format=json&street=${encodeURIComponent(streetName + ' ' + houseNumber)}&country=Greece&limit=5&addressdetails=1`,
          // Strategy 3: Search for just the street name to get the street location
          `https://nominatim.openstreetmap.org/search?format=json&q=${encodeURIComponent(streetName + ', Greece')}&limit=3&addressdetails=1&countrycodes=gr`
        ] : [])
      ]

      let allResults: any[] = []

      for (const searchUrl of searchStrategies) {
        try {
          const response = await fetch(searchUrl, {
            headers: {
              'User-Agent': 'IncidentManagement/1.0'
            }
          })

          if (response.ok) {
            const strategyData = await response.json()
            if (strategyData && strategyData.length > 0) {
              allResults = [...allResults, ...strategyData]
            }
          }
        } catch (err) {
          console.warn('Search strategy failed:', err)
        }
      }

      // Remove duplicates based on lat/lon
      const uniqueResults = allResults.filter((result, index, self) =>
        index === self.findIndex(r => r.lat === result.lat && r.lon === result.lon)
      )

      const data = uniqueResults.slice(0, 5) // Limit to 5 results



      if (data && data.length > 0) {
        // Debug: log the raw data to understand the structure
        console.log('Raw Nominatim data:', data[0])

        const formattedSuggestions = data.map((result: any) => {
          let friendlyAddress = result.display_name

          // Enhanced address formatting for Greek addresses
          if (result.address) {
            const addr = result.address
            const parts = []

            // Handle street and house number - try multiple combinations
            let streetPart = ''
            if (addr.house_number && addr.road) {
              streetPart = `${addr.road} ${addr.house_number}`
            } else if (addr.road && addr.house_number) {
              streetPart = `${addr.road} ${addr.house_number}`
            } else if (addr.house_number && (addr.street || addr.pedestrian)) {
              streetPart = `${addr.street || addr.pedestrian} ${addr.house_number}`
            } else if (addr.road) {
              streetPart = addr.road
            } else if (addr.street) {
              streetPart = addr.street
            } else if (addr.pedestrian) {
              streetPart = addr.pedestrian
            }

            // Check if user searched for a specific number and this result doesn't have it
            const searchedNumber = query.match(/\d+/)
            if (searchedNumber && !addr.house_number && streetPart) {
              // If user searched for a number but result doesn't have house_number,
              // add the searched number to create the desired address
              streetPart = `${streetPart} ${searchedNumber[0]}`
            } else if (!addr.house_number && result.display_name) {
              // Fallback: extract any number from display name
              const numberMatch = result.display_name.match(/(\d+)/)
              if (numberMatch && streetPart) {
                streetPart = `${streetPart} ${numberMatch[1]}`
              }
            }

            if (streetPart) {
              parts.push(streetPart)
            }

            // Add neighborhood/area
            if (addr.suburb) {
              parts.push(addr.suburb)
            } else if (addr.neighbourhood) {
              parts.push(addr.neighbourhood)
            } else if (addr.quarter) {
              parts.push(addr.quarter)
            }

            // Add city/municipality
            if (addr.city) {
              parts.push(addr.city)
            } else if (addr.town) {
              parts.push(addr.town)
            } else if (addr.municipality) {
              parts.push(addr.municipality)
            } else if (addr.village) {
              parts.push(addr.village)
            }

            if (parts.length > 0) {
              friendlyAddress = parts.join(', ')
            }
          }

          // If we still don't have a good address, try to extract from display_name
          if (friendlyAddress === result.display_name && result.display_name) {
            // Try to create a shorter version from the full display name
            const displayParts = result.display_name.split(',').map((p: string) => p.trim())
            if (displayParts.length >= 3) {
              // Take first 3 parts for a more concise address
              friendlyAddress = displayParts.slice(0, 3).join(', ')
            }
          }

          return {
            display_name: friendlyAddress,
            lat: result.lat,
            lon: result.lon,
            address: result.address
          }
        })

        // If user searched for a specific house number but we didn't find it,
        // add a custom suggestion using the street location
        if (houseNumber && formattedSuggestions.length > 0) {
          const streetResult = formattedSuggestions.find(s =>
            s.address && s.address.road && !s.address.house_number
          )

          if (streetResult) {
            // Create a custom suggestion with the desired house number
            const customSuggestion = {
              display_name: `${streetResult.address.road} ${houseNumber}, ${streetResult.address.neighbourhood || streetResult.address.city || 'Piraeus'}`,
              lat: streetResult.lat,
              lon: streetResult.lon,
              address: {
                ...streetResult.address,
                house_number: houseNumber
              }
            }

            // Add the custom suggestion at the beginning
            formattedSuggestions.unshift(customSuggestion)
          }
        }

        setSuggestions(formattedSuggestions)
        setShowSuggestions(true)
      } else {
        setSuggestions([])
        setShowSuggestions(false)
      }
    } catch (error) {
      console.error('Suggestion search error:', error)
      setSuggestions([])
      setShowSuggestions(false)
    }
  }

  // Select a suggestion
  const selectSuggestion = (suggestion: AddressSuggestion) => {
    const lat = parseFloat(suggestion.lat)
    const lng = parseFloat(suggestion.lon)

    // Extract detailed address information
    const detailedAddress = extractDetailedAddress(suggestion.address)
    detailedAddress.address = suggestion.display_name

    onLocationChange(lat, lng, suggestion.display_name, detailedAddress)

    // Also call the detailed address change callback if provided
    if (onDetailedAddressChange) {
      onDetailedAddressChange(detailedAddress)
    }

    setSearchQuery(suggestion.display_name)
    setSuggestions([])
    setShowSuggestions(false)

    // Pan map to the new location
    if (mapRef.current) {
      mapRef.current.setView([lat, lng], 16)
    }
  }

  // Reverse geocoding function with user-friendly formatting
  const reverseGeocode = async (lat: number, lng: number) => {
    try {
      const response = await fetch(
        `https://nominatim.openstreetmap.org/reverse?format=json&lat=${lat}&lon=${lng}&zoom=18&addressdetails=1`,
        {
          headers: {
            'User-Agent': 'IncidentManagement/1.0'
          }
        }
      )

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`)
      }

      const data = await response.json()

      if (data && data.address) {
        // Enhanced address formatting for Greek addresses
        const addr = data.address
        const parts = []

        // Handle street and house number - try multiple combinations
        let streetPart = ''
        if (addr.house_number && addr.road) {
          streetPart = `${addr.road} ${addr.house_number}`
        } else if (addr.road && addr.house_number) {
          streetPart = `${addr.road} ${addr.house_number}`
        } else if (addr.house_number && (addr.street || addr.pedestrian)) {
          streetPart = `${addr.street || addr.pedestrian} ${addr.house_number}`
        } else if (addr.road) {
          streetPart = addr.road
        } else if (addr.street) {
          streetPart = addr.street
        } else if (addr.pedestrian) {
          streetPart = addr.pedestrian
        }

        // Also check if house number is embedded in the display name
        if (!addr.house_number && data.display_name) {
          const numberMatch = data.display_name.match(/(\d+)/)
          if (numberMatch && streetPart) {
            streetPart = `${streetPart} ${numberMatch[1]}`
          }
        }

        if (streetPart) {
          parts.push(streetPart)
        }

        // Add neighborhood/area
        if (addr.suburb) {
          parts.push(addr.suburb)
        } else if (addr.neighbourhood) {
          parts.push(addr.neighbourhood)
        } else if (addr.quarter) {
          parts.push(addr.quarter)
        }

        // Add city/municipality
        if (addr.city) {
          parts.push(addr.city)
        } else if (addr.town) {
          parts.push(addr.town)
        } else if (addr.municipality) {
          parts.push(addr.municipality)
        } else if (addr.village) {
          parts.push(addr.village)
        }

        const friendlyAddress = parts.length > 0 ? parts.join(', ') : data.display_name

        // Extract detailed address information
        const detailedAddress = extractDetailedAddress(data.address)
        detailedAddress.address = friendlyAddress

        // Call the detailed address change callback if provided
        if (onDetailedAddressChange) {
          onDetailedAddressChange(detailedAddress)
        }

        onAddressChange(friendlyAddress)
        setSearchQuery(friendlyAddress)
      } else if (data && data.display_name) {
        // Try to create a shorter version from the full display name
        const displayParts = data.display_name.split(',').map((p: string) => p.trim())
        const shortAddress = displayParts.length >= 3 ? displayParts.slice(0, 3).join(', ') : data.display_name

        onAddressChange(shortAddress)
        setSearchQuery(shortAddress)
      }
    } catch (error) {
      console.error('Reverse geocoding error:', error)
      // Fallback to coordinates
      const coordString = `${lat.toFixed(6)}, ${lng.toFixed(6)}`
      onAddressChange(coordString)
      setSearchQuery(coordString)
    }
  }

  const handleMapClick = useCallback((lat: number, lng: number) => {
    onLocationChange(lat, lng)
    reverseGeocode(lat, lng)
  }, [onLocationChange])

  // Handle search input changes with debouncing
  const handleSearchInputChange = (value: string) => {
    setSearchQuery(value)
    onAddressChange(value)

    // Clear previous timeout
    if (searchTimeoutRef.current) {
      clearTimeout(searchTimeoutRef.current)
    }

    // Set new timeout for suggestions
    searchTimeoutRef.current = setTimeout(() => {
      searchSuggestions(value)
    }, 300)
  }

  // Handle manual search button click
  const handleSearchClick = () => {
    // First check if it's coordinates
    const coordMatch = searchQuery.match(/(-?\d+\.?\d*),?\s*(-?\d+\.?\d*)/)
    if (coordMatch) {
      const lat = parseFloat(coordMatch[1])
      const lng = parseFloat(coordMatch[2])
      if (!isNaN(lat) && !isNaN(lng) && lat >= -90 && lat <= 90 && lng >= -180 && lng <= 180) {
        onLocationChange(lat, lng, `${lat}, ${lng}`)
        if (mapRef.current) {
          mapRef.current.setView([lat, lng], 16)
        }
        return
      }
    }

    // Otherwise search for suggestions and select the first one
    if (suggestions.length > 0) {
      selectSuggestion(suggestions[0])
    } else {
      searchSuggestions(searchQuery)
    }
  }

  const getCurrentLocation = () => {
    if (navigator.geolocation) {
      navigator.geolocation.getCurrentPosition(
        (position) => {
          const lat = position.coords.latitude
          const lng = position.coords.longitude
          onLocationChange(lat, lng)
          reverseGeocode(lat, lng)

          // Pan map to current location
          if (mapRef.current) {
            mapRef.current.setView([lat, lng], 16)
          }
        },
        (error) => {
          console.error('Error getting location:', error)
          alert('Unable to get current location. Please search for an address or click on the map.')
        }
      )
    } else {
      alert('Geolocation is not supported by this browser.')
    }
  }

  // Update search query when address prop changes
  useEffect(() => {
    if (address !== searchQuery) {
      setSearchQuery(address)
    }
  }, [address])

  // Cleanup timeout on unmount
  useEffect(() => {
    return () => {
      if (searchTimeoutRef.current) {
        clearTimeout(searchTimeoutRef.current)
      }
    }
  }, [])

  return (
    <div className="space-y-4">
      {/* Address Search */}
      <div>
        <label className="block text-sm font-medium text-gray-900 dark:text-gray-100 mb-2">
          Search Address
        </label>
        <div className="relative">
          <div className="flex gap-2">
            <div className="flex-1 relative">
              <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 dark:text-gray-500 w-4 h-4" />
              <input
                type="text"
                value={searchQuery}
                onChange={(e) => handleSearchInputChange(e.target.value)}
                onFocus={() => {
                  if (suggestions.length > 0) {
                    setShowSuggestions(true)
                  }
                }}
                className="w-full pl-10 pr-3 py-2 border border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-800 text-gray-900 dark:text-gray-100 rounded-md focus:outline-none focus:ring-2 focus:ring-primary-500"
                placeholder="e.g. Καλλέργη 80 Πειραιάς or Kallergi 80 Piraeus"
              />
            </div>
            <button
              type="button"
              onClick={handleSearchClick}
              disabled={!searchQuery.trim()}
              className="btn btn-secondary flex items-center"
            >
              Search
            </button>
          </div>

          {/* Suggestions Dropdown */}
          {showSuggestions && suggestions.length > 0 && (
            <div className="absolute z-10 w-full mt-1 bg-white dark:bg-gray-800 border border-gray-300 dark:border-gray-600 rounded-md shadow-lg max-h-60 overflow-y-auto">
              {suggestions.map((suggestion, index) => (
                <button
                  key={index}
                  type="button"
                  onClick={() => selectSuggestion(suggestion)}
                  className="w-full text-left px-4 py-2 hover:bg-gray-100 dark:hover:bg-gray-700 focus:bg-gray-100 dark:focus:bg-gray-700 focus:outline-none border-b border-gray-100 dark:border-gray-700 last:border-b-0"
                >
                  <div className="flex items-center">
                    <MapPin className="w-4 h-4 text-gray-400 dark:text-gray-500 mr-2 flex-shrink-0" />
                    <span className="text-sm text-gray-900 dark:text-gray-100 truncate">
                      {suggestion.display_name}
                    </span>
                  </div>
                </button>
              ))}
            </div>
          )}
        </div>
      </div>

      {/* Current Location Button */}
      <div className="flex justify-between items-center">
        <button
          type="button"
          onClick={getCurrentLocation}
          className="btn btn-secondary flex items-center text-sm"
        >
          <Crosshair className="w-4 h-4 mr-1" />
          Use Current Location
        </button>

        {latitude && longitude && (
          <div className="text-sm text-gray-600 dark:text-gray-400">
            <MapPin className="w-4 h-4 inline mr-1" />
            {latitude.toFixed(6)}, {longitude.toFixed(6)}
          </div>
        )}
      </div>

      {/* Interactive Map */}
      <div>
        <label className="block text-sm font-medium text-gray-900 dark:text-gray-100 mb-2">
          Click on map to set location
        </label>
        <div className="border border-gray-300 dark:border-gray-600 rounded-md overflow-hidden" style={{ height: '500px' }}>
          <MapContainer
            center={currentPosition}
            zoom={latitude && longitude ? 16 : 12}
            style={{ height: '100%', width: '100%' }}
            ref={mapRef}
          >
            <TileLayer
              attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
              url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
            />

            <MapClickHandler onLocationSelect={handleMapClick} />

            {latitude && longitude && (
              <Marker position={[latitude, longitude]} />
            )}
          </MapContainer>
        </div>
        <p className="text-xs text-gray-500 dark:text-gray-400 mt-1">
          Click anywhere on the map to place the incident marker
        </p>
      </div>
      <style>
        {`
          /* Dark theme filters for map tiles only - exclude popups */
          .dark .leaflet-layer {
            filter: ${theme === 'dark' 
              ? 'invert(100%) hue-rotate(180deg) brightness(95%) contrast(90%)' 
              : 'none'};
          }
          
          /* Dark theme for zoom controls only */
          .dark .leaflet-control-zoom-in,
          .dark .leaflet-control-zoom-out {
            filter: ${theme === 'dark' 
              ? 'invert(100%) hue-rotate(180deg) brightness(95%) contrast(90%)' 
              : 'none'};
          }
          
          .dark .leaflet-container {
            background-color: #000000 !important;
          }
          
          /* Enhanced popup styling matching MapView */
          .leaflet-popup-content-wrapper {
            background: ${theme === 'dark' ? '#0f172a' : '#ffffff'} !important;
            color: ${theme === 'dark' ? '#e2e8f0' : '#1f2937'} !important;
            border: 1px solid ${theme === 'dark' ? '#334155' : '#e5e7eb'} !important;
            border-radius: 12px !important;
            box-shadow: ${theme === 'dark' 
              ? '0 25px 50px -12px rgba(0, 0, 0, 0.8), 0 0 0 1px rgba(148, 163, 184, 0.1)' 
              : '0 20px 25px -5px rgba(0, 0, 0, 0.1), 0 10px 10px -5px rgba(0, 0, 0, 0.04)'} !important;
            backdrop-filter: ${theme === 'dark' ? 'blur(12px) saturate(180%)' : 'none'} !important;
            animation: popupFadeIn 0.3s ease-out !important;
          }
          
          .leaflet-popup-tip {
            background: ${theme === 'dark' ? '#0f172a' : '#ffffff'} !important;
            border: 1px solid ${theme === 'dark' ? '#334155' : '#e5e7eb'} !important;
            border-top: none !important;
            border-right: none !important;
            box-shadow: ${theme === 'dark' 
              ? '0 8px 16px -4px rgba(0, 0, 0, 0.6)' 
              : '0 4px 6px -1px rgba(0, 0, 0, 0.1)'} !important;
          }
          
          .leaflet-popup-content {
            color: ${theme === 'dark' ? '#e2e8f0' : '#374151'} !important;
            margin: 16px 20px !important;
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif !important;
            line-height: 1.5 !important;
          }
          
          .leaflet-popup-content h3,
          .leaflet-popup-content h4 {
            color: ${theme === 'dark' ? '#f8fafc' : '#1f2937'} !important;
            margin: 0 0 12px 0 !important;
            font-weight: 700 !important;
            font-size: 16px !important;
            letter-spacing: -0.025em !important;
            border-bottom: 2px solid ${theme === 'dark' ? '#334155' : '#e5e7eb'} !important;
            padding-bottom: 8px !important;
          }
          
          .leaflet-popup-content p {
            margin: 8px 0 !important;
            color: ${theme === 'dark' ? '#cbd5e1' : '#4b5563'} !important;
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
          
          /* Enhanced button styling for LocationPicker */
          .leaflet-popup-content button {
            border: 1px solid transparent !important;
            border-radius: 6px !important;
            padding: 8px 16px !important;
            font-weight: 500 !important;
            font-size: 13px !important;
            margin: 8px 4px 4px 0 !important;
            cursor: pointer !important;
            transition: all 0.15s ease !important;
            background: ${theme === 'dark' ? '#059669' : '#22c55e'} !important;
            color: #ffffff !important;
            border-color: ${theme === 'dark' ? '#047857' : '#16a34a'} !important;
            box-shadow: ${theme === 'dark' ? '0 4px 12px rgba(5, 150, 105, 0.3)' : 'none'} !important;
          }
          
          .leaflet-popup-content button:hover {
            background: ${theme === 'dark' ? '#047857' : '#16a34a'} !important;
            transform: translateY(-1px) !important;
            box-shadow: ${theme === 'dark' ? '0 6px 16px rgba(5, 150, 105, 0.4)' : 'none'} !important;
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
          
          /* Show attribution for CodePen style */
          .leaflet-control-attribution {
              display: none !important;
          }           
        `}
      </style>
    </div>
  )
}