import { useState, useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import { useMutation, useQueryClient, useQuery } from '@tanstack/react-query'
import { incidentsApi, incidentTypesApi, stationAssignmentApi, fireStationsApi, coastGuardStationsApi, policeStationsApi, hospitalsApi } from '../lib/api'

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

// Get stations based on agency type
const getStationsByAgency = async (agencyName?: string) => {
  const agencyType = mapAgencyNameToApiString(agencyName)
  
  switch (agencyType) {
    case 'fire':
      return await fireStationsApi.getStations()
    case 'coastguard':
      return await coastGuardStationsApi.getAll()
    case 'police':
      return await policeStationsApi.getStations()
    case 'hospital':
      return await hospitalsApi.getAll()
    default:
      return await fireStationsApi.getStations()
  }
}

import { IncidentPriority, IncidentStatus, IncidentTypeCategory, IncidentTypeSubcategory } from '../types'
import { Save, X, AlertTriangle, MapPin, CheckCircle } from 'lucide-react'
import { useUserStore } from '../lib/userStore'
import { useTranslation } from '../hooks/useTranslation'
import { useTheme } from '../lib/themeContext'
import LocationPicker from '../components/LocationPicker'

export default function NewIncident() {
    const navigate = useNavigate()
    const queryClient = useQueryClient()
    const { user, canCreateIncidents } = useUserStore()
    const t = useTranslation()
    const { language } = useTheme()

    const [formData, setFormData] = useState({
        type: '',
        mainCategory: '',
        subCategory: '',
        notes: '',
        address: '',
        street: '',
        streetNumber: '',
        city: '',
        region: '',
        postalCode: '',
        country: '',
        latitude: 0,
        longitude: 0,
        priority: IncidentPriority.Normal,
        stationId: 0, // Will be set automatically or manually
        createdByUserId: user?.id || 0,
        status: IncidentStatus.Created,
        callers: [] as Array<{
            name: string;
            phoneNumber: string;
            calledAt?: string;
            notes?: string;
        }>
    })

    const [isStationAutoAssigned, setIsStationAutoAssigned] = useState(false)
    const [stationLookupError, setStationLookupError] = useState<string | null>(null)
    const [isLookingUpStation, setIsLookingUpStation] = useState(false)

    // Fetch agency-specific stations for the dropdown
    const { data: stations = [] } = useQuery({
        queryKey: ['stations', user?.agencyName],
        queryFn: () => getStationsByAgency(user?.agencyName),
        staleTime: 5 * 60 * 1000, // Cache for 5 minutes
        enabled: !!user?.agencyName, // Only fetch when user has agency
    })

    // Fetch incident types for the user's agency
    const { data: incidentTypesResponse = null } = useQuery({
        queryKey: ['incident-types', user?.agencyName],
        queryFn: () => user?.agencyName ? incidentTypesApi.getByAgency(user.agencyName) : null,
        enabled: !!user?.agencyName,
        staleTime: 10 * 60 * 1000, // Cache for 10 minutes
    })

    const incidentTypes = incidentTypesResponse?.data

    // Redirect if user cannot create incidents
    useEffect(() => {
        if (!canCreateIncidents()) {
            navigate('/dashboard')
        }
    }, [canCreateIncidents, navigate])

    // Don't render the form if user can't create incidents
    if (!canCreateIncidents()) {
        return null
    }

    // Function to find station by coordinates
    const findStationByLocation = async (latitude: number, longitude: number) => {
        if (!latitude || !longitude) return

        setIsLookingUpStation(true)
        setStationLookupError(null)

        try {
            const agencyType = mapAgencyNameToApiString(user?.agencyName)
            const station = await stationAssignmentApi.findByLocation({ 
                latitude, 
                longitude, 
                agencyType 
            })
            
            if (station) {
                setFormData(prev => ({ ...prev, stationId: station.stationId }))
                setIsStationAutoAssigned(true)
                setStationLookupError(null)
            } else {
                // No station found for this location
                setIsStationAutoAssigned(false)
                setStationLookupError('No station found for this location. Please select manually.')
            }
        } catch (error) {
            console.error('Failed to find station by location:', error)
            setIsStationAutoAssigned(false)
            setStationLookupError('Unable to determine station automatically. Please select manually.')
        } finally {
            setIsLookingUpStation(false)
        }
    }

    // Auto-assign station when coordinates change
    useEffect(() => {
        if (formData.latitude && formData.longitude) {
            findStationByLocation(formData.latitude, formData.longitude)
        }
    }, [formData.latitude, formData.longitude])

    const createIncidentMutation = useMutation({
        mutationFn: (data: typeof formData) => incidentsApi.create(data),
        onSuccess: (response) => {
            queryClient.invalidateQueries({ queryKey: ['incidents'] })
            navigate(`/incidents/${response.data.id}`)
        },
        onError: (error) => {
            console.error('Failed to create incident:', error)
            alert('Failed to create incident. Please try again.')
        }
    })

    const handleSubmit = (e: React.FormEvent) => {
        e.preventDefault()
        if (!formData.mainCategory.trim()) {
            alert('Please select a main category')
            return
        }
        if (!formData.subCategory.trim()) {
            alert('Please select a sub category')
            return
        }
        if (!formData.latitude || !formData.longitude) {
            alert(t.setIncidentLocation)
            return
        }
        if (!formData.stationId) {
            alert(t.selectFireStation)
            return
        }

        createIncidentMutation.mutate(formData)
    }

    const handleCancel = () => {
        navigate('/incidents')
    }

    const handleLocationChange = (lat: number, lng: number, address?: string, detailedAddress?: any) => {
        setFormData(prev => ({
            ...prev,
            latitude: lat,
            longitude: lng,
            ...(address && { address }),
            ...(detailedAddress && {
                street: detailedAddress.street || '',
                streetNumber: detailedAddress.streetNumber || '',
                city: detailedAddress.city || '',
                region: detailedAddress.region || '',
                postalCode: detailedAddress.postalCode || '',
                country: detailedAddress.country || ''
            })
        }))
        // Reset auto-assignment state when location changes
        setIsStationAutoAssigned(false)
        setStationLookupError(null)
    }

    const handleAddressChange = (address: string) => {
        setFormData(prev => ({
            ...prev,
            address
        }))
    }

    const handleDetailedAddressChange = (detailedAddress: any) => {
        setFormData(prev => ({
            ...prev,
            street: detailedAddress.street || '',
            streetNumber: detailedAddress.streetNumber || '',
            city: detailedAddress.city || '',
            region: detailedAddress.region || '',
            postalCode: detailedAddress.postalCode || '',
            country: detailedAddress.country || ''
        }))
    }

    const addCaller = () => {
        const now = new Date().toISOString().slice(0, 16) // Format: YYYY-MM-DDTHH:mm
        setFormData(prev => ({
            ...prev,
            callers: [...prev.callers, { name: '', phoneNumber: '', calledAt: now, notes: '' }]
        }))
    }

    const removeCaller = (index: number) => {
        setFormData(prev => ({
            ...prev,
            callers: prev.callers.filter((_, i) => i !== index)
        }))
    }

    const updateCaller = (index: number, field: string, value: string) => {
        setFormData(prev => ({
            ...prev,
            callers: prev.callers.map((caller, i) => 
                i === index ? { ...caller, [field]: value } : caller
            )
        }))
    }

    const handleStationChange = (stationId: number) => {
        setFormData(prev => ({ ...prev, stationId }))
        // Clear auto-assignment state when manually selecting station
        setIsStationAutoAssigned(false)
        setStationLookupError(null)
    }

    // Helper function to get translated category name based on user's language preference
    const getCategoryName = (category: IncidentTypeCategory) => {
        return language === 'el' ? category.categoryNameEl : category.categoryNameEn
    }

    // Helper function to get translated subcategory name based on user's language preference
    const getSubcategoryName = (subcategory: IncidentTypeSubcategory) => {
        return language === 'el' ? subcategory.subcategoryNameEl : subcategory.subcategoryNameEn
    }

    // Get selected main category by finding the category whose translated name matches the stored value
    const selectedMainCategory = incidentTypes?.categories.find(cat => getCategoryName(cat) === formData.mainCategory)

    // Handle main category change
    const handleMainCategoryChange = (translatedCategoryName: string) => {
        setFormData(prev => ({
            ...prev,
            mainCategory: translatedCategoryName,
            subCategory: '' // Reset subcategory when main category changes
        }))
    }

    return (
        <div>
            <div className="mb-8">
                <h1 className="text-3xl font-bold text-gray-900 dark:text-gray-100">{t.newIncident}</h1>
                <p className="mt-2 text-gray-600 dark:text-gray-400">
                    Create a new emergency incident report
                </p>
            </div>

            <form onSubmit={handleSubmit} className="grid grid-cols-1 lg:grid-cols-2 gap-8">
                <div className="card p-6 space-y-6">
                    {/* Main Category */}
                    <div>
                        <label className="block text-sm font-medium text-gray-900 dark:text-gray-100 mb-2">
                            Main Category *
                        </label>
                        <select
                            value={formData.mainCategory}
                            onChange={(e) => handleMainCategoryChange(e.target.value)}
                            className="input w-full"
                            required
                        >
                            <option value="">Select Main Category</option>
                            {incidentTypes?.categories.map((category) => (
                                <option key={category.categoryKey} value={getCategoryName(category)}>
                                    {getCategoryName(category)}
                                </option>
                            ))}
                        </select>
                    </div>

                    {/* Sub Category */}
                    {selectedMainCategory && (
                        <div>
                            <label className="block text-sm font-medium text-gray-900 dark:text-gray-100 mb-2">
                                Sub Category *
                            </label>
                            <select
                                value={formData.subCategory}
                                onChange={(e) => setFormData(prev => ({ ...prev, subCategory: e.target.value }))}
                                className="input w-full"
                                required
                            >
                                <option value="">Select Sub Category</option>
                                {selectedMainCategory.subcategories.map((subcategory, index) => (
                                    <option key={index} value={getSubcategoryName(subcategory)}>
                                        {getSubcategoryName(subcategory)}
                                    </option>
                                ))}
                            </select>
                        </div>
                    )}

                    {/* Priority */}
                    <div>
                        <label className="block text-sm font-medium text-gray-900 dark:text-gray-100 mb-2">
                            {t.incidentPriority} *
                        </label>
                        <select
                            value={formData.priority}
                            onChange={(e) => setFormData(prev => ({ ...prev, priority: parseInt(e.target.value) as IncidentPriority }))}
                            className="input w-full"
                            required
                        >
                            <option value={IncidentPriority.Critical}>{t.criticalPriority}</option>
                            <option value={IncidentPriority.High}>{t.highPriority}</option>
                            <option value={IncidentPriority.Normal}>{t.normalPriority}</option>
                            <option value={IncidentPriority.Low}>{t.lowPriority}</option>
                        </select>
                    </div>



                    {/* Notes */}
                    <div>
                        <label className="block text-sm font-medium text-gray-900 dark:text-gray-100 mb-2">
                            {t.incidentNotes}
                        </label>
                        <textarea
                            value={formData.notes}
                            onChange={(e) => setFormData(prev => ({ ...prev, notes: e.target.value }))}
                            rows={3}
                            className="input w-full"
                            placeholder={t.briefNotes}
                        />
                    </div>

                    {/* Caller Information Section */}
                    <div className="space-y-4">
                        <div className="flex items-center justify-between">
                            <h3 className="text-lg font-semibold text-gray-900 dark:text-gray-100">
                                {t.callerInformation}
                            </h3>
                            <button
                                type="button"
                                onClick={addCaller}
                                className="btn btn-secondary text-sm"
                            >
                                Add Caller
                            </button>
                        </div>

                        {formData.callers.map((caller, index) => (
                            <div key={index} className="border border-gray-200 dark:border-gray-700 rounded-lg p-4 space-y-4">
                                <div className="flex items-center justify-between">
                                    <h4 className="text-md font-medium text-gray-900 dark:text-gray-100">
                                        {t.caller} {index + 1}
                                    </h4>
                                    <button
                                        type="button"
                                        onClick={() => removeCaller(index)}
                                        className="text-red-600 hover:text-red-800 dark:text-red-400 dark:hover:text-red-300"
                                    >
                                        <X className="w-4 h-4" />
                                    </button>
                                </div>

                                <div className="grid grid-cols-2 gap-4">
                                    <div>
                                        <label className="block text-sm font-medium text-gray-900 dark:text-gray-100 mb-2">
                                            Name
                                        </label>
                                        <input
                                            type="text"
                                            value={caller.name}
                                            onChange={(e) => updateCaller(index, 'name', e.target.value)}
                                            className="input w-full"
                                            placeholder="Caller name"
                                        />
                                    </div>
                                    <div>
                                        <label className="block text-sm font-medium text-gray-900 dark:text-gray-100 mb-2">
                                            Phone Number
                                        </label>
                                        <input
                                            type="tel"
                                            value={caller.phoneNumber}
                                            onChange={(e) => updateCaller(index, 'phoneNumber', e.target.value)}
                                            className="input w-full"
                                            placeholder="Phone number"
                                        />
                                    </div>
                                </div>

                                <div>
                                    <label className="block text-sm font-medium text-gray-900 dark:text-gray-100 mb-2">
                                        {t.notes}
                                    </label>
                                    <textarea
                                        value={caller.notes || ''}
                                        onChange={(e) => updateCaller(index, 'notes', e.target.value)}
                                        rows={2}
                                        className="input w-full"
                                        placeholder={t.notes}
                                    />
                                </div>
                            </div>
                        ))}
                    </div>

                    {/* Address Section */}
                    <div className="space-y-4">
                        <h3 className="text-lg font-semibold text-gray-900 dark:text-gray-100">
                            {t.addressDetails}
                        </h3>

                        {/* Full Address (optional for backward compatibility) */}
                        <div>
                            <label className="block text-sm font-medium text-gray-900 dark:text-gray-100 mb-2">
                                {t.fullAddress}
                            </label>
                            <input
                                type="text"
                                value={formData.address}
                                onChange={(e) => setFormData(prev => ({ ...prev, address: e.target.value }))}
                                className="input w-full"
                                placeholder={t.fullAddress + ' (' + t.optional + ')'}
                            />
                        </div>

                        {/* Street and Street Number */}
                        <div className="grid grid-cols-3 gap-4">
                            <div className="col-span-2">
                                <label className="block text-sm font-medium text-gray-900 dark:text-gray-100 mb-2">
                                    {t.street}
                                </label>
                                <input
                                    type="text"
                                    value={formData.street}
                                    onChange={(e) => setFormData(prev => ({ ...prev, street: e.target.value }))}
                                    className="input w-full"
                                    placeholder={t.street}
                                />
                            </div>
                            <div>
                                <label className="block text-sm font-medium text-gray-900 dark:text-gray-100 mb-2">
                                    {t.streetNumber}
                                </label>
                                <input
                                    type="text"
                                    value={formData.streetNumber}
                                    onChange={(e) => setFormData(prev => ({ ...prev, streetNumber: e.target.value }))}
                                    className="input w-full"
                                    placeholder={t.streetNumber}
                                />
                            </div>
                        </div>

                        {/* City and Postal Code */}
                        <div className="grid grid-cols-2 gap-4">
                            <div>
                                <label className="block text-sm font-medium text-gray-900 dark:text-gray-100 mb-2">
                                    {t.city}
                                </label>
                                <input
                                    type="text"
                                    value={formData.city}
                                    onChange={(e) => setFormData(prev => ({ ...prev, city: e.target.value }))}
                                    className="input w-full"
                                    placeholder={t.city}
                                />
                            </div>
                            <div>
                                <label className="block text-sm font-medium text-gray-900 dark:text-gray-100 mb-2">
                                    {t.postalCode}
                                </label>
                                <input
                                    type="text"
                                    value={formData.postalCode}
                                    onChange={(e) => setFormData(prev => ({ ...prev, postalCode: e.target.value }))}
                                    className="input w-full"
                                    placeholder={t.postalCode}
                                />
                            </div>
                        </div>

                        {/* Region and Country */}
                        <div className="grid grid-cols-2 gap-4">
                            <div>
                                <label className="block text-sm font-medium text-gray-900 dark:text-gray-100 mb-2">
                                    {t.region}
                                </label>
                                <input
                                    type="text"
                                    value={formData.region}
                                    onChange={(e) => setFormData(prev => ({ ...prev, region: e.target.value }))}
                                    className="input w-full"
                                    placeholder={t.region}
                                />
                            </div>
                            <div>
                                <label className="block text-sm font-medium text-gray-900 dark:text-gray-100 mb-2">
                                    {t.country}
                                </label>
                                <input
                                    type="text"
                                    value={formData.country}
                                    onChange={(e) => setFormData(prev => ({ ...prev, country: e.target.value }))}
                                    className="input w-full"
                                    placeholder={t.country}
                                />
                            </div>
                        </div>
                    </div>

                    {/* Coordinates Display */}
                    <div className="grid grid-cols-2 gap-4">
                        <div>
                            <label className="block text-xs text-gray-600 dark:text-gray-400 mb-1">{t.latitude}</label>
                            <input
                                type="number"
                                step="any"
                                value={formData.latitude || ''}
                                onChange={(e) => setFormData(prev => ({ ...prev, latitude: parseFloat(e.target.value) || 0 }))}
                                className="input w-full"
                                placeholder="0.000000"
                            />
                        </div>
                        <div>
                            <label className="block text-xs text-gray-600 dark:text-gray-400 mb-1">{t.longitude}</label>
                            <input
                                type="number"
                                step="any"
                                value={formData.longitude || ''}
                                onChange={(e) => setFormData(prev => ({ ...prev, longitude: parseFloat(e.target.value) || 0 }))}
                                className="input w-full"
                                placeholder="0.000000"
                            />
                        </div>
                    </div>

                    {/* Station */}
                    <div>
                        <label className="block text-sm font-medium text-gray-900 dark:text-gray-100 mb-2">
                            {t.assignToStation} *
                        </label>
                        <div className="relative">
                            <select
                                value={formData.stationId || ''}
                                onChange={(e) => handleStationChange(parseInt(e.target.value) || 0)}
                                className={`w-full px-3 py-2 border rounded-md bg-white dark:bg-gray-800 text-gray-900 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-primary-500 ${
                                    isStationAutoAssigned 
                                        ? 'border-green-300 dark:border-green-600 bg-green-50 dark:bg-green-900' 
                                        : 'border-gray-300 dark:border-gray-600'
                                }`}
                                required
                                disabled={isLookingUpStation}
                            >
                                <option value="">{t.selectStation}</option>
                                {stations.map((station: any) => (
                                    <option key={station.id} value={station.id}>
                                        {station.name}
                                    </option>
                                ))}
                            </select>
                            {isLookingUpStation && (
                                <div className="absolute right-3 top-1/2 transform -translate-y-1/2">
                                    <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-primary-600"></div>
                                </div>
                            )}
                        </div>
                        
                        {/* Auto-assignment status */}
                        {isStationAutoAssigned && (
                            <div className="mt-2 flex items-center text-sm text-green-600 dark:text-green-400">
                                <CheckCircle className="w-4 h-4 mr-1" />
                                {t.automaticallyAssigned} {stations.find((s: any) => s.id === formData.stationId)?.name || 'selected station'} based on incident location
                            </div>
                        )}
                        
                        {/* Station lookup error */}
                        {stationLookupError && (
                            <div className="mt-2 flex items-center text-sm text-amber-600 dark:text-amber-400">
                                <AlertTriangle className="w-4 h-4 mr-1" />
                                {stationLookupError === 'No fire station found for this location. Please select manually.' ? t.noStationFound : t.unableToDetermineStation}
                            </div>
                        )}
                        
                        {/* Location-based assignment info */}
                        {Boolean(formData.latitude && formData.longitude && !isLookingUpStation && !isStationAutoAssigned && !stationLookupError) && (
                        <div className="mt-2 flex items-center text-sm text-blue-600 dark:text-blue-400">
                            <MapPin className="w-4 h-4 mr-1" />
                            {t.stationAssignmentInfo}
                        </div>
                        )}
                    </div>
                    {/* Actions */}
                    <div className="flex justify-end space-x-4 pt-4">
                        <button
                            type="button"
                            onClick={handleCancel}
                            className="btn btn-secondary flex items-center"
                        >
                            <X className="w-4 h-4 mr-2" />
                            {t.cancel}
                        </button>
                        <button
                            type="submit"
                            disabled={createIncidentMutation.isPending}
                            className="btn btn-primary flex items-center"
                        >
                            <Save className="w-4 h-4 mr-2" />
                            {createIncidentMutation.isPending ? t.creating : t.createIncident}
                        </button>
                    </div>
                </div>

                {/* Location Picker - Right Column */}
                <div className="card p-6">
                    <h3 className="text-lg font-semibold text-gray-900 dark:text-gray-100 mb-4">
                        {t.incidentLocation}
                    </h3>
                    <LocationPicker
                        latitude={formData.latitude}
                        longitude={formData.longitude}
                        address={formData.address}
                        onLocationChange={handleLocationChange}
                        onAddressChange={handleAddressChange}
                        onDetailedAddressChange={handleDetailedAddressChange}
                    />
                </div>
            </form>
        </div>
    )
}