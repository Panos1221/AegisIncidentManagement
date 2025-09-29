import { useQuery } from '@tanstack/react-query'
import { CheckCircle, XCircle, AlertCircle } from 'lucide-react'
import { useTranslation } from '../hooks/useTranslation'

export default function SystemStatus() {
  const t = useTranslation()
  const { data: healthCheck, isLoading, error } = useQuery({
    queryKey: ['health'],
    queryFn: () => fetch('/api/health').then(res => res.json()),
    refetchInterval: 30000, // Check every 30 seconds
  })

  if (isLoading) {
    return (
      <div className="flex items-center text-sm text-gray-500 dark:text-gray-400">
        <AlertCircle className="w-4 h-4 mr-1 animate-pulse" />
        Checking system status...
      </div>
    )
  }

  if (error || !healthCheck) {
    return (
      <div className="flex items-center text-sm text-red-600 dark:text-red-400">
        <XCircle className="w-4 h-4 mr-1" />
        API Offline
      </div>
    )
  }

  return (
    <div className="flex items-center text-sm text-green-600 dark:text-green-400">
      <CheckCircle className="w-4 h-4 mr-1" />
      {t.systemOnline}
    </div>
  )
}