import React, { useState } from 'react'
import StationFilter from './StationFilter'

/**
 * Demo component to showcase StationFilter usage
 * This demonstrates the component's functionality and can be used for testing
 */
const StationFilterDemo: React.FC = () => {
  const [selectedStationId, setSelectedStationId] = useState<number | undefined>()

  return (
    <div className="p-6 max-w-md">
      <h3 className="text-lg font-semibold mb-4">Station Filter Demo</h3>
      
      <div className="space-y-4">
        <div>
          <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
            Select Fire Station:
          </label>
          <StationFilter
            selectedStationId={selectedStationId}
            onStationChange={setSelectedStationId}
            placeholder="Choose a station..."
          />
        </div>
        
        <div className="p-3 bg-gray-50 dark:bg-gray-700 rounded-md">
          <p className="text-sm text-gray-600 dark:text-gray-400">
            Selected Station ID: {selectedStationId || 'None'}
          </p>
        </div>
        
        <button
          onClick={() => setSelectedStationId(undefined)}
          className="btn btn-secondary text-sm"
        >
          Clear Selection
        </button>
      </div>
    </div>
  )
}

export default StationFilterDemo