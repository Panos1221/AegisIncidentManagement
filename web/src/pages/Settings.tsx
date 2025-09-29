import { useState } from 'react'
import { Save, Bell, Shield, Database } from 'lucide-react'
import { StationFilter } from '../components'
import { useTranslation } from '../hooks/useTranslation'

export default function Settings() {
  const t = useTranslation()
  const [settings, setSettings] = useState({
    notifications: {
      emailAlerts: true,
      smsAlerts: false,
      pushNotifications: true,
      incidentUpdates: true
    },
    system: {
      autoAssignment: false,
      defaultStation: 1,
      mapProvider: 'google',
      refreshInterval: '30'
    },
    security: {
      sessionTimeout: '60',
      requireMFA: false,
      passwordExpiry: '90'
    }
  })

  const handleSave = () => {
    // TODO: Implement save functionality
    console.log('Saving settings:', settings)
    alert(t.settingsSaved)
  }

  const updateSetting = (category: string, key: string, value: any) => {
    setSettings(prev => ({
      ...prev,
      [category]: {
        ...prev[category as keyof typeof prev],
        [key]: value
      }
    }))
  }

  return (
    <div>
      <div className="mb-8">
        <h1 className="text-3xl font-bold text-gray-900 dark:text-gray-100">{t.settingsPageTitle}</h1>
        <p className="mt-2 text-gray-600 dark:text-gray-400">
          {t.settingsDescription}
        </p>
      </div>

      <div className="space-y-6">
        {/* Notifications */}
        <div className="card p-6">
          <div className="flex items-center mb-4">
            <Bell className="w-5 h-5 text-gray-600 dark:text-gray-400 mr-2" />
            <h2 className="text-lg font-semibold text-gray-900 dark:text-gray-100">{t.notifications}</h2>
          </div>
          
          <div className="space-y-4">
            <div className="flex items-center justify-between">
              <div>
                <label className="text-sm font-medium text-gray-900 dark:text-gray-100">{t.emailAlerts}</label>
                <p className="text-sm text-gray-600 dark:text-gray-400">{t.emailAlertsDescription}</p>
              </div>
              <input
                type="checkbox"
                checked={settings.notifications.emailAlerts}
                onChange={(e) => updateSetting('notifications', 'emailAlerts', e.target.checked)}
                className="h-4 w-4 text-primary-600 focus:ring-primary-500 border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-800 rounded"
              />
            </div>
            
            <div className="flex items-center justify-between">
              <div>
                <label className="text-sm font-medium text-gray-900 dark:text-gray-100">{t.smsAlerts}</label>
                <p className="text-sm text-gray-600 dark:text-gray-400">{t.smsAlertsDescription}</p>
              </div>
              <input
                type="checkbox"
                checked={settings.notifications.smsAlerts}
                onChange={(e) => updateSetting('notifications', 'smsAlerts', e.target.checked)}
                className="h-4 w-4 text-primary-600 focus:ring-primary-500 border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-800 rounded"
              />
            </div>
            
            <div className="flex items-center justify-between">
              <div>
                <label className="text-sm font-medium text-gray-900 dark:text-gray-100">{t.pushNotifications}</label>
                <p className="text-sm text-gray-600 dark:text-gray-400">{t.pushNotificationsDescription}</p>
              </div>
              <input
                type="checkbox"
                checked={settings.notifications.pushNotifications}
                onChange={(e) => updateSetting('notifications', 'pushNotifications', e.target.checked)}
                className="h-4 w-4 text-primary-600 focus:ring-primary-500 border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-800 rounded"
              />
            </div>
          </div>
        </div>

        {/* System Settings */}
        <div className="card p-6">
          <div className="flex items-center mb-4">
            <Database className="w-5 h-5 text-gray-600 dark:text-gray-400 mr-2" />
            <h2 className="text-lg font-semibold text-gray-900 dark:text-gray-100">{t.system}</h2>
          </div>
          
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-900 dark:text-gray-100 mb-1">
                {t.defaultStation}
              </label>
              <StationFilter
                selectedStationId={settings.system.defaultStation}
                onStationChange={(stationId) => updateSetting('system', 'defaultStation', stationId || 1)}
                placeholder={t.selectDefaultStation}
                required
              />
            </div>
            
            <div>
              <label className="block text-sm font-medium text-gray-900 dark:text-gray-100 mb-1">
                {t.mapProvider}
              </label>
              <select
                value={settings.system.mapProvider}
                onChange={(e) => updateSetting('system', 'mapProvider', e.target.value)}
                className="input w-full"
              >
                <option value="google">{t.googleMaps}</option>
                <option value="openstreet">{t.openStreetMap}</option>
                <option value="mapbox">{t.mapbox}</option>
              </select>
            </div>
            
            <div>
              <label className="block text-sm font-medium text-gray-900 dark:text-gray-100 mb-1">
                {t.refreshIntervalSeconds}
              </label>
              <input
                type="number"
                value={settings.system.refreshInterval}
                onChange={(e) => updateSetting('system', 'refreshInterval', e.target.value)}
                className="input w-full"
                min="10"
                max="300"
              />
            </div>
            
            <div className="flex items-center">
              <input
                type="checkbox"
                id="autoAssignment"
                checked={settings.system.autoAssignment}
                onChange={(e) => updateSetting('system', 'autoAssignment', e.target.checked)}
                className="h-4 w-4 text-primary-600 focus:ring-primary-500 border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-800 rounded mr-2"
              />
              <label htmlFor="autoAssignment" className="text-sm font-medium text-gray-900 dark:text-gray-100">
                {t.enableAutoAssignment}
              </label>
            </div>
          </div>
        </div>

        {/* Security Settings */}
        <div className="card p-6">
          <div className="flex items-center mb-4">
            <Shield className="w-5 h-5 text-gray-600 dark:text-gray-400 mr-2" />
            <h2 className="text-lg font-semibold text-gray-900 dark:text-gray-100">{t.security}</h2>
          </div>
          
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-900 dark:text-gray-100 mb-1">
                {t.sessionTimeoutMinutes}
              </label>
              <input
                type="number"
                value={settings.security.sessionTimeout}
                onChange={(e) => updateSetting('security', 'sessionTimeout', e.target.value)}
                className="input w-full"
                min="15"
                max="480"
              />
            </div>
            
            <div>
              <label className="block text-sm font-medium text-gray-900 dark:text-gray-100 mb-1">
                {t.passwordExpiryDays}
              </label>
              <input
                type="number"
                value={settings.security.passwordExpiry}
                onChange={(e) => updateSetting('security', 'passwordExpiry', e.target.value)}
                className="input w-full"
                min="30"
                max="365"
              />
            </div>
            
            <div className="flex items-center">
              <input
                type="checkbox"
                id="requireMFA"
                checked={settings.security.requireMFA}
                onChange={(e) => updateSetting('security', 'requireMFA', e.target.checked)}
                className="h-4 w-4 text-primary-600 focus:ring-primary-500 border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-800 rounded mr-2"
              />
              <label htmlFor="requireMFA" className="text-sm font-medium text-gray-900 dark:text-gray-100">
                {t.requireMFA}
              </label>
            </div>
          </div>
        </div>

        {/* Save Button */}
        <div className="flex justify-end">
          <button
            onClick={handleSave}
            className="btn btn-primary flex items-center"
          >
            <Save className="w-4 h-4 mr-2" />
            {t.saveSettings}
          </button>
        </div>
      </div>
    </div>
  )
}