import React, { useMemo, useState, useCallback, useEffect } from 'react'
import { Polygon, Tooltip } from 'react-leaflet'
import { useFireStationBoundaries } from '../hooks/useFireStationBoundaries'
// FireStationBoundary type is used in the hook
import LoadingSpinner from './LoadingSpinner'
import RetryButton from './RetryButton'
import proj4 from 'proj4'

// Define Greek Grid EPSG:2100 projection
proj4.defs('EPSG:2100', '+proj=tmerc +lat_0=0 +lon_0=24 +k=0.9996 +x_0=500000 +y_0=0 +ellps=GRS80 +towgs84=-199.87,74.79,246.62,0,0,0,0 +units=m +no_defs')

// Coordinate transformation cache for performance
const coordinateCache = new Map<string, [number, number]>()

// Greek Grid EPSG:2100 to WGS84 transformation using proj4
const transformCoordinate = (x: number, y: number): [number, number] => {
  const key = `${x},${y}`
  if (coordinateCache.has(key)) {
    return coordinateCache.get(key)!
  }
  
  try {
    // Transform from Greek Grid (EPSG:2100) to WGS84 (EPSG:4326)
    const transformed = proj4('EPSG:2100', 'EPSG:4326', [x, y])
    const result: [number, number] = [transformed[1], transformed[0]] // [lat, lng]
    
    // Validate coordinates are within Greece bounds
    if (result[0] >= 34 && result[0] <= 42 && result[1] >= 19 && result[1] <= 30) {
      coordinateCache.set(key, result)
      return result
    }
  } catch (error) {
    console.warn('Coordinate transformation failed:', error)
  }
  
  // Return null coordinates if transformation fails or is out of bounds
  return [0, 0]
}

interface OptimizedFireStationBoundariesProps {
    visible: boolean
}

