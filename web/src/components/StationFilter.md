# StationFilter Component

A reusable React component for filtering content by fire station. This component provides a dropdown interface for selecting fire stations and integrates with the fire stations API.

## Features

- **Dropdown Selection**: Clean dropdown interface for station selection
- **API Integration**: Automatically fetches fire stations from the backend
- **Loading States**: Shows loading indicator while fetching data
- **Error Handling**: Displays error message if stations cannot be loaded
- **Caching**: Uses React Query for efficient data caching (5-minute stale time)
- **Accessibility**: Includes proper ARIA labels for screen readers
- **Customizable**: Supports custom styling and placeholder text

## Props

| Prop | Type | Required | Default | Description |
|------|------|----------|---------|-------------|
| `selectedStationId` | `number \| undefined` | No | `undefined` | Currently selected station ID |
| `onStationChange` | `(stationId?: number) => void` | Yes | - | Callback function called when selection changes |
| `className` | `string` | No | `''` | Additional CSS classes to apply |
| `placeholder` | `string` | No | `'All Stations'` | Placeholder text for the "no selection" option |

## Usage

### Basic Usage

```tsx
import React, { useState } from 'react'
import StationFilter from './components/StationFilter'

function MyComponent() {
  const [selectedStation, setSelectedStation] = useState<number | undefined>()

  return (
    <StationFilter
      selectedStationId={selectedStation}
      onStationChange={setSelectedStation}
    />
  )
}
```

### With Custom Styling

```tsx
<StationFilter
  selectedStationId={selectedStation}
  onStationChange={setSelectedStation}
  className="mb-4"
  placeholder="Choose a station..."
/>
```

### Integration with Filtering Logic

```tsx
import React, { useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { vehiclesApi } from '../lib/api'
import StationFilter from './components/StationFilter'

function VehiclesList() {
  const [stationFilter, setStationFilter] = useState<number | undefined>()

  const { data: vehicles } = useQuery({
    queryKey: ['vehicles', stationFilter],
    queryFn: () => vehiclesApi.getAll({ stationId: stationFilter }).then(res => res.data),
  })

  return (
    <div>
      <StationFilter
        selectedStationId={stationFilter}
        onStationChange={setStationFilter}
      />
      
      {/* Render filtered vehicles */}
      {vehicles?.map(vehicle => (
        <div key={vehicle.id}>{vehicle.callsign}</div>
      ))}
    </div>
  )
}
```

## API Dependencies

The component depends on:
- `fireStationsApi.getAll()` - Fetches the list of fire stations
- React Query for caching and state management

## Styling

The component uses the project's design system classes:
- `.input` - For the select element styling
- Tailwind CSS classes for layout and spacing
- Lucide React icons for the filter icon

## Error States

The component handles the following error states:
- **API Error**: Shows "Failed to load stations" message with error icon
- **Loading State**: Shows "Loading stations..." in the dropdown
- **No Data**: Shows placeholder text when no stations are available

## Accessibility

- Uses semantic HTML with proper `<select>` element
- Includes `aria-label` for screen readers
- Maintains focus management for keyboard navigation

## Requirements Satisfied

This component satisfies the following requirements from the fire station management spec:

- **Requirement 3.2**: "WHEN viewing the vehicles page THEN the system SHALL provide a filter dropdown to select specific fire stations"
- **Requirement 4.2**: "WHEN viewing the crew page THEN the system SHALL provide a filter dropdown to select specific fire stations"

## Integration Examples

See `VehiclesListWithStationFilter.tsx` for a complete example of how to integrate this component into an existing page with multiple filters and grouped display logic.