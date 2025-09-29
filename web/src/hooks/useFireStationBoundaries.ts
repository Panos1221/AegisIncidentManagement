import { useQuery } from '@tanstack/react-query'
import { useMemo, useState, useCallback } from 'react'
import { FireStationBoundary } from '../types'
import { fireStationsApi } from '../lib/api'

// Polygon simplification utility using Douglas-Peucker algorithm
const simplifyPolygon = (coordinates: [number, number][], tolerance: number = 0.001): [number, number][] => {
    if (coordinates.length <= 2) return coordinates

    const getPerpendicularDistance = (point: [number, number], lineStart: [number, number], lineEnd: [number, number]): number => {
        const [x0, y0] = point
        const [x1, y1] = lineStart
        const [x2, y2] = lineEnd

        const A = x0 - x1
        const B = y0 - y1
        const C = x2 - x1
        const D = y2 - y1

        const dot = A * C + B * D
        const lenSq = C * C + D * D

        if (lenSq === 0) return Math.sqrt(A * A + B * B)

        const param = dot / lenSq
        let xx: number, yy: number

        if (param < 0) {
            xx = x1
            yy = y1
        } else if (param > 1) {
            xx = x2
            yy = y2
        } else {
            xx = x1 + param * C
            yy = y1 + param * D
        }

        const dx = x0 - xx
        const dy = y0 - yy
        return Math.sqrt(dx * dx + dy * dy)
    }

    const douglasPeucker = (points: [number, number][], epsilon: number): [number, number][] => {
        if (points.length <= 2) return points

        let maxDistance = 0
        let index = 0
        const end = points.length - 1

        for (let i = 1; i < end; i++) {
            const distance = getPerpendicularDistance(points[i], points[0], points[end])
            if (distance > maxDistance) {
                index = i
                maxDistance = distance
            }
        }

        if (maxDistance > epsilon) {
            const left = douglasPeucker(points.slice(0, index + 1), epsilon)
            const right = douglasPeucker(points.slice(index), epsilon)
            return [...left.slice(0, -1), ...right]
        }

        return [points[0], points[end]]
    }

    return douglasPeucker(coordinates, tolerance)
}

// Simplify fire station boundary coordinates for better rendering performance
const simplifyBoundary = (boundary: FireStationBoundary): FireStationBoundary => {
    const simplifiedCoordinates = boundary.coordinates.map(ring =>
        simplifyPolygon(ring, 0.0005) // Adjust tolerance as needed
    )

    return {
        ...boundary,
        coordinates: simplifiedCoordinates
    }
}

interface UseFireStationBoundariesOptions {
    enabled?: boolean
    simplify?: boolean
    cacheTime?: number
    staleTime?: number
    limit?: number
    tolerance?: number
}