const OptimizedFireStationBoundaries: React.FC<OptimizedFireStationBoundariesProps> = ({
    visible
}) => {
    const [hoveredBoundary, setHoveredBoundary] = useState<number | null>(null)

    const {
        data: boundaries,
        isLoading,
        error,
        refetch,
        getCacheInfo
    } = useFireStationBoundaries({
        enabled: visible,
        simplify: false, // Disable simplification for now to debug
        staleTime: 10 * 60 * 1000, // 10 minutes for boundaries (they don't change often)
        cacheTime: 30 * 60 * 1000  // 30 minutes cache time
    })

    // Debug logging
    // console.log('OptimizedFireStationBoundaries Debug:', {
    //     visible,
    //     isLoading,
    //     error: error?.message,
    //     boundariesCount: boundaries?.length,
    //     boundaries: boundaries?.slice(0, 2), // Log first 2 boundaries for inspection
    // })

    // State for batch loading
    const [visibleBoundaries, setVisibleBoundaries] = useState<any[]>([])
    const [batchIndex, setBatchIndex] = useState(0)
    const batchSize = 20 // Load 20 boundaries at a time for better performance

    // Memoize boundary processing with coordinate transformation to avoid recalculation
    const processedBoundaries = useMemo(() => {
        if (!boundaries) {
            console.log('No boundaries data available')
            return []
        }

        console.log('Processing boundaries:', {
            totalBoundaries: boundaries.length,
            firstBoundary: boundaries[0],
            sampleCoordinates: boundaries[0]?.coordinates?.[0]?.slice(0, 3) // First 3 coordinate pairs
        })

        // Transform coordinates once and cache the results
        const transformedBoundaries = boundaries.map((boundary, index) => {
            // Transform coordinates from Greek Grid (EPSG:2100) to WGS84
            const transformedCoordinates = boundary.coordinates.map((ring: [number, number][]) => {
                const validCoords = ring
                    .map((coord: [number, number]) => {
                        try {
                            // Validate input coordinates
                            if (!Array.isArray(coord) || coord.length !== 2) {
                                return null
                            }
                            
                            const [x, y] = coord
                            
                            // Check if coordinates are finite numbers
                            if (!isFinite(x) || !isFinite(y) || x === null || y === null || x === undefined || y === undefined) {
                                return null
                            }
                            
                            // Transform from Greek Grid to WGS84
                            const result = transformCoordinate(x, y)
                            
                            // Skip invalid transformations
                            if (result[0] === 0 && result[1] === 0) {
                                return null
                            }
                            
                            return result
                        } catch (coordError) {
                            console.warn('Error transforming coordinate:', coordError)
                            return null
                        }
                    })
                    .filter((coord): coord is [number, number] => coord !== null)
                
                return validCoords
            }).filter((ring: [number, number][]) => ring.length > 2) // Need at least 3 points for a polygon

            // Skip boundaries with no valid coordinates
            if (transformedCoordinates.length === 0) {
                console.warn(`Skipping boundary ${index} - no valid coordinates after transformation`)
                return null
            }

            return {
                ...boundary,
                transformedCoordinates
            }
        }).filter(Boolean)

        // Sort by area (larger areas first for better layering)
        const sortedBoundaries = transformedBoundaries
            .sort((a, b) => {
                // Try to get area from boundary data, fallback to coordinate count
                const aArea = (a as any)?.area || a?.transformedCoordinates?.[0]?.length || 0
                const bArea = (b as any)?.area || b?.transformedCoordinates?.[0]?.length || 0
                return bArea - aArea // Larger areas first (will be rendered behind smaller ones)
            })

        console.log('Processed boundaries with coordinate transformation:', {
            processedCount: sortedBoundaries.length,
            firstProcessed: sortedBoundaries[0],
            sampleTransformedCoords: sortedBoundaries[0]?.transformedCoordinates?.[0]?.slice(0, 3)
        })

        return sortedBoundaries
    }, [boundaries])

    // Load boundaries in batches for better performance
    useEffect(() => {
        if (!processedBoundaries.length || !visible) {
            setVisibleBoundaries([])
            setBatchIndex(0)
            return
        }

        // Start with first batch
        const firstBatch = processedBoundaries.slice(0, batchSize)
        setVisibleBoundaries(firstBatch)
        setBatchIndex(1)

        // Load remaining batches with delay
        const loadNextBatch = () => {
            setBatchIndex(prevIndex => {
                const startIndex = prevIndex * batchSize
                if (startIndex >= processedBoundaries.length) {
                    return prevIndex // No more batches
                }

                const nextBatch = processedBoundaries.slice(0, startIndex + batchSize)
                setVisibleBoundaries(nextBatch)
                
                console.log(`Loaded batch ${prevIndex + 1}: ${nextBatch.length} boundaries`)

                // Schedule next batch if there are more
                if (startIndex + batchSize < processedBoundaries.length) {
                    setTimeout(loadNextBatch, 200) // 200ms delay between batches for better performance
                }

                return prevIndex + 1
            })
        }

        // Start loading additional batches after initial render
        if (processedBoundaries.length > batchSize) {
            setTimeout(loadNextBatch, 500) // 500ms delay for first additional batch
        }
    }, [processedBoundaries, visible, batchSize])

    // Generate distinct colors for boundaries
    const generateDistinctColor = useCallback((index: number): string => {
        // Use HSL color space for better color distribution
        const hue = (index * 137.508) % 360 // Golden angle for better distribution
        const saturation = 60 + (index % 3) * 15 // Vary saturation
        const lightness = 45 + (index % 2) * 15  // Vary lightness
        return `hsl(${hue}, ${saturation}%, ${lightness}%)`
    }, [])

    // Memoize polygon styles to avoid recalculation
    const polygonStyles = useMemo(() => {
        return visibleBoundaries.map((_, index) => {
            const color = generateDistinctColor(index)
            return {
                color: color,
                weight: 2,
                opacity: 0.8,
                fillColor: color,
                fillOpacity: 0.15,
                // Increase opacity on hover
                ...(hoveredBoundary === index && {
                    opacity: 1,
                    fillOpacity: 0.3,
                    weight: 3
                })
            }
        })
    }, [visibleBoundaries, generateDistinctColor, hoveredBoundary])

    // Handle mouse events for better UX
    const handleMouseOver = useCallback((index: number) => {
        setHoveredBoundary(index)
    }, [])

    const handleMouseOut = useCallback(() => {
        setHoveredBoundary(null)
    }, [])

    // Don't render anything if not visible
    if (!visible) return null

    // Show loading state
    if (isLoading) {
        return (
            <div className="absolute top-4 right-4 z-10 bg-white p-2 rounded shadow">
                <LoadingSpinner size="sm" text="Loading boundaries..." />
            </div>
        )
    }

    // Show error state
    if (error) {
        return (
            <div className="absolute top-4 right-4 z-10 bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 p-3 rounded shadow">
                <div className="flex items-center space-x-2">
                    <span className="text-sm text-red-600 dark:text-red-400">Failed to load boundaries</span>
                    <RetryButton onRetry={() => refetch()} size="sm" />
                </div>
            </div>
        )
    }

    // Show cache info in development
    const cacheInfo = getCacheInfo()
    const showCacheInfo = import.meta.env.DEV

    return (
        <>
            {/* Cache info for development */}
            {showCacheInfo && cacheInfo.cached && (
                <div className="absolute top-4 left-4 z-10 bg-blue-50 border border-blue-200 p-2 rounded shadow text-xs">
                    <div>Cache: {cacheInfo.cached ? 'Hit' : 'Miss'}</div>
                    <div>Age: {Math.round(cacheInfo.age / 1000)}s</div>
                    <div>Stale: {cacheInfo.isStale ? 'Yes' : 'No'}</div>
                </div>
            )}

            {/* Loading indicator for batch loading */}
            {visible && visibleBoundaries.length < processedBoundaries.length && (
                <div className="absolute top-16 right-4 z-10 bg-blue-50 border border-blue-200 p-2 rounded shadow text-xs">
                    <div className="flex items-center space-x-2">
                        <div className="animate-spin w-3 h-3 border border-blue-500 border-t-transparent rounded-full"></div>
                        <span>Loading boundaries... {visibleBoundaries.length}/{processedBoundaries.length}</span>
                    </div>
                </div>
            )}

            {/* Performance info */}
            {showCacheInfo && (
                <div className="absolute bottom-4 left-4 z-10 bg-gray-50 dark:bg-gray-800 border border-gray-200 dark:border-gray-700 p-2 rounded shadow text-xs">
                    <div className="text-gray-900 dark:text-gray-100">Visible: {visibleBoundaries.length}/{processedBoundaries.length}</div>
                    <div className="text-gray-900 dark:text-gray-100">Total: {boundaries?.length || 0} boundaries</div>
                    <div>Batch: {batchIndex} (size: {batchSize})</div>
                </div>
            )}

            {/* Render boundaries */}
            {visibleBoundaries.map((boundary, index) => {
                // Use pre-transformed coordinates
                const leafletCoordinates = (boundary as any).transformedCoordinates

                // Skip if no valid coordinates (should not happen after processing)
                if (!leafletCoordinates || leafletCoordinates.length === 0) {
                    console.warn(`Skipping boundary ${index} - no transformed coordinates`)
                    return null
                }

                // console.log(`Rendering boundary ${index}:`, {
                //     id: boundary.id,
                //     name: boundary.name,
                //     coordinatesLength: leafletCoordinates.length,
                //     firstRingLength: leafletCoordinates[0]?.length,
                //     sampleCoords: leafletCoordinates[0]?.slice(0, 2)
                // })

                return (
                    <Polygon
                        key={`boundary-${boundary.id}-${index}`}
                        positions={leafletCoordinates}
                        pathOptions={polygonStyles[index]}
                        eventHandlers={{
                            mouseover: () => handleMouseOver(index),
                            mouseout: handleMouseOut
                        }}
                    >
                        <Tooltip
                            permanent={false}
                            direction="center"
                            opacity={0.9}
                            className="fire-station-tooltip"
                        >
                            <div className="text-center">
                                <div className="font-semibold text-gray-900 dark:text-gray-100">{boundary.name}</div>
                            </div>
                        </Tooltip>
                    </Polygon>
                )
            }).filter(Boolean)}
        </>
    )
}

export default OptimizedFireStationBoundaries