export const useFireStationBoundaries = (options: UseFireStationBoundariesOptions = {}) => {
    const {
        enabled = true,
        simplify = true,
        cacheTime = 10 * 60 * 1000, // 10 minutes
        staleTime = 5 * 60 * 1000,  // 5 minutes
        limit,
        tolerance = 0.0005
    } = options

    const query = useQuery({
        queryKey: ['fire-districts', { limit, simplify, tolerance }],
        queryFn: async () => {
            console.log('Loading fire department districts...')

            const fireDistrictsData = await fireStationsApi.getFireDistricts()
            console.log('Fire districts data loaded:', fireDistrictsData)

            if (!fireDistrictsData || !fireDistrictsData.features) {
                throw new Error('Invalid fire districts data format')
            }

            // Transform GeoJSON features to FireStationBoundary format
            const boundaries: FireStationBoundary[] = fireDistrictsData.features.map((district: any, index: number) => {
                const stationName = district.properties.PYR_YPIRES || `District ${index + 1}`

                // Handle both Polygon and MultiPolygon geometries
                let coordinates: [number, number][][] = []

                if (district.geometry.type === 'Polygon') {
                    // Single polygon - use outer ring only
                    coordinates = [district.geometry.coordinates[0]]
                } else if (district.geometry.type === 'MultiPolygon') {
                    // Multiple polygons - use first outer ring of each polygon
                    coordinates = district.geometry.coordinates.map((polygon: any) => polygon[0])
                }

                return {
                    id: index.toString(),
                    name: stationName,
                    coordinates: coordinates,
                    region: district.properties.region || 'Fire District'
                }
            })

            console.log('Transformed boundaries:', {
                count: boundaries.length,
                firstBoundary: boundaries[0],
                sampleCoordinates: boundaries[0]?.coordinates?.[0]?.slice(0, 3)
            })

            // Store raw data in localStorage for offline access (with size limit)
            if (typeof window !== 'undefined') {
                try {
                    const cacheKey = `fire-districts-${limit || 'all'}-${simplify}-${tolerance}`
                    const dataString = JSON.stringify({
                        data: boundaries,
                        timestamp: Date.now(),
                        options: { limit, simplify, tolerance }
                    })

                    if (dataString.length < 1024 * 1024) { // 1MB limit
                        localStorage.setItem(cacheKey, dataString)
                        console.log('Cached fire districts to localStorage')
                    } else {
                        console.log('Fire districts too large for localStorage cache, skipping')
                    }
                } catch (error) {
                    if (error instanceof DOMException && error.name === 'QuotaExceededError') {
                        console.warn('localStorage quota exceeded, clearing old cache and retrying')
                        // Clear old boundary caches
                        Object.keys(localStorage).forEach(key => {
                            if (key.startsWith('fire-districts-')) {
                                localStorage.removeItem(key)
                            }
                        })
                    } else {
                        console.warn('Failed to cache fire districts in localStorage:', error)
                    }
                }
            }

            return boundaries
        },
        enabled,
        staleTime,
        gcTime: cacheTime, // Updated from cacheTime to gcTime for newer react-query versions
        retry: (failureCount, error: any) => {
            // Don't retry on client errors
            if (error?.status && error.status < 500) {
                return false
            }
            return failureCount < 2
        },
        // Fallback to cached data if network fails
        placeholderData: () => {
            if (typeof window !== 'undefined') {
                try {
                    const cacheKey = `fire-districts-${limit || 'all'}-${simplify}-${tolerance}`
                    const cached = localStorage.getItem(cacheKey)
                    if (cached) {
                        const { data, timestamp } = JSON.parse(cached)
                        // Use cached data if it's less than 1 hour old
                        if (Date.now() - timestamp < 60 * 60 * 1000) {
                            return data
                        }
                    }
                } catch (error) {
                    console.warn('Failed to load cached fire districts:', error)
                }
            }
            return undefined
        }
    })

    // Memoize simplified boundaries to avoid recalculation on re-renders
    const simplifiedBoundaries = useMemo(() => {
        if (!query.data || !simplify) return query.data

        return query.data.map(simplifyBoundary)
    }, [query.data, simplify])

    return {
        ...query,
        data: simplify ? simplifiedBoundaries : query.data,
        // Additional utility methods
        clearCache: () => {
            if (typeof window !== 'undefined') {
                Object.keys(localStorage).forEach(key => {
                    if (key.startsWith('fire-districts-')) {
                        localStorage.removeItem(key)
                    }
                })
            }
        },
        getCacheInfo: () => {
            if (typeof window !== 'undefined') {
                try {
                    const cacheKey = `fire-districts-${limit || 'all'}-${simplify}-${tolerance}`
                    const cached = localStorage.getItem(cacheKey)
                    if (cached) {
                        const { timestamp } = JSON.parse(cached)
                        return {
                            cached: true,
                            age: Date.now() - timestamp,
                            isStale: Date.now() - timestamp > staleTime
                        }
                    }
                } catch (error) {
                    console.warn('Failed to get cache info:', error)
                }
            }
            return { cached: false, age: 0, isStale: true }
        }
    }
}

// Hook for lazy loading boundaries only when needed
export const useLazyFireStationBoundaries = () => {
    const [enabled, setEnabled] = useState(false)

    const query = useFireStationBoundaries({
        enabled,
        simplify: true
    })

    const loadBoundaries = useCallback(() => {
        setEnabled(true)
    }, [])

    const unloadBoundaries = useCallback(() => {
        setEnabled(false)
    }, [])

    return {
        ...query,
        loadBoundaries,
        unloadBoundaries,
        isEnabled: enabled
    }
}